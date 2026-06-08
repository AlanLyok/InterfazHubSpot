IF OBJECT_ID(N'dbo.InterfazHubSpot_VendedoresHabilitados', N'U') IS NOT NULL
    DROP TABLE dbo.InterfazHubSpot_VendedoresHabilitados;
GO

CREATE TABLE dbo.InterfazHubSpot_VendedoresHabilitados
(
    VendedorID      INT           NOT NULL,
    Descripcion     NVARCHAR(100) NULL,        -- opcional, para documentar
    FechaAlta       DATETIME      NOT NULL CONSTRAINT DF_IHS_VH_FechaAlta DEFAULT GETDATE(),
    UsuarioAlta     NVARCHAR(50)  NULL,

    CONSTRAINT PK_InterfazHubSpot_VendedoresHabilitados 
        PRIMARY KEY CLUSTERED (VendedorID)
);
GO

INSERT INTO dbo.InterfazHubSpot_VendedoresHabilitados (VendedorID, Descripcion, UsuarioAlta)
VALUES
    (107, NULL, SYSTEM_USER),
    ( 37, NULL, SYSTEM_USER),
    ( 91, NULL, SYSTEM_USER);
GO