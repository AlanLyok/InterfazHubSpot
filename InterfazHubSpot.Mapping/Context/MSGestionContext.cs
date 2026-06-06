using System.Configuration;
using System.Data.Entity;
using InterfazHubSpot.Entities;
using Mastersoft.Framework.DataRepository;
using Mastersoft.Framework.Standard;

namespace InterfazHubSpot.Mapping.Context
{
    public class MSGestionContext : DataContext
    {
        static MSGestionContext()
        {
            Database.SetInitializer<MSGestionContext>(null);
        }

        public MSGestionContext(MSContext oContexto) : base(ResolveGestionConnectionString(oContexto))
        {
        }

        /// <summary>
        /// El método base <see cref="DataContext"/> espera una entrada en <c>connectionStrings</c>
        /// derivada de <see cref="MSContext.CNPrefix"/> (p. ej. <c>InterfazHubSpot_MSGestion</c>);
        /// este proyecto suele declarar solo <c>MSGestion</c>, igual que la Web API principal.
        /// </summary>
        private static string ResolveGestionConnectionString(MSContext oContexto)
        {
            ConnectionStringSettings resolved = null;
            var prefix = oContexto != null && !string.IsNullOrWhiteSpace(oContexto.CNPrefix)
                ? oContexto.CNPrefix.Trim()
                : null;

            if (!string.IsNullOrEmpty(prefix))
            {
                resolved = ConfigurationManager.ConnectionStrings[prefix + "_MSGestion"]
                           ?? ConfigurationManager.ConnectionStrings[prefix];
            }

            resolved = resolved ?? ConfigurationManager.ConnectionStrings["MSGestion"];

            if (resolved == null || string.IsNullOrWhiteSpace(resolved.ConnectionString))
            {
                var nombreAlternativo = string.IsNullOrEmpty(prefix)
                    ? "MSGestion"
                    : prefix + "_MSGestion, " + prefix + " o MSGestion";
                throw new ConfigurationErrorsException(
                    "Falta la cadena de conexión para Entity Framework (cola ProcesosSpertaHubSpot, errores, etc.). "
                    + "Defina en Web.config una de: " + nombreAlternativo + ".");
            }

            return resolved.ConnectionString;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new EmpresasMap());
            modelBuilder.Configurations.Add(new ErroresMap());
            modelBuilder.Configurations.Add(new PerfilesWebMap());
            modelBuilder.Configurations.Add(new UsuariosWebMap());
            modelBuilder.Configurations.Add(new ProcesosSpertaHubSpotMap());
            modelBuilder.Configurations.Add(new IntegracionEjecucionLogMap());
        }
    }
}
