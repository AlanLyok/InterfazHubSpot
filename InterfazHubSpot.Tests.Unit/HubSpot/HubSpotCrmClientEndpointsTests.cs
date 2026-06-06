using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using InterfazHubSpot.Business.HubSpot;
using Newtonsoft.Json.Linq;
using Xunit;

namespace InterfazHubSpot.Tests.Unit.HubSpot
{
    /// <summary>
    /// Cubre los endpoints de <see cref="HubSpotCrmClient"/> no probados en HubSpotInternalsTests
    /// (search company, search contact, upsert company POST/PATCH, upsert contact POST/PATCH,
    /// associate, batch update).  Handler fake captura requests para aserciones.
    /// </summary>
    public sealed class HubSpotCrmClientEndpointsTests : IDisposable
    {
        private readonly CapturingHandler _handler;
        private readonly HttpClient _http;
        private readonly HubSpotConfiguration _cfg;

        public HubSpotCrmClientEndpointsTests()
        {
            // Delay 0 para que los tests no esperen 120 ms cada uno
            ConfigurationManager.AppSettings.Set("HubSpot:DelayMillisecondsBetweenCalls", "0");
            _handler = new CapturingHandler();
            _http = new HttpClient(_handler);
            _cfg = new HubSpotConfiguration();
        }

        public void Dispose()
        {
            _http.Dispose();
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private HubSpotCrmClient CreateSut() => new HubSpotCrmClient(_cfg, _http);

        private static HttpResponseMessage JsonOk(string json)
            => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            };

        // ---------------------------------------------------------------
        // SearchCompanyIdByMastersoftIdAsync
        // ---------------------------------------------------------------

        [Fact]
        public async Task SearchCompanyIdByMastersoftIdAsync_TotalCero_DevuelveNull()
        {
            _handler.EnqueueResponse(JsonOk("{\"total\":0,\"results\":[]}"));
            var sut = CreateSut();

            var result = await sut.SearchCompanyIdByMastersoftIdAsync("99").ConfigureAwait(false);

            Assert.Null(result);
        }

        [Fact]
        public async Task SearchCompanyIdByMastersoftIdAsync_ConResultado_DevuelveIdYVerificaRequestCorrect()
        {
            const string mastersoftId = "42";
            const string expectedHubId = "HS-123";
            _handler.EnqueueResponse(JsonOk("{\"total\":1,\"results\":[{\"id\":\"" + expectedHubId + "\"}]}"));
            var sut = CreateSut();

            var result = await sut.SearchCompanyIdByMastersoftIdAsync(mastersoftId).ConfigureAwait(false);

            Assert.Equal(expectedHubId, result);

            var req = Assert.Single(_handler.Requests);
            Assert.Equal(HttpMethod.Post, req.Method);
            Assert.Contains("/crm/v3/objects/companies/search", req.RequestUri.AbsolutePath);
            Assert.Equal("Bearer", req.Headers.Authorization.Scheme);
            var bodyText = _handler.RequestBodies[0];
            // La propiedad puede ser "mastersoft_id_" (default) o lo configurado en App.config
            Assert.Contains(_cfg.PropertyMastersoftId, bodyText);
            Assert.Contains(mastersoftId, bodyText);
        }

        // ---------------------------------------------------------------
        // UpsertCompanyAsync — POST (sin id existente)
        // ---------------------------------------------------------------

        [Fact]
        public async Task UpsertCompanyAsync_SinIdExistente_HacePOSTACompanies()
        {
            _handler.EnqueueResponse(JsonOk("{\"id\":\"NEW-1\"}"));
            var sut = CreateSut();
            var props = new JObject { ["name"] = "Test Corp" };

            var body = await sut.UpsertCompanyAsync(null, props).ConfigureAwait(false);

            Assert.Contains("NEW-1", body);
            var req = Assert.Single(_handler.Requests);
            Assert.Equal(HttpMethod.Post, req.Method);
            Assert.EndsWith("/companies", req.RequestUri.AbsolutePath);
        }

