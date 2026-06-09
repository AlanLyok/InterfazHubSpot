/*
  Limpieza greenfield — elimina objetos legacy antes del despliegue canónico.
  Ejecutar solo en entornos donde se acepta recrear cola/log HubSpot (dev/Calzetta).
*/

-- Tabla log legacy (nombre antiguo)
IF OBJECT_ID(N'dbo.IntegracionEjecucionLog', N'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.IntegracionEjecucionLog;
    PRINT 'Eliminada tabla legacy dbo.IntegracionEjecucionLog';
END
GO

-- Tabla cola con nombre legacy (pre-rename)
DECLARE @legacyCola SYSNAME = N'ProcesosSperta' + N'API';
IF OBJECT_ID(N'dbo.' + @legacyCola, N'U') IS NOT NULL
BEGIN
    DECLARE @dropLegacyCola NVARCHAR(500) = N'DROP TABLE dbo.' + QUOTENAME(@legacyCola);
    EXEC sp_executesql @dropLegacyCola;
    PRINT 'Eliminada tabla legacy cola pre-rename';
END
GO

-- Cola con esquema legacy (columnas *Utc o Identificador VARCHAR / FechaAlta)
IF OBJECT_ID(N'dbo.ProcesosSpertaHubSpot', N'U') IS NOT NULL
   AND (
        EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.ProcesosSpertaHubSpot') AND name = N'FechaCreacionUtc')
     OR EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.ProcesosSpertaHubSpot') AND name = N'FechaAlta')
     OR EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.ProcesosSpertaHubSpot') AND name = N'Id')
     OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.ProcesosSpertaHubSpot') AND name = N'ProcesoId')
   )
BEGIN
    DROP TABLE dbo.ProcesosSpertaHubSpot;
    PRINT 'Eliminada dbo.ProcesosSpertaHubSpot (esquema legacy) — se recreará en 001';
END
GO

-- Log con esquema legacy (FechaUtc / ClienteId como columna de log)
IF OBJECT_ID(N'dbo.ProcesosSpertaHubSpotLog', N'U') IS NOT NULL
   AND (
        EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.ProcesosSpertaHubSpotLog') AND name = N'FechaUtc')
     OR EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.ProcesosSpertaHubSpotLog') AND name = N'ClienteId')
     OR NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.ProcesosSpertaHubSpotLog') AND name = N'FechaGrabacion')
   )
BEGIN
    DROP TABLE dbo.ProcesosSpertaHubSpotLog;
    PRINT 'Eliminada dbo.ProcesosSpertaHubSpotLog (esquema legacy) — se recreará en 002';
END
GO

-- Función CC obsoleta (lógica inline en SP 004 y 006)
IF OBJECT_ID(N'dbo.InterfazHubSpot_ManejoCuentaCorriente_Texto', N'FN') IS NOT NULL
BEGIN
    DROP FUNCTION dbo.InterfazHubSpot_ManejoCuentaCorriente_Texto;
    PRINT 'Eliminada función obsoleta dbo.InterfazHubSpot_ManejoCuentaCorriente_Texto';
END
GO

PRINT 'Cleanup legacy HubSpot completado.';
GO
