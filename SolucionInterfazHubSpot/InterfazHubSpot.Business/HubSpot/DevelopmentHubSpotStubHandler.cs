using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace InterfazHubSpot.Business.HubSpot
{
    /// <summary>Intercepta llamadas CRM v3 y devuelve JSON mínimo para desarrollo (sin HubSpot real).</summary>
    internal sealed class DevelopmentHubSpotStubHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var uri = request.RequestUri;
            if (uri == null)
                return Completed(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var path = Uri.UnescapeDataString(uri.AbsolutePath ?? string.Empty).TrimEnd('/');

            if (HttpMethod.Post.Equals(request.Method))
            {
                if (EndsWithInsensitive(path, "/crm/v3/objects/companies/search"))
                    return Completed(Ok("{ \"total\":0,\"results\":[]}"));

                if (EndsWithInsensitive(path, "/crm/v3/objects/contacts/search"))
                    return Completed(Ok("{ \"total\":0,\"results\":[]}"));

                if (string.Equals(path, "/crm/v3/objects/companies", StringComparison.OrdinalIgnoreCase))
                    return Completed(
                        Ok("{ \"id\":\"mock-comp-" + ShortId() + "\",\"properties\":{},\"createdAt\":\"2020-01-01T00:00:00.000Z\"}"));

                if (string.Equals(path, "/crm/v3/objects/contacts", StringComparison.OrdinalIgnoreCase))
                    return Completed(
                        Ok("{ \"id\":\"mock-contact-" + ShortId() + "\",\"properties\":{},\"createdAt\":\"2020-01-01T00:00:00.000Z\"}"));

                if (EndsWithInsensitive(path, "/crm/v3/objects/companies/batch/update"))
                    return Completed(Ok("{}"));
            }

            if (HttpMethod.Put.Equals(request.Method))
            {
                // Asociaciones v3 PUT .../contact_to_company (u otras) → éxito sin cuerpo
                if (path.IndexOf("/crm/v3/objects/contacts/", StringComparison.OrdinalIgnoreCase) >= 0
                    && path.IndexOf("/associations/companies/", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return Completed(new HttpResponseMessage(HttpStatusCode.NoContent));
                }
            }

            if (new HttpMethod("PATCH").Equals(request.Method))
            {
                var cid = ExtractIdAfterSegments(path, "/crm/v3/objects/companies/");
                if (cid != null)
                    return Completed(Ok("{ \"id\":\"" + EscapeJson(cid) + "\",\"properties\":{}}"));

                var ctId = ExtractIdAfterSegments(path, "/crm/v3/objects/contacts/");
                if (ctId != null)
                    return Completed(Ok("{ \"id\":\"" + EscapeJson(ctId) + "\",\"properties\":{}}"));
            }

            return Completed(Ok("{}"));
        }

        private static Task<HttpResponseMessage> Completed(HttpResponseMessage r)
        {
            return Task.FromResult(r);
        }

        private static HttpResponseMessage Ok(string json)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json ?? "{}", System.Text.Encoding.UTF8, "application/json"),
            };
        }

        private static bool EndsWithInsensitive(string full, string suffix)
        {
            return full != null && suffix != null &&
                   full.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;

            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static string ShortId()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 12);
        }

        /// <returns>Segmento después de prefijo hasta próximo '/' exclusivo.</returns>
        private static string ExtractIdAfterSegments(string path, string segmentPrefix)
        {
            var i = path.IndexOf(segmentPrefix, StringComparison.OrdinalIgnoreCase);
            if (i < 0)
                return null;
            var start = i + segmentPrefix.Length;
            if (start >= path.Length)
                return null;
            var slash = path.IndexOf('/', start);
            if (slash >= 0)
                return path.Substring(start, slash - start);

            return path.Substring(start);
        }
    }
}
