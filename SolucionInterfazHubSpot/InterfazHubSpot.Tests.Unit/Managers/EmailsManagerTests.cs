using System;
using System.Configuration;
using InterfazHubSpot.Business.Managers;
using Mastersoft.Framework.Standard;
using Xunit;

namespace InterfazHubSpot.Tests.Unit.Managers
{
    [Collection("ConfigurationAppSettings")]
    public sealed class EmailsManagerTests
    {
        [Fact]
        public void StoredProcedureAgregar_UsaMSEMails_Agregar()
        {
            Assert.Equal("dbo.MSEMails_Agregar", EmailsManager.StoredProcedureAgregar);
        }

        [Fact, Trait("Category", "Security")]
        public void TruncarAsunto_RespetaLimite100()
        {
            var largo = new string('x', 120);
            var truncado = EmailsManager.TruncarAsunto(largo);
            Assert.Equal(100, truncado.Length);
            Assert.Equal(new string('x', 100), truncado);
        }

        [Fact, Trait("Category", "Security")]
        public void TruncarAsunto_Corto_NoModifica()
        {
            const string asunto = "[HubSpot 2A] Error ProcesoId=9";
            Assert.Equal(asunto, EmailsManager.TruncarAsunto(asunto));
        }

        [Fact, Trait("Category", "Security")]
        public void ResolverRemitente_EmailErrDeTienePrioridad()
        {
            var originalDe = ConfigurationManager.AppSettings["EmailDe"];
            try
            {
                ConfigurationManager.AppSettings["EmailDe"] = "fallback@test.com";
                Assert.Equal("remitente@test.com", EmailsManager.ResolverRemitente("remitente@test.com"));
            }
            finally
            {
                ConfigurationManager.AppSettings["EmailDe"] = originalDe;
            }
        }

        [Fact]
        public void ResolverRemitente_FallbackEmailDe()
        {
            var originalDe = ConfigurationManager.AppSettings["EmailDe"];
            try
            {
                ConfigurationManager.AppSettings["EmailDe"] = "fallback@test.com";
                Assert.Equal("fallback@test.com", EmailsManager.ResolverRemitente(null));
                Assert.Equal("fallback@test.com", EmailsManager.ResolverRemitente("   "));
            }
            finally
            {
                ConfigurationManager.AppSettings["EmailDe"] = originalDe;
            }
        }

        [Fact]
        public void GrabarEmailErrores_EmailErrParaVacio_NoEncola()
        {
            var original = ConfigurationManager.AppSettings["EmailErrPara"];
            try
            {
                ConfigurationManager.AppSettings["EmailErrPara"] = "   ";

                using (var mgr = new EmailsManager(new MSContext()))
                {
                    var ex = Record.Exception(() =>
                        mgr.GrabarEmailErrores("[HubSpot] test", "proceso-test", new[] { "detalle" }));

                    Assert.Null(ex);
                }
            }
            finally
            {
                ConfigurationManager.AppSettings["EmailErrPara"] = original;
            }
        }

        [Fact]
        public void ConstruirHtmlErrores_SinPlantilla_UsaFallbackHtml()
        {
            using (var mgr = new EmailsManager(new MSContext()))
            {
                var html = mgr.ConstruirHtmlErrores("proceso-test", new[] { "linea error" }, new DateTime(2026, 6, 9, 12, 0, 0));
                Assert.Contains("proceso-test", html);
                Assert.Contains("linea error", html);
                Assert.Contains("2026-06-09", html);
            }
        }

        [Fact]
        public void GrabarEmailErroresProcesamiento_SinEmailErrPara_NoEncola()
        {
            var original = ConfigurationManager.AppSettings["EmailErrPara"];
            try
            {
                ConfigurationManager.AppSettings["EmailErrPara"] = null;

                using (var mgr = new EmailsManager(new MSContext()))
                {
                    var ex = Record.Exception(() =>
                        mgr.GrabarEmailErroresProcesamiento("JobTest", new[] { "error" }));

                    Assert.Null(ex);
                }
            }
            finally
            {
                ConfigurationManager.AppSettings["EmailErrPara"] = original;
            }
        }
    }

    [CollectionDefinition("ConfigurationAppSettings", DisableParallelization = true)]
    public sealed class ConfigurationAppSettingsCollection
    {
    }
}
