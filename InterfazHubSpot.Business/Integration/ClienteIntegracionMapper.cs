using System;
using System.Collections.Generic;
using System.Data;
using InterfazHubSpot.Business.Integration.Dtos;

namespace InterfazHubSpot.Business.Integration
{
    public static class ClienteIntegracionMapper
    {
        public static ClienteIntegracionDto MapearCliente(
            DataTable cabecera,
            DataTable contactos,
            DataTable direcciones)
        {
            if (cabecera == null || cabecera.Rows.Count == 0)
                return null;

            var row = cabecera.Rows[0];
            var dto = new ClienteIntegracionDto
            {
                ClienteId = GetInt(row, "ClienteId"),
                CodigoCliente = GetStr(row, "CodigoCliente"),
                Cliente = new ClienteDatosDto
                {
                    RazonSocial = GetStr(row, "RazonSocial"),
                    ApellidoYNombre = GetStr(row, "ApellidoYNombre"),
                    Contacto = GetStr(row, "Contacto"),
                    NumeroDocumento = GetStr(row, "NumeroDocumento"),
                    Calle = GetStr(row, "Calle"),
                    Puerta = GetStr(row, "Puerta"),
                    Localidad = GetStr(row, "Localidad"),
                    CodigoPostal = GetStr(row, "CodigoPostal"),
                    CodigoProvinciaCliente = GetStr(row, "CodigoProvinciaCliente"),
                    CodigoPais = GetStr(row, "CodigoPais"),
                    ZonaId = GetStr(row, "ZonaId"),
                    VendedorId = GetStr(row, "VendedorId"),
                    ResponsableCuentaId = GetStr(row, "ResponsableCuentaId"),
                    ListaPreciosId = GetStr(row, "ListaPreciosId"),
                    CondicionVentaId = GetStr(row, "CondicionVentaId"),
                    DiasParaDeuda = GetStr(row, "DiasParaDeuda"),
                    LimiteCredito = GetStr(row, "LimiteCredito"),
                    CategoriaClienteId = GetStr(row, "CategoriaClienteId"),
                }
            };

            if (contactos != null)
            {
                foreach (DataRow c in contactos.Rows)
                {
                    dto.Cliente.ListaClientesContactos.Add(new ContactoDto
                    {
                        ApellidoYNombre = GetStr(c, "ApellidoYNombre"),
                        CorreoElectronico = GetStr(c, "CorreoElectronico"),
                        Telefono = GetStr(c, "Telefono"),
                        SectorId = GetStr(c, "SectorId"),
                    });
                }
            }

            if (direcciones != null)
            {
                foreach (DataRow d in direcciones.Rows)
                {
                    dto.Cliente.ListaDireccionEntregas.Add(new DireccionEntregaDto
                    {
                        Domicilio = GetStr(d, "Domicilio"),
                        CodigoPostal = GetStr(d, "CodigoPostal"),
                        Localidad = GetStr(d, "Localidad"),
                        ProvinciaId = GetStr(d, "ProvinciaId"),
                    });
                }
            }

            return dto;
        }

        public static PaginaCuentaCorrienteDto MapearPaginaCuentaCorriente(
            DataTable tabla,
            int pageSize)
        {
            if (tabla == null)
                return new PaginaCuentaCorrienteDto { HayMas = false };

            var hayMas = tabla.Rows.Count > pageSize;
            var items = new List<ItemCuentaCorrienteDto>();
            var limite = hayMas ? pageSize : tabla.Rows.Count;

            for (var i = 0; i < limite; i++)
            {
                var r = tabla.Rows[i];
                items.Add(new ItemCuentaCorrienteDto
                {
                    ClienteId = GetInt(r, "ClienteId"),
                    ManejoCuentaCorriente = GetStr(r, "ManejoCuentaCorriente"),
                });
            }

            var siguiente = hayMas && items.Count > 0 ? items[items.Count - 1].ClienteId : 0;

            return new PaginaCuentaCorrienteDto
            {
                Items = items,
                HayMas = hayMas,
                SiguienteCursor = siguiente,
            };
        }

        private static string GetStr(DataRow row, string col)
        {
            if (!row.Table.Columns.Contains(col))
                return string.Empty;
            var v = row[col];
            return v == null || v == DBNull.Value ? string.Empty : v.ToString();
        }

        private static int GetInt(DataRow row, string col)
        {
            if (!row.Table.Columns.Contains(col))
                return 0;
            var v = row[col];
            if (v == null || v == DBNull.Value)
                return 0;
            int result;
            return int.TryParse(v.ToString(), out result) ? result : 0;
        }
    }
}
