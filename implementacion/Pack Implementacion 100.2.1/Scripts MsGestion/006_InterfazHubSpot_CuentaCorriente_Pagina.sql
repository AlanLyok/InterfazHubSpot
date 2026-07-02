/*---------------------------------------------------------------------------------------------------  
Modificado por    : Alan Lipshitz     
Fecha             : 29/06/2026
Incidente/Actividad      : (19512)
Descripcion       : Se crea el store para la InterfazHubSpot
---------------------------------------------------------------------------------------------------*/  


/*
  InterfazHubSpot_CuentaCorriente_Pagina  (v3 — filtro vendedores)
  ─────────────────────────────────────────────────────────────────────────────
  Paginación keyset de clientes activos con texto preformateado para HubSpot
  (propiedad manejo_cuenta_corriente — flujo 2B).

  RESULTADO POR CLIENTE
  ─────────────────────
  Con deuda:                               
    Cuenta Corriente al 07/06/2026         
                                           
    FAC 0001-00067143 / VTO:15/03/2026 --- $1.234,56
    FAC 0001-00001234 / VTO:02/04/2026 --- $89.200,50
                                           
    Deuda total: $90.435,06

 Sin deuda:
	Cuenta actualizada al 07/06/2026. Deuda: $0


  PAGINACIÓN KEYSET
  ─────────────────
  @Cursor   — ClienteID exclusivo desde donde paginar (0 = primera llamada).
  @PageSize — El caller C# pasa pageSize+1 (ej. 101) para detectar si hay más:
                rows.Count == 101  → hay más; enviar primeros 100 a HubSpot,
                                     próximo cursor = rows[99].ClienteID
                rows.Count  < 101  → última página; enviar todo y terminar.

  EJEMPLO DE USO
  ──────────────
EXEC dbo.InterfazHubSpot_CuentaCorriente_Pagina @Cursor = 0, @PageSize = 101
EXEC dbo.InterfazHubSpot_CuentaCorriente_Pagina @Cursor = 5977, @PageSize = 101
EXEC dbo.InterfazHubSpot_CuentaCorriente_Pagina @Cursor = 10714, @PageSize = 101
EXEC dbo.InterfazHubSpot_CuentaCorriente_Pagina @Cursor = 22876, @PageSize = 101
EXEC dbo.InterfazHubSpot_CuentaCorriente_Pagina @Cursor = 31505, @PageSize = 101
EXEC dbo.InterfazHubSpot_CuentaCorriente_Pagina @Cursor = 37960, @PageSize = 101
EXEC dbo.InterfazHubSpot_CuentaCorriente_Pagina @Cursor = 42029, @PageSize = 101
EXEC dbo.InterfazHubSpot_CuentaCorriente_Pagina @Cursor = 48643, @PageSize = 101
EXEC dbo.InterfazHubSpot_CuentaCorriente_Pagina @Cursor = 59977, @PageSize = 101
EXEC dbo.InterfazHubSpot_CuentaCorriente_Pagina @Cursor = 71419, @PageSize = 101
EXEC dbo.InterfazHubSpot_CuentaCorriente_Pagina @Cursor = 78224, @PageSize = 101
EXEC dbo.InterfazHubSpot_CuentaCorriente_Pagina @Cursor = 83885, @PageSize = 101
EXEC dbo.InterfazHubSpot_CuentaCorriente_Pagina @Cursor = 87794, @PageSize = 101
EXEC dbo.InterfazHubSpot_CuentaCorriente_Pagina @Cursor = 104006, @PageSize = 101
EXEC dbo.InterfazHubSpot_CuentaCorriente_Pagina @Cursor = 126659, @PageSize = 101



  
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

    -- Defaults de seguridad
    IF @PageSize IS NULL OR @PageSize <= 0
        SET @PageSize = 101;
    IF @Cursor IS NULL OR @Cursor < 0
        SET @Cursor = 0;

    DECLARE @HoyTexto NVARCHAR(20) = CONVERT(VARCHAR(10), GETDATE(), 103);

    ;WITH

    /* ─── 1. Página de clientes ─────────────────────────────────────────────
       TOP keyset: devuelve hasta @PageSize clientes activos con ClienteID
       estrictamente mayor que el cursor.  El caller envía @PageSize = 101;
       si recibe 101 filas sabe que hay más y usa la fila 100 como nuevo cursor.
    ─────────────────────────────────────────────────────────────────────── */
    PaginaClientes AS (
        SELECT TOP (@PageSize)
            c.ClienteID
        FROM dbo.Clientes AS c
        WHERE c.InHabilitado = 0
          AND c.ClienteID > @Cursor
          AND EXISTS (                             
                SELECT 1
                FROM dbo.InterfazHubSpot_VendedoresHabilitados AS v
                WHERE v.VendedorID = c.VendedorID
              )
        ORDER BY c.ClienteID
    ),

    /* ─── 2. Comprobantes impagos de esa página ──────────────────────────── */
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
                        ELSE                            sv.ESaldo
                    END
        FROM PaginaClientes AS pc
        INNER JOIN dbo.VeCtasCtes AS cc
            ON  cc.ClienteID = pc.ClienteID
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

    /* ─── 3. Fecha de vencimiento (mín. de VeComprobantesVto, o FechaEMI) ── */
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

    /* ─── 4. Línea de texto formateada ──────────────────────────────────────
       Formato final: "FAC 0001-00067143 / VTO:15/03/2026 --- $1.234,56"
       • Abreviatura  → dbo.ComprobVen.Abreviatura (por CodCompID)
       • Sucursal zero-padded a 4 dígitos
       • NroComp  zero-padded a 8 dígitos
    ─────────────────────────────────────────────────────────────────────── */
    DeudaTexto AS (
        SELECT
            dl.ClienteID,
            dl.FechaVencimiento,   -- retenida para ordenar las líneas
            dl.Saldo,              -- retenido para sumar el total en DeudaAgregada
            Linea =
                -- ── Referencia del comprobante ───────────────────────────
                ISNULL(cmp.Abreviatura, CAST(dl.CodCompID AS VARCHAR(10)))
                + N' '
                + RIGHT(N'0000'     + CAST(dl.Sucursal AS VARCHAR(10)), 4)
                + N'-'
                + RIGHT(N'00000000' + CAST(dl.NroComp  AS VARCHAR(10)), 8)
                -- ── Fecha de vencimiento y saldo ─────────────────────────
                + N' / VTO:'
                + CONVERT(VARCHAR(10), dl.FechaVencimiento, 103)
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
        LEFT JOIN dbo.ComprobVen AS cmp
            ON cmp.CodCompID = dl.CodCompID
    ),

    /* ─── 5. Concatenación de líneas por cliente (orden: vencimiento asc.) ─ */
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
            -- Total formateado: suma de todos los saldos impagos del cliente
            TotalTexto = N'Deuda total: $'
                + REPLACE(
                    REPLACE(
                        REPLACE(
                            CONVERT(VARCHAR(30), CAST(SUM(ABS(dt.Saldo)) AS MONEY), 1),
                            N',', N'|'
                        ),
                        N'.', N','
                    ),
                    N'|', N'.'
                )
        FROM DeudaTexto AS dt
        GROUP BY dt.ClienteID
    )

    /* ─── 6. Resultado final ────────────────────────────────────────────────
       Clientes con deuda  → texto multilínea con comprobantes.
       Clientes sin deuda  → "Cuenta actualizada al DD/MM/YYYY. Deuda: $0"
       (el campo nunca se vacía, según diseño del flujo 2B)
    ─────────────────────────────────────────────────────────────────────── */
    SELECT
        ClienteId             = pc.ClienteID,
        NumeroDocumento       = REPLACE(REPLACE(REPLACE(c.NroDocumento, '-',''), '.',''), ',',''),
        ManejoCuentaCorriente = CASE
            WHEN da.ManejoCuentaCorriente IS NOT NULL
                -- Con deuda:
                --   Cuenta actualizada al DD/MM/YYYY
                --   <línea en blanco>
                --   FAC 0001-00067143 / VTO:15/03/2026 --- $1.234,56
                --   FAC 0001-00001234 / VTO:02/04/2026 --- $89.200,50
                --   <línea en blanco>
                --   Deuda total: $90.435,06
                THEN N'Cuenta Corriente al ' + @HoyTexto
                     + CHAR(10) + CHAR(10)
                     + da.ManejoCuentaCorriente
                     + CHAR(10) + CHAR(10)
                     + da.TotalTexto
            ELSE
                -- Sin deuda: una sola línea
                N'Cuenta Corriente al ' + @HoyTexto + N'. Deuda: $0'
        END
    FROM PaginaClientes AS pc
    INNER JOIN dbo.Clientes AS c
        ON c.ClienteID = pc.ClienteID
    LEFT JOIN DeudaAgregada AS da
        ON da.ClienteID = pc.ClienteID
    ORDER BY pc.ClienteID;

END
GO