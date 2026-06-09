namespace InterfazHubSpot.Business.Diagnostics
{
    /// <summary>No-op para ejecución sin traza HTTP.</summary>
    public sealed class NullProcesoPasoReporter : IProcesoPasoReporter
    {
        public static readonly IProcesoPasoReporter Instance = new NullProcesoPasoReporter();

        public void RegistrarPaso(ProcesoPasoSeverity severidad, ProcesoPasoCategoria categoria, string codigo, string mensaje, object datos)
        {
        }

        private NullProcesoPasoReporter()
        {
        }
    }
}
