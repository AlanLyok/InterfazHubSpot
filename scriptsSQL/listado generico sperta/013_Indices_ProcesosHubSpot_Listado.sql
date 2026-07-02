/*
  Indices de soporte para ListadoHubSpotProcesosCola_Buscar.
  Idempotente: IF NOT EXISTS por nombre de indice.
*/

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.ProcesosSpertaHubSpotLog')
      AND name = N'IX_ProcesosSpertaHubSpotLog_ProcesoFecha'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_ProcesosSpertaHubSpotLog_ProcesoFecha
        ON dbo.ProcesosSpertaHubSpotLog (ProcesoId, FechaGrabacion DESC)
        INCLUDE (Fase, Exito, Identificador);
    PRINT 'Indice IX_ProcesosSpertaHubSpotLog_ProcesoFecha creado.';
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.ProcesosSpertaHubSpot')
      AND name = N'IX_ProcesosSpertaHubSpot_IdentificadorEstado'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_ProcesosSpertaHubSpot_IdentificadorEstado
        ON dbo.ProcesosSpertaHubSpot (Identificador, Estado, FechaCreacion DESC);
    PRINT 'Indice IX_ProcesosSpertaHubSpot_IdentificadorEstado creado.';
END
GO
