
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Mastersoft.Framework.Interfaces;

namespace InterfazHubSpot.Entities
{
    /// <summary>Fila de cola outbox para integraciones (ERP → sistemas externos).</summary>
    public partial class ProcesosSpertaHubSpot : Entity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ProcesoId { get; set; }

        public string TenantId { get; set; }

        public int? EmpresaId { get; set; }

        public string Destino { get; set; }

        public string TipoEntidad { get; set; }

        public string TipoOperacion { get; set; }

        /// <summary>PK del maestro según <see cref="TipoEntidad"/> (p. ej. ClienteID).</summary>
        public int Identificador { get; set; }

        /// <summary>0=Pendiente, 1=EnProceso, 2=Ok, 3=Error.</summary>
        public byte Estado { get; set; }

        public int Intentos { get; set; }

        public string MensajeUltimoError { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaInicioProceso { get; set; }

        public DateTime? FechaFinProceso { get; set; }
    }
}
