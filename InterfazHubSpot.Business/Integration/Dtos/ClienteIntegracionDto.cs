using System.Collections.Generic;

namespace InterfazHubSpot.Business.Integration.Dtos
{
    public sealed class ClienteIntegracionDto
    {
        public int ClienteId { get; set; }
        public string CodigoCliente { get; set; }
        public ClienteDatosDto Cliente { get; set; }
    }

    public sealed class ClienteDatosDto
    {
        public string RazonSocial { get; set; }
        public string ApellidoYNombre { get; set; }
        public string Contacto { get; set; }
        public string NumeroDocumento { get; set; }
        public string Calle { get; set; }
        public string Puerta { get; set; }
        public string Localidad { get; set; }
        public string CodigoPostal { get; set; }
        public string CodigoProvinciaCliente { get; set; }
        public string CodigoPais { get; set; }
        public string ZonaId { get; set; }
        public string VendedorId { get; set; }
        public string ResponsableCuentaId { get; set; }
        public string ListaPreciosId { get; set; }
        public string CondicionVentaId { get; set; }
        public string DiasParaDeuda { get; set; }
        public string LimiteCredito { get; set; }
        public string CategoriaClienteId { get; set; }
        public List<ContactoDto> ListaClientesContactos { get; set; }
        public List<DireccionEntregaDto> ListaDireccionEntregas { get; set; }

        public ClienteDatosDto()
        {
            ListaClientesContactos = new List<ContactoDto>();
            ListaDireccionEntregas = new List<DireccionEntregaDto>();
        }
    }

    public sealed class ContactoDto
    {
        public string ApellidoYNombre { get; set; }
        public string CorreoElectronico { get; set; }
        public string Telefono { get; set; }
        public string SectorId { get; set; }
    }

    public sealed class DireccionEntregaDto
    {
        public string Domicilio { get; set; }
        public string CodigoPostal { get; set; }
        public string Localidad { get; set; }
        public string ProvinciaId { get; set; }
    }

    public sealed class PaginaCuentaCorrienteDto
    {
        public List<ItemCuentaCorrienteDto> Items { get; set; }
        public bool HayMas { get; set; }
        public int SiguienteCursor { get; set; }

        public PaginaCuentaCorrienteDto()
        {
            Items = new List<ItemCuentaCorrienteDto>();
        }
    }

    public sealed class ItemCuentaCorrienteDto
    {
        public int ClienteId { get; set; }
        public string ManejoCuentaCorriente { get; set; }
    }
}
