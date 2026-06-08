using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using InterfazHubSpot.Business.Integration;
using InterfazHubSpot.Business.Integration.Dtos;
using InterfazHubSpot.Mapping.Context;
using Mastersoft.Framework.Standard;

namespace InterfazHubSpot.Business.Managers
{
    public sealed class ClienteIntegracionManager
    {
        private readonly MSContext _ctx;

        public ClienteIntegracionManager(MSContext ctx)
        {
            if (ctx == null) throw new ArgumentNullException("ctx");
            _ctx = ctx;
        }

        public ClienteIntegracionDto ObtenerClienteParaHubSpot(int clienteId)
        {
            var cabecera = new DataTable();
            var direcciones = new DataTable();

            using (var db = new MSGestionContext(_ctx))
            {
                var conn = db.Database.Connection;
                var wasOpen = conn.State == ConnectionState.Open;
                if (!wasOpen) conn.Open();
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "EXEC dbo.InterfazHubSpot_Cliente_Obtener @ClienteId";
                        cmd.Parameters.Add(new SqlParameter("@ClienteId", clienteId));
                        using (var reader = cmd.ExecuteReader())
                        {
                            cabecera.Load(reader);
                            if (reader.NextResult())
                                direcciones.Load(reader);
                        }
                    }
                }
                finally
                {
                    if (!wasOpen) conn.Close();
                }
            }

            return ClienteIntegracionMapper.MapearCliente(cabecera, direcciones);
        }

        public List<ContactoDto> ObtenerContactosClienteParaHubSpot(int clienteId)
        {
            var contactos = new DataTable();

            using (var db = new MSGestionContext(_ctx))
            {
                var conn = db.Database.Connection;
                var wasOpen = conn.State == ConnectionState.Open;
                if (!wasOpen) conn.Open();
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "EXEC dbo.InterfazHubSpot_Clientes_Contactos_Obtener @ClienteId";
                        cmd.Parameters.Add(new SqlParameter("@ClienteId", clienteId));
                        using (var reader = cmd.ExecuteReader())
                        {
                            contactos.Load(reader);
                        }
                    }
                }
                finally
                {
                    if (!wasOpen) conn.Close();
                }
            }

            return ClienteIntegracionMapper.MapearContactos(contactos);
        }

        public PaginaCuentaCorrienteDto ObtenerPaginaCuentaCorriente(int cursor, int pageSize)
        {
            var tabla = new DataTable();

            using (var db = new MSGestionContext(_ctx))
            {
                var conn = db.Database.Connection;
                var wasOpen = conn.State == ConnectionState.Open;
                if (!wasOpen) conn.Open();
                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "EXEC dbo.InterfazHubSpot_CuentaCorriente_Pagina @Cursor, @PageSize";
                        cmd.Parameters.Add(new SqlParameter("@Cursor", cursor));
                        cmd.Parameters.Add(new SqlParameter("@PageSize", pageSize + 1));
                        using (var reader = cmd.ExecuteReader())
                        {
                            tabla.Load(reader);
                        }
                    }
                }
                finally
                {
                    if (!wasOpen) conn.Close();
                }
            }

            return ClienteIntegracionMapper.MapearPaginaCuentaCorriente(tabla, pageSize);
        }
    }
}
