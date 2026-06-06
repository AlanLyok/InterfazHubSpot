using System;
using System.Globalization;
using InterfazHubSpot.Entities;

namespace InterfazHubSpot.Business.Integration
{
    /// <summary>Interpreta <see cref="ProcesosSpertaHubSpot.Identificador"/> según <see cref="ProcesosSpertaHubSpot.TipoEntidad"/>.</summary>
    public static class IntegracionColaIdentificador
    {
        /// <summary>Para <see cref="IntegracionTipoEntidad.Cliente"/>, el identificador es el PK cliente ERP.</summary>
        public static bool TryGetClienteId(ProcesosSpertaHubSpot item, out int clienteId, out string errorMessage)
        {
            clienteId = 0;
            errorMessage = null;

            if (item == null)
            {
                errorMessage = "Ítem de cola nulo.";
                return false;
            }

            var tipo = (item.TipoEntidad ?? string.Empty).Trim();
            if (!string.Equals(tipo, IntegracionTipoEntidad.Cliente, StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "TipoEntidad no soportado para HubSpot 2A (se esperaba Cliente): {0}",
                    tipo);
                return false;
            }

            clienteId = item.Identificador;
            if (clienteId <= 0)
            {
                errorMessage = "Identificador debe ser mayor que cero para TipoEntidad Cliente.";
                return false;
            }

            return true;
        }
    }
}
