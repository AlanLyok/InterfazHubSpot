using BatchSpertaAPI.Business.Diagnostics;
using Xunit;

namespace BatchSpertaAPI.Tests.Unit
{
    public sealed class ProcesoPasoReporterTests
    {
        [Fact]
        public void NullProcesoPasoReporter_acepta_registros_sin_efectos()
        {
            var rep = NullProcesoPasoReporter.Instance;
            rep.RegistrarPaso(ProcesoPasoSeverity.Information, ProcesoPasoCategoria.Infraestructura, null, null, null);
        }

        [Fact]
        public void ProcesoPasoCollector_acumula_campos_basico()
        {
            var collector = new ProcesoPasoCollector();
            collector.RegistrarPaso(
                ProcesoPasoSeverity.Warning,
                ProcesoPasoCategoria.Infraestructura,
                "test.code",
                "mensaje demo",
                new { foo = 1 });

            var list = collector.ObtenerPasos();
            Assert.Single(list);
            Assert.Equal("test.code", list[0].Codigo);
            Assert.Equal(ProcesoPasoSeverity.Warning, list[0].Severidad);
        }

        [Fact]
        public void ProcesoPasoCollector_codigo_null_se_normaliza_y_lista_es_copia()
        {
            var collector = new ProcesoPasoCollector();
            collector.RegistrarPaso(ProcesoPasoSeverity.Information, ProcesoPasoCategoria.Infraestructura, null, null, null);
            var lista1 = collector.ObtenerPasos();
            lista1.Clear();
            var lista2 = collector.ObtenerPasos();
            Assert.Single(lista2);
            Assert.Equal(string.Empty, lista2[0].Codigo ?? string.Empty);
        }
    }
}
