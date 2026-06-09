using System;
using InterfazHubSpot.Business.Integration;
using InterfazHubSpot.Interfaces.Managers;
using Xunit;

namespace InterfazHubSpot.Tests.Unit.Security
{
    public sealed class IntegracionErrorNotifierSecurityTests
    {
        [Fact, Trait("Category", "Security")]
        public void Constructor_EmailsNull_LanzaArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new IntegracionErrorNotifier(null));
        }

        [Fact, Trait("Category", "Security")]
        public void NotificarErrorAuth_ConInnerException_EncolaDetalle()
        {
            var fake = new FakeEmailsManager();
            var notifier = new IntegracionErrorNotifier(fake);
            var ex = new Exception("401", new Exception("token revocado"));

            notifier.NotificarErrorAuth(ex);

            Assert.Single(fake.Calls);
            Assert.Contains("Inner: token revocado", string.Join("|", fake.Calls[0].Errores));
        }

        [Fact, Trait("Category", "Security")]
        public void NotificarErrorFatalJob_NombreVacio_UsaDesconocido()
        {
            var fake = new FakeEmailsManager();
            var notifier = new IntegracionErrorNotifier(fake);

            notifier.NotificarErrorFatalJob("   ", new Exception("fail"));

            Assert.Equal("[HubSpot] Error fatal — desconocido", fake.Calls[0].Asunto);
        }

        private sealed class FakeEmailsManager : IEmailsManager
        {
            public System.Collections.Generic.List<EmailCallRecord> Calls { get; } =
                new System.Collections.Generic.List<EmailCallRecord>();

            public void GrabarEmailErroresProcesamiento(string entidad, System.Collections.Generic.IEnumerable<string> errores = null)
            {
                GrabarEmailErrores(entidad, entidad, errores);
            }

            public void GrabarEmailErrores(string asunto, string proceso, System.Collections.Generic.IEnumerable<string> errores = null)
            {
                Calls.Add(new EmailCallRecord { Asunto = asunto, Proceso = proceso, Errores = errores });
            }
        }

        private sealed class EmailCallRecord
        {
            public string Asunto { get; set; }

            public string Proceso { get; set; }

            public System.Collections.Generic.IEnumerable<string> Errores { get; set; }
        }
    }
}
