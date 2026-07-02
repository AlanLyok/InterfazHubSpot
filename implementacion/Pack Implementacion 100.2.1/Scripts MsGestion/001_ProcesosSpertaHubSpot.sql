/*---------------------------------------------------------------------------------------------------  
Modificado por    : Alan Lipshitz     
Fecha             : 29/06/2026
Incidente/Actividad      : (19512)
Descripcion       : Se crea el store para la InterfazHubSpot
---------------------------------------------------------------------------------------------------*/  


 IF OBJECT_ID(N'dbo.ProcesosSpertaHubSpot', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProcesosSpertaHubSpot (
        ProcesoId              BIGINT IDENTITY(1,1) NOT NULL,
        EmpresaId              INT NULL,
        Destino                NVARCHAR(50) NOT NULL,
        TipoEntidad            NVARCHAR(50) NOT NULL,
        TipoOperacion          NVARCHAR(20) NOT NULL,
        Identificador          INT NOT NULL,
        Estado                 TINYINT NOT NULL,
        Intentos               INT NOT NULL,
        MensajeUltimoError     NVARCHAR(MAX) NULL,
        FechaCreacion          DATETIME2 NOT NULL CONSTRAINT DF_ProcesosSpertaHubSpot_FechaCreacion DEFAULT GETDATE(),
        FechaInicioProceso     DATETIME2 NULL,
        FechaFinProceso        DATETIME2 NULL,
        CONSTRAINT PK_ProcesosSpertaHubSpot PRIMARY KEY CLUSTERED (ProcesoId)
    );
    PRINT 'Tabla creada: dbo.ProcesosSpertaHubSpot';
END
ELSE
BEGIN
    PRINT 'Tabla dbo.ProcesosSpertaHubSpot ya existe — sin cambios.';
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.ProcesosSpertaHubSpot')
      AND name = N'IX_ProcesosSpertaHubSpot_DestinoEstadoFecha'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_ProcesosSpertaHubSpot_DestinoEstadoFecha
        ON dbo.ProcesosSpertaHubSpot (Destino, Estado, FechaCreacion)
        INCLUDE (Identificador, TipoEntidad, Intentos);
    PRINT 'Indice IX_ProcesosSpertaHubSpot_DestinoEstadoFecha creado.';
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.ProcesosSpertaHubSpot')
      AND name = N'UX_ProcesosSpertaHubSpot_ActivoCliente'
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_ProcesosSpertaHubSpot_ActivoCliente
        ON dbo.ProcesosSpertaHubSpot (Destino, TipoEntidad, Identificador)
        WHERE Estado IN (0, 1);
    PRINT 'Indice UX_ProcesosSpertaHubSpot_ActivoCliente creado.';
END
GO
