using InterfazHubSpot.Business.Integration;
using Xunit;

namespace InterfazHubSpot.Tests.Unit.Integration
{
    public sealed class HubSpotCuitCuilHelperTests
    {
        [Fact]
        public void Normalizar_ConGuionesPuntosYComas_QuitaSeparadores()
        {
            Assert.Equal("30123456789", HubSpotCuitCuilHelper.Normalizar("30-12.345,678-9"));
        }

        [Fact]
        public void Normalizar_YaNormalizado_DevuelveIgual()
        {
            Assert.Equal("30123456789", HubSpotCuitCuilHelper.Normalizar("30123456789"));
        }

        [Fact]
        public void Normalizar_Vacio_DevuelveNull()
        {
            Assert.Null(HubSpotCuitCuilHelper.Normalizar(null));
            Assert.Null(HubSpotCuitCuilHelper.Normalizar("   "));
        }

        [Fact]
        public void TryGetClaveUnica_Cuit_DevuelveSoloDigitos()
        {
            string clave;
            string err;
            var ok = HubSpotCuitCuilHelper.TryGetClaveUnica("30-54798102-9", out clave, out err);

            Assert.True(ok);
            Assert.Equal("30547981029", clave);
            Assert.Null(err);
        }

        [Fact]
        public void TryGetClaveUnica_Dni_DevuelveSoloDigitos()
        {
            string clave;
            string err;
            var ok = HubSpotCuitCuilHelper.TryGetClaveUnica("13.018.824", out clave, out err);

            Assert.True(ok);
            Assert.Equal("13018824", clave);
            Assert.Null(err);
        }

        [Fact]
        public void TryGetClaveUnica_SinDocumento_DevuelveFalseConMensaje()
        {
            string clave;
            string err;
            var ok = HubSpotCuitCuilHelper.TryGetClaveUnica(string.Empty, out clave, out err);

            Assert.False(ok);
            Assert.Null(clave);
            Assert.Contains("cuitcuil_unica", err);
        }
    }
}
