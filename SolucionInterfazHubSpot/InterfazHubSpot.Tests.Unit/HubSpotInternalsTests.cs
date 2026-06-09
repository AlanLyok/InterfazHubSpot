using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using InterfazHubSpot.Business.HubSpot;
using Xunit;

namespace InterfazHubSpot.Tests.Unit
{
    public sealed class HubSpotInternalsTests
    {
        [Fact]
        public void HubSpotConfiguration_ValidarToken_con_token_en_App_config_no_lanza()
        {
            var cfg = new HubSpotConfiguration();
            cfg.ValidarToken();
            Assert.True(cfg.TienePrivateAppToken);
        }

        [Fact]
        public async Task HubSpotCrmClient_SearchCompany_post_y_Bearer_correctos()
        {
            var cfg = new HubSpotConfiguration();
            HttpRequestMessage captured = null;

            var handler = new DelegateHandler((request, ct) =>
            {
                captured = request;
                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("{\"total\":0,\"results\":[]}"),
                    });
            });

            using (var http = new HttpClient(handler))
            {
                var sut = new HubSpotCrmClient(cfg, http);
                var id = await sut.SearchCompanyIdByMastersoftIdAsync("42").ConfigureAwait(false);
                Assert.Null(id);
                Assert.NotNull(captured?.Headers.Authorization);
                Assert.Equal("Bearer", captured.Headers.Authorization.Scheme);
                Assert.Equal("fake-token-unit-test", captured.Headers.Authorization.Parameter);
            }
        }

        [Fact]
        public async Task HubSpotCrmClient_BatchUpdateCompaniesManejoCcAsync_lista_vacia_o_null_no_http()
        {
            var cfg = new HubSpotConfiguration();
            var handler = new AssertNotInvokedHandler();
            using (var http = new HttpClient(handler))
            {
                var sut = new HubSpotCrmClient(cfg, http);
                await sut.BatchUpdateCompaniesManejoCcAsync(null).ConfigureAwait(false);
                await sut.BatchUpdateCompaniesManejoCcAsync(new Tuple<string, string>[0]).ConfigureAwait(false);
            }
        }

        private sealed class DelegateHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _resolver;

            public DelegateHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> resolver)
            {
                _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return _resolver(request, cancellationToken);
            }
        }

        private sealed class AssertNotInvokedHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("No se esperaba HTTP.");
            }
        }
    }
}
