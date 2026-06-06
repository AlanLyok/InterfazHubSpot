using InterfazHubSpot.Business.Diagnostics;
using Xunit;

namespace InterfazHubSpot.Tests.Unit.Diagnostics
{
    public sealed class NullProcesoPasoReporterTests
    {
        [Fact]
        public void Instance_NoEsNull()
        {
            Assert.NotNull(NullProcesoPasoReporter.Instance);
        }

        [Fact]
        public void Instance_EsSingleton_DosAccesosDevuelvenMismaReferencia()
        {
            var a = NullProcesoPasoReporter.Instance;
            var b = NullProcesoPasoReporter.Instance;
            Assert.Same(a, b);
        }

        [Fact]
        public void RegistrarPaso_ConValoresNormales_NoLanza()
        {
            var reporter = NullProcesoPasoReporter.Instance;

            reporter.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.Cola,
                "test.codigo",
                "Mensaje de prueba",
                new { clave = "valor" });
        }

        [Fact]
        public void RegistrarPaso_ConMensajeNull_NoLanza()
        {
            var reporter = NullProcesoPasoReporter.Instance;

            reporter.RegistrarPaso(
                ProcesoPasoSeverity.Warning,
                ProcesoPasoCategoria.Infraestructura,
                null,
                null,
                null);
        }

        [Fact]
        public void RegistrarPaso_ConDatosNull_NoLanza()
        {
            var reporter = NullProcesoPasoReporter.Instance;

            reporter.RegistrarPaso(
                ProcesoPasoSeverity.Error,
                ProcesoPasoCategoria.DestinoExterno,
                "codigo",
                "mensaje",
                null);
        }
    }
}
