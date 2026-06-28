using System;
using System.Collections.Generic;

namespace InterfazHubSpot.Business.Integration
{
    /// <summary>Normalización y validación de NroDocumento para correlación HubSpot (<c>cuitcuil_unica</c>).</summary>
    public static class HubSpotCuitCuilHelper
    {
        /// <summary>Misma regla que SP 004/006: quita guiones, puntos y comas; trim.</summary>
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

        /// <summary>Formato HubSpot: grupos de 3 dígitos desde la derecha (CUIT, DNI, etc.).</summary>
        public static string FormatearParaHubSpot(string soloDigitos)
        {
            if (string.IsNullOrEmpty(soloDigitos) || soloDigitos.Length <= 3)
                return soloDigitos;

            var grupos = new List<string>();
            for (var i = soloDigitos.Length; i > 0; i -= 3)
            {
                var start = i - 3;
                if (start < 0)
                    start = 0;
                grupos.Insert(0, soloDigitos.Substring(start, i - start));
            }

            return string.Join(".", grupos);
        }

        /// <summary>Obtiene la clave única para búsqueda/upsert en HubSpot.</summary>
        public static bool TryGetClaveUnica(string numeroDocumento, out string clave, out string errorMessage)
        {
            var digits = Normalizar(numeroDocumento);
            if (!string.IsNullOrEmpty(digits))
            {
                clave = FormatearParaHubSpot(digits);
                errorMessage = null;
                return true;
            }

            clave = null;
            errorMessage = "NroDocumento requerido para correlación HubSpot (cuitcuil_unica).";
            return false;
        }
    }
}
