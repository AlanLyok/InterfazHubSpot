using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using InterfazHubSpot.Tests.Unit.Managers;
using InterfazHubSpot.Business.HubSpot;
using InterfazHubSpot.Business.Managers;
using Mastersoft.Framework.Standard;
using Newtonsoft.Json.Linq;
using Xunit;

namespace InterfazHubSpot.Tests.Unit.HubSpot
{
    [Collection("ConfigurationAppSettings")]
    public sealed class HubSpotHttpRetryTests : IDisposable
    {
        private readonly CapturingHandler _handler;
        private readonly HttpClient _http;
        private readonly HubSpotConfiguration _cfg;

        public HubSpotHttpRetryTests()
        {
            _handler = new CapturingHandler();
            _http = new HttpClient(_handler);
            _cfg = new HubSpotConfiguration();
        }

        public void Dispose()
        {
            _http.Dispose();
        }

        private HubSpotCrmClient CreateSut() => new HubSpotCrmClient(_cfg, _http);

        [Fact]
        public async Task PostJson_401_LanzaHubSpotAuthException_SinReintentos()
        {
            _handler.EnqueueResponse(Status(HttpStatusCode.Unauthorized, "invalid token"));
            var sut = CreateSut();
            var reporter = new RecordingReporter();

            var ex = await Assert.ThrowsAsync<HubSpotAuthException>(() =>
                sut.UpsertCompanyAsync(null, new JObject { ["name"] = "X" }, 99L, reporter)).ConfigureAwait(false);

            Assert.Contains("401", ex.Message);
            Assert.Single(_handler.Requests);
            Assert.Equal(0, reporter.Calls);
        }

        [Fact]
        public async Task PostJson_429_ReintentaEIncrementaIntentos_HastaAgotar()
        {
            _handler.EnqueueResponse(Status((HttpStatusCode)429, "rate limit"));
            _handler.EnqueueResponse(Status((HttpStatusCode)429, "rate limit"));
            _handler.EnqueueResponse(Status((HttpStatusCode)429, "rate limit"));
            var sut = CreateSut();
            var reporter = new RecordingReporter();

            var ex = await Assert.ThrowsAsync<HubSpotHttpRetriesExhaustedException>(() =>
                sut.UpsertCompanyAsync(null, new JObject { ["name"] = "X" }, 42L, reporter)).ConfigureAwait(false);

            Assert.Contains("429", ex.Message);
            Assert.Equal(3, _handler.Requests.Count);
            Assert.Equal(2, reporter.Calls);
            Assert.Equal(42L, reporter.LastProcesoId);
        }

        [Fact]
        public async Task PostJson_429_LuegoOk_RetornaBody()
        {
            _handler.EnqueueResponse(Status((HttpStatusCode)429, "rate limit"));
            _handler.EnqueueResponse(Ok("{\"id\":\"OK-1\"}"));
            var sut = CreateSut();
            var reporter = new RecordingReporter();

            var body = await sut.UpsertCompanyAsync(null, new JObject { ["name"] = "X" }, 7L, reporter).ConfigureAwait(false);

            Assert.Contains("OK-1", body);
            Assert.Equal(2, _handler.Requests.Count);
            Assert.Equal(1, reporter.Calls);
        }

        private static HttpResponseMessage Ok(string json)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            };
        }

        private static HttpResponseMessage Status(HttpStatusCode code, string body)
        {
            return new HttpResponseMessage(code)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "text/plain"),
            };
        }

        private sealed class RecordingReporter : IHubSpotColaIntentosReporter
        {
            public int Calls { get; private set; }

            public long? LastProcesoId { get; private set; }

            public void OnHttpRetryFailed(long? procesoId)
            {
                Calls++;
                LastProcesoId = procesoId;
            }
        }

        private sealed class CapturingHandler : HttpMessageHandler
        {
            public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

            private readonly Queue<HttpResponseMessage> _responses = new Queue<HttpResponseMessage>();

            public void EnqueueResponse(HttpResponseMessage response) => _responses.Enqueue(response);

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Requests.Add(request);
                if (_responses.Count == 0)
                    return Task.FromResult(Ok("{}"));

                return Task.FromResult(_responses.Dequeue());
            }
        }
    }

    public sealed class HubSpotColaIntentosReporterTests
    {
        [Fact]
        public void OnHttpRetryFailed_ConProcesoIdNull_NoLanza()
        {
            var reporter = new HubSpotColaIntentosReporter(new ProcesosSpertaHubSpotManager(new MSContext()));
            reporter.OnHttpRetryFailed(null);
        }
    }
}
