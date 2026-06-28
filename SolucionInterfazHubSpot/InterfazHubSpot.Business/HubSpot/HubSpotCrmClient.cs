using System;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace InterfazHubSpot.Business.HubSpot
{
    internal sealed class HubSpotConfiguration
    {
        internal HubSpotConfiguration()
        {
            BaseUrl = (ConfigurationManager.AppSettings["HubSpot:BaseUrl"] ?? "https://api.hubapi.com").TrimEnd('/');
            PrivateAppToken = ConfigurationManager.AppSettings["HubSpot:PrivateAppToken"] ?? string.Empty;
            PropertyMastersoftId = ConfigurationManager.AppSettings["HubSpot:PropertyMastersoftId"] ?? "mastersoft_id_";
            PropertyCuitCuilUnica = ConfigurationManager.AppSettings["HubSpot:PropertyCuitCuilUnica"] ?? "cuitcuil_unica";
            PropertyManejoCuentaCorriente =
                ConfigurationManager.AppSettings["HubSpot:PropertyManejoCuentaCorriente"] ?? "manejo_cuenta_corriente";
            DelayMsBetweenCalls = ParseInt(ConfigurationManager.AppSettings["HubSpot:DelayMillisecondsBetweenCalls"], 120);
            CuentaCorrientePageSize = ParseInt(ConfigurationManager.AppSettings["HubSpot:CuentaCorrientePageSize"], 500);
            MaxHttpRetries = ParseInt(ConfigurationManager.AppSettings["HubSpot:MaxHttpRetries"], 3);
            HttpRetryBackoffMilliseconds = ParseInt(ConfigurationManager.AppSettings["HubSpot:HttpRetryBackoffMilliseconds"], 1000);
            UseDevelopmentMock = ParseBoolTrue(ConfigurationManager.AppSettings["HubSpot:UseDevelopmentMock"], false);
        }

        internal string BaseUrl { get; }

        internal string PrivateAppToken { get; }

        internal string PropertyMastersoftId { get; }

        internal string PropertyCuitCuilUnica { get; }

        internal string PropertyManejoCuentaCorriente { get; }

        internal int DelayMsBetweenCalls { get; }

        internal int CuentaCorrientePageSize { get; }

        internal int MaxHttpRetries { get; }

        internal int HttpRetryBackoffMilliseconds { get; }

        /// <summary>Si está activo no se usa el token contra api.hubapi.com; cliente HTTP puede ser stub (solo desarrollo).</summary>
        internal bool UseDevelopmentMock { get; }

        internal bool TienePrivateAppToken => !string.IsNullOrWhiteSpace(PrivateAppToken);

        internal void ValidarToken()
        {
            if (UseDevelopmentMock)
                return;
            if (string.IsNullOrWhiteSpace(PrivateAppToken))
                throw new InvalidOperationException("Configure HubSpot:PrivateAppToken en Web.config / App.config del batch.");
        }

        private static bool ParseBoolTrue(string s, bool fallback)
        {
            if (string.IsNullOrWhiteSpace(s))
                return fallback;
            s = s.Trim();
            return string.Equals(s, "true", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(s, "1", StringComparison.Ordinal)
                   || string.Equals(s, "yes", StringComparison.OrdinalIgnoreCase);
        }

        private static int ParseInt(string s, int fallback)
        {
            int v;
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out v) ? v : fallback;
        }
    }

    /// <summary>Cliente mínimo CRM v3 (companies / contacts / batch).</summary>
    internal sealed class HubSpotCrmClient
    {
        static HubSpotCrmClient()
        {
            // MVC lo hace en Global.asax; el servicio Windows batch no — HubSpot exige TLS 1.2+.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        private static readonly HttpClient SharedHttp = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };

        private readonly HubSpotConfiguration _cfg;

        private readonly HttpClient _http;

        internal HubSpotCrmClient(HubSpotConfiguration cfg)
            : this(cfg, null)
        {
        }

        /// <summary>Permite tests con <see cref="HttpMessageHandler"/> mockeado.</summary>
        internal HubSpotCrmClient(HubSpotConfiguration cfg, HttpClient httpClient)
        {
            _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
            _http = httpClient ?? SharedHttp;
        }

        internal async Task<string> SearchCompanyIdByCuitCuilUnicaAsync(string cuitCuilUnica)
        {
            return await SearchCompanyIdByCuitCuilUnicaAsync(cuitCuilUnica, null, null).ConfigureAwait(false);
        }

        internal async Task<string> SearchCompanyIdByCuitCuilUnicaAsync(
            string cuitCuilUnica,
            long? procesoId,
            IHubSpotColaIntentosReporter reporter)
        {
            await ThrottleAsync().ConfigureAwait(false);
            var url = _cfg.BaseUrl + "/crm/v3/objects/companies/search";
            var bodyObj = new JObject
            {
                ["filterGroups"] = new JArray
                {
                    new JObject
                    {
                        ["filters"] = new JArray
                        {
                            new JObject
                            {
                                ["propertyName"] = _cfg.PropertyCuitCuilUnica,
                                ["operator"] = "EQ",
                                ["value"] = cuitCuilUnica ?? string.Empty,
                            },
                        },
                    },
                },
                ["properties"] = new JArray(_cfg.PropertyCuitCuilUnica, "name"),
                ["limit"] = 1,
            };

            var json = await PostJsonAsync(url, bodyObj.ToString(), procesoId, reporter).ConfigureAwait(false);
            var jo = JObject.Parse(json);
            var total = jo["total"]?.Value<int>() ?? 0;
            if (total <= 0)
                return null;

            var arr = jo["results"] as JArray;
            if (arr == null || arr.Count == 0)
                return null;

            return arr[0]["id"]?.ToString();
        }

        internal async Task<string> UpsertCompanyAsync(string hubSpotCompanyIdOrNull, JObject properties)
        {
            return await UpsertCompanyAsync(hubSpotCompanyIdOrNull, properties, null, null).ConfigureAwait(false);
        }

        internal async Task<string> UpsertCompanyAsync(
            string hubSpotCompanyIdOrNull,
            JObject properties,
            long? procesoId,
            IHubSpotColaIntentosReporter reporter)
        {
            await ThrottleAsync().ConfigureAwait(false);
            var envelope = new JObject { ["properties"] = properties };
            var payload = envelope.ToString();

            if (!string.IsNullOrEmpty(hubSpotCompanyIdOrNull))
            {
                var url = _cfg.BaseUrl + "/crm/v3/objects/companies/" + Uri.EscapeDataString(hubSpotCompanyIdOrNull);
                return await PatchJsonAsync(url, payload, procesoId, reporter).ConfigureAwait(false);
            }

            var createUrl = _cfg.BaseUrl + "/crm/v3/objects/companies";
            return await PostJsonAsync(createUrl, payload, procesoId, reporter).ConfigureAwait(false);
        }

        internal async Task<string> SearchContactIdByEmailAsync(string email)
        {
            return await SearchContactIdByEmailAsync(email, null, null).ConfigureAwait(false);
        }

        internal async Task<string> SearchContactIdByEmailAsync(
            string email,
            long? procesoId,
            IHubSpotColaIntentosReporter reporter)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            await ThrottleAsync().ConfigureAwait(false);
            var url = _cfg.BaseUrl + "/crm/v3/objects/contacts/search";
            var bodyObj = new JObject
            {
                ["filterGroups"] = new JArray
                {
                    new JObject
                    {
                        ["filters"] = new JArray
                        {
                            new JObject
                            {
                                ["propertyName"] = "email",
                                ["operator"] = "EQ",
                                ["value"] = email.Trim(),
                            },
                        },
                    },
                },
                ["properties"] = new JArray("email", "firstname"),
                ["limit"] = 1,
            };

            var json = await PostJsonAsync(url, bodyObj.ToString(), procesoId, reporter).ConfigureAwait(false);
            var jo = JObject.Parse(json);
            var total = jo["total"]?.Value<int>() ?? 0;
            if (total <= 0)
                return null;

            var arr = jo["results"] as JArray;
            if (arr == null || arr.Count == 0)
                return null;

            return arr[0]["id"]?.ToString();
        }

        /// <returns>Tupla: <c>Item1</c> true si se creó (POST), false si actualización (PATCH); <c>Item2</c> id HubSpot.</returns>
        internal async Task<Tuple<bool, string>> UpsertContactAsync(string hubSpotContactIdOrNull, JObject properties)
        {
            return await UpsertContactAsync(hubSpotContactIdOrNull, properties, null, null).ConfigureAwait(false);
        }

        internal async Task<Tuple<bool, string>> UpsertContactAsync(
            string hubSpotContactIdOrNull,
            JObject properties,
            long? procesoId,
            IHubSpotColaIntentosReporter reporter)
        {
            await ThrottleAsync().ConfigureAwait(false);
            var envelope = new JObject { ["properties"] = properties };
            var payload = envelope.ToString();

            if (!string.IsNullOrEmpty(hubSpotContactIdOrNull))
            {
                var url = _cfg.BaseUrl + "/crm/v3/objects/contacts/" + Uri.EscapeDataString(hubSpotContactIdOrNull);
                await PatchJsonAsync(url, payload, procesoId, reporter).ConfigureAwait(false);
                return Tuple.Create(false, hubSpotContactIdOrNull);
            }

            var createUrl = _cfg.BaseUrl + "/crm/v3/objects/contacts";
            var resp = await PostJsonAsync(createUrl, payload, procesoId, reporter).ConfigureAwait(false);
            var jo = JObject.Parse(resp);
            var newId = jo["id"]?.ToString();
            return Tuple.Create(true, newId);
        }

        internal async Task AssociateContactToCompanyAsync(string contactHubId, string companyHubId)
        {
            await AssociateContactToCompanyAsync(contactHubId, companyHubId, null, null).ConfigureAwait(false);
        }

        internal async Task AssociateContactToCompanyAsync(
            string contactHubId,
            string companyHubId,
            long? procesoId,
            IHubSpotColaIntentosReporter reporter)
        {
            await ThrottleAsync().ConfigureAwait(false);
            var url = string.Format(
                CultureInfo.InvariantCulture,
                "{0}/crm/v3/objects/contacts/{1}/associations/companies/{2}/contact_to_company",
                _cfg.BaseUrl,
                Uri.EscapeDataString(contactHubId),
                Uri.EscapeDataString(companyHubId));

            await SendWithRetryAsync(
                () =>
                {
                    var req = new HttpRequestMessage(HttpMethod.Put, url);
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _cfg.PrivateAppToken);
                    req.Content = new StringContent("{}", Encoding.UTF8, "application/json");
                    return req;
                },
                "HubSpot PUT asociación contacto-compañía " + url,
                procesoId,
                reporter).ConfigureAwait(false);
        }

        internal async Task BatchUpdateCompaniesManejoCcAsync(System.Collections.Generic.IReadOnlyList<Tuple<string, string>> hubIdAndTexto)
        {
            await BatchUpdateCompaniesManejoCcAsync(hubIdAndTexto, null, null).ConfigureAwait(false);
        }

        internal async Task BatchUpdateCompaniesManejoCcAsync(
            System.Collections.Generic.IReadOnlyList<Tuple<string, string>> hubIdAndTexto,
            long? procesoId,
            IHubSpotColaIntentosReporter reporter)
        {
            if (hubIdAndTexto == null || hubIdAndTexto.Count == 0)
                return;

            await ThrottleAsync().ConfigureAwait(false);
            var url = _cfg.BaseUrl + "/crm/v3/objects/companies/batch/update";
            var inputs = new JArray();
            foreach (var pair in hubIdAndTexto)
            {
                inputs.Add(new JObject
                {
                    ["id"] = pair.Item1,
                    ["properties"] = new JObject
                    {
                        [_cfg.PropertyManejoCuentaCorriente] = pair.Item2 ?? string.Empty,
                    },
                });
            }

            var payload = new JObject { ["inputs"] = inputs }.ToString();
            await PostJsonAsync(url, payload, procesoId, reporter).ConfigureAwait(false);
        }

        private Task ThrottleAsync()
        {
            var ms = _cfg.DelayMsBetweenCalls;
            if (ms <= 0)
                return Task.Delay(0);

            return Task.Delay(ms);
        }

        private async Task<string> PostJsonAsync(string url, string jsonBody)
        {
            return await PostJsonAsync(url, jsonBody, null, null).ConfigureAwait(false);
        }

        private async Task<string> PostJsonAsync(
            string url,
            string jsonBody,
            long? procesoId,
            IHubSpotColaIntentosReporter reporter)
        {
            return await SendJsonWithRetryAsync(
                () =>
                {
                    var req = new HttpRequestMessage(HttpMethod.Post, url);
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _cfg.PrivateAppToken);
                    req.Content = new StringContent(jsonBody ?? "{}", Encoding.UTF8, "application/json");
                    return req;
                },
                "HubSpot POST " + url,
                procesoId,
                reporter).ConfigureAwait(false);
        }

        private async Task<string> PatchJsonAsync(string url, string jsonBody)
        {
            return await PatchJsonAsync(url, jsonBody, null, null).ConfigureAwait(false);
        }

        private async Task<string> PatchJsonAsync(
            string url,
            string jsonBody,
            long? procesoId,
            IHubSpotColaIntentosReporter reporter)
        {
            return await SendJsonWithRetryAsync(
                () =>
                {
                    var req = new HttpRequestMessage(new HttpMethod("PATCH"), url);
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _cfg.PrivateAppToken);
                    req.Content = new StringContent(jsonBody ?? "{}", Encoding.UTF8, "application/json");
                    return req;
                },
                "HubSpot PATCH " + url,
                procesoId,
                reporter).ConfigureAwait(false);
        }

        private async Task<string> SendJsonWithRetryAsync(
            Func<HttpRequestMessage> createRequest,
            string operationLabel,
            long? procesoId,
            IHubSpotColaIntentosReporter reporter)
        {
            using (var response = await SendWithRetryAsyncCore(createRequest, operationLabel, procesoId, reporter).ConfigureAwait(false))
            {
                var body = response.Content == null ? null : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return body ?? string.Empty;
            }
        }

        private async Task SendWithRetryAsync(
            Func<HttpRequestMessage> createRequest,
            string operationLabel,
            long? procesoId,
            IHubSpotColaIntentosReporter reporter)
        {
            using (var response = await SendWithRetryAsyncCore(createRequest, operationLabel, procesoId, reporter).ConfigureAwait(false))
            {
            }
        }

        private async Task<HttpResponseMessage> SendWithRetryAsyncCore(
            Func<HttpRequestMessage> createRequest,
            string operationLabel,
            long? procesoId,
            IHubSpotColaIntentosReporter reporter)
        {
            var maxRetries = _cfg.MaxHttpRetries;
            if (maxRetries < 0)
                maxRetries = 0;

            var backoffMs = _cfg.HttpRetryBackoffMilliseconds;

            for (var attempt = 0; attempt <= maxRetries; attempt++)
            {
                var req = createRequest();
                var res = await _http.SendAsync(req).ConfigureAwait(false);
                if (res.IsSuccessStatusCode)
                {
                    req.Dispose();
                    return res;
                }

                var body = res.Content == null ? null : await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                var statusCode = (int)res.StatusCode;

                req.Dispose();
                res.Dispose();

                if (statusCode == 401)
                    throw new HubSpotAuthException(operationLabel + ": HTTP 401 " + body);

                if (!IsRetryableStatusCode((HttpStatusCode)statusCode))
                    throw new HttpRequestException(operationLabel + ": HTTP " + statusCode + " " + body);

                if (attempt >= maxRetries)
                {
                    throw new HubSpotHttpRetriesExhaustedException(
                        operationLabel + ": reintentos agotados tras HTTP " + statusCode,
                        statusCode,
                        body);
                }

                if (reporter != null)
                    reporter.OnHttpRetryFailed(procesoId);

                if (backoffMs > 0)
                    await Task.Delay(backoffMs).ConfigureAwait(false);
            }

            throw new InvalidOperationException(operationLabel + ": SendWithRetryAsync terminó sin respuesta.");
        }

        private static bool IsRetryableStatusCode(HttpStatusCode statusCode)
        {
            var code = (int)statusCode;
            return code == 429
                   || code == 500
                   || code == 502
                   || code == 503
                   || code == 504;
        }
    }
}
