using System.Configuration;
using InterfazHubSpot.Business.Managers;
using Xunit;

namespace InterfazHubSpot.IntegrationTests.Managers
{
    [Collection("ConfigurationAppSettings")]
    public sealed class EmailsManagerResolverIntegrationTests
    {
        [Fact, Trait("Category", "Integration")]
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

        [Fact, Trait("Category", "Integration")]
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
    }

    [CollectionDefinition("ConfigurationAppSettings", DisableParallelization = true)]
    public sealed class ConfigurationAppSettingsCollection
    {
    }
}
