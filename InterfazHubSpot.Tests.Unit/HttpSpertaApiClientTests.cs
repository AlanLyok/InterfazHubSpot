using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using InterfazHubSpot.Business.Integration;
using Xunit;

namespace InterfazHubSpot.Tests.Unit
{
    public sealed class HttpSpertaApiClientTests
    {
        [Fact]
        public void BuildUrl_concatena_sin_doble_barra()
        {
            var u = HttpSpertaApiClient.BuildUrl("http://localhost:1", "api/v100/health");
            Assert.Equal("http://localhost:1/api/v100/health", u);

            u = HttpSpertaApiClient.BuildUrl("http://localhost:1/", "/api/v100/health");
            Assert.Equal("http://localhost:1/api/v100/health", u);
        }

        [Fact]
        public void BuildUrl_base_vacia_lanza_InvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => HttpSpertaApiClient.BuildUrl(null, "api/v100/health"));
            Assert.Throws<InvalidOperationException>(() => HttpSpertaApiClient.BuildUrl("   ", "api/v100/health"));
        }

        [Fact]
        public void BuildUrl_relative_vacia_o_solo_blancos_arg()
        {
            Assert.Throws<ArgumentException>(() => HttpSpertaApiClient.BuildUrl("http://a/", null));
            Assert.Throws<ArgumentException>(() => HttpSpertaApiClient.BuildUrl("http://a/", "   "));
        }

        [Fact]
        public void BuildUrl_url_absoluta_invalida_lanza()
        {
            Assert.Throws<InvalidOperationException>(() =>
                HttpSpertaApiClient.BuildUrl("http://%%%invalid", "api/v100/health"));
        }

        [Fact]
        public async Task PostReciboAsync_sin_ruta_configurada_lanza_antes_de_http()
        {
            var handler = new ThrowsIfInvokedHandler();
            var http = new HttpClient(handler);
            var client = new HttpSpertaApiClient(http);

            await Assert.ThrowsAsync<InvalidOperationException>(() => client.PostReciboAsync("{}")).ConfigureAwait(false);
        }

        [Fact]
        public async Task GetHealthAsync_según_cliente_HTTP_inyectado()
        {
            var handler = new OkTextHandler("\"ok\"", request =>
            {
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.Equal(new Uri("http://localhost:59999/api/v100/health"), request.RequestUri);
            });
            var http = new HttpClient(handler);
            var client = new HttpSpertaApiClient(http);
            var body = await client.GetHealthAsync().ConfigureAwait(false);
            Assert.Equal("\"ok\"", body);
        }

        [Fact]
        public void Listener_OAuth_Registrar_limpiar_sin_lanza()
        {
            try
            {
                HttpSpertaApiClient.SetOAuthDiagnosticsListener(delegate { });
                HttpSpertaApiClient.ClearOAuthDiagnosticsListener();
            }
            finally
            {
                HttpSpertaApiClient.ClearOAuthDiagnosticsListener();
            }
        }

        private sealed class ThrowsIfInvokedHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("No se esperaba tráfico HTTP.");
            }
        }

        private sealed class OkTextHandler : HttpMessageHandler
        {
            private readonly Action<HttpRequestMessage> _assertRequest;

            private readonly string _text;

            public OkTextHandler(string text, Action<HttpRequestMessage> assertRequest = null)
            {
                _text = text;
                _assertRequest = assertRequest;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                _assertRequest?.Invoke(request);
                var msg = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_text, System.Text.Encoding.UTF8, "application/json"),
                };
                return Task.FromResult(msg);
            }
        }
    }
}
