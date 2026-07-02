/*---------------------------------------------------------------------------------------------------  
Modificado por    : Alan Lipshitz     
Fecha             : 29/06/2026
Incidente/Actividad      : (19512)
Descripcion       : Se crea el store para la InterfazHubSpot
---------------------------------------------------------------------------------------------------*/  

IF OBJECT_ID(N'dbo.ProcesosSpertaHubSpotLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProcesosSpertaHubSpotLog (
        LogId          BIGINT IDENTITY(1,1) NOT NULL,
        ProcesoId      BIGINT NULL,
        Destino        NVARCHAR(50) NOT NULL,
        Identificador  INT NULL,
        Fase           NVARCHAR(80) NOT NULL,
        Exito          BIT NOT NULL,
        Detalle        NVARCHAR(MAX) NULL,
        FechaGrabacion DATETIME2 NOT NULL CONSTRAINT DF_ProcesosSpertaHubSpotLog_FechaGrabacion DEFAULT GETDATE(),
        CONSTRAINT PK_ProcesosSpertaHubSpotLog PRIMARY KEY CLUSTERED (LogId)
    );
    PRINT 'Tabla creada: dbo.ProcesosSpertaHubSpotLog';
END
ELSE
BEGIN
    PRINT 'Tabla dbo.ProcesosSpertaHubSpotLog ya existe — sin cambios.';
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.ProcesosSpertaHubSpotLog')
      AND name = N'IX_ProcesosSpertaHubSpotLog_DestinoFecha'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_ProcesosSpertaHubSpotLog_DestinoFecha
        ON dbo.ProcesosSpertaHubSpotLog (Destino, FechaGrabacion DESC)
        INCLUDE (ProcesoId, Identificador, Fase, Exito);
    PRINT 'Indice IX_ProcesosSpertaHubSpotLog_DestinoFecha creado.';
END
GO
