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
            dt.Columns.Add("ZonaId", typeof(string));
            dt.Columns.Add("VendedorId", typeof(string));
            dt.Columns.Add("ResponsableCuentaId", typeof(string));
            dt.Columns.Add("ListaPreciosId", typeof(string));
            dt.Columns.Add("CondicionVentaId", typeof(string));
            dt.Columns.Add("DiasParaDeuda", typeof(string));
            dt.Columns.Add("LimiteCredito", typeof(string));
            dt.Columns.Add("CategoriaClienteId", typeof(string));
            dt.Rows.Add(clienteId, "COD001", razonSocial, "Apellido Nombre", "Contacto S", "20123456789",
                "Calle Falsa", "123", "Capital", "1000", "BA", "AR", "1", "2", "3", "LP1", "CV1", "30", "100000", "A");
            return dt;
        }

        private static DataTable BuildContactosTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("ClienteId", typeof(int));
            dt.Columns.Add("ApellidoYNombre", typeof(string));
            dt.Columns.Add("CorreoElectronico", typeof(string));
            dt.Columns.Add("Telefono", typeof(string));
            dt.Columns.Add("SectorId", typeof(string));
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
            dt.Columns.Add("ProvinciaId", typeof(string));
            dt.Rows.Add(1, "Av. Corrientes 1234", "1043", "CABA", "BA");
            return dt;
        }

        [Fact]
        public void MapearCliente_CabeceraVacia_RetornaNulo()
        {
            var result = ClienteIntegracionMapper.MapearCliente(new DataTable(), null, null);
            Assert.Null(result);
        }

        [Fact]
        public void MapearCliente_ConDatosCompletos_MapeoCorrectamente()
        {
            var result = ClienteIntegracionMapper.MapearCliente(
                BuildCabeceraTable(), BuildContactosTable(), BuildDireccionesTable());

            Assert.NotNull(result);
            Assert.Equal(1, result.ClienteId);
            Assert.Equal("COD001", result.CodigoCliente);
            Assert.Equal("Test SA", result.Cliente.RazonSocial);
            Assert.Single(result.Cliente.ListaClientesContactos);
            Assert.Equal("juan@test.com", result.Cliente.ListaClientesContactos[0].CorreoElectronico);
            Assert.Single(result.Cliente.ListaDireccionEntregas);
            Assert.Equal("Av. Corrientes 1234", result.Cliente.ListaDireccionEntregas[0].Domicilio);
        }

        [Fact]
        public void MapearCliente_SinContactos_RetornaListaVacia()
        {
            var result = ClienteIntegracionMapper.MapearCliente(
                BuildCabeceraTable(), new DataTable(), new DataTable());

            Assert.NotNull(result);
            Assert.Empty(result.Cliente.ListaClientesContactos);
            Assert.Empty(result.Cliente.ListaDireccionEntregas);
        }

        [Fact]
        public void MapearPaginaCuentaCorriente_MenosItemsQuePageSize_HayMasFalso()
        {
            var tabla = new DataTable();
            tabla.Columns.Add("ClienteId", typeof(int));
            tabla.Columns.Add("ManejoCuentaCorriente", typeof(string));
            tabla.Rows.Add(100, "SI");
            tabla.Rows.Add(200, "NO");

            var result = ClienteIntegracionMapper.MapearPaginaCuentaCorriente(tabla, 10);

            Assert.False(result.HayMas);
            Assert.Equal(2, result.Items.Count);
        }

        [Fact]
        public void MapearPaginaCuentaCorriente_MasItemsQuePageSize_HayMasVerdadero()
        {
            var tabla = new DataTable();
            tabla.Columns.Add("ClienteId", typeof(int));
            tabla.Columns.Add("ManejoCuentaCorriente", typeof(string));
            tabla.Rows.Add(100, "SI");
            tabla.Rows.Add(200, "NO");
            tabla.Rows.Add(300, "SI"); // tercer item = pageSize+1

            var result = ClienteIntegracionMapper.MapearPaginaCuentaCorriente(tabla, 2);

            Assert.True(result.HayMas);
            Assert.Equal(2, result.Items.Count); // solo los primeros 2
            Assert.Equal(200, result.SiguienteCursor); // último item incluido
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
