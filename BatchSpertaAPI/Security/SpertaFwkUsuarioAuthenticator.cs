using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Mastersoft.Framework.Business;

namespace BatchSpertaAPI.Security
{
    /// <summary>
    /// Autentica usuarios contra dbo.Usuarios de la base MSFwk (misma lógica que SpertaAPI).
    /// </summary>
    public static class SpertaFwkUsuarioAuthenticator
    {
        private const string ConnectionStringNameMsfwk = "MSFwk";

        private const string ConnectionStringNameMsgestion = "MSGestion";

        private const string NomTablaMaestroUsuarios = "Usuarios";

        private const string HabilitadoSi = "S";

        public static bool TryAuthenticate(
            string usuario,
            string password,
            string companyIdHeader,
            out SpertaFwkUsuarioAuthResult result)
        {
            SpertaFwkUsuarioAuthFailureKind fk;
            return TryAuthenticate(usuario, password, companyIdHeader, out result, out fk);
        }

        public static bool TryAuthenticate(
            string usuario,
            string password,
            string companyIdHeader,
            out SpertaFwkUsuarioAuthResult result,
            out SpertaFwkUsuarioAuthFailureKind failureKind)
        {
            result = null;
            failureKind = SpertaFwkUsuarioAuthFailureKind.None;

            int requestedEmpresa;
            if (!TryParseCompanyIdHeader(companyIdHeader, out requestedEmpresa))
            {
                failureKind = SpertaFwkUsuarioAuthFailureKind.CompanyIdRequiredOrInvalid;
                return false;
            }

            var csFwk = ConfigurationManager.ConnectionStrings[ConnectionStringNameMsfwk];
            if (csFwk == null || string.IsNullOrWhiteSpace(csFwk.ConnectionString))
            {
                failureKind = SpertaFwkUsuarioAuthFailureKind.ConnectionMisconfigured;
                return false;
            }

            var csMsg = ConfigurationManager.ConnectionStrings[ConnectionStringNameMsgestion];
            var msgestionCs = csMsg != null ? csMsg.ConnectionString : null;

            try
            {
                UsuarioRow row;
                if (!TryReadUsuario(csFwk.ConnectionString, usuario, out row))
                {
                    failureKind = SpertaFwkUsuarioAuthFailureKind.InvalidCredentialsOrUserDisabled;
                    return false;
                }

                if (!string.Equals(row.Habilitado.Trim(), HabilitadoSi, StringComparison.OrdinalIgnoreCase))
                {
                    failureKind = SpertaFwkUsuarioAuthFailureKind.InvalidCredentialsOrUserDisabled;
                    return false;
                }

                var seguridad = new Seguridad();
                if (!seguridad.ValidarPassword(row.Clave, password))
                {
                    failureKind = SpertaFwkUsuarioAuthFailureKind.InvalidCredentialsOrUserDisabled;
                    return false;
                }

                if (string.IsNullOrWhiteSpace(msgestionCs))
                {
                    failureKind = SpertaFwkUsuarioAuthFailureKind.ConnectionMisconfigured;
                    return false;
                }

                if (!EmpresaExistsEnMsgestion(msgestionCs, requestedEmpresa))
                {
                    failureKind = SpertaFwkUsuarioAuthFailureKind.CompanyNotFoundInEmpresas;
                    return false;
                }

                if (row.EmpresaId > 0)
                {
                    if (requestedEmpresa != row.EmpresaId)
                    {
                        failureKind = SpertaFwkUsuarioAuthFailureKind.CompanyMismatchFixedUser;
                        return false;
                    }

                    result = BuildResult(row, requestedEmpresa);
                    return true;
                }

                if (!MsgestionTablasCompartidasLookup.GetMulti(NomTablaMaestroUsuarios))
                {
                    failureKind = SpertaFwkUsuarioAuthFailureKind.SharedUserTablasCompartidasMismatch;
                    return false;
                }

                // PerfilId NULL = acceso total (sin SPE). PerfilId > 0: SPE solo limita si hay filas para ese perfil.
                if (row.PerfilId.HasValue && row.PerfilId.Value > 0)
                {
                    HashSet<int> restriccionesSpe;
                    if (!TryLoadSeguridadPorEmpresa(csFwk.ConnectionString, row.PerfilId.Value, out restriccionesSpe))
                    {
                        failureKind = SpertaFwkUsuarioAuthFailureKind.UnexpectedError;
                        return false;
                    }

                    if (restriccionesSpe.Count > 0 && !restriccionesSpe.Contains(requestedEmpresa))
                    {
                        failureKind = SpertaFwkUsuarioAuthFailureKind.CompanyNotInSeguridadPorEmpresa;
                        return false;
                    }
                }

                result = BuildResult(row, requestedEmpresa);
                return true;
            }
            catch (SqlException)
            {
                failureKind = SpertaFwkUsuarioAuthFailureKind.UnexpectedError;
                return false;
            }
            catch
            {
                failureKind = SpertaFwkUsuarioAuthFailureKind.UnexpectedError;
                return false;
            }
        }

