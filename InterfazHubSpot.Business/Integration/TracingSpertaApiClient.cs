using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using BatchSpertaAPI.Business.Diagnostics;
using Newtonsoft.Json.Linq;

namespace BatchSpertaAPI.Business.Integration
{
    /// <summary>
    /// Decora un <see cref="ISpertaApiClient"/> con pasos neutros sobre llamadas seleccionadas (GET integraciones cliente / cuenta corriente).
    /// OAuth se notifica mediante <see cref="HttpSpertaApiClient.SetOAuthDiagnosticsListener"/>.
    /// </summary>
    public sealed class TracingSpertaApiClient : ISpertaApiClient
    {
        private readonly ISpertaApiClient _inner;

        private readonly IProcesoPasoReporter _pasos;

        public TracingSpertaApiClient(ISpertaApiClient inner, IProcesoPasoReporter pasos)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _pasos = pasos ?? NullProcesoPasoReporter.Instance;
        }

        public System.Net.Http.HttpClient Http => _inner.Http;

        public Task<string> GetHealthAsync()
        {
            return _inner.GetHealthAsync();
        }

        public Task<string> PostClientesGrabarAsync(string jsonBody)
        {
            return _inner.PostClientesGrabarAsync(jsonBody);
        }

        public Task<string> PostPedidosGrabarAsync(string jsonBody)
        {
            return _inner.PostPedidosGrabarAsync(jsonBody);
        }

        public Task<string> PostReciboAsync(string jsonBody)
        {
            return _inner.PostReciboAsync(jsonBody);
        }

        public async Task<string> GetIntegracionesClienteAsync(int clienteId)
        {
            var relative = "api/v100/sperta/integraciones/clientes/" + clienteId.ToString(CultureInfo.InvariantCulture);
            var urlAbsoluta = ConstruirUrlSpertaApiAbsoluta(relative);
            _pasos.RegistrarPaso(
                ProcesoPasoSeverity.Information,
                ProcesoPasoCategoria.SpertaApi,
                "spertaapi.get.integraciones.cliente.solicitud",
                "Solicitud GET integraciones cliente",
                new
                {
                    httpMethod = "GET",
                    relativePath = relative,
                    urlAbsoluta,
                    clienteId,
                });

            var sw = Stopwatch.StartNew();
            string json = null;

            try
            {
                json = await _inner.GetIntegracionesClienteAsync(clienteId).ConfigureAwait(false);
                sw.Stop();

                bool? hayErr = null;
                try
                {
                    var jo = JObject.Parse(json);
                    hayErr = jo.Value<bool?>("HayError");
                }
                catch
                {
                }

                _pasos.RegistrarPaso(
                    ProcesoPasoSeverity.Information,
                    ProcesoPasoCategoria.SpertaApi,
                    "spertaapi.get.integraciones.cliente",
                    "Respuesta GET integraciones cliente",
                    new
                    {
                        relativePath = relative,
                        urlAbsoluta,
                        duracionMs = sw.ElapsedMilliseconds,
                        contenidoTamano = json != null ? json.Length : (int?)null,
                        hayErrorEnvelope = hayErr,
                        muestraTruncada = DiagnosticsTextHelper.TruncateForTrace(json ?? string.Empty),
                    });

                return json;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _pasos.RegistrarPaso(
                    ProcesoPasoSeverity.Error,
                    ProcesoPasoCategoria.SpertaApi,
                    "spertaapi.get.integraciones.cliente.error",
                    ex.Message,
                    new
                    {
                        relativePath = relative,
                        urlAbsoluta,
                        duracionMs = sw.ElapsedMilliseconds,
                        tipoExcepcion = ex.GetType().Name,
                        muestraTruncada =
                            DiagnosticsTextHelper.TruncateForTrace(
                                json != null ? json : string.Empty),
                    });

                throw;
            }
        }

        public async Task<string> GetIntegracionesHubSpotCuentaCorrienteAsync(int cursor, int pageSize)
        {
            var relative = string.Format(
                CultureInfo.InvariantCulture,
                "api/v100/sperta/integraciones/hubspot/cuenta-corriente-clientes?cursor={0}&pageSize={1}",
                cursor,
                pageSize);
            var sw = Stopwatch.StartNew();

            try
            {
                var json = await _inner.GetIntegracionesHubSpotCuentaCorrienteAsync(cursor, pageSize).ConfigureAwait(false);
                sw.Stop();
                bool? hayErr = null;
                try
                {
                    var jo = JObject.Parse(json);
                    hayErr = jo.Value<bool?>("HayError");
                }
                catch
                {
                }

                _pasos.RegistrarPaso(
                    ProcesoPasoSeverity.Information,
                    ProcesoPasoCategoria.SpertaApi,
                    "spertaapi.get.hubspot.cuenta_corriente",
                    "Respuesta página cuenta corriente HubSpot",
                    new
                    {
                        relativeQuery = relative,
                        cursor,
                        pageSize,
                        duracionMs = sw.ElapsedMilliseconds,
                        contenidoTamano = json != null ? json.Length : (int?)null,
                        hayErrorEnvelope = hayErr,
                        muestraTruncada =
                            DiagnosticsTextHelper.TruncateForTrace(json ?? string.Empty),
                    });

                return json;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _pasos.RegistrarPaso(
                    ProcesoPasoSeverity.Error,
                    ProcesoPasoCategoria.SpertaApi,
                    "spertaapi.get.hubspot.cuenta_corriente.error",
                    ex.Message,
                    new { relativeQuery = relative, duracionMs = sw.ElapsedMilliseconds });

                throw;
            }
        }

        /// <summary>Sin credenciales; solo ensambla base + ruta para depuración MVC.</summary>
        private static string ConstruirUrlSpertaApiAbsoluta(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return string.Empty;

            var baseUrl = (ConfigurationManager.AppSettings["SpertaAPIBaseUrl"] ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(baseUrl))
                return relativePath.TrimStart('/');

            return baseUrl.TrimEnd('/') + "/" + relativePath.TrimStart('/');
        }
    }
}
