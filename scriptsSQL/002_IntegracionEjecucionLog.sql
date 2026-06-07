/*
  Log de ejecuciones de integración HubSpot.
  Alineado con InterfazHubSpot.Entities.IntegracionEjecucionLog (EF6).
*/

IF OBJECT_ID(N'dbo.IntegracionEjecucionLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IntegracionEjecucionLog (
        LogId      BIGINT IDENTITY(1,1) NOT NULL,
        ProcesoId  BIGINT NULL,
        Destino    NVARCHAR(50) NOT NULL,
        ClienteId  INT NULL,
        Fase       NVARCHAR(80) NOT NULL,
        Exito      BIT NOT NULL,
        Detalle    NVARCHAR(MAX) NULL,
        FechaUtc   DATETIME2 NOT NULL,
        CONSTRAINT PK_IntegracionEjecucionLog PRIMARY KEY CLUSTERED (LogId)
    );
    PRINT 'Tabla creada: dbo.IntegracionEjecucionLog';
END
ELSE
BEGIN
    PRINT 'Tabla dbo.IntegracionEjecucionLog ya existe — sin cambios.';
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.IntegracionEjecucionLog')
      AND name = N'IX_IntegracionEjecucionLog_DestinoFecha'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_IntegracionEjecucionLog_DestinoFecha
        ON dbo.IntegracionEjecucionLog (Destino, FechaUtc DESC)
        INCLUDE (ProcesoId, ClienteId, Fase, Exito);
    PRINT 'Indice IX_IntegracionEjecucionLog_DestinoFecha creado.';
END
GO
