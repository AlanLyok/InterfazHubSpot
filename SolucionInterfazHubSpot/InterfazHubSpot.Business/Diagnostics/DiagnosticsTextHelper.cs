using System.Configuration;

namespace InterfazHubSpot.Business.Diagnostics
{
    /// <summary>Trunca textos grandes en pasos (<c>Diagnostics:PasoDatosMaxChars</c>).</summary>
    public static class DiagnosticsTextHelper
    {
        private const int DefaultMaxChars = 8192;

        public static string TruncateForTrace(string text)
        {
            var max = ReadMaxChars();
            if (string.IsNullOrEmpty(text) || max <= 0 || text.Length <= max)
            {
                return text ?? string.Empty;
            }

            return text.Substring(0, max) + "...[" + text.Length + " chars total]";
        }

        public static int ReadMaxChars()
        {
            var s = ConfigurationManager.AppSettings["Diagnostics:PasoDatosMaxChars"];
            int max;
            if (int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out max) && max > 0)
            {
                return max;
            }

            return DefaultMaxChars;
        }
    }
}
