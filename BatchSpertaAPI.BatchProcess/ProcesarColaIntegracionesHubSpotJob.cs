using System;
using System.Xml;
using BatchSpertaAPI.Business;
using BatchSpertaAPI.Business.Common;
using BatchSpertaAPI.Business.Diagnostics;
using BatchSpertaAPI.Business.Integration;
using BatchSpertaAPI.Business.Managers;
using BatchSpertaAPI.Entities;
using InterfazHubSpot;
using Mastersoft.Framework.Standard;
using Mastersoft.Scheduler452.Intefaces;

namespace BatchSpertaAPI.BatchProcess
{
    /// <summary>Despacha filas <c>dbo.ProcesosSpertaAPI</c> con destino HubSpot (flujo 2A).</summary>
    public sealed class ProcesarColaIntegracionesHubSpotJob : IScheduler
    {
        public MSContext Contexto { get; set; }

        /// <summary>Acumula pasos MVC/diagnóstico; opcional.</summary>
        public IProcesoPasoReporter PasoReporter { get; set; }

        /// <summary>Si es true enciende traza HTTP/oauth vía <see cref="TracingSpertaApiClient"/> y listener OAuth.</summary>
        public bool EmitirTrazaHttpSpertaApi { get; set; }

        public bool Finished { get; set; }

        /// <remarks>Misma corrida que <see cref="Execute"/> pero sin capturar excepción (uso desde MVC).</remarks>
        public ProcesoColaEjecucionResumen EjecutarColaHubSpotDiagnostic()
        {
            return EjecutarColaInterno();
        }

        public void Execute(XmlElement oParam, XmlElement oReturn)
        {
            try
            {
                EjecutarColaInterno();
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(ProcesarColaIntegracionesHubSpotJob) + ": " + ex);

                try
                {
                    var errores = new ErroresManager(Contexto);
                    errores.Grabar(new Errores
                    {
                        TenantId = Contexto != null ? Contexto.TenantId : null,
                        ErrorDateTime = DateTime.UtcNow,
                        MachineName = Environment.MachineName,
                        AppDomainName = AppDomain.CurrentDomain.FriendlyName,
                        Message = ex.Message,
                        FullException = ex.ToString(),
                    });
                }
                catch (Exception ex2)
                {
                    Logger.Log("No se pudo grabar en Errores: " + ex2.Message);
                }

                try
                {
                    using (var emails = new EmailsManager(Contexto))
                    {
                        emails.GrabarEmailErroresProcesamiento(
                            nameof(ProcesarColaIntegracionesHubSpotJob),
                            new[] { ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty });
                    }
                }
                catch (Exception ex2)
                {
                    Logger.Log("No se pudo encolar email: " + ex2.Message);
                }
            }
        }

        /// <remarks>No captura excepciones; el caller MVC puede registrar paso fatal.</remarks>
        private ProcesoColaEjecucionResumen EjecutarColaInterno()
        {
            var rep = PasoReporter ?? NullProcesoPasoReporter.Instance;
            var instrumentarHttp = EmitirTrazaHttpSpertaApi;
            try
            {
                if (instrumentarHttp)
                {
                    HttpSpertaApiClient.SetOAuthDiagnosticsListener((ok, msg) =>
                        rep.RegistrarPaso(
                            ok ? ProcesoPasoSeverity.Information : ProcesoPasoSeverity.Warning,
                            ProcesoPasoCategoria.SpertaApi,
                            "spertaapi.oauth.token",
                            ok ? "OAuth completado contra SpertaAPI." : "Fallo en OAuth contra SpertaAPI.",
                            new { ok, mensaje = msg }));
                }

                var runner = new HubSpotIntegracionRunner(Contexto, null, rep, instrumentarHttp);
                ProcesoColaEjecucionResumen r;
                runner.ProcesarColaHubSpot(25, out r);
                return r;
            }
            finally
            {
                if (instrumentarHttp)
                {
                    HttpSpertaApiClient.ClearOAuthDiagnosticsListener();
                }
            }
        }
    }
}
