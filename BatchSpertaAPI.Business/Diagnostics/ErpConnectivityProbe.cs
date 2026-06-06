using System;
using System.Data.Entity;
using System.Data.SqlClient;
using BatchSpertaAPI.Mapping.Context;
using Mastersoft.Framework.Standard;

namespace BatchSpertaAPI.Business.Diagnostics
{
    /// <summary>Verifica lectura contra la base donde viven cola y errores ERP (MSGestion).</summary>
    public static class ErpConnectivityProbe
    {
        /// <summary>Ejecuta <c>SELECT 1</c> y construye vista resumida de conexión (sin contraseña).</summary>
        public static object ProbarMsgestion(MSContext ctx, out bool ok)
        {
            ok = false;
            var servidor = string.Empty;
            var baseDatos = string.Empty;
            var usuario = string.Empty;

            try
            {
                using (var db = new MSGestionContext(ctx))
                {
                    var cs = db.Database.Connection.ConnectionString ?? string.Empty;
                    TryParseConnectionPieces(cs, out servidor, out baseDatos, out usuario);

                    ok = EjecutarSelectUno(db);
                }

                return new
                {
                    ConexionNombre = "MSGestion",
                    DataSource = servidor,
                    Catalogo = baseDatos,
                    UserId = usuario,
                    Select1Ok = ok,
                    ErrorMessage = string.Empty,
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    ConexionNombre = "MSGestion",
                    DataSource = servidor,
                    Catalogo = baseDatos,
                    UserId = usuario,
                    Select1Ok = false,
                    ErrorMessage = SanitizeSqlError(ex.Message),
                };
            }
        }

        private static bool EjecutarSelectUno(DbContext db)
        {
            var conn = db.Database.Connection;
            if (conn.State != System.Data.ConnectionState.Open)
            {
                conn.Open();
            }

            try
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT 1";
                    var scalar = cmd.ExecuteScalar();
                    return scalar != null && scalar != DBNull.Value;
                }
            }
            finally
            {
                conn.Close();
            }
        }

        private static void TryParseConnectionPieces(string connectionString, out string dataSource, out string initialCatalog, out string userId)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                dataSource = string.Empty;
                initialCatalog = string.Empty;
                userId = string.Empty;
                return;
            }

            try
            {
                var csb = new SqlConnectionStringBuilder(connectionString);
                dataSource = csb.DataSource ?? string.Empty;
                initialCatalog = csb.InitialCatalog ?? string.Empty;
                userId = csb.IntegratedSecurity ? "(IntegratedSecurity)" : (csb.UserID ?? string.Empty);
            }
            catch
            {
                dataSource = ExtractKeyFallback(connectionString, "Data Source=", "Server=");
                initialCatalog = ExtractKeyFallback(connectionString, "Initial Catalog=", "Database=");
                userId = ExtractKeyFallback(connectionString, "User ID=", "Uid=");
                if (string.IsNullOrEmpty(userId) && ContainsIntegratedSecurity(connectionString))
                {
                    userId = "(IntegratedSecurity)";
                }
            }
        }

        private static bool ContainsIntegratedSecurity(string s)
        {
            return !string.IsNullOrEmpty(s) && s.IndexOf("Integrated Security", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string ExtractKeyFallback(string s, string p1, string p2)
        {
            foreach (var p in new[] { p1, p2 })
            {
                var i = s.IndexOf(p, StringComparison.OrdinalIgnoreCase);
                if (i >= 0)
                {
                    i += p.Length;
                    var end = s.IndexOf(';', i);
                    return end >= 0 ? s.Substring(i, end - i).Trim() : s.Substring(i).Trim();
                }
            }

            return string.Empty;
        }

        private static string SanitizeSqlError(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return string.Empty;
            }

            return message.Length > 2000 ? message.Substring(0, 2000) + "..." : message;
        }
    }
}
