using System;
using System.Reflection;
using InterfazHubSpot.Business.Managers;
using InterfazHubSpot.Entities;
using Xunit;

namespace InterfazHubSpot.IntegrationTests.Managers
{
    /// <summary>Proyeccion de cola sin BD: ejercita <see cref="ProcesosSpertaHubSpotManager"/> via reflexion.</summary>
    public sealed class ProcesosSpertaHubSpotManagerProjectionTests
    {
        [Fact, Trait("Category", "Integration")]
        public void ProjectarMuestra_MapeaTodosLosCampos()
        {
            var fecha = new DateTime(2026, 6, 9, 10, 30, 0);
            var entity = new ProcesosSpertaHubSpot
            {
                ProcesoId = 1001,
                EmpresaId = 5,
                Destino = "HubSpot",
                TipoEntidad = "Cliente",
                TipoOperacion = "Alta",
                Identificador = 77,
                Intentos = 2,
                FechaCreacion = fecha,
            };

            var muestra = InvokeProjectarMuestra(entity);

            Assert.Equal(1001, muestra.ProcesoId);
            Assert.Equal(5, muestra.EmpresaId);
            Assert.Equal("HubSpot", muestra.Destino);
            Assert.Equal("Cliente", muestra.TipoEntidad);
            Assert.Equal("Alta", muestra.TipoOperacion);
            Assert.Equal(77, muestra.Identificador);
            Assert.Equal(2, muestra.Intentos);
            Assert.Equal(fecha, muestra.FechaCreacion);
        }

        [Fact, Trait("Category", "Integration")]
        public void ColaIntegracionPendienteMuestra_TipoPublico_VisibleDesdeIntegrationProject()
        {
            Assert.NotNull(typeof(ColaIntegracionPendienteMuestra));
        }

        [Fact, Trait("Category", "Integration")]
        public void ProcesosSpertaHubSpotManager_TipoPublico_VisibleDesdeIntegrationProject()
        {
            Assert.NotNull(typeof(ProcesosSpertaHubSpotManager));
        }

        private static ColaIntegracionPendienteMuestra InvokeProjectarMuestra(ProcesosSpertaHubSpot entity)
        {
            var method = typeof(ProcesosSpertaHubSpotManager).GetMethod(
                "ProjectarMuestra",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (ColaIntegracionPendienteMuestra)method.Invoke(null, new object[] { entity });
        }
    }
}
