using System.Data;
using InterfazHubSpot.Business.Integration;
using Xunit;

namespace InterfazHubSpot.Tests.Unit.Managers
{
    public class ClienteIntegracionMapperTests
    {
        private static DataTable BuildCabeceraTable(int clienteId = 1, string razonSocial = "Test SA")
        {
            var dt = new DataTable();
            dt.Columns.Add("ClienteId", typeof(int));
            dt.Columns.Add("CodigoCliente", typeof(string));
            dt.Columns.Add("RazonSocial", typeof(string));
            dt.Columns.Add("ApellidoYNombre", typeof(string));
            dt.Columns.Add("Contacto", typeof(string));
            dt.Columns.Add("NumeroDocumento", typeof(string));
            dt.Columns.Add("Calle", typeof(string));
            dt.Columns.Add("Puerta", typeof(string));
            dt.Columns.Add("Localidad", typeof(string));
            dt.Columns.Add("CodigoPostal", typeof(string));
            dt.Columns.Add("CodigoProvinciaCliente", typeof(string));
            dt.Columns.Add("CodigoPais", typeof(string));
            dt.Columns.Add("Zona", typeof(string));
            dt.Columns.Add("Vendedor", typeof(string));
            dt.Columns.Add("ResponsableCuenta", typeof(string));
            dt.Columns.Add("ListaPrecios", typeof(string));
            dt.Columns.Add("CondicionVenta", typeof(string));
            dt.Columns.Add("DiasParaDeuda", typeof(string));
            dt.Columns.Add("LimiteCredito", typeof(string));
            dt.Columns.Add("CategoriaCliente", typeof(string));
            dt.Columns.Add("ManejoCuentaCorriente", typeof(string));
            dt.Rows.Add(clienteId, "COD001", razonSocial, "Apellido Nombre", "Contacto S", "20123456789",
                "Calle Falsa", "123", "Capital", "1000", "BA", "AR", "Zona Norte", "Vend 1", "Resp 1", "LP1", "CV1", "30", "100000", "Cat A",
                "Cuenta Corriente al 01/01/2026. Deuda: $0");
            return dt;
        }

        private static DataTable BuildContactosTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("ClienteId", typeof(int));
            dt.Columns.Add("ApellidoYNombre", typeof(string));
            dt.Columns.Add("CorreoElectronico", typeof(string));
            dt.Columns.Add("Telefono", typeof(string));
            dt.Columns.Add("Sector", typeof(string));
            dt.Rows.Add(1, "Juan Perez", "juan@test.com", "1234567890", "VENTAS");
            return dt;
        }

        private static DataTable BuildDireccionesTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("ClienteId", typeof(int));
            dt.Columns.Add("Domicilio", typeof(string));
            dt.Columns.Add("CodigoPostal", typeof(string));
            dt.Columns.Add("Localidad", typeof(string));
            dt.Columns.Add("Provincia", typeof(string));
            dt.Columns.Add("Pais", typeof(string));
            dt.Rows.Add(1, "Av. Corrientes 1234", "1043", "CABA", "BA", "Argentina");
            return dt;
        }

        [Fact]
        public void MapearCliente_CabeceraVacia_RetornaNulo()
        {
            var result = ClienteIntegracionMapper.MapearCliente(new DataTable(), null);
            Assert.Null(result);
        }

        [Fact]
        public void MapearCliente_ConDatosCompletos_MapeoCorrectamente()
        {
            var result = ClienteIntegracionMapper.MapearCliente(
                BuildCabeceraTable(), BuildDireccionesTable());

            Assert.NotNull(result);
            Assert.Equal(1, result.ClienteId);
            Assert.Equal("COD001", result.CodigoCliente);
            Assert.Equal("Test SA", result.Cliente.RazonSocial);
            Assert.Equal("Zona Norte", result.Cliente.ZonaId);
            Assert.Equal("Vend 1", result.Cliente.VendedorId);
            Assert.Equal("Resp 1", result.Cliente.ResponsableCuentaId);
            Assert.Equal("LP1", result.Cliente.ListaPreciosId);
            Assert.Equal("CV1", result.Cliente.CondicionVentaId);
            Assert.Equal("Cat A", result.Cliente.CategoriaClienteId);
            Assert.Equal("Cuenta Corriente al 01/01/2026. Deuda: $0", result.Cliente.ManejoCuentaCorriente);
            Assert.Single(result.Cliente.ListaDireccionEntregas);
            Assert.Equal("Av. Corrientes 1234", result.Cliente.ListaDireccionEntregas[0].Domicilio);
            Assert.Equal("BA", result.Cliente.ListaDireccionEntregas[0].ProvinciaId);
            Assert.Equal("Argentina", result.Cliente.ListaDireccionEntregas[0].Pais);
        }

        [Fact]
        public void MapearCliente_SinDirecciones_RetornaListaVacia()
        {
            var result = ClienteIntegracionMapper.MapearCliente(
                BuildCabeceraTable(), new DataTable());

            Assert.NotNull(result);
            Assert.Empty(result.Cliente.ListaDireccionEntregas);
        }

        [Fact]
        public void MapearContactos_ConDatos_MapeaSector()
        {
            var result = ClienteIntegracionMapper.MapearContactos(BuildContactosTable());

            Assert.Single(result);
            Assert.Equal("juan@test.com", result[0].CorreoElectronico);
            Assert.Equal("VENTAS", result[0].SectorId);
        }

        [Fact]
        public void MapearContactos_TablaNula_RetornaListaVacia()
        {
            var result = ClienteIntegracionMapper.MapearContactos(null);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void MapearPaginaCuentaCorriente_MenosItemsQuePageSize_HayMasFalso()
        {
            var tabla = new DataTable();
            tabla.Columns.Add("ClienteId", typeof(int));
            tabla.Columns.Add("NumeroDocumento", typeof(string));
            tabla.Columns.Add("ManejoCuentaCorriente", typeof(string));
            tabla.Rows.Add(100, "20123456789", "SI");
            tabla.Rows.Add(200, "20987654321", "NO");

            var result = ClienteIntegracionMapper.MapearPaginaCuentaCorriente(tabla, 10);

            Assert.False(result.HayMas);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal("20123456789", result.Items[0].NumeroDocumento);
        }

        [Fact]
        public void MapearPaginaCuentaCorriente_MasItemsQuePageSize_HayMasVerdadero()
        {
            var tabla = new DataTable();
            tabla.Columns.Add("ClienteId", typeof(int));
            tabla.Columns.Add("NumeroDocumento", typeof(string));
            tabla.Columns.Add("ManejoCuentaCorriente", typeof(string));
            tabla.Rows.Add(100, "20123456789", "SI");
            tabla.Rows.Add(200, "20987654321", "NO");
            tabla.Rows.Add(300, "SI");

            var result = ClienteIntegracionMapper.MapearPaginaCuentaCorriente(tabla, 2);

            Assert.True(result.HayMas);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(200, result.SiguienteCursor);
        }

        [Fact]
        public void MapearPaginaCuentaCorriente_TablaNula_RetornaVacia()
        {
            var result = ClienteIntegracionMapper.MapearPaginaCuentaCorriente(null, 10);
            Assert.NotNull(result);
            Assert.False(result.HayMas);
            Assert.Empty(result.Items);
        }
    }
}
