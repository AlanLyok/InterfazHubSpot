using InterfazHubSpot.Business.Managers;
using Xunit;

namespace InterfazHubSpot.IntegrationTests.Managers
{
    public sealed class EmailsManagerIntegrationTests
    {
        [Fact, Trait("Category", "Integration")]
        public void StoredProcedureAgregar_UsaMSEMails_Agregar()
        {
            Assert.Equal("dbo.MSEMails_Agregar", EmailsManager.StoredProcedureAgregar);
        }

        [Fact, Trait("Category", "Integration")]
        public void TruncarAsunto_RespetaLimite100()
        {
            var largo = new string('x', 120);
            var truncado = EmailsManager.TruncarAsunto(largo);
            Assert.Equal(100, truncado.Length);
        }

        [Fact, Trait("Category", "Integration")]
        public void TruncarAsunto_Null_DevuelveVacio()
        {
            Assert.Equal(string.Empty, EmailsManager.TruncarAsunto(null));
        }
    }
}
