/*
  USP_Integracion_HubSpot_CuentaCorriente_Pagina
  Devuelve 1 result set paginado por cursor (keyset pagination) para
  hidratar el batch de sincronización de cuenta corriente (flujo 2B).

  Paginación keyset:
    - Filtra ClienteId > @Cursor para avanzar al siguiente bloque.
    - Devuelve TOP (@PageSize + 1) filas para que el consumer en C# detecte
      si hay más páginas:
        * Si se devuelven @PageSize + 1 filas -> HayMas = true,
          SiguienteCursor = último ClienteId del bloque (@PageSize filas).
        * Si se devuelven <= @PageSize filas   -> HayMas = false, fin de paginación.
    - El SP NO calcula HayMas ni SiguienteCursor; esa lógica vive en HubSpotSincronizarCuentaCorrienteJob.

  Uso: EXEC dbo.USP_Integracion_HubSpot_CuentaCorriente_Pagina @Cursor = 0, @PageSize = 500

  Primera llamada: @Cursor = 0 (devuelve desde el primer ClienteId existente).

  NOTA: todos los nombres de tabla y columna están marcados con -- CONFIRMAR.
        Revisar contra el esquema real de MSGestion antes de poner en producción.
*/

IF OBJECT_ID('dbo.USP_Integracion_HubSpot_CuentaCorriente_Pagina', 'P') IS NOT NULL
    DROP PROCEDURE dbo.USP_Integracion_HubSpot_CuentaCorriente_Pagina;
GO

CREATE PROCEDURE dbo.USP_Integracion_HubSpot_CuentaCorriente_Pagina
    @Cursor   INT,   -- ClienteId desde el cual paginar (exclusive); usar 0 en la primera llamada
    @PageSize INT    -- cantidad de filas del bloque (el SP devuelve hasta @PageSize + 1)
AS
BEGIN
    SET NOCOUNT ON;

    -- =========================================================
    -- Result set 1: Página de clientes con su flag de cuenta corriente
    -- Tabla origen: dbo.Clientes -- CONFIRMAR: nombre de tabla real en MSGestion
    -- =========================================================
    SELECT TOP (@PageSize + 1)
        c.ClienteId,                        -- CONFIRMAR: columna PK en Clientes
        c.ManejoCuentaCorriente             -- CONFIRMAR: columna/flag que indica si el cliente
                                            --            maneja cuenta corriente (BIT, CHAR, etc.)
                                            --            Mapea a HubSpot property 'manejo_cuenta_corriente'
    FROM
        dbo.Clientes AS c                   -- CONFIRMAR: nombre de tabla real en MSGestion
    WHERE
        c.ClienteId > @Cursor               -- CONFIRMAR: nombre de columna PK para el keyset cursor
    ORDER BY
        c.ClienteId ASC;                    -- CONFIRMAR: orden ascendente por PK para paginación estable
END
GO
