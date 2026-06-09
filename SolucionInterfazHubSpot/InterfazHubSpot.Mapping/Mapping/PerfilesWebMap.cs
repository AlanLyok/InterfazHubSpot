using System.Data.Entity.ModelConfiguration;

namespace InterfazHubSpot.Entities
{
    public class PerfilesWebMap : EntityTypeConfiguration<PerfilesWeb>
    {
        public PerfilesWebMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Descripcion)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("PerfilesWeb");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.EmpresaId).HasColumnName("EmpresaId");
            this.Property(t => t.Descripcion).HasColumnName("Descripcion");
        }
    }
}





