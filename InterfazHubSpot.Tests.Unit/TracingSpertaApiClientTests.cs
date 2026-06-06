using System;
using System.Net.Http;
using System.Threading.Tasks;
using BatchSpertaAPI.Business.Diagnostics;
using BatchSpertaAPI.Business.Integration;
using Xunit;

namespace BatchSpertaAPI.Tests.Unit
{
    public sealed class TracingSpertaApiClientTests
    {
        [Fact]
        public async Task GetIntegracionesClienteAsync_registra_paso_y_json_envelope()
        {
            const string Json = "{\"HayError\":false,\"Datos\":null}";
            var inner = new FakeSpertaInner(integracionesClienteBody: Json, integracionesCcBody: "{}");
            var collector = new ProcesoPasoCollector();
            var tracing = new TracingSpertaApiClient(inner, collector);

            var json = await tracing.GetIntegracionesClienteAsync(123).ConfigureAwait(false);
            Assert.Equal(Json, json);

            var pasos = collector.ObtenerPasos();
            Assert.Contains(pasos, p => p.Codigo == "spertaapi.get.integraciones.cliente.solicitud");
            Assert.Contains(pasos, p => p.Codigo == "spertaapi.get.integraciones.cliente");
        }

        [Fact]
        public async Task GetIntegracionesClienteAsync_error_interno_registra_paso_critico_y_relanza()
        {
            var inner = new FakeSpertaThrows(new InvalidOperationException("falló red"));
            var collector = new ProcesoPasoCollector();
            var tracing = new TracingSpertaApiClient(inner, collector);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                tracing.GetIntegracionesClienteAsync(1)).ConfigureAwait(false);

            var pasos = collector.ObtenerPasos();
            Assert.Contains(pasos, p => p.Codigo == "spertaapi.get.integraciones.cliente.error");
        }

        [Fact]
        public async Task GetIntegracionesHubSpotCuentaCorrienteAsync_registra_paso_exito()
        {
            var inner = new FakeSpertaInner("{}", "{\"HayError\":null}");
            var collector = new ProcesoPasoCollector();
            var tracing = new TracingSpertaApiClient(inner, collector);
            await tracing.GetIntegracionesHubSpotCuentaCorrienteAsync(10, 20).ConfigureAwait(false);
            Assert.Contains(collector.ObtenerPasos(), p => p.Codigo == "spertaapi.get.hubspot.cuenta_corriente");
        }

        private sealed class FakeSpertaInner : ISpertaApiClient
        {
            private readonly string _cc;

            private readonly string _integrations;

            public FakeSpertaInner(string integracionesClienteBody, string integracionesCcBody)
            {
                _integrations = integracionesClienteBody;
                _cc = integracionesCcBody;
            }

            public HttpClient Http => new HttpClient();

            public Task<string> GetHealthAsync() => Task.FromResult("{}");

            public Task<string> PostClientesGrabarAsync(string jsonBody) => Task.FromResult("{}");

            public Task<string> PostPedidosGrabarAsync(string jsonBody) => Task.FromResult("{}");

            public Task<string> PostReciboAsync(string jsonBody) => Task.FromResult("{}");

            public Task<string> GetIntegracionesClienteAsync(int clienteId)
            {
                Assert.True(clienteId > 0);
                return Task.FromResult(_integrations);
            }

            public Task<string> GetIntegracionesHubSpotCuentaCorrienteAsync(int cursor, int pageSize)
            {
                return Task.FromResult(_cc);
            }
        }

        private sealed class FakeSpertaThrows : ISpertaApiClient
        {
            private readonly Exception _ex;

            public FakeSpertaThrows(Exception ex)
            {
                _ex = ex;
            }

            public HttpClient Http => new HttpClient();

            public Task<string> GetHealthAsync() => Throw();

            public Task<string> PostClientesGrabarAsync(string jsonBody) => Throw();

            public Task<string> PostPedidosGrabarAsync(string jsonBody) => Throw();

            public Task<string> PostReciboAsync(string jsonBody) => Throw();

            public Task<string> GetIntegracionesClienteAsync(int clienteId)
            {
                throw _ex;
            }

            public Task<string> GetIntegracionesHubSpotCuentaCorrienteAsync(int cursor, int pageSize) => Throw();

            private Task<string> Throw()
            {
                throw new NotSupportedException();
            }
        }
    }
}
