using System.Data.Entity.ModelConfiguration;

namespace BatchSpertaAPI.Entities
{
    public class UsuariosWebMap : EntityTypeConfiguration<UsuariosWeb>
    {
        public UsuariosWebMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.EMail)
                .HasMaxLength(100);

            this.Property(t => t.Usuario)
                .HasMaxLength(100);

            this.Property(t => t.NombreCompleto)
                .HasMaxLength(100);

            this.Property(t => t.Hash)
                .HasMaxLength(50);

            this.Property(t => t.Salt)
                .HasMaxLength(10);

            this.Property(t => t.HeaderColor)
                .HasMaxLength(10);

            this.Property(t => t.KendoThemeClaro)
                .HasMaxLength(20);

            this.Property(t => t.KendoThemeOscuro)
                .HasMaxLength(20);

            this.Property(t => t.ImagenFondo)
                .HasMaxLength(50);

            this.Property(t => t.ColorEtiquetas)
                .HasMaxLength(10);

            // Table & Column Mappings
            this.ToTable("UsuariosWeb");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.EmpresaId).HasColumnName("EmpresaId");
            this.Property(t => t.Usuario).HasColumnName("Usuario");
            this.Property(t => t.EMail).HasColumnName("EMail");
            this.Property(t => t.NombreCompleto).HasColumnName("NombreCompleto");
            this.Property(t => t.Hash).HasColumnName("Hash");
            this.Property(t => t.Salt).HasColumnName("Salt");
            this.Property(t => t.UsaDobleFactor).HasColumnName("UsaDobleFactor");
            this.Property(t => t.MuestraQR).HasColumnName("MuestraQR");
            this.Property(t => t.PerfilId).HasColumnName("PerfilId");
            this.Property(t => t.HeaderColor).HasColumnName("HeaderColor");
            this.Property(t => t.ModoOscuro).HasColumnName("ModoOscuro");
            this.Property(t => t.KendoThemeClaro).HasColumnName("KendoThemeClaro");
            this.Property(t => t.KendoThemeOscuro).HasColumnName("KendoThemeOscuro");
            this.Property(t => t.ImagenFondo).HasColumnName("ImagenFondo");
            this.Property(t => t.ColorEtiquetas).HasColumnName("ColorEtiquetas");
        }
    }
}


