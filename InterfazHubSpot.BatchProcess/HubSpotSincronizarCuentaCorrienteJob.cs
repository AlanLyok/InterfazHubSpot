using System;
using System.Xml;
using BatchSpertaAPI.Business;
using BatchSpertaAPI.Business.Common;
using BatchSpertaAPI.Business.Managers;
using BatchSpertaAPI.Entities;
using InterfazHubSpot;
using Mastersoft.Framework.Standard;
using Mastersoft.Scheduler452.Intefaces;

namespace BatchSpertaAPI.BatchProcess
{
    /// <summary>Sincronización masiva de cuenta corriente text en HubSpot (flujo 2B).</summary>
    public sealed class HubSpotSincronizarCuentaCorrienteJob : IScheduler
    {
        public MSContext Contexto { get; set; }

        public bool Finished { get; set; }

        public void Execute(XmlElement oParam, XmlElement oReturn)
        {
            try
            {
                var runner = new HubSpotIntegracionRunner(Contexto);
                runner.EjecutarSincronizacionCuentaCorriente();
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(HubSpotSincronizarCuentaCorrienteJob) + ": " + ex);

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
                            nameof(HubSpotSincronizarCuentaCorrienteJob),
                            new[] { ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty });
                    }
                }
                catch (Exception ex2)
                {
                    Logger.Log("No se pudo encolar email: " + ex2.Message);
                }
            }
        }
    }
}
