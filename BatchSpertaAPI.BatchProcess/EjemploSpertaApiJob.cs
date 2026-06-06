using System;
using System.Xml;
using BatchSpertaAPI.Business;
using BatchSpertaAPI.Business.Common;
using BatchSpertaAPI.Business.Integration;
using BatchSpertaAPI.Business.Managers;
using BatchSpertaAPI.Entities;
using Mastersoft.Framework.Standard;
using Mastersoft.Scheduler452.Intefaces;

namespace BatchSpertaAPI.BatchProcess
{
    /// <summary>Proceso plantilla: llama a <c>GET api/v100/health</c> y registra el resultado (extender con llamadas reales a la API).</summary>
    public sealed class EjemploSpertaApiJob : IScheduler
    {
        public MSContext Contexto { get; set; }
        public bool Finished { get; set; }

        public void Execute(XmlElement oParam, XmlElement oReturn)
        {
            try
            {
                var client = new HttpSpertaApiClient();
                var json = client.GetHealthAsync().GetAwaiter().GetResult();
                Logger.Log("SpertaAPI health: " + json);
            }
            catch (Exception ex)
            {
                Logger.Log("EjemploSpertaApiJob: " + ex);

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
                        FullException = ex.ToString()
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
                            nameof(EjemploSpertaApiJob),
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
