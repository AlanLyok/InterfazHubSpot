using System.Data.Entity.ModelConfiguration;

namespace BatchSpertaAPI.Entities
{
    public class ErroresMap : EntityTypeConfiguration<Errores>
    {
        public ErroresMap()
        {

            // Primary Key
            this.HasKey(t => t.ErrorId);

            // Properties
            this.Property(t => t.MachineName)
                .HasMaxLength(50);

            this.Property(t => t.AppDomainName)
                .HasMaxLength(50);

            this.Property(t => t.ThreadIdentity)
                .HasMaxLength(50);

            this.Property(t => t.WindowsIdentity)
                .HasMaxLength(50);

            this.Property(t => t.Message)
                .HasMaxLength(200);

            // Table & Column Mappings
            this.ToTable("Errores");
            this.Property(t => t.ErrorId).HasColumnName("ErrorId");
            this.Property(t => t.ErrorDateTime).HasColumnName("ErrorDateTime");
            this.Property(t => t.MachineName).HasColumnName("MachineName");
            this.Property(t => t.AppDomainName).HasColumnName("AppDomainName");
            this.Property(t => t.ThreadIdentity).HasColumnName("ThreadIdentity");
            this.Property(t => t.WindowsIdentity).HasColumnName("WindowsIdentity");
            this.Property(t => t.Message).HasColumnName("Message");
            this.Property(t => t.FullException).HasColumnName("FullException");
        }
    }
}



