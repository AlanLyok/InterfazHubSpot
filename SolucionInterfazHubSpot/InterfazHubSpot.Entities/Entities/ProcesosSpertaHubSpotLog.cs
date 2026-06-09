
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Mastersoft.Framework.Interfaces;

namespace InterfazHubSpot.Entities
{
    /// <summary>Auditoría por corrida de integración (OK / error detallado).</summary>
    public partial class ProcesosSpertaHubSpotLog : Entity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long LogId { get; set; }

        public long? ProcesoId { get; set; }

        public string Destino { get; set; }

        public int? Identificador { get; set; }

        public string Fase { get; set; }

        public bool Exito { get; set; }

        public string Detalle { get; set; }

        public DateTime FechaGrabacion { get; set; }
    }
}
