using InterfazHubSpot.Business.Diagnostics;
using Xunit;

namespace InterfazHubSpot.Tests.Unit
{
    public sealed class DiagnosticsTextHelperTests
    {
        [Fact]
        public void TruncateForTrace_sin_texto_retorna_vacio_y_texto_Corto_sin_cambiar()
        {
            Assert.Equal(string.Empty, DiagnosticsTextHelper.TruncateForTrace(null));
            Assert.Equal(string.Empty, DiagnosticsTextHelper.TruncateForTrace(""));
            Assert.Equal("hola", DiagnosticsTextHelper.TruncateForTrace("hola"));
        }

        [Fact]
        public void TruncateForTrace_texto_largo_trunca_según_config()
        {
            var s = new string('a', 120);
            var t = DiagnosticsTextHelper.TruncateForTrace(s);
            Assert.True(t.Length < s.Length);
            Assert.StartsWith(new string('a', 80), t, System.StringComparison.Ordinal);
            Assert.Contains("120 chars total", t);
        }

        [Fact]
        public void ReadMaxChars_al_menos_fallback_positivo()
        {
            var m = DiagnosticsTextHelper.ReadMaxChars();
            Assert.True(m > 0);
        }
    }
}
