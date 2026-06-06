using System;
using BatchSpertaAPI.BatchProcess;
using BatchSpertaAPI.Business.Common;
using BatchSpertaAPI.Business.Diagnostics;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Mvc;
using BatchSpertaAPI.Business.Integration;
using BatchSpertaAPI.Business.Managers;
using BatchSpertaAPI.Entities;
using Mastersoft.Framework.Standard;

namespace BatchSpertaAPI.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Error()
        {
            return View();
        }

        [HttpPost]
        public JsonResult EjemploSpertaApi()
        {
            var proceso = new EjemploSpertaApiJob();
            proceso.Contexto = GetContext();
            proceso.Execute(null, null);
            return Json(true);
        }

        [HttpPost]
        public JsonResult GrabarEmailError()
        {
            var proceso = new GrabarEmailError();
            proceso.Contexto = GetContext();
            proceso.Execute(null, null);
            return Json(true);
        }

        [HttpPost]
        public JsonResult ProcesarColaHubSpot()
        {
            var proceso = new ProcesarColaIntegracionesHubSpotJob();
            proceso.Contexto = GetContext();
            proceso.Execute(null, null);
            return Json(true);
        }

        /// <summary>Corrida asíncrona en cliente + JSON de pasos (evita secretos OAuth/HubSpot en traza).</summary>
        [HttpPost]
        public JsonResult ProcesarColaHubSpotTraza()
        {
            var correlacionId = Guid.NewGuid();
            var collector = new ProcesoPasoCollector();
            var ctx = GetContext();

            collector.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Infraestructura,
                "batchmvc.ejecucion_inicio",
                "Inicio ejecución con traza (cola HubSpot).",
                new
                {
                    correlacionId,
                    empresaId = ctx.EmpresaId,
                    ctx.TenantId,
                    cnPrefix = ctx.CNPrefix,
                    spertaApiBaseUrl = ConfigurationManager.AppSettings["SpertaAPIBaseUrl"],
                });

            ProcesoColaEjecucionResumen resumen = null;
            var ok = true;
            string errorFatal = null;

            try
            {
                var job = new ProcesarColaIntegracionesHubSpotJob
                {
                    Contexto = ctx,
                    PasoReporter = collector,
                    EmitirTrazaHttpSpertaApi = true,
                };

                resumen = job.EjecutarColaHubSpotDiagnostic();
            }
            catch (Exception ex)
            {
                ok = false;
                errorFatal = ex.Message;
                RegistrarErrorFatal(collector, ex);

                Logger.Log(
                    "[Traza MVC " + correlacionId + "] " + nameof(HomeController.ProcesarColaHubSpotTraza) + ": " + ex);
            }

            return CrearJsonTrazaSalida(correlacionId, ok, errorFatal, resumen, collector);
        }

        /// <summary>Vista de depuración: MSGestion + conteos + muestra de pendientes (sin reclamar ni HubSpot).</summary>
        [HttpPost]
        public JsonResult ProcesarColaHubSpotTrazaCola()
        {
            var correlacionId = Guid.NewGuid();
            var collector = new ProcesoPasoCollector();
            var ctx = GetContext();

            collector.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Infraestructura,
                "batchmvc.ejecucion_inicio",
                "Inicio traza: consulta cola HubSpot (solo lectura, sin reclamar).",
                new
                {
                    correlacionId,
                    empresaId = ctx.EmpresaId,
                    ctx.TenantId,
                    cnPrefix = ctx.CNPrefix,
                    spertaApiBaseUrl = ConfigurationManager.AppSettings["SpertaAPIBaseUrl"],
                });

            var ok = true;
            string errorFatal = null;

            try
            {
                bool bdOk;
                var bdDatos = ErpConnectivityProbe.ProbarMsgestion(ctx, out bdOk);
                collector.RegistrarPaso(
                    bdOk ? ProcesoPasoSeverity.Information : ProcesoPasoSeverity.Warning,
                    ProcesoPasoCategoria.Infraestructura,
                    "infra.bd.msgestion",
                    bdOk ? "Conexión MSGestion y SELECT 1 OK." : "No se pudo validar MSGestion.",
                    bdDatos);

                var destino = IntegracionDestinos.HubSpot;
                var mgr = new ProcesosSpertaApiManager(ctx);
                var pendientes = mgr.ContarPendientes(destino);
                var enProceso = mgr.ContarEnProceso(destino);
                collector.RegistrarPaso(
                    ProcesoPasoSeverity.Information,
                    ProcesoPasoCategoria.Cola,
                    "cola." + destino + ".estado_previo",
                    "Filas Pendiente / EnProceso (vista previa sin reclamo).",
                    new { pendientes, en_proceso = enProceso });

                const int maxItems = 40;
                var muestra = mgr.ListarMuestraPendientes(destino, maxItems);
                collector.RegistrarPaso(
                    ProcesoPasoSeverity.Information,
                    ProcesoPasoCategoria.Cola,
                    "cola." + destino + ".muestra_pendientes_debug",
                    "Muestra de filas Pendiente (solo lectura, hasta " + maxItems + ").",
                    new
                    {
                        maxItems,
                        devueltas = muestra.Count,
                        filas = muestra,
                    });
            }
            catch (Exception ex)
            {
                ok = false;
                errorFatal = ex.Message;
                RegistrarErrorFatal(collector, ex);
                Logger.Log(
                    "[Traza MVC " + correlacionId + "] " + nameof(HomeController.ProcesarColaHubSpotTrazaCola) + ": " +
                    ex);
            }

            return CrearJsonTrazaSalida(correlacionId, ok, errorFatal, null, collector);
        }

        /// <summary>GET integraciones cliente con traza; <paramref name="clienteId"/> opcional (query) o primera fila pendiente HubSpot.</summary>
        [HttpPost]
        public async Task<JsonResult> ProcesarColaHubSpotTrazaCliente(int? clienteId = null)
        {
            var correlacionId = Guid.NewGuid();
            var collector = new ProcesoPasoCollector();
            var ctx = GetContext();

            collector.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Infraestructura,
                "batchmvc.ejecucion_inicio",
                "Inicio traza: GET SpertaAPI integraciones cliente (sin HubSpot).",
                new
                {
                    correlacionId,
                    empresaId = ctx.EmpresaId,
                    ctx.TenantId,
                    cnPrefix = ctx.CNPrefix,
                    spertaApiBaseUrl = ConfigurationManager.AppSettings["SpertaAPIBaseUrl"],
                    clienteIdParametro = clienteId,
                });

            var ok = true;
            string errorFatal = null;
            const bool instrumentarHttp = true;

            if (instrumentarHttp)
            {
                HttpSpertaApiClient.SetOAuthDiagnosticsListener((oauthOk, msg) =>
                    collector.RegistrarPaso(
                        oauthOk ? ProcesoPasoSeverity.Information : ProcesoPasoSeverity.Warning,
                        ProcesoPasoCategoria.SpertaApi,
                        "spertaapi.oauth.token",
                        oauthOk ? "OAuth completado contra SpertaAPI." : "Fallo en OAuth contra SpertaAPI.",
                        new { ok = oauthOk, mensaje = msg }));
            }

            try
            {
                var api = new TracingSpertaApiClient(new HttpSpertaApiClient(), collector);
                int resolvedClienteId;
                string origenResolucion;

                if (clienteId.HasValue && clienteId.Value > 0)
                {
                    resolvedClienteId = clienteId.Value;
                    origenResolucion = "parametro_clienteId";
                }
                else
                {
                    var destino = IntegracionDestinos.HubSpot;
                    var mgr = new ProcesosSpertaApiManager(ctx);
                    var muestra = mgr.ListarMuestraPendientes(destino, 1);
                    if (muestra == null || muestra.Count == 0)
                    {
                        collector.RegistrarPaso(
                            ProcesoPasoSeverity.Warning,
                            ProcesoPasoCategoria.Cola,
                            "cola.hubspot.sin_pendiente_muestra",
                            "No hay filas Pendiente HubSpot para derivar ClienteId; indicar clienteId en la URL de prueba.",
                            new { destino });
                        return CrearJsonTrazaSalida(correlacionId, true, null, null, collector);
                    }

                    var fila = muestra[0];
                    string parseErr;
                    if (!IntegracionColaIdentificador.TryGetClienteId(new ProcesosSpertaApi
                    {
                        TipoEntidad = fila.TipoEntidad,
                        Identificador = fila.Identificador,
                    }, out resolvedClienteId, out parseErr))
                    {
                        throw new InvalidOperationException(parseErr ?? "No se pudo obtener ClienteId desde la cola.");
                    }

                    origenResolucion = "primera_fila_pendiente_hubspot";
                }

                collector.RegistrarPaso(
                    ProcesoPasoSeverity.Information,
                    ProcesoPasoCategoria.Cola,
                    "batchmvc.cliente_id_resuelto",
                    "ClienteId para GET integraciones.",
                    new { clienteId = resolvedClienteId, origenResolucion });

                await api.GetIntegracionesClienteAsync(resolvedClienteId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ok = false;
                errorFatal = ex.Message;
                RegistrarErrorFatal(collector, ex);
                Logger.Log(
                    "[Traza MVC " + correlacionId + "] " + nameof(HomeController.ProcesarColaHubSpotTrazaCliente) + ": " +
                    ex);
            }
            finally
            {
                if (instrumentarHttp)
                {
                    HttpSpertaApiClient.ClearOAuthDiagnosticsListener();
                }
            }

            return CrearJsonTrazaSalida(correlacionId, ok, errorFatal, null, collector);
        }

        [HttpPost]
        public JsonResult HubSpotCuentaCorrienteBatch()
        {
            var proceso = new HubSpotSincronizarCuentaCorrienteJob();
            proceso.Contexto = GetContext();
            proceso.Execute(null, null);
            return Json(true);
        }

        private static MSContext GetContext()
        {
            var context = new MSContext();
            var prefix = ConfigurationManager.AppSettings["FrameworkCNPrefix"];
            context.CNPrefix = string.IsNullOrEmpty(prefix) ? "BatchSpertaAPI" : prefix;

            int empresaIdValue;
            if (int.TryParse(ConfigurationManager.AppSettings["EmpresaId"], out empresaIdValue))
                context.EmpresaId = empresaIdValue;

            context.TenantId = ConfigurationManager.AppSettings["TenantId"];

            return context;
        }

        private static JsonResult CrearJsonTrazaSalida(
            Guid correlacionId,
            bool ok,
            string errorFatal,
            ProcesoColaEjecucionResumen resumen,
            ProcesoPasoCollector collector)
        {
            return new JsonResult
            {
                Data = new
                {
                    correlacionId,
                    ok,
                    erroresFatal = errorFatal,
                    pasos = collector.ObtenerPasos(),
                    resumen,
                },
                JsonRequestBehavior = JsonRequestBehavior.DenyGet,
                MaxJsonLength = int.MaxValue,
            };
        }

        private static void RegistrarErrorFatal(ProcesoPasoCollector collector, Exception ex)
        {
            collector.RegistrarPaso(
                ProcesoPasoSeverity.Error,
                ProcesoPasoCategoria.Infraestructura,
                "batchmvc.error_fatal",
                ex.Message ?? "Error no controlado durante la corrida.",
                new
                {
                    tipoExcepcion = ex.GetType().Name,
                    detalleTruncado =
                        DiagnosticsTextHelper.TruncateForTrace(ex.ToString()),
                });
        }
    }
}
