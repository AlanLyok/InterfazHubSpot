using InterfazHubSpot.Business.Integration;
using Xunit;

namespace InterfazHubSpot.Tests.Unit
{
    public sealed class IntegracionColaEstadosTests
    {
        [Fact]
        public void Estados_y_destinos_coinciden_con_contrato_cola_neutra()
        {
            Assert.Equal((byte)0, IntegracionColaEstados.Pendiente);
            Assert.Equal((byte)1, IntegracionColaEstados.EnProceso);
            Assert.Equal((byte)2, IntegracionColaEstados.Ok);
            Assert.Equal((byte)3, IntegracionColaEstados.Error);
            Assert.Equal("HubSpot", IntegracionDestinos.HubSpot);
            Assert.Equal("Alta", IntegracionTipoOperacion.Alta);
            Assert.Equal("Modificacion", IntegracionTipoOperacion.Modificacion);
            Assert.Equal("Cliente", IntegracionTipoEntidad.Cliente);
        }
    }
}
