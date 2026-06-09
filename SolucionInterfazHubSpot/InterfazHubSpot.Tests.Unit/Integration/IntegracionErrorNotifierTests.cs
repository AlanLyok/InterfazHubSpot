using System.Collections.Generic;
using InterfazHubSpot.Business.Integration;
using InterfazHubSpot.Interfaces.Managers;
using Xunit;

namespace InterfazHubSpot.Tests.Unit.Integration
{
    public sealed class IntegracionErrorNotifierTests
    {
        [Fact]
        public void NotificarErrorFila2A_UsaAsuntoContextualizado()
        {
            var fake = new FakeEmailsManager();
            var notifier = new IntegracionErrorNotifier(fake);

            notifier.NotificarErrorFila2A(10, 77, "SyncClienteCola", new System.Exception("fallo hub"));

            Assert.Single(fake.Calls);
            Assert.Contains("[HubSpot 2A]", fake.Calls[0].Asunto);
            Assert.Contains("ProcesoId=10", fake.Calls[0].Asunto);
            Assert.Contains("ClienteId=77", fake.Calls[0].Asunto);
            Assert.Contains("Fase=SyncClienteCola", fake.Calls[0].Asunto);
        }

        [Fact]
        public void NotificarErrorAuth_UsaAsuntoAutenticacion()
        {
            var fake = new FakeEmailsManager();
            var notifier = new IntegracionErrorNotifier(fake);

            notifier.NotificarErrorAuth(new System.Exception("401"));

            Assert.Single(fake.Calls);
            Assert.Equal("[HubSpot] Error autenticación", fake.Calls[0].Asunto);
        }

        [Fact]
        public void NotificarErrorBatch2B_UsaAsuntoConLote()
        {
            var fake = new FakeEmailsManager();
            var notifier = new IntegracionErrorNotifier(fake);

            notifier.NotificarErrorBatch2B(3, new System.Exception("429 agotado"));

            Assert.Single(fake.Calls);
            Assert.Equal("[HubSpot 2B] Error batch lote 3", fake.Calls[0].Asunto);
            Assert.Equal("HubSpot integración 2B", fake.Calls[0].Proceso);
        }

        [Fact]
        public void NotificarErrorFatalJob_UsaAsuntoConNombreJob()
        {
            var fake = new FakeEmailsManager();
            var notifier = new IntegracionErrorNotifier(fake);

            notifier.NotificarErrorFatalJob("ProcesarColaIntegracionesHubSpotJob", new System.Exception("crash"));

            Assert.Single(fake.Calls);
            Assert.Equal("[HubSpot] Error fatal — ProcesarColaIntegracionesHubSpotJob", fake.Calls[0].Asunto);
        }

        private sealed class FakeEmailsManager : IEmailsManager
        {
            public List<EmailCallRecord> Calls { get; } = new List<EmailCallRecord>();

            public void GrabarEmailErroresProcesamiento(string entidad, IEnumerable<string> errores = null)
            {
                GrabarEmailErrores(entidad + " - Errores en procesamiento", entidad, errores);
            }

            public void GrabarEmailErrores(string asunto, string proceso, IEnumerable<string> errores = null)
            {
                Calls.Add(new EmailCallRecord { Asunto = asunto, Proceso = proceso, Errores = errores });
            }
        }

        private sealed class EmailCallRecord
        {
            public string Asunto { get; set; }

            public string Proceso { get; set; }

            public IEnumerable<string> Errores { get; set; }
        }
    }
}
