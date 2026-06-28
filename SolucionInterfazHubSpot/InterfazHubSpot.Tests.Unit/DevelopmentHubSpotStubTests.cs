using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using InterfazHubSpot.Business.HubSpot;
using Newtonsoft.Json.Linq;
using Xunit;

namespace InterfazHubSpot.Tests.Unit
{
    public sealed class DevelopmentHubSpotStubTests
    {
        [Fact, Trait("Category", "Security")]
        public async Task Stub_POST_companies_search_devuelve_vacio_parseable()
        {
            using (var invoker = new HttpMessageInvoker(new DevelopmentHubSpotStubHandler()))
            {
                var req = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://api.hubapi.com/crm/v3/objects/companies/search");
                req.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

                var res = await invoker.SendAsync(req, CancellationToken.None).ConfigureAwait(false);
                Assert.Equal(HttpStatusCode.OK, res.StatusCode);
                var text = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                Assert.Contains("\"total\":0", text);
            }
        }

        [Fact]
        public async Task Stub_PUT_asociacion_devuelve_204_sin_cuerpo()
        {
            using (var invoker = new HttpMessageInvoker(new DevelopmentHubSpotStubHandler()))
            {
                var req = new HttpRequestMessage(
                    HttpMethod.Put,
                    "https://api.hubapi.com/crm/v3/objects/contacts/aaa/associations/companies/bbb/contact_to_company");
                req.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

                var res = await invoker.SendAsync(req, CancellationToken.None).ConfigureAwait(false);
                Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
            }
        }

        [Fact]
        public async Task HubSpotCrmClient_con_stub_crea_company_y_entrega_id()
        {
            var cfg = new HubSpotConfiguration();

            using (var http = new HttpClient(new DevelopmentHubSpotStubHandler())
            {
                Timeout = System.TimeSpan.FromSeconds(60),
            })
            {
                var sut = new HubSpotCrmClient(cfg, http);
                var idSearch = await sut.SearchCompanyIdByCuitCuilUnicaAsync("30999999990").ConfigureAwait(false);
                Assert.Null(idSearch);

                var props = new JObject { ["name"] = "Unit" };

                var body = await sut.UpsertCompanyAsync(null, props).ConfigureAwait(false);
                Assert.Contains("\"id\"", body, System.StringComparison.Ordinal);
                Assert.Contains("mock-comp-", body, System.StringComparison.Ordinal);
            }
        }
    }
}
