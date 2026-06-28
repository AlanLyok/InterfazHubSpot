using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using InterfazHubSpot.Business.Common;
using InterfazHubSpot.Business.Diagnostics;
using InterfazHubSpot.Business.Integration;
using InterfazHubSpot.Business.Integration.Dtos;
using InterfazHubSpot.Business.Managers;
using InterfazHubSpot.Entities;
using Mastersoft.Framework.Standard;
using Newtonsoft.Json.Linq;

namespace InterfazHubSpot.Business.HubSpot
{
    /// <summary>
    /// Orquesta flujo 2A (cola <see cref="ProcesosSpertaHubSpot"/>) y 2B (cuenta corriente paginada) usando <see cref="ClienteIntegracionManager"/>.
    /// </summary>
    public sealed class HubSpotIntegracionRunner
    {
        private readonly ClienteIntegracionManager _cli;

        private readonly HubSpotConfiguration _hubCfg;

        private HubSpotCrmClient _hub;

        private readonly ProcesosSpertaHubSpotManager _cola;

        private readonly ProcesosSpertaHubSpotLogManager _log;

        private readonly IProcesoPasoReporter _pasos;

        private readonly MSContext _msCtx;

        private readonly IHubSpotColaIntentosReporter _intentosReporter;

        private readonly IntegracionErrorNotifier _errorNotifier;

