using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace InterfazHubSpot.Entities
{
    public class IntegracionEjecucionLogMap : EntityTypeConfiguration<IntegracionEjecucionLog>
    {
        public IntegracionEjecucionLogMap()
        {
            HasKey(t => t.LogId);

            Property(t => t.LogId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(t => t.Destino).IsRequired().HasMaxLength(50);
            Property(t => t.Fase).IsRequired().HasMaxLength(80);
            Property(t => t.Detalle).IsMaxLength();

            ToTable("IntegracionEjecucionLog");
            Property(t => t.LogId).HasColumnName("LogId");
            Property(t => t.ProcesoId).HasColumnName("ProcesoId");
            Property(t => t.Destino).HasColumnName("Destino");
            Property(t => t.ClienteId).HasColumnName("ClienteId");
            Property(t => t.Fase).HasColumnName("Fase");
            Property(t => t.Exito).HasColumnName("Exito");
            Property(t => t.Detalle).HasColumnName("Detalle");
            Property(t => t.FechaUtc).HasColumnName("FechaUtc");
        }
    }
}
