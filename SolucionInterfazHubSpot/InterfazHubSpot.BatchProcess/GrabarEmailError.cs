using System.Xml;
using InterfazHubSpot.Business.Managers;
using Mastersoft.Framework.Standard;
using Mastersoft.Scheduler452.Intefaces;

namespace InterfazHubSpot.BatchProcess
{
    /// <summary>Encola un correo de prueba usando <see cref="EmailsManager"/> y claves <c>EmailErrDE</c> / <c>EmailErrPara</c> en configuración.</summary>
    public sealed class GrabarEmailError : IScheduler
    {
        public MSContext Contexto { get; set; }
        public bool Finished { get; set; }

        public void Execute(XmlElement oParam, XmlElement oReturn)
        {
            using (var emailsManager = new EmailsManager(Contexto))
            {
                emailsManager.GrabarEmailErroresProcesamiento(
                    nameof(GrabarEmailError),
                    new[] { "Mensaje de prueba del proceso GrabarEmailError (batch neutro)." });
            }
        }
    }
}
