using System.Data.Entity.ModelConfiguration;
using InterfazHubSpot.Entities;

namespace InterfazHubSpot.Entities
{
    public class EmpresasMap : EntityTypeConfiguration<Empresas>
    {
        public EmpresasMap()
        {
            // Primary Key
            this.HasKey(t => t.CodEmpre);

            // Properties
            this.Property(t => t.Descrip)
                .HasMaxLength(40);

            this.Property(t => t.Direccion)
                .HasMaxLength(50);

            this.Property(t => t.Localidad)
                .HasMaxLength(50);

            this.Property(t => t.Cuit)
                .IsFixedLength()
                .HasMaxLength(13);

            this.Property(t => t.NroIngBrutos)
                .HasMaxLength(15);

            this.Property(t => t.Demo)
                .IsFixedLength()
                .HasMaxLength(1);

            this.Property(t => t.Telefono)
                .HasMaxLength(70);

            this.Property(t => t.Mail)
                .HasMaxLength(70);

            this.Property(t => t.ContentTypeLogo)
                .HasMaxLength(50);

            this.Property(t => t.NombreArchLogo)
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("Empresas");
            this.Property(t => t.CodEmpre).HasColumnName("CodEmpre");
            this.Property(t => t.Descrip).HasColumnName("Descrip");
            this.Property(t => t.Direccion).HasColumnName("Direccion");
            this.Property(t => t.Localidad).HasColumnName("Localidad");
            this.Property(t => t.Provinciaid).HasColumnName("Provinciaid");
            this.Property(t => t.CondFiscalID).HasColumnName("CondFiscalID");
            this.Property(t => t.Cuit).HasColumnName("Cuit");
            this.Property(t => t.NroIngBrutos).HasColumnName("NroIngBrutos");
            this.Property(t => t.Demo).HasColumnName("Demo");
            this.Property(t => t.Telefono).HasColumnName("Telefono");
            this.Property(t => t.Mail).HasColumnName("Mail");
            this.Property(t => t.ImagenLogo).HasColumnName("ImagenLogo");
            this.Property(t => t.ContentTypeLogo).HasColumnName("ContentTypeLogo");
            this.Property(t => t.NombreArchLogo).HasColumnName("NombreArchLogo");
        }
    }
}



