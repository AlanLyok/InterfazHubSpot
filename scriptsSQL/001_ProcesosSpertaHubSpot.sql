/*
  Cola outbox HubSpot — dbo.ProcesosSpertaHubSpot
  Alineado con InterfazHubSpot.Entities.ProcesosSpertaHubSpot (EF6).

  GATE WinForms (antes de sp_rename desde ProcesosSpertaAPI):
  1. Desplegar 003_USER_POS_Clientes_Agregar.sql
  2. Verificar post-grabación WinForms invoca USER_POS_Clientes_Agregar
  3. Coordinar ventana de mantenimiento si aplica
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
        FechaCreacionUtc       DATETIME2 NOT NULL,
        FechaInicioProcesoUtc  DATETIME2 NULL,
        FechaFinProcesoUtc     DATETIME2 NULL,
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
        ON dbo.ProcesosSpertaHubSpot (Destino, Estado, FechaCreacionUtc)
        INCLUDE (Identificador, TipoEntidad, Intentos);
    PRINT 'Indice IX_ProcesosSpertaHubSpot_DestinoEstadoFecha creado.';
END
GO
