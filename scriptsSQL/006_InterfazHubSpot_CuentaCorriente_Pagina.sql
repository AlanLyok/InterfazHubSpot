/*
  InterfazHubSpot_CuentaCorriente_Pagina  (v2)
  ─────────────────────────────────────────────────────────────────────────────
  Paginación keyset de clientes activos con texto preformateado para HubSpot
  (propiedad manejo_cuenta_corriente — flujo 2B).

  FORMATO DE LÍNEA CON DEUDA
  ──────────────────────────
      FAC 0001-00067143 / 15/03/2026 --- $1.234,56
      │   │    └─ NroComp  (8 dígitos, zero-pad)
      │   └────── Sucursal (4 dígitos, zero-pad)
      └────────── Abrev de ComprobCom (por CodCompID)
                                        └─ fecha vcto (o FechaEMI si sin vto)
                                                        └─ saldo ARS

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

    ;WITH
    PaginaClientes AS (
        SELECT TOP (@PageSize)
            c.ClienteID
        FROM dbo.Clientes AS c
        WHERE c.InHabilitado = 0
          AND c.ClienteID    > @Cursor
          AND EXISTS (                                    
                SELECT 1
                FROM dbo.InterfazHubSpot_VendedoresHabilitados AS v
                WHERE v.VendedorID = c.VendedorID
              )
        ORDER BY c.ClienteID
    )
    SELECT
        ClienteId             = pc.ClienteID,
        ManejoCuentaCorriente = dbo.InterfazHubSpot_ManejoCuentaCorriente_Texto(pc.ClienteID)
    FROM PaginaClientes AS pc
    ORDER BY pc.ClienteID;
END
GO