        // ---------------------------------------------------------------
        // UpsertCompanyAsync — PATCH (con id existente)
        // ---------------------------------------------------------------

        [Fact]
        public async Task UpsertCompanyAsync_ConIdExistente_HacePATCHACompaniesSlashId()
        {
            const string existingId = "HS-XYZ";
            _handler.EnqueueResponse(JsonOk("{\"id\":\"" + existingId + "\"}"));
            var sut = CreateSut();
            var props = new JObject { ["name"] = "Updated Corp" };

            await sut.UpsertCompanyAsync(existingId, props).ConfigureAwait(false);

            var req = Assert.Single(_handler.Requests);
            Assert.Equal("PATCH", req.Method.Method);
            Assert.Contains(Uri.EscapeDataString(existingId), req.RequestUri.AbsolutePath);
        }

        // ---------------------------------------------------------------
        // SearchContactIdByEmailAsync — email vacío no hace HTTP
        // ---------------------------------------------------------------

        [Fact]
        public async Task SearchContactIdByEmailAsync_EmailVacio_DevuelveNull_SinHttp()
        {
            var sut = CreateSut();

            var result = await sut.SearchContactIdByEmailAsync(string.Empty).ConfigureAwait(false);

            Assert.Null(result);
            Assert.Empty(_handler.Requests);
        }

        [Fact]
        public async Task SearchContactIdByEmailAsync_EmailNull_DevuelveNull_SinHttp()
        {
            var sut = CreateSut();

            var result = await sut.SearchContactIdByEmailAsync(null).ConfigureAwait(false);

            Assert.Null(result);
            Assert.Empty(_handler.Requests);
        }

        [Fact]
        public async Task SearchContactIdByEmailAsync_TotalCero_DevuelveNull()
        {
            _handler.EnqueueResponse(JsonOk("{\"total\":0,\"results\":[]}"));
            var sut = CreateSut();

            var result = await sut.SearchContactIdByEmailAsync("test@example.com").ConfigureAwait(false);

            Assert.Null(result);
        }

        [Fact]
        public async Task SearchContactIdByEmailAsync_TrimmeaElEmail_EnBody()
        {
            _handler.EnqueueResponse(JsonOk("{\"total\":0,\"results\":[]}"));
            var sut = CreateSut();

            await sut.SearchContactIdByEmailAsync("  contact@test.com  ").ConfigureAwait(false);

            var req = Assert.Single(_handler.Requests);
            var bodyText = _handler.RequestBodies[0];
            Assert.Contains("contact@test.com", bodyText);
            Assert.DoesNotContain("  contact@test.com  ", bodyText);
        }

        // ---------------------------------------------------------------
        // UpsertContactAsync — POST (sin id)
        // ---------------------------------------------------------------

        [Fact]
        public async Task UpsertContactAsync_SinId_DevuelveTupleCreatedTrueYIdDeRespuesta()
        {
            _handler.EnqueueResponse(JsonOk("{\"id\":\"C-NEW\"}"));
            var sut = CreateSut();
            var props = new JObject { ["email"] = "a@b.com" };

            var result = await sut.UpsertContactAsync(null, props).ConfigureAwait(false);

            Assert.True(result.Item1);  // creado = true
            Assert.Equal("C-NEW", result.Item2);

            var req = Assert.Single(_handler.Requests);
            Assert.Equal(HttpMethod.Post, req.Method);
        }

        // ---------------------------------------------------------------
        // UpsertContactAsync — PATCH (con id)
        // ---------------------------------------------------------------

        [Fact]
        public async Task UpsertContactAsync_ConId_DevuelveTupleCreatedFalseYIdOriginal()
        {
            const string existingContactId = "C-EXIST";
            _handler.EnqueueResponse(JsonOk("{\"id\":\"" + existingContactId + "\"}"));
            var sut = CreateSut();
            var props = new JObject { ["email"] = "x@y.com" };

            var result = await sut.UpsertContactAsync(existingContactId, props).ConfigureAwait(false);

            Assert.False(result.Item1);  // creado = false
            Assert.Equal(existingContactId, result.Item2);

            var req = Assert.Single(_handler.Requests);
            Assert.Equal("PATCH", req.Method.Method);
        }

