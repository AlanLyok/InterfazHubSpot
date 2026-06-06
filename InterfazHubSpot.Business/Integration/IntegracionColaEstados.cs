namespace InterfazHubSpot.Business.Integration
{
    /// <summary>Valores de la columna Estado en dbo.ProcesosSpertaHubSpot.</summary>
    public static class IntegracionColaEstados
    {
        public const byte Pendiente = 0;

        public const byte EnProceso = 1;

        public const byte Ok = 2;

        public const byte Error = 3;
    }

    public static class IntegracionDestinos
    {
        public const string HubSpot = "HubSpot";
    }

    public static class IntegracionTipoOperacion
    {
        public const string Alta = "Alta";

        public const string Modificacion = "Modificacion";
    }

    /// <summary>Valores de <c>TipoEntidad</c> en dbo.ProcesosSpertaHubSpot.</summary>
    public static class IntegracionTipoEntidad
    {
        public const string Cliente = "Cliente";
    }
}
