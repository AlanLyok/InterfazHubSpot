using System;
using System.Xml;
using InterfazHubSpot.Business;
using InterfazHubSpot.Business.Common;
using InterfazHubSpot.Business.Diagnostics;
using InterfazHubSpot.Business.Integration;
using InterfazHubSpot.Business.Managers;
using InterfazHubSpot.Entities;
using InterfazHubSpot.Business.HubSpot;
using Mastersoft.Framework.Standard;
using Mastersoft.Scheduler452.Intefaces;

namespace InterfazHubSpot.BatchProcess
{
    /// <summary>Despacha filas <c>dbo.ProcesosSpertaHubSpot</c> con destino HubSpot (flujo 2A).</summary>
    public sealed class ProcesarColaIntegracionesHubSpotJob : IScheduler
    {
        public MSContext Contexto { get; set; }

        /// <summary>Acumula pasos MVC/diagnóstico; opcional.</summary>
        public IProcesoPasoReporter PasoReporter { get; set; }

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
                    var notifier = IntegracionErrorNotifier.Create(Contexto);
                    notifier.NotificarErrorFatalJob(nameof(ProcesarColaIntegracionesHubSpotJob), ex);
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
            var runner = new HubSpotIntegracionRunner(Contexto, rep);
            ProcesoColaEjecucionResumen r;
            runner.ProcesarColaHubSpot(25, out r);
            return r;
        }
    }
}
