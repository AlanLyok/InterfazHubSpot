using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using InterfazHubSpot.Business.Diagnostics;
using InterfazHubSpot.Business.Integration;
using InterfazHubSpot.Business.Managers;
using InterfazHubSpot.Entities;
using Mastersoft.Framework.Standard;
using Newtonsoft.Json.Linq;

namespace InterfazHubSpot.Business.HubSpot
{
    /// <summary>
    /// Orquesta flujo 2A (cola <see cref="ProcesosSpertaHubSpot"/>) y 2B (cuenta corriente paginada) usando <see cref="ISpertaApiClient"/>.
    /// </summary>
    public sealed class HubSpotIntegracionRunner
    {
        private readonly ISpertaApiClient _api;

        private readonly HubSpotConfiguration _hubCfg;

        private HubSpotCrmClient _hub;

        private readonly ProcesosSpertaHubSpotManager _cola;

        private readonly IntegracionEjecucionLogManager _log;

        private readonly IProcesoPasoReporter _pasos;

        private readonly MSContext _msCtx;

        public HubSpotIntegracionRunner(
            MSContext ctx,
            ISpertaApiClient apiClient = null,
            IProcesoPasoReporter pasos = null,
            bool instrumentarClienteHttpSpertaApi = false)
        {
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx));
            _msCtx = ctx;
            _pasos = pasos ?? NullProcesoPasoReporter.Instance;

            ISpertaApiClient resolvedApi;
            if (apiClient != null)
                resolvedApi = apiClient;
            else if (instrumentarClienteHttpSpertaApi)
                resolvedApi = new TracingSpertaApiClient(new HttpSpertaApiClient(), _pasos);
            else
                resolvedApi = new HttpSpertaApiClient();
            _api = resolvedApi;

