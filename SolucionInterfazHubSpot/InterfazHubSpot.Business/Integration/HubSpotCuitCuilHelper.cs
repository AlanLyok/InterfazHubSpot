using System;

namespace InterfazHubSpot.Business.Integration
{
    /// <summary>Normalización y validación de NroDocumento para correlación HubSpot (<c>cuitcuil_unica</c>).</summary>
    public static class HubSpotCuitCuilHelper
    {
        /// <summary>Misma regla que SP 004/006: quita guiones, puntos y comas; trim. Solo dígitos para HubSpot.</summary>
        public static string Normalizar(string nroDocumento)
        {
            if (string.IsNullOrWhiteSpace(nroDocumento))
                return null;

            return nroDocumento
                .Replace("-", string.Empty)
                .Replace(".", string.Empty)
                .Replace(",", string.Empty)
                .Trim();
        }

        /// <summary>Obtiene la clave única para búsqueda/upsert en HubSpot (solo dígitos).</summary>
        public static bool TryGetClaveUnica(string numeroDocumento, out string clave, out string errorMessage)
        {
            clave = Normalizar(numeroDocumento);
            if (!string.IsNullOrEmpty(clave))
            {
                errorMessage = null;
                return true;
            }

            errorMessage = "NroDocumento requerido para correlación HubSpot (cuitcuil_unica).";
            return false;
        }
    }
}
