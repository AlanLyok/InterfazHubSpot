/*
  InterfazHubSpot_ManejoCuentaCorriente_Texto
  Texto preformateado de cuenta corriente para un cliente (flujos 2A y 2B).
  Usada por InterfazHubSpot_Cliente_Obtener (004) y InterfazHubSpot_CuentaCorriente_Pagina (006).

  SELECT dbo.InterfazHubSpot_ManejoCuentaCorriente_Texto(298)
*/

IF OBJECT_ID(N'dbo.InterfazHubSpot_ManejoCuentaCorriente_Texto', N'FN') IS NOT NULL
    DROP FUNCTION dbo.InterfazHubSpot_ManejoCuentaCorriente_Texto;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE FUNCTION dbo.InterfazHubSpot_ManejoCuentaCorriente_Texto
(
    @ClienteId INT
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
    DECLARE @HoyTexto NVARCHAR(20) = CONVERT(VARCHAR(10), GETDATE(), 103);
    DECLARE @Result NVARCHAR(MAX);

    ;WITH
    ClienteUnico AS (
        SELECT c.ClienteID
        FROM dbo.clientes AS c
        WHERE c.ClienteID    = @ClienteId
          AND c.inhabilitado = 0
          AND EXISTS (
                SELECT 1
                FROM dbo.InterfazHubSpot_VendedoresHabilitados AS v
                WHERE v.VendedorID = c.VendedorID
              )
    ),
    DeudaDetalle AS (
        SELECT
            cu.ClienteID,
            cc.CodCompID,
            cc.Sucursal,
            cc.NroComp,
            cc.ValorizacionID,
            cc.FechaEMI,
            Saldo = CASE
                        WHEN cc.ValorizacionID < 3 THEN sv.LSaldo
                        ELSE                            sv.ESaldo
                    END
        FROM ClienteUnico AS cu
        INNER JOIN dbo.VeCtasCtes AS cc
            ON  cc.ClienteID = cu.ClienteID
            AND cc.DebeHaber = N'D'
        INNER JOIN dbo.Saldos_Comprob_Ventas AS sv
            ON  sv.EmpresaID = cc.EmpresaID
            AND sv.CodCompID = cc.CodCompID
            AND sv.Sucursal  = cc.Sucursal
            AND sv.NroComp   = cc.NroComp
        WHERE CASE
                  WHEN cc.ValorizacionID < 3 THEN sv.LSaldo
                  ELSE sv.ESaldo
              END <> 0
          AND (sv.LSaldo > 0.01 OR sv.ESaldo > 0.01)
    ),
    DeudaLineas AS (
        SELECT
            d.ClienteID,
            d.CodCompID,
            d.Sucursal,
            d.NroComp,
            FechaVencimiento = ISNULL(
                (
                    SELECT MIN(vto.FechaVto)
                    FROM dbo.VeComprobantes      AS vc
                    INNER JOIN dbo.VeComprobantesVto AS vto
                        ON vto.ComprobantesID = vc.ComprobantesID
                    WHERE vc.CodCompID = d.CodCompID
                      AND vc.Sucursal  = d.Sucursal
                      AND vc.NroComp   = d.NroComp
                ),
                d.FechaEMI
            ),
            d.Saldo
        FROM DeudaDetalle AS d
    ),
    DeudaTexto AS (
        SELECT
            dl.ClienteID,
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
    DeudaAgregada AS (
        SELECT
            dt.ClienteID,
            ManejoCuentaCorriente = STUFF((
                SELECT CHAR(10) + dt2.Linea
                FROM DeudaTexto AS dt2
                WHERE dt2.ClienteID = dt.ClienteID
                ORDER BY dt2.FechaVencimiento, dt2.Linea
                FOR XML PATH(''), TYPE
            ).value(N'.', N'NVARCHAR(MAX)'), 1, 1, N''),
            TotalTexto = N'Total: $'
                + REPLACE(REPLACE(REPLACE(
                    CONVERT(VARCHAR(30), CAST(SUM(ABS(dt.Saldo)) AS MONEY), 1),
                    N',', N'|'), N'.', N','), N'|', N'.')
        FROM DeudaTexto AS dt
        GROUP BY dt.ClienteID
    )
    SELECT @Result = CASE
        WHEN da.ManejoCuentaCorriente IS NOT NULL
            THEN N'Cuenta Corriente al ' + @HoyTexto
                 + CHAR(10) + CHAR(10)
                 + da.ManejoCuentaCorriente
                 + CHAR(10) + CHAR(10)
                 + da.TotalTexto
        ELSE
            N'Cuenta Corriente al ' + @HoyTexto + N'. Deuda: $0'
    END
    FROM ClienteUnico AS cu
    LEFT JOIN DeudaAgregada AS da
        ON da.ClienteID = cu.ClienteID;

    RETURN @Result;  -- NULL si el cliente no pasa el filtro
END
GO
