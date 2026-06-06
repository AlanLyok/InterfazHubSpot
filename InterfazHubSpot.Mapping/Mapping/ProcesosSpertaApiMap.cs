using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BatchSpertaAPI.Entities
{
    public class ProcesosSpertaApiMap : EntityTypeConfiguration<ProcesosSpertaApi>
    {
        public ProcesosSpertaApiMap()
        {
            HasKey(t => t.ProcesoId);

            Property(t => t.ProcesoId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(t => t.TenantId).HasMaxLength(64);
            Property(t => t.Destino).IsRequired().HasMaxLength(50);
            Property(t => t.TipoEntidad).IsRequired().HasMaxLength(50);
            Property(t => t.TipoOperacion).IsRequired().HasMaxLength(20);
            Property(t => t.MensajeUltimoError).IsMaxLength();

            ToTable("ProcesosSpertaAPI");
            Property(t => t.ProcesoId).HasColumnName("ProcesoId");
            Property(t => t.TenantId).HasColumnName("TenantId");
            Property(t => t.EmpresaId).HasColumnName("EmpresaId");
            Property(t => t.Destino).HasColumnName("Destino");
            Property(t => t.TipoEntidad).HasColumnName("TipoEntidad");
            Property(t => t.TipoOperacion).HasColumnName("TipoOperacion");
            Property(t => t.Identificador).HasColumnName("Identificador");
            Property(t => t.Estado).HasColumnName("Estado");
            Property(t => t.Intentos).HasColumnName("Intentos");
            Property(t => t.MensajeUltimoError).HasColumnName("MensajeUltimoError");
            Property(t => t.FechaCreacionUtc).HasColumnName("FechaCreacionUtc");
            Property(t => t.FechaInicioProcesoUtc).HasColumnName("FechaInicioProcesoUtc");
            Property(t => t.FechaFinProcesoUtc).HasColumnName("FechaFinProcesoUtc");
        }
    }
}
