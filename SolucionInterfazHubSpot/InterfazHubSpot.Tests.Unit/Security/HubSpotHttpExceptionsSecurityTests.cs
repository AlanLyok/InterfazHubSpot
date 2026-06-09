using InterfazHubSpot.Business.HubSpot;
using Xunit;

namespace InterfazHubSpot.Tests.Unit.Security
{
    public sealed class HubSpotHttpExceptionsSecurityTests
    {
        [Fact, Trait("Category", "Security")]
        public void HubSpotAuthException_PreservaMensajeInterno()
        {
            var ex = new HubSpotAuthException("401 Unauthorized");
            Assert.Contains("401", ex.Message);
        }

        [Fact, Trait("Category", "Security")]
        public void HubSpotHttpRetriesExhaustedException_PreservaCodigoHttp()
        {
            var ex = new HubSpotHttpRetriesExhaustedException("agotado", 429, "rate limit");
            Assert.Equal(429, ex.StatusCode);
            Assert.Equal("rate limit", ex.ResponseBody);
            Assert.Equal("agotado", ex.Message);
        }
    }
}
