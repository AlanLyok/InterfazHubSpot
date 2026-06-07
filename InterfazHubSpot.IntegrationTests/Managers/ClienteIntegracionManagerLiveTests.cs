using System;
using InterfazHubSpot.Business.Managers;
using Xunit;

namespace InterfazHubSpot.IntegrationTests.Managers
{
    /// <summary>
    /// Tests de integración que verifican el contrato de <see cref="ClienteIntegracionManager"/>
    /// (SPs <c>InterfazHubSpot_Cliente_Obtener</c> y <c>InterfazHubSpot_CuentaCorriente_Pagina</c>)
    /// contra una base de datos MSGestion real.  Todos requieren conexión activa y están
    /// tageados <c>Category=Live</c> para ser excluidos del run por defecto.
    ///
    /// Para ejecutar desde ambiente Live:
    ///   1. Configurar App.config con connection string MSGestion.
    ///   2. Cambiar [Fact(Skip=...)] a [Fact, Trait("Category","Live")].
    ///   3. Opcionalmente setear la env var INTEGRATION_TEST_CLIENTE_ID con un clienteId existente.
    ///
    /// Instanciación de MSContext: requiere referencia a Mastersoft.Framework.Standard.dll
    /// — al ejecutar desde Visual Studio o desde Test-InterfazHubSpot.ps1 -IncludeLive,
    /// la DLL se resuelve transitivamente desde InterfazHubSpot.Business.
    /// </summary>
    public sealed class ClienteIntegracionManagerLiveTests
    {
        private const string SkipNeedsMSContext = "MSContext requiere referencia a Mastersoft.Framework.Standard — ver docs de setup Live";

        [Fact(Skip = SkipNeedsMSContext), Trait("Category", "Live")]
        public void ObtenerClienteParaHubSpot_ClienteExistente_DevuelveDtoNoNulo_ConClienteId()
        {
            // Para ejecutar:
            // var clienteIdStr = Environment.GetEnvironmentVariable("INTEGRATION_TEST_CLIENTE_ID");
            // if (string.IsNullOrWhiteSpace(clienteIdStr)) return; // Skip silencioso
            // var clienteId = int.Parse(clienteIdStr);
            // var manager = new ClienteIntegracionManager(new MSContext());
            // var dto = manager.ObtenerClienteParaHubSpot(clienteId);
            // Assert.NotNull(dto);
            // Assert.NotNull(dto.Cliente);
            // Assert.True(dto.ClienteId >= 0);
            throw new InvalidOperationException(SkipNeedsMSContext);
        }

        [Fact(Skip = SkipNeedsMSContext), Trait("Category", "Live")]
        public void ObtenerClienteParaHubSpot_ClienteInexistente_DevuelveDtoConClienteIdCeroONull()
        {
            // Contrato actual del mapper: si el SP no devuelve filas,
            // el mapper devuelve null o un DTO con ClienteId=0.
            // Para ejecutar:
            // var manager = new ClienteIntegracionManager(new MSContext());
            // var dto = manager.ObtenerClienteParaHubSpot(int.MaxValue);
            // Assert.True(dto == null || dto.ClienteId == 0);
            throw new InvalidOperationException(SkipNeedsMSContext);
        }

        [Fact(Skip = SkipNeedsMSContext), Trait("Category", "Live")]
        public void ObtenerPaginaCuentaCorriente_PageSize10_DevuelvePaginaConHayMasBoolean_Y_ItemsConteoMenorOIgualA10()
        {
            // Para ejecutar:
            // var manager = new ClienteIntegracionManager(new MSContext());
            // var pagina = manager.ObtenerPaginaCuentaCorriente(cursor: 0, pageSize: 10);
            // Assert.NotNull(pagina);
            // Assert.True(pagina.Items.Count <= 10);
            // Assert.True(pagina.SiguienteCursor >= 0 || !pagina.HayMas);
            throw new InvalidOperationException(SkipNeedsMSContext);
        }
    }
}
