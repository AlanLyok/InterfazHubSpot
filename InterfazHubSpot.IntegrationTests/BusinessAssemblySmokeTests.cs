using BatchSpertaAPI.Business;
using Xunit;

namespace BatchSpertaAPI.IntegrationTests
{
    /// <summary>Humo mínimo: referencia compilada contra Business sin BD/API (pruebas "Live" usarán Trait Category=Live).</summary>
    public sealed class BusinessAssemblySmokeTests
    {
        [Fact]
        public void ErroresManager_tipo_visible_desde_integration_project()
        {
            Assert.NotNull(typeof(ErroresManager));
        }
    }
}
