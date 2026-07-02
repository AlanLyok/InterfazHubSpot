/*---------------------------------------------------------------------------------------------------  
Modificado por			: Alan Lipshitz     
Fecha					: 29/06/2026
Incidente/Actividad     : (19512)
Descripcion				: Se crea el store para la InterfazHubSpot
---------------------------------------------------------------------------------------------------*/  

IF OBJECT_ID(N'dbo.InterfazHubSpot_VendedoresHabilitados', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.InterfazHubSpot_VendedoresHabilitados
    (
        VendedorID INT      NOT NULL,
        FechaAlta  DATETIME NOT NULL CONSTRAINT DF_IHS_VH_FechaAlta DEFAULT GETDATE(),
        CONSTRAINT PK_InterfazHubSpot_VendedoresHabilitados
            PRIMARY KEY CLUSTERED (VendedorID)
    );
    PRINT 'Tabla creada: dbo.InterfazHubSpot_VendedoresHabilitados';
END
ELSE
BEGIN
    PRINT 'Tabla dbo.InterfazHubSpot_VendedoresHabilitados ya existe — sin cambios.';
END
GO

INSERT INTO dbo.InterfazHubSpot_VendedoresHabilitados (VendedorID)
SELECT v.VendedorID
FROM (VALUES (107), (37), (91)) AS v(VendedorID)
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.InterfazHubSpot_VendedoresHabilitados AS h
    WHERE h.VendedorID = v.VendedorID
);
GO
