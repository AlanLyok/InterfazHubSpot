using System;
using InterfazHubSpot.Business.Integration;
using InterfazHubSpot.Business.Managers;
using Xunit;

namespace InterfazHubSpot.IntegrationTests.Managers
{
    /// <summary>
    /// Tests de integración que documentan el contrato de <see cref="ProcesosSpertaHubSpotManager"/>
    /// contra una base de datos MSGestion real.  Todos requieren conexión activa y están
    /// tageados <c>Category=Live</c> para ser excluidos del run por defecto.
    ///
    /// Para ejecutar desde ambiente Live:
    ///   1. Agregar referencia a Mastersoft.Framework.Standard.dll en InterfazHubSpot.IntegrationTests.csproj.
    ///   2. Configurar App.config con connection string MSGestion.
    ///   3. Reemplazar los Fact(Skip=...) por Fact con Trait("Category","Live").
    ///   4. Descomentar el código de instanciación en cada test.
    ///
    /// Contrato documentado:
    ///   - ContarPendientes(destino): COUNT(*) WHERE Estado=Pendiente AND Destino=@d >= 0
    ///   - ContarEnProceso(destino): COUNT(*) WHERE Estado=EnProceso AND Destino=@d >= 0
    ///   - ListarMuestraPendientes(destino, 0): devuelve lista vacía sin tocar la DB
    ///   - ReclamarPendientes: marca filas Pendiente -> EnProceso, incrementa Intentos
    ///   - MarcarOk: Estado=Ok, FechaFinProceso=Now, MensajeUltimoError=null
    ///   - MarcarError: Estado=Error, MensajeUltimoError truncado a 8000 chars
    ///   - ReponerEnCola: Estado=Pendiente, FechaInicioProceso=null, FechaFinProceso=null
    /// </summary>
    public sealed class ProcesosSpertaHubSpotManagerLiveTests
    {
        private const string SkipSetup = "Requiere referencia a Mastersoft.Framework.Standard y App.config con MSGestion — ver docs de setup Live";
        private const string SkipWriteDb = "Requiere DB de prueba con acceso de escritura — run manual solamente";

        [Fact(Skip = SkipSetup), Trait("Category", "Live")]
        public void ContarPendientes_DestinoHubSpot_DevuelveNumeroNoNegativo()
        {
            // Para activar: instanciar MSContext + manager y ejecutar:
            // var count = manager.ContarPendientes(IntegracionDestinos.HubSpot);
            // Assert.True(count >= 0);
        }

        [Fact(Skip = SkipSetup), Trait("Category", "Live")]
        public void ContarEnProceso_DestinoHubSpot_DevuelveNumeroNoNegativo()
        {
            // var count = manager.ContarEnProceso(IntegracionDestinos.HubSpot);
            // Assert.True(count >= 0);
        }

        [Fact(Skip = SkipSetup), Trait("Category", "Live")]
        public void ListarMuestraPendientes_MaxItemsCero_DevuelveListaVacia()
        {
            // var result = manager.ListarMuestraPendientes(IntegracionDestinos.HubSpot, 0);
            // Assert.NotNull(result);
            // Assert.Empty(result);
        }

        [Fact(Skip = SkipWriteDb), Trait("Category", "Live")]
        public void ReclamarPendientes_Y_MarcarOk_RoundTrip()
        {
            // Setup: insertar fila de prueba via SQL, obtener ProcesoId.
            // var tomados = manager.ReclamarPendientes(IntegracionDestinos.HubSpot, 1);
            // Assert.Single(tomados);
            // var item = tomados[0];
            // Assert.Equal(IntegracionColaEstados.EnProceso, item.Estado);
            // manager.MarcarOk(item.ProcesoId);
            // Verificar en DB: Estado=Ok.
        }

        [Fact(Skip = SkipSetup), Trait("Category", "Live")]
        public void MarcarError_TruncaMensajeMayorA8000Caracteres()
        {
            // Contrato de truncado implementado en el manager:
            //   p.MensajeUltimoError = mensaje != null && mensaje.Length > 8000
            //       ? mensaje.Substring(0, 8000) : mensaje;
            // Para verificar con ID real de prueba:
            // manager.MarcarError(testProcesoId, new string('x', 9000));
            // Verificar en DB: MensajeUltimoError.Length <= 8000.
        }

        [Fact(Skip = SkipWriteDb), Trait("Category", "Live")]
        public void ReponerEnCola_VuelveAEstadoPendiente_LimpiaFechas()
        {
            // Necesita ProcesoId en estado Error/Ok para reponer.
            // manager.ReponerEnCola(testProcesoId);
            // Verificar en DB: Estado=Pendiente, FechaInicioProceso=null, FechaFinProceso=null.
        }
    }
}
