using System;
using System.Configuration;
using System.Globalization;
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
            PropertyManejoCuentaCorriente =
                ConfigurationManager.AppSettings["HubSpot:PropertyManejoCuentaCorriente"] ?? "manejo_cuenta_corriente";
            DelayMsBetweenCalls = ParseInt(ConfigurationManager.AppSettings["HubSpot:DelayMillisecondsBetweenCalls"], 120);
            CuentaCorrientePageSize = ParseInt(ConfigurationManager.AppSettings["HubSpot:CuentaCorrientePageSize"], 500);
            UseDevelopmentMock = ParseBoolTrue(ConfigurationManager.AppSettings["HubSpot:UseDevelopmentMock"], false);
        }

        internal string BaseUrl { get; }

        internal string PrivateAppToken { get; }

        internal string PropertyMastersoftId { get; }

        internal string PropertyManejoCuentaCorriente { get; }

        internal int DelayMsBetweenCalls { get; }

        internal int CuentaCorrientePageSize { get; }

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

        internal async Task<string> SearchCompanyIdByMastersoftIdAsync(string mastersoftId)
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
                                ["propertyName"] = _cfg.PropertyMastersoftId,
                                ["operator"] = "EQ",
                                ["value"] = mastersoftId ?? string.Empty,
                            },
                        },
                    },
                },
                ["properties"] = new JArray(_cfg.PropertyMastersoftId, "name"),
                ["limit"] = 1,
            };

            var json = await PostJsonAsync(url, bodyObj.ToString()).ConfigureAwait(false);
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
            await ThrottleAsync().ConfigureAwait(false);
            var envelope = new JObject { ["properties"] = properties };
            var payload = envelope.ToString();

            if (!string.IsNullOrEmpty(hubSpotCompanyIdOrNull))
            {
                var url = _cfg.BaseUrl + "/crm/v3/objects/companies/" + Uri.EscapeDataString(hubSpotCompanyIdOrNull);
                return await PatchJsonAsync(url, payload).ConfigureAwait(false);
            }

            var createUrl = _cfg.BaseUrl + "/crm/v3/objects/companies";
            return await PostJsonAsync(createUrl, payload).ConfigureAwait(false);
        }

        internal async Task<string> SearchContactIdByEmailAsync(string email)
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

            var json = await PostJsonAsync(url, bodyObj.ToString()).ConfigureAwait(false);
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
            await ThrottleAsync().ConfigureAwait(false);
            var envelope = new JObject { ["properties"] = properties };
            var payload = envelope.ToString();

            if (!string.IsNullOrEmpty(hubSpotContactIdOrNull))
            {
                var url = _cfg.BaseUrl + "/crm/v3/objects/contacts/" + Uri.EscapeDataString(hubSpotContactIdOrNull);
                await PatchJsonAsync(url, payload).ConfigureAwait(false);
                return Tuple.Create(false, hubSpotContactIdOrNull);
            }

            var createUrl = _cfg.BaseUrl + "/crm/v3/objects/contacts";
            var resp = await PostJsonAsync(createUrl, payload).ConfigureAwait(false);
            var jo = JObject.Parse(resp);
            var newId = jo["id"]?.ToString();
            return Tuple.Create(true, newId);
        }

        internal async Task AssociateContactToCompanyAsync(string contactHubId, string companyHubId)
        {
            await ThrottleAsync().ConfigureAwait(false);
            var url = string.Format(
                CultureInfo.InvariantCulture,
                "{0}/crm/v3/objects/contacts/{1}/associations/companies/{2}/contact_to_company",
                _cfg.BaseUrl,
                Uri.EscapeDataString(contactHubId),
                Uri.EscapeDataString(companyHubId));

            using (var req = new HttpRequestMessage(HttpMethod.Put, url))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _cfg.PrivateAppToken);
                req.Content = new StringContent("{}", Encoding.UTF8, "application/json");

                using (var res = await _http.SendAsync(req).ConfigureAwait(false))
                {
                    var body = res.Content == null ? null : await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!res.IsSuccessStatusCode)
                        throw new HttpRequestException("HubSpot asociación contacto-compañía: HTTP " + (int)res.StatusCode + " " + body);
                }
            }
        }

        internal async Task BatchUpdateCompaniesManejoCcAsync(System.Collections.Generic.IReadOnlyList<Tuple<string, string>> hubIdAndTexto)
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
            await PostJsonAsync(url, payload).ConfigureAwait(false);
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
            using (var req = new HttpRequestMessage(HttpMethod.Post, url))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _cfg.PrivateAppToken);
                req.Content = new StringContent(jsonBody ?? "{}", Encoding.UTF8, "application/json");

                using (var res = await _http.SendAsync(req).ConfigureAwait(false))
                {
                    var body = res.Content == null ? null : await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!res.IsSuccessStatusCode)
                        throw new HttpRequestException("HubSpot POST " + url + ": HTTP " + (int)res.StatusCode + " " + body);

                    return body ?? string.Empty;
                }
            }
        }

        private async Task<string> PatchJsonAsync(string url, string jsonBody)
        {
            using (var req = new HttpRequestMessage(new HttpMethod("PATCH"), url))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _cfg.PrivateAppToken);
                req.Content = new StringContent(jsonBody ?? "{}", Encoding.UTF8, "application/json");

                using (var res = await _http.SendAsync(req).ConfigureAwait(false))
                {
                    var body = res.Content == null ? null : await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!res.IsSuccessStatusCode)
                        throw new HttpRequestException("HubSpot PATCH " + url + ": HTTP " + (int)res.StatusCode + " " + body);

                    return body ?? string.Empty;
                }
            }
        }
    }
}
