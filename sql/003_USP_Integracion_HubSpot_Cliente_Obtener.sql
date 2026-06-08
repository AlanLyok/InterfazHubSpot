/*
  Copia versionada alineada con scriptsSQL/004_InterfazHubSpot_Cliente_Obtener.sql
  Devuelve 2 result sets: cabecera + direcciones TOP 3 (sin contactos).
*/

IF OBJECT_ID(N'dbo.USP_Integracion_HubSpot_Cliente_Obtener', N'P') IS NOT NULL
    DROP PROCEDURE dbo.USP_Integracion_HubSpot_Cliente_Obtener;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.USP_Integracion_HubSpot_Cliente_Obtener
    @ClienteId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ClienteId              = c.ClienteID,
        CodigoCliente          = c.CodCli,
        RazonSocial            = c.RazonSocial,
        ApellidoYNombre        = c.ApeyNom,
        Contacto               = c.Contacto,
        NumeroDocumento        = c.NroDocumento,
        Calle                  = c.Calle,
        Puerta                 = CONVERT(VARCHAR(20), c.Puerta),
        Localidad              = c.Localidad,
        CodigoPostal           = c.CodPostal,
        CodigoProvinciaCliente = ISNULL(pro.Descripcion, N''),
        CodigoPais             = ISNULL(pai.Descripcion, N''),
        Zona                   = ISNULL(zon.Descripcion, N''),
        Vendedor               = ISNULL(ven.Descripcion, N''),
        ResponsableCuenta      = ISNULL(resp.Descripcion, N''),
        ListaPrecios           = ISNULL(lp.Descripcion, N''),
        CondicionVenta         = ISNULL(cv.Descripcion, N''),
        DiasParaDeuda          = CONVERT(VARCHAR(20), c.DiasParaDeuda),
        LimiteCredito          = CONVERT(VARCHAR(30), c.LimiteCredito),
        CategoriaCliente       = ISNULL(cat.Descripcion, N''),
        ManejoCuentaCorriente  = dbo.InterfazHubSpot_ManejoCuentaCorriente_Texto(@ClienteId)
    FROM dbo.Clientes AS c
    LEFT JOIN dbo.Provincias AS pro
        ON pro.ProvinciaID = c.ProvinciaID
    LEFT JOIN dbo.PAISA AS pai
        ON pai.PaisID = c.CodPais
    LEFT JOIN dbo.Zonas AS zon
        ON zon.ZonaID = c.ZonaId
    LEFT JOIN dbo.Vendedores AS ven
        ON ven.VendedorID = c.VendedorId
    LEFT JOIN dbo.Vendedores AS resp
        ON resp.VendedorID = c.ResponsableCuentaID
    LEFT JOIN dbo.ListaDePrecios AS lp
        ON lp.ListaID = c.ListaID
    LEFT JOIN dbo.CondVenta AS cv
        ON cv.CondVentaID = c.CondVentaID
    LEFT JOIN dbo.CategClientes AS cat
        ON cat.CategClienteID = c.CategClienteID
    WHERE c.ClienteID = @ClienteId;

    SELECT TOP 3
        ClienteId      = de.ClienteID,
        Domicilio      = de.Domicilio,
        CodigoPostal   = de.CP,
        Localidad      = de.Localidad,
        Provincia      = ISNULL(proDe.Descripcion, N''),
        Pais           = ISNULL(paisDe.Descripcion, N'')
    FROM dbo.DireccionEntregas AS de
    LEFT JOIN dbo.Provincias AS proDe
        ON proDe.ProvinciaID = de.ProvinciaID
    LEFT JOIN dbo.Cuitpais AS paisDe
        ON paisDe.PaisID = de.PaisID
    WHERE de.ClienteID = @ClienteId
    ORDER BY
        de.Predeterminada DESC,
        de.DireccionID ASC;
END
GO