        public HubSpotIntegracionRunner(
            MSContext ctx,
            IProcesoPasoReporter pasos = null)
        {
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx));
            _msCtx = ctx;
            _pasos = pasos ?? NullProcesoPasoReporter.Instance;
            _cli = new ClienteIntegracionManager(ctx);

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
                    propertyCuitCuilUnica = _hubCfg.PropertyCuitCuilUnica,
                    propertyManejoCuentaCorriente = _hubCfg.PropertyManejoCuentaCorriente,
                    delayMsEntreLlamadas = _hubCfg.DelayMsBetweenCalls,
                    cuentaCorrientePageSize = _hubCfg.CuentaCorrientePageSize,
                    maxHttpRetries = _hubCfg.MaxHttpRetries,
                    httpRetryBackoffMs = _hubCfg.HttpRetryBackoffMilliseconds,
                    privateAppTokenConfigurado = _hubCfg.TienePrivateAppToken,
                    useDevelopmentMock = _hubCfg.UseDevelopmentMock,
                });

            _cola = new ProcesosSpertaHubSpotManager(ctx);
            _intentosReporter = new HubSpotColaIntentosReporter(_cola);
            _errorNotifier = IntegracionErrorNotifier.Create(ctx);
            _log = new ProcesosSpertaHubSpotLogManager(ctx);
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
                        "Sincronizacion HubSpot terminada OK para el ítem.",
                        new { item.ProcesoId, clienteId = clienteCola });
                }
                catch (HubSpotAuthException ex)
                {
                    errItems++;
                    _cola.MarcarError(item.ProcesoId, ex.Message);
                    _log.Registrar(item.ProcesoId, IntegracionDestinos.HubSpot, clienteCola, "SyncClienteCola", false, ex.ToString());
                    NotificarErrorFila2ASafe(item.ProcesoId, clienteCola, "autenticación HubSpot", ex);
                    _pasos.RegistrarPaso(
                        ProcesoPasoSeverity.Error,
                        ProcesoPasoCategoria.Cola,
                        "cola.item.error.auth",
                        ex.Message,
                        new { item.ProcesoId, clienteId = clienteCola, tipoExcepcion = ex.GetType().Name });
                }
                catch (HubSpotHttpRetriesExhaustedException ex)
                {
                    errItems++;
                    _cola.MarcarError(item.ProcesoId, ex.Message);
                    _log.Registrar(item.ProcesoId, IntegracionDestinos.HubSpot, clienteCola, "SyncClienteCola", false, ex.ToString());
                    NotificarErrorFila2ASafe(item.ProcesoId, clienteCola, "HTTP reintentos agotados", ex);
                    _pasos.RegistrarPaso(
                        ProcesoPasoSeverity.Error,
                        ProcesoPasoCategoria.Cola,
                        "cola.item.error.retries",
                        ex.Message,
                        new { item.ProcesoId, clienteId = clienteCola, tipoExcepcion = ex.GetType().Name });
                }
                catch (Exception ex)
                {
                    errItems++;
                    _cola.MarcarError(item.ProcesoId, ex.Message);
                    _log.Registrar(item.ProcesoId, IntegracionDestinos.HubSpot, clienteCola, "SyncClienteCola", false, ex.ToString());
                    NotificarErrorFila2ASafe(item.ProcesoId, clienteCola, "SyncClienteCola", ex);
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
                var loteNum = 0;
                while (true)
                {
                    var pagina = _cli.ObtenerPaginaCuentaCorriente(cursor, tam);
                    if (pagina == null || pagina.Items.Count == 0)
                        break;

                    var hayMas = pagina.HayMas;
                    var siguiente = pagina.SiguienteCursor;

                    var batchTuples = new List<Tuple<string, string>>();
                    foreach (var it in pagina.Items)
                    {
                        var clienteId = it.ClienteId;
                        var texto = it.ManejoCuentaCorriente ?? string.Empty;
                        if (clienteId <= 0)
                            continue;

                        string cuitCuilUnica;
                        if (!HubSpotCuitCuilHelper.TryGetClaveUnica(it.NumeroDocumento, out cuitCuilUnica, out var docErr))
                        {
                            _log.Registrar(null, IntegracionDestinos.HubSpot, clienteId, "BatchCcSinNroDocumento", false,
                                docErr + " ClienteId=" + clienteId.ToString(CultureInfo.InvariantCulture) + ".");
                            continue;
                        }

                        string hubCompanyId;
                        try
                        {
                            hubCompanyId = await _hub.SearchCompanyIdByCuitCuilUnicaAsync(cuitCuilUnica).ConfigureAwait(false);
                        }
                        catch (HubSpotAuthException ex)
                        {
                            _log.Registrar(null, IntegracionDestinos.HubSpot, clienteId, "BatchCcAuth", false, ex.ToString());
                            NotificarErrorAuthSafe(ex);
                            throw;
                        }

                        if (string.IsNullOrEmpty(hubCompanyId))
                        {
                            _log.Registrar(null, IntegracionDestinos.HubSpot, clienteId, "BatchCcSinCompanyHubSpot", false,
                                "Sin compañía HubSpot para " + _hubCfg.PropertyCuitCuilUnica + "=" + cuitCuilUnica + ".");
                            continue;
                        }

                        batchTuples.Add(Tuple.Create(hubCompanyId, texto));
                        if (batchTuples.Count >= 100)
                        {
                            loteNum++;
                            await EnviarBatchCuentaCorrienteAsync(batchTuples, loteNum).ConfigureAwait(false);
                            batchTuples.Clear();
                        }
                    }

                    if (batchTuples.Count > 0)
                    {
                        loteNum++;
                        await EnviarBatchCuentaCorrienteAsync(batchTuples, loteNum).ConfigureAwait(false);
                    }

                    if (!hayMas)
                        break;

                    cursor = siguiente;
                }

                _log.Registrar(null, IntegracionDestinos.HubSpot, null, "BatchCcCompleto", true, "OK");
            });
        }

        private async Task EnviarBatchCuentaCorrienteAsync(List<Tuple<string, string>> batchTuples, int loteNum)
        {
            try
            {
                await _hub.BatchUpdateCompaniesManejoCcAsync(batchTuples).ConfigureAwait(false);
            }
            catch (HubSpotAuthException ex)
            {
                _log.Registrar(null, IntegracionDestinos.HubSpot, null, "BatchCcAuth", false, ex.ToString());
                NotificarErrorAuthSafe(ex);
                throw;
            }
            catch (HubSpotHttpRetriesExhaustedException ex)
            {
                _log.Registrar(null, IntegracionDestinos.HubSpot, null, "BatchCcRetries", false, ex.ToString());
                NotificarErrorBatch2BSafe(loteNum, ex);
            }
            catch (Exception ex)
            {
                _log.Registrar(null, IntegracionDestinos.HubSpot, null, "BatchCcError", false, ex.ToString());
                NotificarErrorBatch2BSafe(loteNum, ex);
            }
        }

        private void NotificarErrorFila2ASafe(long procesoId, int clienteId, string fase, Exception ex)
        {
            try
            {
                _errorNotifier.NotificarErrorFila2A(procesoId, clienteId, fase, ex);
            }
            catch (Exception notifyEx)
            {
                Logger.Log("No se pudo encolar email (2A): " + notifyEx);
            }
        }

        private void NotificarErrorBatch2BSafe(int lote, Exception ex)
        {
            try
            {
                _errorNotifier.NotificarErrorBatch2B(lote, ex);
            }
            catch (Exception notifyEx)
            {
                Logger.Log("No se pudo encolar email (2B): " + notifyEx);
            }
        }

        private void NotificarErrorAuthSafe(Exception ex)
        {
            try
            {
                _errorNotifier.NotificarErrorAuth(ex);
            }
            catch (Exception notifyEx)
            {
                Logger.Log("No se pudo encolar email (auth): " + notifyEx);
            }
        }

        private async Task SincronizarClienteColaAsync(long procesoId, int clienteIdCola)
        {
            _pasos.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Infraestructura,
                "bd.sp.cliente_obtener",
                "Ejecutando SP para obtener datos del cliente.",
                new { procesoId, clienteId = clienteIdCola });

            var dto = _cli.ObtenerClienteParaHubSpot(clienteIdCola);
            if (dto == null)
                throw new InvalidOperationException("SP no devolvió datos para clienteId " + clienteIdCola + ".");

            var clientePk = dto.ClienteId > 0 ? dto.ClienteId : clienteIdCola;
            var codigoCliente = dto.CodigoCliente;

            string cuitCuilUnica;
            string cuitErr;
            if (!HubSpotCuitCuilHelper.TryGetClaveUnica(dto.Cliente != null ? dto.Cliente.NumeroDocumento : null, out cuitCuilUnica, out cuitErr))
                throw new InvalidOperationException(cuitErr);

            _pasos.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Mapeo,
                "integraciones.cliente.payload_parseado",
                "Datos SP cliente cargados.",
                new
                {
                    procesoId,
                    clientePk,
                    codigoCliente,
                    cuitCuilUnica,
                    muestraDatosTruncada = DiagnosticsTextHelper.TruncateForTrace(Newtonsoft.Json.JsonConvert.SerializeObject(dto)),
                });

            var hubCompanyExistingId = await _hub.SearchCompanyIdByCuitCuilUnicaAsync(cuitCuilUnica, procesoId, _intentosReporter).ConfigureAwait(false);
            _pasos.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.DestinoExterno,
                "destinoexterno.hubspot.company.search_by_cuitcuil_unica",
                hubCompanyExistingId == null ? "Sin compañía existente por cuitcuil_unica." : "Compañía existente encontrada.",
                new { procesoId, cuitCuilUnica, hubSpotCompanyId = hubCompanyExistingId ?? "(crear nueva)" });

            var companyProps = BuildCompanyProperties(dto.Cliente, clientePk, codigoCliente, cuitCuilUnica);
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

            var respCompany = await _hub.UpsertCompanyAsync(hubCompanyExistingId, companyProps, procesoId, _intentosReporter).ConfigureAwait(false);
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

            PersistirIdHubSpotEnErp(clientePk, hubCompanyId, procesoId);

            _pasos.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Infraestructura,
                "bd.sp.contactos_obtener",
                "Ejecutando SP para obtener contactos del cliente.",
                new { procesoId, clienteId = clientePk });

            var contactos = _cli.ObtenerContactosClienteParaHubSpot(clientePk) ?? new List<ContactoDto>();
            var idxContacto = 0;
            foreach (var c in contactos)
            {
                idxContacto++;
                if (c == null)
                    continue;

                var email = c.CorreoElectronico;
                if (string.IsNullOrWhiteSpace(email))
                {
                    _log.Registrar(procesoId, IntegracionDestinos.HubSpot, clientePk, "ContactoSinEmail", false,
                        "Contacto omitido sin CorreoElectronico.");
                    continue;
                }

                var hubContactExisting = await _hub.SearchContactIdByEmailAsync(email, procesoId, _intentosReporter).ConfigureAwait(false);
                _pasos.RegistrarPaso(
                    ProcesoPasoSeverity.Information,
                    ProcesoPasoCategoria.DestinoExterno,
                    "destinoexterno.hubspot.contact.search_by_email",
                    hubContactExisting == null ? "Contacto nuevo por email." : "Contacto existente.",
                    new { procesoId, indice = idxContacto, email = (c.CorreoElectronico ?? string.Empty).Trim(), hubSpotContactId = hubContactExisting ?? "(crear nuevo)" });

                var contactProps = BuildContactProperties(c);
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

                var upsertContact = await _hub.UpsertContactAsync(hubContactExisting, contactProps, procesoId, _intentosReporter).ConfigureAwait(false);
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
                    await _hub.AssociateContactToCompanyAsync(contactIdReturned, hubCompanyId, procesoId, _intentosReporter).ConfigureAwait(false);
                    _pasos.RegistrarPaso(
                        ProcesoPasoSeverity.Information,
                        ProcesoPasoCategoria.DestinoExterno,
                        "destinoexterno.hubspot.contact_associate_company",
                        "Asociación contacto a compañía.",
                        new { procesoId, contactIdReturned, hubCompanyId });
                }
            }
        }

        private JObject BuildCompanyProperties(ClienteDatosDto ms, int clientePk, string codigoCliente, string cuitCuilUnica)
        {
            var raz = Coalesce(ms.RazonSocial, ms.ApellidoYNombre, ms.Contacto);
            var fantasy = ms.ApellidoYNombre;

            var props = new JObject
            {
                ["name"] = raz ?? string.Empty,
                ["nombre_fantasia"] = fantasy ?? string.Empty,
                [_hubCfg.PropertyCuitCuilUnica] = cuitCuilUnica ?? string.Empty,
                ["nro_cliente"] = codigoCliente ?? string.Empty,
                [_hubCfg.PropertyMastersoftId] = clientePk.ToString(CultureInfo.InvariantCulture),
                ["address"] = ms.Calle ?? string.Empty,
                ["puerta"] = ms.Puerta ?? string.Empty,
                ["city"] = ms.Localidad ?? string.Empty,
                ["zip"] = ms.CodigoPostal ?? string.Empty,
                ["state"] = ms.CodigoProvinciaCliente ?? string.Empty,
                ["country"] = ms.CodigoPais ?? string.Empty,
                ["zona_vta"] = ms.ZonaId ?? string.Empty,
                ["vendedor"] = ms.VendedorId ?? string.Empty,
                ["responsable_de_cuenta"] = ms.ResponsableCuentaId ?? string.Empty,
                ["lista_de_precios"] = ms.ListaPreciosId ?? string.Empty,
                ["condicion_de_venta"] = ms.CondicionVentaId ?? string.Empty,
                ["dias_para_deuda"] = ms.DiasParaDeuda ?? string.Empty,
                ["limite_de_credito"] = ms.LimiteCredito ?? string.Empty,
                ["categoria_cliente"] = ms.CategoriaClienteId ?? string.Empty,
                [_hubCfg.PropertyManejoCuentaCorriente] = ms.ManejoCuentaCorriente ?? string.Empty,
            };

            var dirs = ms.ListaDireccionEntregas;
            if (dirs != null)
            {
                for (var i = 0; i < dirs.Count && i < 3; i++)
                {
                    var d = dirs[i];
                    if (d == null)
                        continue;

                    var n = i + 1;
                    props["direccion_" + n + "_domicilio"] = d.Domicilio ?? string.Empty;
                    props["direccion_" + n + "_cp"] = d.CodigoPostal ?? string.Empty;
                    props["direccion_" + n + "_localidad"] = d.Localidad ?? string.Empty;
                    props["direccion_" + n + "_provincia"] = d.ProvinciaId ?? string.Empty;
                    props["direccion_" + n + "_pais"] = d.Pais ?? string.Empty;
                }
            }

            return props;
        }

        private static JObject BuildContactProperties(ContactoDto jo)
        {
            var nombre = jo.ApellidoYNombre ?? string.Empty;
            var sector = jo.SectorId ?? string.Empty;
            return new JObject
            {
                ["firstname"] = nombre,
                ["email"] = jo.CorreoElectronico ?? string.Empty,
                ["phone"] = jo.Telefono ?? string.Empty,
                ["sector"] = sector,
            };
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

        // ── Métodos diagnóstico granular (sin tocar cola) ──────────────────────────

        /// <summary>Diagnóstico paso 3: busca una compañía en HubSpot por cuitcuil_unica (NroDocumento del SP).</summary>
        public void DiagnosticarBuscarEmpresaHubSpot(int clienteId)
        {
            EnsureHubClient();

            var dto = _cli.ObtenerClienteParaHubSpot(clienteId);
            if (dto == null)
                throw new InvalidOperationException("SP no devolvió datos para clienteId " + clienteId + ".");

            string cuitCuilUnica;
            string cuitErr;
            if (!HubSpotCuitCuilHelper.TryGetClaveUnica(dto.Cliente != null ? dto.Cliente.NumeroDocumento : null, out cuitCuilUnica, out cuitErr))
                throw new InvalidOperationException(cuitErr);

            var hubId = RunSync(() => _hub.SearchCompanyIdByCuitCuilUnicaAsync(cuitCuilUnica));
            _pasos.RegistrarPaso(
                hubId != null ? ProcesoPasoSeverity.Information : ProcesoPasoSeverity.Warning,
                ProcesoPasoCategoria.DestinoExterno,
                "destinoexterno.hubspot.company.search_by_cuitcuil_unica",
                hubId != null
                    ? "Compañía encontrada en HubSpot."
                    : "Compañía NO encontrada en HubSpot para ese cuitcuil_unica.",
                new { clienteId, cuitCuilUnica, hubSpotCompanyId = hubId ?? "(no encontrada)" });
        }

        /// <summary>Diagnóstico pasos 2-4: SP datos cliente + búsqueda + upsert compañía HubSpot (sin cola ni contactos).</summary>
        public void DiagnosticarUpsertEmpresaHubSpot(int clienteId)
        {
            EnsureHubClient();

            _pasos.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Infraestructura,
                "bd.sp.cliente_obtener",
                "Ejecutando SP para obtener datos del cliente.",
                new { clienteId });

            var dto = _cli.ObtenerClienteParaHubSpot(clienteId);
            if (dto == null)
                throw new InvalidOperationException("SP no devolvió datos para clienteId " + clienteId + ".");

            var clientePk = dto.ClienteId > 0 ? dto.ClienteId : clienteId;

            string cuitCuilUnica;
            string cuitErr;
            if (!HubSpotCuitCuilHelper.TryGetClaveUnica(dto.Cliente != null ? dto.Cliente.NumeroDocumento : null, out cuitCuilUnica, out cuitErr))
                throw new InvalidOperationException(cuitErr);

            _pasos.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Mapeo,
                "integraciones.cliente.payload_parseado",
                "Datos SP cliente cargados.",
                new
                {
                    clientePk,
                    codigoCliente = dto.CodigoCliente,
                    cuitCuilUnica,
                    muestraDatosTruncada = DiagnosticsTextHelper.TruncateForTrace(
                        Newtonsoft.Json.JsonConvert.SerializeObject(dto)),
                });

            var hubCompanyExistingId = RunSync(() => _hub.SearchCompanyIdByCuitCuilUnicaAsync(cuitCuilUnica));
            _pasos.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.DestinoExterno,
                "destinoexterno.hubspot.company.search_by_cuitcuil_unica",
                hubCompanyExistingId == null
                    ? "Sin compañía existente en HubSpot — se creará."
                    : "Compañía existente en HubSpot — se actualizará.",
                new { cuitCuilUnica, hubSpotCompanyId = hubCompanyExistingId ?? "(crear nueva)" });

            var companyProps = BuildCompanyProperties(dto.Cliente, clientePk, dto.CodigoCliente, cuitCuilUnica);
            var resp = RunSync(() => _hub.UpsertCompanyAsync(hubCompanyExistingId, companyProps));

            var joCompany = JObject.Parse(resp);
            var hubCompanyId = joCompany["id"]?.ToString() ?? hubCompanyExistingId;
            if (string.IsNullOrEmpty(hubCompanyId))
                throw new InvalidOperationException("HubSpot no devolvió id de compañía.");

            PersistirIdHubSpotEnErp(clientePk, hubCompanyId, null);

            _pasos.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.DestinoExterno,
                "destinoexterno.hubspot.company_upsert",
                hubCompanyExistingId == null
                    ? "Compañía CREADA en HubSpot (POST)."
                    : "Compañía ACTUALIZADA en HubSpot (PATCH).",
                new { hubCompanyId, fueCreacion = hubCompanyExistingId == null });
        }

        private void PersistirIdHubSpotEnErp(int clientePk, string hubCompanyId, long? procesoId)
        {
            _cli.GuardarIdHubSpotCliente(clientePk, hubCompanyId);
            _pasos.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Infraestructura,
                "bd.sp.cliente_guardar_id_hubspot",
                "ID HubSpot persistido en atributo variable CLIENTES.id_hubspot.",
                new { procesoId, clienteId = clientePk, hubSpotCompanyId = hubCompanyId });
        }

        /// <summary>Diagnóstico paso 5: busca un contacto en HubSpot por email.</summary>
        public void DiagnosticarBuscarContactoHubSpot(string email)
        {
            EnsureHubClient();
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Se requiere un email para buscar el contacto.");

            var hubId = RunSync(() => _hub.SearchContactIdByEmailAsync(email.Trim()));
            _pasos.RegistrarPaso(
                hubId != null ? ProcesoPasoSeverity.Information : ProcesoPasoSeverity.Warning,
                ProcesoPasoCategoria.DestinoExterno,
                "destinoexterno.hubspot.contact.search_by_email",
                hubId != null
                    ? "Contacto encontrado en HubSpot."
                    : "Contacto NO encontrado en HubSpot para ese email.",
                new { email, hubSpotContactId = hubId ?? "(no encontrado)" });
        }

        /// <summary>Diagnóstico pasos 5-6 completo: SP contactos del cliente + upsert en HubSpot + asociación si fueron creados.</summary>
        public void DiagnosticarSincronizarContactosCliente(int clienteId, string hubCompanyId)
        {
            EnsureHubClient();
            if (string.IsNullOrWhiteSpace(hubCompanyId))
                throw new ArgumentException("Se requiere el HubSpot Company ID para asociar contactos nuevos.");

            _pasos.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Infraestructura,
                "bd.sp.contactos_obtener",
                "Ejecutando SP para obtener contactos del cliente.",
                new { clienteId });

            var contactos = _cli.ObtenerContactosClienteParaHubSpot(clienteId) ?? new List<ContactoDto>();
            _pasos.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Infraestructura,
                "bd.sp.contactos_obtenidos",
                "Contactos obtenidos del SP MSGestion.",
                new { clienteId, total = contactos.Count });

            var idxContacto = 0;
            foreach (var c in contactos)
            {
                idxContacto++;
                if (c == null)
                    continue;

                var email = c.CorreoElectronico;
                if (string.IsNullOrWhiteSpace(email))
                {
                    _pasos.RegistrarPaso(
                        ProcesoPasoSeverity.Warning,
                        ProcesoPasoCategoria.Mapeo,
                        "contacto.sin_email",
                        "Contacto omitido: sin CorreoElectronico.",
                        new { indice = idxContacto });
                    continue;
                }

                var hubContactExisting = RunSync(() => _hub.SearchContactIdByEmailAsync(email));
                _pasos.RegistrarPaso(
                    ProcesoPasoSeverity.Information,
                    ProcesoPasoCategoria.DestinoExterno,
                    "destinoexterno.hubspot.contact.search_by_email",
                    hubContactExisting == null ? "Contacto nuevo por email." : "Contacto existente en HubSpot.",
                    new
                    {
                        indice = idxContacto,
                        email,
                        hubSpotContactId = hubContactExisting ?? "(crear nuevo)",
                    });

                var contactProps = BuildContactProperties(c);
                var upsertContact = RunSync(() => _hub.UpsertContactAsync(hubContactExisting, contactProps));
                var created = upsertContact.Item1;
                var contactIdReturned = upsertContact.Item2;

                _pasos.RegistrarPaso(
                    ProcesoPasoSeverity.Information,
                    ProcesoPasoCategoria.DestinoExterno,
                    "destinoexterno.hubspot.contact_upsert",
                    created ? "Contacto CREADO (POST)." : "Contacto ACTUALIZADO (PATCH).",
                    new { indice = idxContacto, contactIdReturned, fueCreacion = created });

                if (string.IsNullOrEmpty(contactIdReturned))
                    throw new InvalidOperationException("HubSpot no devolvió id de contacto para " + email);

                if (created)
                {
                    RunSync(() => _hub.AssociateContactToCompanyAsync(contactIdReturned, hubCompanyId));
                    _pasos.RegistrarPaso(
                        ProcesoPasoSeverity.Information,
                        ProcesoPasoCategoria.DestinoExterno,
                        "destinoexterno.hubspot.contact_associate_company",
                        "Contacto nuevo asociado a compañía.",
                        new { indice = idxContacto, contactIdReturned, hubCompanyId });
                }
            }
        }

        // ── helpers ─────────────────────────────────────────────────────────────────

        private static void RunSync(Func<Task> asyncWork)
        {
            asyncWork().GetAwaiter().GetResult();
        }

        private static T RunSync<T>(Func<Task<T>> asyncWork)
        {
            return asyncWork().GetAwaiter().GetResult();
        }
    }
}
