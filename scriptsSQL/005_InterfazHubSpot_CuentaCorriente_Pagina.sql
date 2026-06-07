/*
  InterfazHubSpot_CuentaCorriente_Pagina
  Paginación keyset de clientes activos con texto preformateado para HubSpot
  (propiedad manejo_cuenta_corriente — flujo 2B).

  @Cursor   ClienteID exclusive; 0 en la primera llamada.
  @PageSize El caller C# envía pageSize+1 para detectar HayMas.

  Uso: EXEC dbo.InterfazHubSpot_CuentaCorriente_Pagina @Cursor = 0, @PageSize = 50
*/

IF OBJECT_ID(N'dbo.InterfazHubSpot_CuentaCorriente_Pagina', N'P') IS NOT NULL
    DROP PROCEDURE dbo.InterfazHubSpot_CuentaCorriente_Pagina;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.InterfazHubSpot_CuentaCorriente_Pagina
    @Cursor   INT,
    @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageSize IS NULL OR @PageSize <= 0
        SET @PageSize = 501;

    IF @Cursor IS NULL OR @Cursor < 0
        SET @Cursor = 0;

    DECLARE @HoyTexto NVARCHAR(20) = CONVERT(VARCHAR(10), GETDATE(), 103);

    ;WITH PaginaClientes AS (
        SELECT TOP (@PageSize)
            c.ClienteID
        FROM dbo.Clientes AS c
        WHERE c.InHabilitado = 0
          AND c.ClienteID > @Cursor
        ORDER BY c.ClienteID
    ),
    DeudaDetalle AS (
        SELECT
            pc.ClienteID,
            cc.CodCompID,
            cc.Sucursal,
            cc.NroComp,
            cc.ValorizacionID,
            cc.FechaEMI,
            Saldo = CASE
                WHEN cc.ValorizacionID < 3 THEN sv.LSaldo
                ELSE sv.ESaldo
            END
        FROM PaginaClientes AS pc
        INNER JOIN dbo.VeCtasCtes AS cc
            ON cc.ClienteID = pc.ClienteID
           AND cc.DebeHaber = N'D'
        INNER JOIN dbo.Saldos_Comprob_Ventas AS sv
            ON sv.EmpresaID = cc.EmpresaID
           AND sv.CodCompID = cc.CodCompID
           AND sv.Sucursal = cc.Sucursal
           AND sv.NroComp = cc.NroComp
        WHERE CASE
                WHEN cc.ValorizacionID < 3 THEN sv.LSaldo
                ELSE sv.ESaldo
              END <> 0
    ),
    DeudaLineas AS (
        SELECT
            d.ClienteID,
            FechaVencimiento = ISNULL(
                (
                    SELECT MIN(vto.FechaVto)
                    FROM dbo.VeComprobantes AS vc
                    INNER JOIN dbo.VeComprobantesVto AS vto
                        ON vto.ComprobantesID = vc.ComprobantesID
                    WHERE vc.CodCompID = d.CodCompID
                      AND vc.Sucursal = d.Sucursal
                      AND vc.NroComp = d.NroComp
                ),
                d.FechaEMI
            ),
            d.Saldo
        FROM DeudaDetalle AS d
    ),
    DeudaTexto AS (
        SELECT
            dl.ClienteID,
            Linea = CONVERT(VARCHAR(10), dl.FechaVencimiento, 103)
                + N' --- $'
                + REPLACE(
                    REPLACE(
                        REPLACE(
                            CONVERT(VARCHAR(30), CAST(ABS(dl.Saldo) AS MONEY), 1),
                            N',', N'|'
                        ),
                        N'.', N','
                    ),
                    N'|', N'.'
                )
        FROM DeudaLineas AS dl
    ),
    DeudaAgregada AS (
        SELECT
            dt.ClienteID,
            ManejoCuentaCorriente = STUFF((
                SELECT CHAR(10) + dt2.Linea
                FROM DeudaTexto AS dt2
                WHERE dt2.ClienteID = dt.ClienteID
                ORDER BY dt2.Linea
                FOR XML PATH(''), TYPE
            ).value(N'.', N'NVARCHAR(MAX)'), 1, 1, N'')
        FROM DeudaTexto AS dt
        GROUP BY dt.ClienteID
    )
    SELECT
        ClienteId              = pc.ClienteID,
        ManejoCuentaCorriente  = COALESCE(
            da.ManejoCuentaCorriente,
            N'Cuenta actualizada al ' + @HoyTexto + N'. Deuda: $0'
        )
    FROM PaginaClientes AS pc
    LEFT JOIN DeudaAgregada AS da
        ON da.ClienteID = pc.ClienteID
    ORDER BY pc.ClienteID;
END
GO
