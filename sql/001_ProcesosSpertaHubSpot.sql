/*
  Copia versionada alineada con scriptsSQL/001_ProcesosSpertaHubSpot.sql
*/

DECLARE @legacy SYSNAME = N'ProcesosSperta' + N'API';
DECLARE @target SYSNAME = N'ProcesosSpertaHubSpot';

IF OBJECT_ID(N'dbo.' + @target, N'U') IS NULL
   AND OBJECT_ID(N'dbo.' + @legacy, N'U') IS NOT NULL
BEGIN
    DECLARE @renameSql NVARCHAR(500) =
        N'EXEC sp_rename N''dbo.' + @legacy + N''', N''' + @target + N'''';
    EXEC sp_executesql @renameSql;
    PRINT 'Tabla renombrada hacia ProcesosSpertaHubSpot';
END
ELSE IF OBJECT_ID(N'dbo.' + @target, N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProcesosSpertaHubSpot (
        ProcesoId              BIGINT IDENTITY(1,1) NOT NULL,
        TenantId               NVARCHAR(64) NULL,
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
