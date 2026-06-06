using InterfazHubSpot.Business.Integration;
using InterfazHubSpot.Entities;
using Xunit;

namespace InterfazHubSpot.Tests.Unit.Integration
{
    public sealed class IntegracionColaIdentificadorTests
    {
        // ---------------------------------------------------------------
        // Rama: item nulo
        // ---------------------------------------------------------------
        [Fact]
        public void TryGetClienteId_ItemNulo_DevuelveFalseConMensaje()
        {
            int id;
            string err;

            var result = IntegracionColaIdentificador.TryGetClienteId(null, out id, out err);

            Assert.False(result);
            Assert.Equal(0, id);
            Assert.Equal("Ítem de cola nulo.", err);
        }

        // ---------------------------------------------------------------
        // Rama: TipoEntidad vacío
        // ---------------------------------------------------------------
        [Fact]
        public void TryGetClienteId_TipoEntidadVacio_DevuelveFalseConMensajeMencionandoCliente()
        {
            var item = new ProcesosSpertaHubSpot { TipoEntidad = "", Identificador = 1 };
            int id;
            string err;

            var result = IntegracionColaIdentificador.TryGetClienteId(item, out id, out err);

            Assert.False(result);
            Assert.Contains("Cliente", err);
        }

        // ---------------------------------------------------------------
        // Rama: TipoEntidad con valor desconocido
        // ---------------------------------------------------------------
        [Fact]
        public void TryGetClienteId_TipoEntidadOtroValor_DevuelveFalseConMensajeMencionandoTipoRecibido()
        {
            const string tipoRaro = "Proveedor";
            var item = new ProcesosSpertaHubSpot { TipoEntidad = tipoRaro, Identificador = 1 };
            int id;
            string err;

            var result = IntegracionColaIdentificador.TryGetClienteId(item, out id, out err);

            Assert.False(result);
            Assert.Contains(tipoRaro, err);
        }

        // ---------------------------------------------------------------
        // Rama: TipoEntidad = "Cliente" aceptado en distintas variantes
        // ---------------------------------------------------------------
        [Theory]
        [InlineData("cliente")]
        [InlineData("CLIENTE")]
        [InlineData("Cliente")]
        public void TryGetClienteId_TipoEntidadClienteCaseInsensitive_AceptaVariantes(string variante)
        {
            var item = new ProcesosSpertaHubSpot { TipoEntidad = variante, Identificador = 42 };
            int id;
            string err;

            var result = IntegracionColaIdentificador.TryGetClienteId(item, out id, out err);

            Assert.True(result, "Variante '" + variante + "' debería ser aceptada.");
            Assert.Equal(42, id);
            Assert.Null(err);
        }

        // ---------------------------------------------------------------
        // Rama: Identificador == 0
        // ---------------------------------------------------------------
        [Fact]
        public void TryGetClienteId_IdentificadorCero_DevuelveFalseConMensajeMayorQueCero()
        {
            var item = new ProcesosSpertaHubSpot
            {
                TipoEntidad = IntegracionTipoEntidad.Cliente,
                Identificador = 0,
            };
            int id;
            string err;

            var result = IntegracionColaIdentificador.TryGetClienteId(item, out id, out err);

            Assert.False(result);
            Assert.Contains("mayor que cero", err);
        }

        // ---------------------------------------------------------------
        // Rama: Identificador negativo
        // ---------------------------------------------------------------
        [Fact]
        public void TryGetClienteId_IdentificadorNegativo_DevuelveFalseConMensajeMayorQueCero()
        {
            var item = new ProcesosSpertaHubSpot
            {
                TipoEntidad = IntegracionTipoEntidad.Cliente,
                Identificador = -5,
            };
            int id;
            string err;

            var result = IntegracionColaIdentificador.TryGetClienteId(item, out id, out err);

            Assert.False(result);
            Assert.Contains("mayor que cero", err);
        }

        // ---------------------------------------------------------------
        // Rama: caso OK
        // ---------------------------------------------------------------
        [Fact]
        public void TryGetClienteId_Ok_DevuelveTrueYClienteIdIgualAIdentificador()
        {
            var item = new ProcesosSpertaHubSpot
            {
                TipoEntidad = IntegracionTipoEntidad.Cliente,
                Identificador = 77,
            };
            int id;
            string err;

            var result = IntegracionColaIdentificador.TryGetClienteId(item, out id, out err);

            Assert.True(result);
            Assert.Equal(77, id);
            Assert.Null(err);
        }
    }
}
