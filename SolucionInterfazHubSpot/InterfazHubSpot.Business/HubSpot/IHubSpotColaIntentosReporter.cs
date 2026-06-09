namespace InterfazHubSpot.Business.HubSpot
{
    /// <summary>Notifica fallos HTTP reintentables para incrementar <c>Intentos</c> en la cola 2A.</summary>
    public interface IHubSpotColaIntentosReporter
    {
        /// <summary>Invocado en cada fallo HTTP reintentable (429/5xx) antes del siguiente intento.</summary>
        void OnHttpRetryFailed(long? procesoId);
    }
}
