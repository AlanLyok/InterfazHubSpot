using System;
using InterfazHubSpot.BatchProcess;
using InterfazHubSpot.Business.Common;
using InterfazHubSpot.Business.Diagnostics;
using InterfazHubSpot.Business.HubSpot;
using System.Configuration;
using System.Web.Mvc;
using InterfazHubSpot.Business.Integration;
using InterfazHubSpot.Business.Managers;
using InterfazHubSpot.Core;
using InterfazHubSpot.Entities;
using Mastersoft.Framework.Standard;

namespace InterfazHubSpot.Controllers
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
                var mgr = new ProcesosSpertaHubSpotManager(ctx);
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

        /// <summary>SP integraciones cliente con traza; <paramref name="clienteId"/> opcional (query) o primera fila pendiente HubSpot.</summary>
        [HttpPost]
        public JsonResult ProcesarColaHubSpotTrazaCliente(int? clienteId = null)
        {
            var correlacionId = Guid.NewGuid();
            var collector = new ProcesoPasoCollector();
            var ctx = GetContext();

            collector.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Infraestructura,
                "batchmvc.ejecucion_inicio",
                "Inicio traza: SP integraciones cliente (sin HubSpot).",
                new
                {
                    correlacionId,
                    empresaId = ctx.EmpresaId,
                    ctx.TenantId,
                    cnPrefix = ctx.CNPrefix,
                    clienteIdParametro = clienteId,
                });

            var ok = true;
            string errorFatal = null;

            try
            {
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
                    var mgr = new ProcesosSpertaHubSpotManager(ctx);
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
                    if (!IntegracionColaIdentificador.TryGetClienteId(new ProcesosSpertaHubSpot
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
                    "ClienteId para SP integraciones.",
                    new { clienteId = resolvedClienteId, origenResolucion });

                var cliMgr = new ClienteIntegracionManager(ctx);
                var dto = cliMgr.ObtenerClienteParaHubSpot(resolvedClienteId);

                collector.RegistrarPaso(
                    ProcesoPasoSeverity.Information,
                    ProcesoPasoCategoria.Infraestructura,
                    "bd.sp.cliente_obtener",
                    dto != null ? "SP ejecutado. Datos de cliente cargados." : "SP ejecutado pero sin datos para el cliente.",
                    new
                    {
                        clienteId = resolvedClienteId,
                        encontrado = dto != null,
                        codigoCliente = dto != null ? dto.CodigoCliente : null,
                    });
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

            return CrearJsonTrazaSalida(correlacionId, ok, errorFatal, null, collector);
        }

        /// <summary>Diagnóstico paso 3: busca la empresa en HubSpot por cuitcuil_unica (NroDocumento del SP) para el clienteId dado.</summary>
        [HttpPost]
        public JsonResult TrazaHubSpotBuscarEmpresa(int? clienteId = null)
        {
            var correlacionId = Guid.NewGuid();
            var collector = new ProcesoPasoCollector();
            var ctx = GetContext();

            collector.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Infraestructura,
                "batchmvc.ejecucion_inicio",
                "Inicio traza: buscar empresa en HubSpot por cuitcuil_unica.",
                new { correlacionId, clienteId });

            var ok = true;
            string errorFatal = null;

            try
            {
                if (!clienteId.HasValue || clienteId.Value <= 0)
                    throw new ArgumentException("Indicar clienteId en la URL (ej. ?clienteId=12345).");

                var runner = new HubSpotIntegracionRunner(ctx, collector);
                runner.DiagnosticarBuscarEmpresaHubSpot(clienteId.Value);
            }
            catch (Exception ex)
            {
                ok = false;
                errorFatal = ex.Message;
                RegistrarErrorFatal(collector, ex);
                Logger.Log("[Traza MVC " + correlacionId + "] TrazaHubSpotBuscarEmpresa: " + ex);
            }

            return CrearJsonTrazaSalida(correlacionId, ok, errorFatal, null, collector);
        }

        /// <summary>Diagnóstico pasos 2-4: SP datos cliente + búsqueda + upsert empresa en HubSpot.</summary>
        [HttpPost]
        public JsonResult TrazaHubSpotUpsertEmpresa(int? clienteId = null)
        {
            var correlacionId = Guid.NewGuid();
            var collector = new ProcesoPasoCollector();
            var ctx = GetContext();

            collector.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Infraestructura,
                "batchmvc.ejecucion_inicio",
                "Inicio traza: SP datos cliente + crear/actualizar empresa HubSpot.",
                new { correlacionId, clienteId });

            var ok = true;
            string errorFatal = null;

            try
            {
                if (!clienteId.HasValue || clienteId.Value <= 0)
                    throw new ArgumentException("Indicar clienteId en la URL (ej. ?clienteId=12345).");

                var runner = new HubSpotIntegracionRunner(ctx, collector);
                runner.DiagnosticarUpsertEmpresaHubSpot(clienteId.Value);
            }
            catch (Exception ex)
            {
                ok = false;
                errorFatal = ex.Message;
                RegistrarErrorFatal(collector, ex);
                Logger.Log("[Traza MVC " + correlacionId + "] TrazaHubSpotUpsertEmpresa: " + ex);
            }

            return CrearJsonTrazaSalida(correlacionId, ok, errorFatal, null, collector);
        }

        /// <summary>Diagnóstico paso 5: busca un contacto en HubSpot por email.</summary>
        [HttpPost]
        public JsonResult TrazaHubSpotBuscarContacto(string email = null)
        {
            var correlacionId = Guid.NewGuid();
            var collector = new ProcesoPasoCollector();
            var ctx = GetContext();

            collector.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Infraestructura,
                "batchmvc.ejecucion_inicio",
                "Inicio traza: buscar contacto en HubSpot por email.",
                new { correlacionId, email });

            var ok = true;
            string errorFatal = null;

            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Indicar email en la URL (ej. ?email=contacto@empresa.com).");

                var runner = new HubSpotIntegracionRunner(ctx, collector);
                runner.DiagnosticarBuscarContactoHubSpot(email);
            }
            catch (Exception ex)
            {
                ok = false;
                errorFatal = ex.Message;
                RegistrarErrorFatal(collector, ex);
                Logger.Log("[Traza MVC " + correlacionId + "] TrazaHubSpotBuscarContacto: " + ex);
            }

            return CrearJsonTrazaSalida(correlacionId, ok, errorFatal, null, collector);
        }

        /// <summary>Diagnóstico pasos 5-6: SP contactos del cliente + upsert en HubSpot + asociación si fueron creados.</summary>
        [HttpPost]
        public JsonResult TrazaHubSpotSincronizarContactos(int? clienteId = null, string hubCompanyId = null)
        {
            var correlacionId = Guid.NewGuid();
            var collector = new ProcesoPasoCollector();
            var ctx = GetContext();

            collector.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Infraestructura,
                "batchmvc.ejecucion_inicio",
                "Inicio traza: SP contactos + sincronizar en HubSpot.",
                new { correlacionId, clienteId, hubCompanyId });

            var ok = true;
            string errorFatal = null;

            try
            {
                if (!clienteId.HasValue || clienteId.Value <= 0)
                    throw new ArgumentException("Indicar clienteId en la URL (ej. ?clienteId=12345).");
                if (string.IsNullOrWhiteSpace(hubCompanyId))
                    throw new ArgumentException("Indicar hubCompanyId en la URL (ej. &hubCompanyId=12345678).");

                var runner = new HubSpotIntegracionRunner(ctx, collector);
                runner.DiagnosticarSincronizarContactosCliente(clienteId.Value, hubCompanyId);
            }
            catch (Exception ex)
            {
                ok = false;
                errorFatal = ex.Message;
                RegistrarErrorFatal(collector, ex);
                Logger.Log("[Traza MVC " + correlacionId + "] TrazaHubSpotSincronizarContactos: " + ex);
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
            var context = Util.GetMSContext();

            var prefix = ConfigurationManager.AppSettings["FrameworkCNPrefix"];
            if (!string.IsNullOrWhiteSpace(prefix))
                context.CNPrefix = prefix;

            int empresaIdValue;
            if (int.TryParse(ConfigurationManager.AppSettings["EmpresaId"], out empresaIdValue))
                context.EmpresaId = empresaIdValue;

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