            _hubCfg = new HubSpotConfiguration();
            _pasos.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.DestinoExterno,
                "destinoexterno.hubspot.cfg",
                "Configuración efectiva HubSpot (sin secretos)",
                new
                {
                    baseUrl = _hubCfg.BaseUrl,
                    propertyMastersoftId = _hubCfg.PropertyMastersoftId,
                    propertyManejoCuentaCorriente = _hubCfg.PropertyManejoCuentaCorriente,
                    delayMsEntreLlamadas = _hubCfg.DelayMsBetweenCalls,
                    cuentaCorrientePageSize = _hubCfg.CuentaCorrientePageSize,
                    privateAppTokenConfigurado = _hubCfg.TienePrivateAppToken,
                    useDevelopmentMock = _hubCfg.UseDevelopmentMock,
                });

            _cola = new ProcesosSpertaHubSpotManager(ctx);
            _log = new IntegracionEjecucionLogManager(ctx);
        }

        private void EnsureHubClient()
        {
            if (_hub != null)
                return;

            if (_hubCfg.UseDevelopmentMock)
            {
                _pasos.RegistrarPaso(
                    ProcesoPasoSeverity.Information,
                    ProcesoPasoCategoria.DestinoExterno,
                    "destinoexterno.hubspot.mock",
                    "HubSpot CRM modo desarrollo mock (sin llamadas externas CRM v3).",
                    new { });
                var httpDev = new HttpClient(new DevelopmentHubSpotStubHandler()) { Timeout = TimeSpan.FromMinutes(5) };
                _hub = new HubSpotCrmClient(_hubCfg, httpDev);
                return;
            }

            _hubCfg.ValidarToken();
            _hub = new HubSpotCrmClient(_hubCfg);
        }

        /// <summary>Despacha ítems HubSpot pendientes (sin estadísticas de retorno).</summary>
        public void ProcesarColaHubSpot(int maxPorEjecucion)
        {
            ProcesoColaEjecucionResumen unused;
            ProcesarColaHubSpot(maxPorEjecucion, out unused);
        }

        /// <summary>Despacha ítems HubSpot pendientes y devuelve contadores agregados.</summary>
        public void ProcesarColaHubSpot(int maxPorEjecucion, out ProcesoColaEjecucionResumen resumen)
        {
            resumen = new ProcesoColaEjecucionResumen();
            var destino = IntegracionDestinos.HubSpot;

            bool bdOk;
            var bdDatos = ErpConnectivityProbe.ProbarMsgestion(_msCtx, out bdOk);
            _pasos.RegistrarPaso(
                bdOk ? ProcesoPasoSeverity.Information : ProcesoPasoSeverity.Warning,
                ProcesoPasoCategoria.Infraestructura,
                "infra.bd.msgestion",
                bdOk ? "Conexión MSGestion y SELECT 1 OK." : "No se pudo validar MSGestion.",
                bdDatos);

            var pendientes = _cola.ContarPendientes(destino);
            var enProceso = _cola.ContarEnProceso(destino);
            _pasos.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Cola,
                "cola." + destino + ".estado_previo",
                "Filas Pendiente / EnProceso antes del reclamo.",
                new { pendientes, en_proceso = enProceso });

            var tomados = _cola.ReclamarPendientes(destino, maxPorEjecucion);
            var idsEjemplo = tomados.Select(t => t.ProcesoId).Take(40).ToList();
            _pasos.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Cola,
                "cola." + destino + ".reclamados",
                tomados.Count + " fila(s) reclamada(s) y marcada(s) EnProceso.",
                new
                {
                    reclamadosN = tomados.Count,
                    maxItems = maxPorEjecucion,
                    procesoIdsMuestra = idsEjemplo,
                });

            resumen.ItemsReclamados = tomados.Count;
            var okItems = 0;
            var errItems = 0;

            foreach (var item in tomados)
            {
                EnsureHubClient();

                _pasos.RegistrarPaso(
                    ProcesoPasoSeverity.Information,
                    ProcesoPasoCategoria.Cola,
                    "cola.item.inicio",
                    "Procesando fila de cola.",
                    new
                    {
                        item.ProcesoId,
                        item.TipoEntidad,
                        item.TipoOperacion,
                        item.Destino,
                        item.Identificador,
                        item.Intentos,
                    });

                int clienteCola;
                if (!IntegracionColaIdentificador.TryGetClienteId(item, out clienteCola, out var parseErr))
                {
                    errItems++;
                    _cola.MarcarError(item.ProcesoId, parseErr);
                    _log.Registrar(item.ProcesoId, IntegracionDestinos.HubSpot, null, "SyncClienteColaPayload", false, parseErr);
                    _pasos.RegistrarPaso(
                        ProcesoPasoSeverity.Error,
                        ProcesoPasoCategoria.Cola,
                        "cola.item.payload_invalido",
                        parseErr,
                        new { item.ProcesoId });
                    continue;
                }

                try
                {
                    RunSync(() => SincronizarClienteColaAsync(item.ProcesoId, clienteCola));
                    _cola.MarcarOk(item.ProcesoId);
                    _log.Registrar(item.ProcesoId, IntegracionDestinos.HubSpot, clienteCola, "SyncClienteCola", true, "OK");
                    okItems++;
                    _pasos.RegistrarPaso(
                        ProcesoPasoSeverity.Information,
                        ProcesoPasoCategoria.Cola,
                        "cola.item.ok",
                        "Sincronización HubSpot terminada OK para el ítem.",
                        new { item.ProcesoId, clienteId = clienteCola });
                }
                catch (Exception ex)
                {
                    errItems++;
                    _cola.MarcarError(item.ProcesoId, ex.Message);
                    _log.Registrar(item.ProcesoId, IntegracionDestinos.HubSpot, clienteCola, "SyncClienteCola", false, ex.ToString());
                    _pasos.RegistrarPaso(
                        ProcesoPasoSeverity.Error,
                        ProcesoPasoCategoria.Cola,
                        "cola.item.error",
                        ex.Message,
                        new
                        {
                            item.ProcesoId,
                            clienteId = clienteCola,
                            tipoExcepcion = ex.GetType().Name,
                            detalleTruncado = DiagnosticsTextHelper.TruncateForTrace(ex.ToString()),
                        });
                }
            }

            resumen.ItemsSincronizadosOk = okItems;
            resumen.ItemsFallidos = errItems;
            _pasos.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Cola,
                "integracion.cola.resumen_corrida",
                "Fin corrida cola " + destino + ".",
                new
                {
                    resumen.ItemsReclamados,
                    resumen.ItemsSincronizadosOk,
                    resumen.ItemsFallidos,
                });
        }

        /// <summary>Páginas completas del GET cuenta corriente + batch update HubSpot (100).</summary>
        public void EjecutarSincronizacionCuentaCorriente()
        {
            EnsureHubClient();

            RunSync(async () =>
            {
                var cursor = 0;
                var tam = Math.Min(500, Math.Max(1, _hubCfg.CuentaCorrientePageSize));
                while (true)
                {
                    var json = await _api.GetIntegracionesHubSpotCuentaCorrienteAsync(cursor, tam).ConfigureAwait(false);
                    var envelope = JObject.Parse(json);
                    if (envelope.Value<bool>("HayError"))
                        throw new InvalidOperationException("SpertaAPI cuenta corriente: " + LeerPrimerError(envelope, json));

                    var datos = envelope["Datos"] as JObject;
                    if (datos == null)
                        break;

                    var hayMas = datos.Value<bool>("HayMas");
                    var siguiente = datos["SiguienteCursor"]?.Value<int?>();
                    var items = datos["Items"] as JArray ?? new JArray();

                    var batchTuples = new List<Tuple<string, string>>();
                    foreach (var it in items)
                    {
                        var clienteId = it["ClienteId"]?.Value<int>() ?? 0;
                        var texto = it["ManejoCuentaCorriente"]?.Value<string>() ?? string.Empty;
                        if (clienteId <= 0)
                            continue;

                        var mastersoftKey = clienteId.ToString(CultureInfo.InvariantCulture);
                        var hubCompanyId = await _hub.SearchCompanyIdByMastersoftIdAsync(mastersoftKey).ConfigureAwait(false);
                        if (string.IsNullOrEmpty(hubCompanyId))
                        {
                            _log.Registrar(null, IntegracionDestinos.HubSpot, clienteId, "BatchCcSinCompanyHubSpot", false,
                                "Sin compañía HubSpot para " + _hubCfg.PropertyMastersoftId + "=" + mastersoftKey + ".");
                            continue;
                        }

                        batchTuples.Add(Tuple.Create(hubCompanyId, texto));
                        if (batchTuples.Count >= 100)
                        {
                            await _hub.BatchUpdateCompaniesManejoCcAsync(batchTuples).ConfigureAwait(false);
                            batchTuples.Clear();
                        }
                    }

                    if (batchTuples.Count > 0)
                        await _hub.BatchUpdateCompaniesManejoCcAsync(batchTuples).ConfigureAwait(false);

                    if (!hayMas)
                        break;
                    if (!siguiente.HasValue)
                        break;

                    cursor = siguiente.Value;
                }

                _log.Registrar(null, IntegracionDestinos.HubSpot, null, "BatchCcCompleto", true, "OK");
            });
        }

        private async Task SincronizarClienteColaAsync(long procesoId, int clienteIdCola)
        {
            var json = await _api.GetIntegracionesClienteAsync(clienteIdCola).ConfigureAwait(false);
            var envelope = JObject.Parse(json);
            if (envelope.Value<bool>("HayError"))
                throw new InvalidOperationException("SpertaAPI cliente: " + LeerPrimerError(envelope, json));

            var datos = envelope["Datos"] as JObject;
            if (datos == null)
                throw new InvalidOperationException("Respuesta sin Datos.");

            var clientePk = datos.Value<int?>("ClienteId") ?? clienteIdCola;
            var codigoCliente = datos["CodigoCliente"]?.Value<string>();
            var ms = datos["Clientes"] as JObject;
            if (ms == null)
                throw new InvalidOperationException("Respuesta sin nodo Clientes.");

            _pasos.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Mapeo,
                "integraciones.cliente.payload_parseado",
                "Envelope integraciones cliente parseado.",
                new
                {
                    procesoId,
                    clientePk,
                    codigoCliente,
                    muestraDatosTruncada = DiagnosticsTextHelper.TruncateForTrace(datos.ToString()),
                });

            var mastersoftKey = clientePk.ToString(CultureInfo.InvariantCulture);

            var hubCompanyExistingId = await _hub.SearchCompanyIdByMastersoftIdAsync(mastersoftKey).ConfigureAwait(false);
            _pasos.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.DestinoExterno,
                "destinoexterno.hubspot.company.search_by_mastersoft",
                hubCompanyExistingId == null ? "Sin compañía existente por mastersoft id." : "Compañía existente encontrada.",
                new { procesoId, mastersoftKey, hubSpotCompanyId = hubCompanyExistingId ?? "(crear nueva)" });

            var companyProps = BuildCompanyProperties(ms, clientePk, codigoCliente);
            var envelopeCompany = new JObject { ["properties"] = companyProps };
            _pasos.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Mapeo,
                "mapeo.hubspot.company_properties",
                "Propiedades compañía preparadas para HubSpot.",
                new
                {
                    procesoId,
                    jsonTruncado = DiagnosticsTextHelper.TruncateForTrace(envelopeCompany.ToString()),
                });

            var respCompany = await _hub.UpsertCompanyAsync(hubCompanyExistingId, companyProps).ConfigureAwait(false);
            _pasos.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.DestinoExterno,
                "destinoexterno.hubspot.company_upsert",
                "Respuesta API compañías HubSpot.",
                new { procesoId, muestraTruncada = DiagnosticsTextHelper.TruncateForTrace(respCompany) });

            var joCompany = JObject.Parse(respCompany);
            var hubCompanyId = joCompany["id"]?.ToString() ?? hubCompanyExistingId;
            if (string.IsNullOrEmpty(hubCompanyId))
                throw new InvalidOperationException("HubSpot no devolvió id de compañía.");

            var contactos = ms["ListaClientesContactos"] as JArray ?? new JArray();
            var idxContacto = 0;
            foreach (var c in contactos)
            {
                idxContacto++;
                var jo = c as JObject;
                if (jo == null)
                    continue;

                var email = jo["CorreoElectronico"]?.Value<string>();
                if (string.IsNullOrWhiteSpace(email))
                {
                    _log.Registrar(procesoId, IntegracionDestinos.HubSpot, clientePk, "ContactoSinEmail", false,
                        "Contacto omitido sin CorreoElectronico.");
                    continue;
                }

                var hubContactExisting = await _hub.SearchContactIdByEmailAsync(email).ConfigureAwait(false);
                _pasos.RegistrarPaso(
                    ProcesoPasoSeverity.Information,
                    ProcesoPasoCategoria.DestinoExterno,
                    "destinoexterno.hubspot.contact.search_by_email",
                    hubContactExisting == null ? "Contacto nuevo por email." : "Contacto existente.",
                    new { procesoId, indice = idxContacto, email = email.Trim(), hubSpotContactId = hubContactExisting ?? "(crear nuevo)" });

                var contactProps = BuildContactProperties(jo);
                var envelopeContact = new JObject { ["properties"] = contactProps };
                _pasos.RegistrarPaso(
                    ProcesoPasoSeverity.Information,
                    ProcesoPasoCategoria.Mapeo,
                    "mapeo.hubspot.contact_properties",
                    "Propiedades contacto preparadas para HubSpot.",
                    new
                    {
                        procesoId,
                        indice = idxContacto,
                        jsonTruncado = DiagnosticsTextHelper.TruncateForTrace(envelopeContact.ToString()),
                    });

                var upsertContact = await _hub.UpsertContactAsync(hubContactExisting, contactProps).ConfigureAwait(false);
                var created = upsertContact.Item1;
                var contactIdReturned = upsertContact.Item2;
                _pasos.RegistrarPaso(
                    ProcesoPasoSeverity.Information,
                    ProcesoPasoCategoria.DestinoExterno,
                    "destinoexterno.hubspot.contact_upsert",
                    created ? "Contacto creado (POST)." : "Contacto actualizado (PATCH).",
                    new { procesoId, contactIdReturned, fueCreacion = created });

                if (string.IsNullOrEmpty(contactIdReturned))
                    throw new InvalidOperationException("HubSpot no devolvió id de contacto para " + email);

                if (created)
                {
                    await _hub.AssociateContactToCompanyAsync(contactIdReturned, hubCompanyId).ConfigureAwait(false);
                    _pasos.RegistrarPaso(
                        ProcesoPasoSeverity.Information,
                        ProcesoPasoCategoria.DestinoExterno,
                        "destinoexterno.hubspot.contact_associate_company",
                        "Asociación contacto a compañía.",
                        new { procesoId, contactIdReturned, hubCompanyId });
                }
            }
        }

        private JObject BuildCompanyProperties(JObject ms, int clientePk, string codigoCliente)
        {
            var raz = Coalesce(ms["RazonSocial"]?.ToString(), ms["ApellidoYNombre"]?.ToString(), ms["Contacto"]?.ToString());
            var fantasy = ms["ApellidoYNombre"]?.ToString();
            var nroDoc = ms["NumeroDocumento"]?.ToString();

            var props = new JObject
            {
                ["name"] = raz ?? string.Empty,
                ["nombre_fantasia"] = fantasy ?? string.Empty,
                ["cuitcuil"] = nroDoc ?? string.Empty,
                ["nro_cliente"] = codigoCliente ?? string.Empty,
                [_hubCfg.PropertyMastersoftId] = clientePk.ToString(CultureInfo.InvariantCulture),
                ["adress"] = ms["Calle"]?.ToString() ?? string.Empty,
                ["puerta"] = ms["Puerta"]?.ToString() ?? string.Empty,
                ["city"] = ms["Localidad"]?.ToString() ?? string.Empty,
                ["zip"] = ms["CodigoPostal"]?.ToString() ?? string.Empty,
                ["state"] = ms["CodigoProvinciaCliente"]?.ToString() ?? string.Empty,
                ["Country"] = CodigoPaisAString(ms["CodigoPais"]),
                ["zona_vta"] = ms["ZonaId"]?.ToString() ?? string.Empty,
                ["vendedor"] = ms["VendedorId"]?.ToString() ?? string.Empty,
                ["responsable_de_cuenta"] = ms["ResponsableCuentaId"]?.ToString() ?? string.Empty,
                ["lista_de_precios"] = ms["ListaPreciosId"]?.ToString() ?? string.Empty,
                ["condicion_de_venta"] = ms["CondicionVentaId"]?.ToString() ?? string.Empty,
                ["dias_para_deuda"] = ms["DiasParaDeuda"]?.ToString() ?? string.Empty,
                ["limite_de_credito"] = ms["LimiteCredito"]?.ToString() ?? string.Empty,
                ["categoria_cliente"] = ms["CategoriaClienteId"]?.ToString() ?? string.Empty,
            };

            var dirs = ms["ListaDireccionEntregas"] as JArray;
            if (dirs != null)
            {
                for (var i = 0; i < dirs.Count && i < 3; i++)
                {
                    var d = dirs[i] as JObject;
                    if (d == null)
                        continue;

                    var n = i + 1;
                    props["direccion_" + n + "_domicilio"] = Coalesce(d["Domicilio"]?.ToString(), d["Descripcion"]?.ToString()) ?? string.Empty;
                    props["direccion_" + n + "_cp"] = d["CodigoPostal"]?.ToString() ?? string.Empty;
                    props["direccion_" + n + "_localidad"] = d["Localidad"]?.ToString() ?? string.Empty;
                    props["direccion_" + n + "_provincia"] = d["ProvinciaId"]?.ToString() ?? string.Empty;
                }
            }

            return props;
        }

        private static JObject BuildContactProperties(JObject jo)
        {
            var nombre = jo["ApellidoYNombre"]?.ToString() ?? string.Empty;
            var sector = jo["SectorId"]?.ToString() ?? string.Empty;
            return new JObject
            {
                ["firstname"] = nombre,
                ["email"] = jo["CorreoElectronico"]?.ToString() ?? string.Empty,
                ["phone"] = jo["Telefono"]?.ToString() ?? string.Empty,
                ["sector"] = sector,
            };
        }

        private static string CodigoPaisAString(JToken codigoPais)
        {
            if (codigoPais == null || codigoPais.Type == JTokenType.Null)
                return string.Empty;
            return codigoPais.ToString();
        }

        private static string LeerPrimerError(JObject envelope, string fallbackJson)
        {
            var listaErr = envelope["ListaErrores"] as JArray;
            if (listaErr != null && listaErr.Count > 0)
                return listaErr[0]["DescripcionError"]?.ToString() ?? listaErr.ToString();
            return fallbackJson;
        }

        private static string Coalesce(params string[] valores)
        {
            foreach (var v in valores)
            {
                if (!string.IsNullOrWhiteSpace(v))
                    return v.Trim();
            }

            return null;
        }

        private static void RunSync(Func<Task> asyncWork)
        {
            asyncWork().GetAwaiter().GetResult();
        }
    }
}
