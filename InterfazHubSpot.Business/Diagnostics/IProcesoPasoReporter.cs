namespace InterfazHubSpot.Business.Diagnostics
{
    /// <summary>Recibe pasos neutros durante la ejecución de un proceso (cola / integraciones / batch).</summary>
    public interface IProcesoPasoReporter
    {
        /// <summary>Registra un paso; debe ser barato en coste CPU y seguro sin secretos en <paramref name="datos"/>.</summary>
        void RegistrarPaso(ProcesoPasoSeverity severidad, ProcesoPasoCategoria categoria, string codigo, string mensaje, object datos);
    }
}