        internal static bool TryParseCompanyIdHeader(string companyIdHeader, out int requestedEmpresa)
        {
            requestedEmpresa = 0;
            if (string.IsNullOrWhiteSpace(companyIdHeader))
                return false;
            return int.TryParse(companyIdHeader.Trim(), out requestedEmpresa) && requestedEmpresa > 0;
        }

        private static SpertaFwkUsuarioAuthResult BuildResult(UsuarioRow row, int empresaEfectiva)
        {
            return new SpertaFwkUsuarioAuthResult
            {
                UsuarioId = row.UsuarioId,
                Usuario = row.Usuario,
                EmpresaId = empresaEfectiva,
                PerfilId = row.PerfilId ?? 0,
            };
        }

        private static bool EmpresaExistsEnMsgestion(string connectionString, int codEmpre)
        {
            const string sql = @"SELECT TOP 1 1 FROM dbo.Empresas WHERE CodEmpre = @CodEmpre";

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@CodEmpre", SqlDbType.Int).Value = codEmpre;
                conn.Open();
                var scalar = cmd.ExecuteScalar();
                return scalar != null && scalar != DBNull.Value;
            }
        }

        private static bool TryLoadSeguridadPorEmpresa(string msfwkConnectionString, int perfilId, out HashSet<int> empresaIds)
        {
            empresaIds = new HashSet<int>();
            const string sql = @"
                SELECT DISTINCT EmpresaId
                FROM dbo.SeguridadPorEmpresa
                WHERE PerfilId = @PerfilId";

            using (var conn = new SqlConnection(msfwkConnectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@PerfilId", SqlDbType.Int).Value = perfilId;
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                            empresaIds.Add(reader.GetInt32(0));
                    }
                }
            }

            return true;
        }

        private static bool TryReadUsuario(string connectionString, string usuario, out UsuarioRow row)
        {
            row = null;

            const string sql = @"
                SELECT TOP 1
                    UsuarioId,
                    EmpresaId,
                    Usuario,
                    Clave,
                    PerfilId,
                    Habilitado
                FROM dbo.Usuarios
                WHERE Usuario = @Usuario
                ORDER BY UsuarioId";

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@Usuario", SqlDbType.NVarChar, 50).Value = usuario;

                    using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (!reader.Read())
                            return false;

                        int? perfilId = reader.IsDBNull(reader.GetOrdinal("PerfilId"))
                            ? (int?)null
                            : reader.GetInt32(reader.GetOrdinal("PerfilId"));

                        row = new UsuarioRow
                        {
                            UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId")),
                            EmpresaId = reader.GetInt32(reader.GetOrdinal("EmpresaId")),
                            Usuario = reader.GetString(reader.GetOrdinal("Usuario")),
                            Clave = reader.GetString(reader.GetOrdinal("Clave")),
                            PerfilId = perfilId,
                            Habilitado = reader.IsDBNull(reader.GetOrdinal("Habilitado"))
                                             ? string.Empty
                                             : reader.GetString(reader.GetOrdinal("Habilitado")),
                        };

                        return true;
                    }
                }
            }
        }

        private sealed class UsuarioRow
        {
            public int UsuarioId { get; set; }

            public int EmpresaId { get; set; }

            public string Usuario { get; set; }

            public string Clave { get; set; }

            public int? PerfilId { get; set; }

            public string Habilitado { get; set; }
        }
    }

    public sealed class SpertaFwkUsuarioAuthResult
    {
        public int UsuarioId { get; set; }

        public string Usuario { get; set; }

        public int EmpresaId { get; set; }

        public int PerfilId { get; set; }
    }
}
