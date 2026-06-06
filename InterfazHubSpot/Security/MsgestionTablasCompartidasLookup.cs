using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace InterfazHubSpot.Security
{
    /// <summary>
    /// Igual que <c>SpertaAPI.Services.TablasCompartidasLookup</c>: consulta <c>dbo.TablasCompartidas</c> en MSGestion.
    /// </summary>
    internal static class MsgestionTablasCompartidasLookup
    {
        private const string ConnectionStringNameMsgestion = "MSGestion";

        internal static bool GetMulti(string nomTabla)
        {
            if (string.IsNullOrWhiteSpace(nomTabla))
                return false;

            var cs = ConfigurationManager.ConnectionStrings[ConnectionStringNameMsgestion];
            if (cs == null || string.IsNullOrWhiteSpace(cs.ConnectionString))
                return false;

            const string sql = @"
                SELECT TOP 1 1
                FROM dbo.TablasCompartidas
                WHERE LOWER(LTRIM(RTRIM(nomtabla))) = LOWER(LTRIM(RTRIM(@NomTabla)));";

            try
            {
                using (var conn = new SqlConnection(cs.ConnectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@NomTabla", SqlDbType.NVarChar, 256).Value = nomTabla.Trim();
                    conn.Open();
                    var scalar = cmd.ExecuteScalar();
                    return scalar != null && scalar != DBNull.Value;
                }
            }
            catch (SqlException)
            {
                return false;
            }
        }
    }
}
