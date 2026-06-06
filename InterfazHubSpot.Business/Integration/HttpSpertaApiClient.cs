using System;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace InterfazHubSpot.Business.Integration
{
    /// <summary>Implementación de <see cref="ISpertaApiClient"/> sobre rutas <c>api/v100</c> documentadas en SpertaAPI.</summary>
    public sealed class HttpSpertaApiClient : ISpertaApiClient
    {
        private static readonly object OAuthDiagnosticsLock = new object();

        private static Action<bool, string> OAuthDiagnosticsListener;

        private static readonly HttpClient SharedClient = CreateSharedClient();

        private readonly HttpClient _http;

        public HttpSpertaApiClient()
            : this(null)
        {
        }

        /// <param name="httpClient"><c>null</c> para usar el cliente compartido del proceso.</param>
        public HttpSpertaApiClient(HttpClient httpClient)
        {
            _http = httpClient ?? SharedClient;
        }

        public HttpClient Http => _http;

        private static string BaseUrl => ConfigurationManager.AppSettings["SpertaAPIBaseUrl"];

        private static string UserName => ConfigurationManager.AppSettings["SpertaAPIUserName"];

        private static string Password => ConfigurationManager.AppSettings["SpertaAPIPassword"];

        private static string CompanyId => ConfigurationManager.AppSettings["SpertaAPICompanyId"];

        private static string ReciboRelativePath => ConfigurationManager.AppSettings["SpertaAPIReciboRelativePath"];

        private static string GetSpertaApiCompanyIdRequired()
        {
            var c = (CompanyId ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(c))
            {
                throw new InvalidOperationException(
                    "Configure SpertaAPICompanyId en App.config / Web.config con el identificador de empresa ERP (cabecera HTTP CompanyId en OAuth de SpertaAPI).");
            }

            return c;
        }

        /// <summary>Registrar escucha OAuth (valor del token jamás pasado). No apto para paralelismo pesado: un listener global por proceso.</summary>
        public static void SetOAuthDiagnosticsListener(Action<bool, string> listener)
        {
            lock (OAuthDiagnosticsLock)
            {
                OAuthDiagnosticsListener = listener;
            }
        }

        public static void ClearOAuthDiagnosticsListener()
        {
            lock (OAuthDiagnosticsLock)
            {
                OAuthDiagnosticsListener = null;
            }
        }

        private static void NotifyOAuthDiagnostics(bool ok, string message)
        {
            Action<bool, string> h;
            lock (OAuthDiagnosticsLock)
            {
                h = OAuthDiagnosticsListener;
            }

            if (h != null)
            {
                h(ok, message ?? string.Empty);
            }
        }

        public async Task<string> GetHealthAsync()
        {
            var url = BuildUrl(BaseUrl, "api/v100/health");
            return await SendWithoutAuthAsync(HttpMethod.Get, url, null).ConfigureAwait(false);
        }

        public async Task<string> PostClientesGrabarAsync(string jsonBody)
        {
            var token = await ObtainAccessTokenAsync().ConfigureAwait(false);
            var url = BuildUrl(BaseUrl, "api/v100/clientes/grabar");
            return await SendWithBearerAsync(HttpMethod.Post, url, token, jsonBody).ConfigureAwait(false);
        }

        public async Task<string> PostPedidosGrabarAsync(string jsonBody)
        {
            var token = await ObtainAccessTokenAsync().ConfigureAwait(false);
            var url = BuildUrl(BaseUrl, "api/v100/pedidos/grabar");
            return await SendWithBearerAsync(HttpMethod.Post, url, token, jsonBody).ConfigureAwait(false);
        }

        public async Task<string> PostReciboAsync(string jsonBody)
        {
            var path = (ReciboRelativePath ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException(
                    "No hay endpoint de recibos en el núcleo SpertaAPI: configure la clave SpertaAPIReciboRelativePath (ruta relativa al host, p. ej. api/v100/…/recibos/grabar) si el despliegue lo expone.");
            }

            var token = await ObtainAccessTokenAsync().ConfigureAwait(false);
            var url = BuildUrl(BaseUrl, path);
            return await SendWithBearerAsync(HttpMethod.Post, url, token, jsonBody).ConfigureAwait(false);
        }

        public async Task<string> GetIntegracionesClienteAsync(int clienteId)
        {
            var token = await ObtainAccessTokenAsync().ConfigureAwait(false);
            var relative = "api/v100/sperta/integraciones/clientes/" + clienteId;
            var url = BuildUrl(BaseUrl, relative);
            return await SendWithBearerAsync(HttpMethod.Get, url, token, null).ConfigureAwait(false);
        }

        public async Task<string> GetIntegracionesHubSpotCuentaCorrienteAsync(int cursor, int pageSize)
        {
            var token = await ObtainAccessTokenAsync().ConfigureAwait(false);
            var relative = string.Format(
                CultureInfo.InvariantCulture,
                "api/v100/sperta/integraciones/hubspot/cuenta-corriente-clientes?cursor={0}&pageSize={1}",
                cursor,
                pageSize);
            var url = BuildUrl(BaseUrl, relative);
            return await SendWithBearerAsync(HttpMethod.Get, url, token, null).ConfigureAwait(false);
        }

        private static HttpClient CreateSharedClient()
        {
            var c = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
            return c;
        }

        internal static string BuildUrl(string baseUrl, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("La ruta no puede ser nula o vacía.", nameof(path));
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("SpertaAPIBaseUrl no está configurado en App.config / Web.config.");

            var trimmedBase = baseUrl.TrimEnd('/');
            var trimmedPath = path.StartsWith("/") ? path : "/" + path;
            var full = trimmedBase + trimmedPath;
            if (!Uri.TryCreate(full, UriKind.Absolute, out _))
                throw new InvalidOperationException("La URL resultante no es válida: " + full);
            return full;
        }

        private async Task<string> SendWithoutAuthAsync(HttpMethod method, string url, string jsonBody)
        {
            using (var req = new HttpRequestMessage(method, url))
            {
                if (jsonBody != null)
                    req.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                using (var res = await _http.SendAsync(req).ConfigureAwait(false))
                {
                    var body = res.Content == null ? null : await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                    EnsureSuccess(res, body);
                    return body ?? string.Empty;
                }
            }
        }

        private async Task<string> SendWithBearerAsync(HttpMethod method, string url, string bearerToken, string jsonBody)
        {
            using (var req = new HttpRequestMessage(method, url))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
                req.Headers.TryAddWithoutValidation("CompanyId", GetSpertaApiCompanyIdRequired());

                if (jsonBody != null)
                    req.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                using (var res = await _http.SendAsync(req).ConfigureAwait(false))
                {
                    var body = res.Content == null ? null : await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                    EnsureSuccess(res, body);
                    return body ?? string.Empty;
                }
            }
        }

        private static void EnsureSuccess(HttpResponseMessage response, string body)
        {
            if (response == null)
                throw new InvalidOperationException("Respuesta HTTP nula.");

            if (response.IsSuccessStatusCode)
                return;

            var snippet = string.IsNullOrEmpty(body) ? string.Empty : (body.Length > 500 ? body.Substring(0, 500) + "…" : body);
            throw new HttpRequestException(
                string.Format(
                    "Error HTTP {0} ({1}). Cuerpo: {2}",
                    (int)response.StatusCode,
                    response.ReasonPhrase,
                    snippet));
        }

        private static async Task<string> ObtainAccessTokenAsync()
        {
            try
            {
                var companyCfg = GetSpertaApiCompanyIdRequired();

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

                var tokenUrl = BuildUrl(BaseUrl, "api/v100/token");
                var form = string.Format(
                    "grant_type=password&username={0}&password={1}",
                    Uri.EscapeDataString(UserName ?? string.Empty),
                    Uri.EscapeDataString(Password ?? string.Empty));

                using (var http = new HttpClient())
                {
                    using (var req = new HttpRequestMessage(HttpMethod.Post, tokenUrl))
                    {
                        req.Headers.TryAddWithoutValidation("CompanyId", companyCfg);

                        req.Content = new StringContent(form, Encoding.UTF8, "application/x-www-form-urlencoded");

                        using (var res = await http.SendAsync(req).ConfigureAwait(false))
                        {
                            var json = res.Content == null ? null : await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                            EnsureSuccess(res, json);

                            if (string.IsNullOrWhiteSpace(json))
                                throw new InvalidOperationException("Respuesta de token vacía.");

                            var jo = JObject.Parse(json);
                            var err = jo["error"]?.ToString();
                            if (!string.IsNullOrEmpty(err))
                            {
                                var desc = jo["error_description"]?.ToString() ?? json;
                                throw new InvalidOperationException("Token OAuth: " + desc);
                            }

                            var access = jo["access_token"]?.ToString();
                            if (string.IsNullOrEmpty(access))
                                throw new InvalidOperationException("Respuesta de token sin access_token.");

                            NotifyOAuthDiagnostics(true, "Token OAuth obtenido OK (valor no registrado por seguridad).");
                            return access;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NotifyOAuthDiagnostics(false, ex.Message ?? string.Empty);
                throw;
            }
        }
    }
}
