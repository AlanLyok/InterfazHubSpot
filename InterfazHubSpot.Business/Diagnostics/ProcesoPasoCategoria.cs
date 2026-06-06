namespace InterfazHubSpot.Business.Diagnostics
{
    /// <summary>Capa lógica del paso para filtrado sin acoplar a un destino concreto (HubSpot, etc.).</summary>
    public enum ProcesoPasoCategoria
    {
        Infraestructura,

        Cola,

        FuenteDatos,

        Mapeo,

        DestinoExterno,
    }
}
