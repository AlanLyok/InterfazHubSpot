using System;
using InterfazHubSpot.Business.Managers;

namespace InterfazHubSpot.Business.HubSpot
{
    /// <summary>Persiste incrementos de <c>Intentos</c> vía <see cref="ProcesosSpertaHubSpotManager"/>.</summary>
    public sealed class HubSpotColaIntentosReporter : IHubSpotColaIntentosReporter
    {
        private readonly ProcesosSpertaHubSpotManager _cola;

        public HubSpotColaIntentosReporter(ProcesosSpertaHubSpotManager cola)
        {
            _cola = cola ?? throw new ArgumentNullException(nameof(cola));
        }

        public void OnHttpRetryFailed(long? procesoId)
        {
            if (!procesoId.HasValue)
                return;

            _cola.IncrementarIntentos(procesoId.Value);
        }
    }
}
