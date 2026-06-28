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
        public void FormatearParaHubSpot_CuitEjemploHubSpot()
        {
            Assert.Equal("30.547.981.029", HubSpotCuitCuilHelper.FormatearParaHubSpot("30547981029"));
        }

        [Fact]
        public void FormatearParaHubSpot_DniEjemploHubSpot()
        {
            Assert.Equal("13.018.824", HubSpotCuitCuilHelper.FormatearParaHubSpot("13018824"));
        }

        [Fact]
        public void FormatearParaHubSpot_MilesCorto()
        {
            Assert.Equal("1.234.567", HubSpotCuitCuilHelper.FormatearParaHubSpot("1234567"));
        }

        [Fact]
        public void FormatearParaHubSpot_TresODMenosDigitos_SinPuntos()
        {
            Assert.Equal("123", HubSpotCuitCuilHelper.FormatearParaHubSpot("123"));
            Assert.Equal("12", HubSpotCuitCuilHelper.FormatearParaHubSpot("12"));
        }

        [Fact]
        public void TryGetClaveUnica_ConDocumentoValido_DevuelveFormatoHubSpot()
        {
            string clave;
            string err;
            var ok = HubSpotCuitCuilHelper.TryGetClaveUnica("30-12345678-9", out clave, out err);

            Assert.True(ok);
            Assert.Equal("30.123.456.789", clave);
            Assert.Null(err);
        }

        [Fact]
        public void TryGetClaveUnica_CuitCalzetta_FormatoConPuntos()
        {
            string clave;
            string err;
            var ok = HubSpotCuitCuilHelper.TryGetClaveUnica("30-54798102-9", out clave, out err);

            Assert.True(ok);
            Assert.Equal("30.547.981.029", clave);
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
