namespace InterfazHubSpot.Business.Diagnostics
{
    /// <summary>Estadísticas agregadas de una corrida de cola por destino (neutral).</summary>
    public sealed class ProcesoColaEjecucionResumen
    {
        public int ItemsReclamados { get; set; }

        public int ItemsSincronizadosOk { get; set; }

        public int ItemsFallidos { get; set; }
    }
}
