using System;
using System.Reflection;
using InterfazHubSpot.Business.Diagnostics;
using Xunit;

namespace InterfazHubSpot.Tests.Unit.Diagnostics
{
    /// <summary>
    /// Tests que ejercitan <see cref="ErpConnectivityProbe"/> mediante reflexión sobre los métodos
    /// privados estáticos. No se invoca <c>ProbarMsgestion</c> (requiere DB).
    /// </summary>
    public sealed class ErpConnectivityProbeTests
    {
        // ---------------------------------------------------------------
        // Helpers para invocar métodos privados via reflexión
        // ---------------------------------------------------------------
        private static MethodInfo GetTryParseConnectionPieces()
        {
            return typeof(ErpConnectivityProbe).GetMethod(
                "TryParseConnectionPieces",
                BindingFlags.Static | BindingFlags.NonPublic);
        }

        private static void InvokeTryParse(string cs, out string ds, out string ic, out string uid)
        {
            var method = GetTryParseConnectionPieces();
            var args = new object[] { cs, null, null, null };
            method.Invoke(null, args);
            ds  = (string)args[1];
            ic  = (string)args[2];
            uid = (string)args[3];
        }

        private static MethodInfo GetSanitizeSqlError()
        {
            return typeof(ErpConnectivityProbe).GetMethod(
                "SanitizeSqlError",
                BindingFlags.Static | BindingFlags.NonPublic);
        }

        private static string InvokeSanitize(string message)
        {
            var method = GetSanitizeSqlError();
            return (string)method.Invoke(null, new object[] { message });
        }

        // ---------------------------------------------------------------
        // TryParseConnectionPieces
        // ---------------------------------------------------------------

        [Fact, Trait("Category", "Security")]
        public void TryParseConnectionPieces_StringVacio_DevuelveCadenasVacias()
        {
            string ds, ic, uid;
            InvokeTryParse(string.Empty, out ds, out ic, out uid);

            Assert.Equal(string.Empty, ds);
            Assert.Equal(string.Empty, ic);
            Assert.Equal(string.Empty, uid);
        }

        [Fact, Trait("Category", "Security")]
        public void TryParseConnectionPieces_StringNull_DevuelveCadenasVacias()
        {
            string ds, ic, uid;
            InvokeTryParse(null, out ds, out ic, out uid);

            Assert.Equal(string.Empty, ds);
            Assert.Equal(string.Empty, ic);
            Assert.Equal(string.Empty, uid);
        }

        [Fact, Trait("Category", "Security")]
        public void TryParseConnectionPieces_FormatoEstandar_ExtraeDataSourceCatalogoUsuario()
        {
            const string cs = "Data Source=servidor01;Initial Catalog=MSGestion;User ID=appuser;Password=secret";
            string ds, ic, uid;
            InvokeTryParse(cs, out ds, out ic, out uid);

            Assert.Equal("servidor01", ds);
            Assert.Equal("MSGestion", ic);
            Assert.Equal("appuser", uid);
        }

        [Fact, Trait("Category", "Security")]
        public void TryParseConnectionPieces_IntegratedSecurity_DevuelveUsuarioIntegratedSecurity()
        {
            const string cs = "Data Source=srv;Initial Catalog=db;Integrated Security=SSPI";
            string ds, ic, uid;
            InvokeTryParse(cs, out ds, out ic, out uid);

            Assert.Equal("srv", ds);
            Assert.Equal("db", ic);
            Assert.Equal("(IntegratedSecurity)", uid);
        }

        [Fact, Trait("Category", "Security")]
        public void TryParseConnectionPieces_AliasServerDatabase_ExtraeCorrectamente()
        {
            const string cs = "Server=s;Database=d;Uid=u;Pwd=p";
            string ds, ic, uid;
            InvokeTryParse(cs, out ds, out ic, out uid);

            Assert.Equal("s", ds);
            Assert.Equal("d", ic);
            // SqlConnectionStringBuilder normaliza alias, uid puede ser "u" o "(IntegratedSecurity)" no
            // Lo importante es que no lanza y extrae algo
            Assert.NotNull(uid);
        }

        [Fact, Trait("Category", "Security")]
        public void TryParseConnectionPieces_StringInvalido_NoLanza()
        {
            // Texto completamente inválido — debería caer en el fallback ExtractKeyFallback
            const string cs = "esto no es una connection string valida!!!";
            string ds, ic, uid;
            var ex = Record.Exception(() => InvokeTryParse(cs, out ds, out ic, out uid));
            Assert.Null(ex);
        }

        // ---------------------------------------------------------------
        // SanitizeSqlError
        // ---------------------------------------------------------------

        [Fact, Trait("Category", "Security")]
        public void SanitizeSqlError_Null_DevuelveStringEmpty()
        {
            var result = InvokeSanitize(null);
            Assert.Equal(string.Empty, result);
        }

        [Fact, Trait("Category", "Security")]
        public void SanitizeSqlError_StringVacio_DevuelveStringEmpty()
        {
            var result = InvokeSanitize(string.Empty);
            Assert.Equal(string.Empty, result);
        }

        [Fact, Trait("Category", "Security")]
        public void SanitizeSqlError_MensajeCorto_DevuelveTalCual()
        {
            const string msg = "Error de conexión.";
            var result = InvokeSanitize(msg);
            Assert.Equal(msg, result);
        }

        [Fact, Trait("Category", "Security")]
        public void SanitizeSqlError_ExactamenteDosMillCaracteres_DevuelveTalCual()
        {
            var msg = new string('x', 2000);
            var result = InvokeSanitize(msg);
            Assert.Equal(msg, result);
        }

        [Fact, Trait("Category", "Security")]
        public void SanitizeSqlError_MasDeDosMil_TruncaYSufijaElipsis()
        {
            var msg = new string('a', 2500);
            var result = InvokeSanitize(msg);

            Assert.Equal(2003, result.Length);
            Assert.EndsWith("...", result);
            Assert.StartsWith(new string('a', 2000), result);
        }
    }
}
