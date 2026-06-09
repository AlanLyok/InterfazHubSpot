using System;

namespace InterfazHubSpot.Business.Diagnostics
{
    /// <summary>Entrada de traza serializable hacia MVC u otros consumidores.</summary>
    public sealed class ProcesoPasoDto
    {
        public DateTime FechaUtc { get; set; }

        public ProcesoPasoSeverity Severidad { get; set; }

        public ProcesoPasoCategoria Categoria { get; set; }

        /// <summary>Código estable (p. ej. <c>spertaapi.get.integraciones.cliente</c>).</summary>
        public string Codigo { get; set; }

        public string Mensaje { get; set; }

        /// <summary>Datos opcionales (JSON-friendly vía MVC serializer).</summary>
        public object Datos { get; set; }
    }
}