        // ---------------------------------------------------------------
        // AssociateContactToCompanyAsync
        // ---------------------------------------------------------------

        [Fact]
        public async Task AssociateContactToCompanyAsync_HttpOk_NoLanza_Y_UsaPUT()
        {
            _handler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.NoContent));
            var sut = CreateSut();

            await sut.AssociateContactToCompanyAsync("C-1", "COMP-1").ConfigureAwait(false);

            var req = Assert.Single(_handler.Requests);
            Assert.Equal(HttpMethod.Put, req.Method);
        }

        [Fact]
        public async Task AssociateContactToCompanyAsync_Http500_LanzaHttpRequestExceptionConStatusCode()
        {
            _handler.EnqueueResponse(new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new System.Net.Http.StringContent("server error"),
            });
            var sut = CreateSut();

            var ex = await Record.ExceptionAsync(() => sut.AssociateContactToCompanyAsync("C-1", "COMP-1")).ConfigureAwait(false);

            Assert.IsType<HttpRequestException>(ex);
            Assert.Contains("500", ex.Message);
        }

        // ---------------------------------------------------------------
        // BatchUpdateCompaniesManejoCcAsync
        // ---------------------------------------------------------------

        [Fact]
        public async Task BatchUpdateCompaniesManejoCcAsync_ListaVacia_NoHaceHttp()
        {
            var sut = CreateSut();

            await sut.BatchUpdateCompaniesManejoCcAsync(new List<Tuple<string, string>>()).ConfigureAwait(false);

            Assert.Empty(_handler.Requests);
        }

        [Fact]
        public async Task BatchUpdateCompaniesManejoCcAsync_LlamaBatchUpdate_ConTodasLasTuplas()
        {
            const int n = 3;
            _handler.EnqueueResponse(JsonOk("{\"status\":\"COMPLETE\"}"));
            var sut = CreateSut();
            var tuples = new List<Tuple<string, string>>();
            for (var i = 0; i < n; i++)
                tuples.Add(Tuple.Create("ID-" + i, "texto-" + i));

            await sut.BatchUpdateCompaniesManejoCcAsync(tuples).ConfigureAwait(false);

            var req = Assert.Single(_handler.Requests);
            Assert.Equal(HttpMethod.Post, req.Method);
            var bodyText = _handler.RequestBodies[0];
            var jo = JObject.Parse(bodyText);
            var inputs = (JArray)jo["inputs"];
            Assert.Equal(n, inputs.Count);
            foreach (var item in inputs)
            {
                Assert.NotNull(item["id"]);
                // La propiedad puede ser "manejo_cuenta_corriente" (default) o lo configurado en App.config
                var props = item["properties"] as JObject;
                Assert.NotNull(props);
                Assert.NotNull(props.Property(_cfg.PropertyManejoCuentaCorriente));
            }
        }

        // ---------------------------------------------------------------
        // Inner helper: CapturingHandler
        // ---------------------------------------------------------------

        private sealed class CapturingHandler : HttpMessageHandler
        {
            private readonly Queue<HttpResponseMessage> _responses = new Queue<HttpResponseMessage>();

            public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

            /// <summary>Cuerpos de requests capturados como strings (el contenido se desecha antes de que el test lo lea).</summary>
            public List<string> RequestBodies { get; } = new List<string>();

            public void EnqueueResponse(HttpResponseMessage response)
            {
                _responses.Enqueue(response);
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // Buffering del body antes de que HttpClient libere el contenido.
                string body = string.Empty;
                if (request.Content != null)
                    body = await request.Content.ReadAsStringAsync().ConfigureAwait(false);

                Requests.Add(request);
                RequestBodies.Add(body);

                if (_responses.Count == 0)
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new System.Net.Http.StringContent("{}", System.Text.Encoding.UTF8, "application/json"),
                    };

                return _responses.Dequeue();
            }
        }
    }
}
