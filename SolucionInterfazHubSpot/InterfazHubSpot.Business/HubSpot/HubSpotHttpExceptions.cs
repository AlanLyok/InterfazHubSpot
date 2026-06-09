using System;

namespace InterfazHubSpot.Business.HubSpot
{
    /// <summary>Token HubSpot inválido o revocado (HTTP 401). Sin reintentos.</summary>
    public sealed class HubSpotAuthException : Exception
    {
        public HubSpotAuthException(string message)
            : base(message)
        {
        }

        public int StatusCode => 401;
    }

    /// <summary>Se agotaron los reintentos HTTP configurables ante 429/5xx.</summary>
    public sealed class HubSpotHttpRetriesExhaustedException : Exception
    {
        public HubSpotHttpRetriesExhaustedException(string message, int statusCode, string responseBody)
            : base(message)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        public int StatusCode { get; }

        public string ResponseBody { get; }
    }
}
