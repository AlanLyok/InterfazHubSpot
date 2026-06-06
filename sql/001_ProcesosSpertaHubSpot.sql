/*
  Migración tabla cola hacia dbo.ProcesosSpertaHubSpot

  GATE WinForms (OBLIGATORIO antes de sp_rename):
  1. Desplegar sql/002_USER_POS_Clientes_Agregar.sql (INSERT en ProcesosSpertaHubSpot).
  2. Verificar en WinForms Calzetta que post-grabación invoca USER_POS_Clientes_Agregar.
  3. Confirmar OBJECT_DEFINITION del SP activo en servidor apunta a ProcesosSpertaHubSpot.
  4. Coordinar ventana de mantenimiento si hace falta pausar altas de clientes.
  5. SOLO ENTONCES ejecutar el sp_rename de esta migración.

  El batch .NET (EF ToTable ProcesosSpertaHubSpot) debe desplegarse antes o en la misma ventana.
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
        ProcesoId              BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
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
        FechaFinProcesoUtc     DATETIME2 NULL
    );
    PRINT 'Tabla creada: dbo.ProcesosSpertaHubSpot';
END
ELSE
BEGIN
    PRINT 'Tabla dbo.ProcesosSpertaHubSpot ya existe — sin cambios.';
END
GO
