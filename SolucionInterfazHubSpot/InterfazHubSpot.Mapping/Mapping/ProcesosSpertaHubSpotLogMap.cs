using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace InterfazHubSpot.Entities
{
    public class ProcesosSpertaHubSpotLogMap : EntityTypeConfiguration<ProcesosSpertaHubSpotLog>
    {
        public ProcesosSpertaHubSpotLogMap()
        {
            HasKey(t => t.LogId);

            Property(t => t.LogId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(t => t.Destino).IsRequired().HasMaxLength(50);
            Property(t => t.Fase).IsRequired().HasMaxLength(80);
            Property(t => t.Detalle).IsMaxLength();

            ToTable("ProcesosSpertaHubSpotLog");
            Property(t => t.LogId).HasColumnName("LogId");
            Property(t => t.ProcesoId).HasColumnName("ProcesoId");
            Property(t => t.Destino).HasColumnName("Destino");
            Property(t => t.Identificador).HasColumnName("Identificador");
            Property(t => t.Fase).HasColumnName("Fase");
            Property(t => t.Exito).HasColumnName("Exito");
            Property(t => t.Detalle).HasColumnName("Detalle");
            Property(t => t.FechaGrabacion).HasColumnName("FechaGrabacion");
        }
    }
}
