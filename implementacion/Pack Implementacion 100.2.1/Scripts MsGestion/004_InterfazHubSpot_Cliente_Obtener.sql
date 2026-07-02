/*---------------------------------------------------------------------------------------------------  
Modificado por    : Alan Lipshitz     
Fecha             : 29/06/2026
Incidente/Actividad      : (19512)
Descripcion       : Se crea el store para la InterfazHubSpot
---------------------------------------------------------------------------------------------------*/  
 -- EXEC dbo.InterfazHubSpot_Cliente_Obtener @ClienteId = 77


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

    DECLARE @HoyTexto              NVARCHAR(20)  = CONVERT(VARCHAR(10), GETDATE(), 103);
    DECLARE @ManejoCuentaCorriente NVARCHAR(MAX);

    -- ── Bloque 1: pre-calcular ManejoCuentaCorriente ──────────────────────
    -- Se resuelve en un solo barrido de las tablas de deuda para @ClienteId.
    -- Sin ClienteID en los CTEs porque ya está filtrado por el parámetro.
    ;WITH
    DeudaDetalle AS (
        SELECT
            cc.CodCompID,
            cc.Sucursal,
            cc.NroComp,
            cc.FechaEMI,
            Saldo = CASE
                        WHEN cc.ValorizacionID < 3 THEN sv.LSaldo
                        ELSE                            sv.ESaldo
                    END
        FROM dbo.VeCtasCtes AS cc
        INNER JOIN dbo.Saldos_Comprob_Ventas AS sv
            ON  sv.EmpresaID = cc.EmpresaID
            AND sv.CodCompID = cc.CodCompID
            AND sv.Sucursal  = cc.Sucursal
            AND sv.NroComp   = cc.NroComp
        WHERE cc.ClienteID = @ClienteId
          AND cc.DebeHaber = N'D'
          AND CASE
                  WHEN cc.ValorizacionID < 3 THEN sv.LSaldo
                  ELSE sv.ESaldo
              END <> 0
          AND (sv.LSaldo > 0.01 OR sv.ESaldo > 0.01)
    ),
    DeudaLineas AS (
        SELECT
            d.CodCompID,
            d.Sucursal,
            d.NroComp,
            FechaVencimiento = ISNULL(fv.FechaVto, d.FechaEMI),
            d.Saldo
        FROM DeudaDetalle AS d
        OUTER APPLY (
            SELECT FechaVto = MIN(vto.FechaVto)
            FROM dbo.VeComprobantes      AS vc
            INNER JOIN dbo.VeComprobantesVto AS vto
                ON vto.ComprobantesID = vc.ComprobantesID
            WHERE vc.CodCompID = d.CodCompID
              AND vc.Sucursal  = d.Sucursal
              AND vc.NroComp   = d.NroComp
        ) AS fv
    ),
    DeudaTexto AS (
        SELECT
            dl.FechaVencimiento,
            dl.Saldo,
            Linea =
                ISNULL(cmp.Abreviatura, CAST(dl.CodCompID AS VARCHAR(10)))
                + N' '
                + RIGHT(N'0000'     + CAST(dl.Sucursal AS VARCHAR(10)), 4)
                + N'-'
                + RIGHT(N'00000000' + CAST(dl.NroComp  AS VARCHAR(10)), 8)
                + N' / VTO:'
                + CONVERT(VARCHAR(10), dl.FechaVencimiento, 103)
                + N' --- $'
                + REPLACE(REPLACE(REPLACE(
                    CONVERT(VARCHAR(30), CAST(ABS(dl.Saldo) AS MONEY), 1),
                    N',', N'|'), N'.', N','), N'|', N'.')
        FROM DeudaLineas AS dl
        LEFT JOIN dbo.ComprobVen AS cmp
            ON cmp.CodCompID = dl.CodCompID
    ),
    -- Sin GROUP BY: agregación implícita sobre todas las filas del cliente.
    -- Si DeudaTexto está vacía, devuelve 1 fila con Lineas = NULL → ELSE.
    DeudaAgregada AS (
        SELECT
            Lineas     = STUFF((
                SELECT CHAR(10) + dt2.Linea
                FROM DeudaTexto AS dt2
                ORDER BY dt2.FechaVencimiento, dt2.Linea
                FOR XML PATH(''), TYPE
            ).value(N'.', N'NVARCHAR(MAX)'), 1, 1, N''),
            TotalTexto = N'Deuda total: $'
                + REPLACE(REPLACE(REPLACE(
                    CONVERT(VARCHAR(30), CAST(SUM(ABS(dt.Saldo)) AS MONEY), 1),
                    N',', N'|'), N'.', N','), N'|', N'.')
        FROM DeudaTexto AS dt
    )
    SELECT @ManejoCuentaCorriente = CASE
        WHEN da.Lineas IS NOT NULL AND da.Lineas <> N''
            THEN N'Cuenta Corriente al ' + @HoyTexto
                 + CHAR(10) + CHAR(10)
                 + da.Lineas
                 + CHAR(10) + CHAR(10)
                 + da.TotalTexto
        ELSE
            N'Cuenta Corriente al ' + @HoyTexto + N'. Deuda: $0'
    END
    FROM DeudaAgregada AS da;

    -- Fallback por si el cliente no existe en VeCtasCtes en absoluto
    IF @ManejoCuentaCorriente IS NULL
        SET @ManejoCuentaCorriente = N'Cuenta Corriente al ' + @HoyTexto + N'. Deuda: $0';

    -- ── Result set 1: cabecera ────────────────────────────────────────────
    SELECT
        ClienteId              = c.ClienteID,
        CodigoCliente          = c.CodCli,
        RazonSocial            = c.RazonSocial,
        ApellidoYNombre        = c.ApeyNom,
        Contacto               = c.Contacto,
        NumeroDocumento        = REPLACE(REPLACE(REPLACE(c.NroDocumento, '-',''), '.',''), ',',''),
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
        ManejoCuentaCorriente  = @ManejoCuentaCorriente
    FROM dbo.Clientes AS c
    LEFT JOIN dbo.Provincias       AS pro  ON pro.ProvinciaID    = c.ProvinciaID
    LEFT JOIN dbo.PAISA            AS pai  ON pai.PaisID          = c.CodPais
    LEFT JOIN dbo.Zonas            AS zon  ON zon.ZonaID          = c.ZonaId
    LEFT JOIN dbo.Vendedores       AS ven  ON ven.VendedorID      = c.VendedorId
    LEFT JOIN dbo.ResponsableCuenta       AS resp ON resp.ID     = c.ResponsableCuentaID
    LEFT JOIN dbo.ListaDePrecios   AS lp   ON lp.ListaID          = c.ListaID
    LEFT JOIN dbo.CondVenta        AS cv   ON cv.CondVentaID      = c.CondVentaID
    LEFT JOIN dbo.CategClientes    AS cat  ON cat.CategClienteID  = c.CategClienteID
    WHERE c.ClienteID    = @ClienteId
      AND c.inhabilitado = 0
      AND EXISTS (
            SELECT 1
            FROM dbo.InterfazHubSpot_VendedoresHabilitados AS v
            WHERE v.VendedorID = c.VendedorID
          );

    -- ── Result set 2: direcciones de entrega (máx. 3) ─────────────────────
    SELECT TOP 3
        ClienteId    = de.ClienteID,
        Domicilio    = de.Domicilio,
        CodigoPostal = de.CP,
        Localidad    = de.Localidad,
        Provincia    = ISNULL(proDe.Descripcion, N''),
        Pais         = ISNULL(paisDe.Descripcion, N'')
    FROM dbo.DireccionEntregas AS de
    LEFT JOIN dbo.Provincias AS proDe  ON proDe.ProvinciaID = de.ProvinciaID
    LEFT JOIN dbo.Cuitpais   AS paisDe ON paisDe.PaisID     = de.PaisID
    INNER JOIN dbo.Clientes  AS cl
        ON  cl.ClienteID    = de.ClienteID
        AND cl.inhabilitado = 0
        AND EXISTS (
              SELECT 1
              FROM dbo.InterfazHubSpot_VendedoresHabilitados AS v
              WHERE v.VendedorID = cl.VendedorID
            )
    WHERE de.ClienteID = @ClienteId
    ORDER BY de.Predeterminada DESC, de.DireccionID ASC;
END
GO
