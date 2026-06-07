/*
  InterfazHubSpot_Cliente_Obtener
  Devuelve 3 result sets para ClienteIntegracionManager / ClienteIntegracionMapper:
    1. Cabecera cliente (dbo.Clientes + lookups)
    2. Contactos (dbo.Clientes_Contactos)
    3. Direcciones de entrega TOP 3 (dbo.DireccionEntregas)

  Uso: EXEC dbo.InterfazHubSpot_Cliente_Obtener @ClienteId = 12345
*/

IF OBJECT_ID(N'dbo.InterfazHubSpot_Cliente_Obtener', N'P') IS NOT NULL
    DROP PROCEDURE dbo.InterfazHubSpot_Cliente_Obtener;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.InterfazHubSpot_Cliente_Obtener
    @ClienteId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Result set 1: cabecera
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
        ZonaId                 = ISNULL(zon.Descripcion, N''),
        VendedorId             = ISNULL(ven.Descripcion, N''),
        ResponsableCuentaId    = ISNULL(resp.Descripcion, N''),
        ListaPreciosId         = ISNULL(lp.Descripcion, N''),
        CondicionVentaId       = ISNULL(cv.Descripcion, N''),
        DiasParaDeuda          = CONVERT(VARCHAR(20), c.DiasParaDeuda),
        LimiteCredito          = CONVERT(VARCHAR(30), c.LimiteCredito),
        CategoriaClienteId     = ISNULL(cat.Descripcion, N'')
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

    -- Result set 2: contactos
    SELECT
        ClienteId          = cc.ClienteID,
        ApellidoYNombre    = cc.ApeyNom,
        CorreoElectronico  = cc.Email,
        Telefono           = cc.Telefono,
        SectorId           = ISNULL(sec.Descripcion, N'')
    FROM dbo.Clientes_Contactos AS cc
    LEFT JOIN dbo.SectoresDeContactosClientes AS sec
        ON sec.SectorID = cc.SectorID
    WHERE cc.ClienteID = @ClienteId;

    -- Result set 3: direcciones de entrega (máx. 3)
    SELECT TOP 3
        ClienteId      = de.ClienteID,
        Domicilio      = de.Domicilio,
        CodigoPostal   = de.CP,
        Localidad      = de.Localidad,
        ProvinciaId    = ISNULL(proDe.Descripcion, N'')
    FROM dbo.DireccionEntregas AS de
    LEFT JOIN dbo.Provincias AS proDe
        ON proDe.ProvinciaID = de.ProvinciaID
    WHERE de.ClienteID = @ClienteId
    ORDER BY
        de.Predeterminada DESC,
        de.DireccionID ASC;
END
GO
