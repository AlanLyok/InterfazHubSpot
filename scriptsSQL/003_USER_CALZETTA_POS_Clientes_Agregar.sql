/*
  Post-grabación WinForms — encola cliente para sincronización HubSpot (flujo 2A).

  Uso:
    EXEC dbo.USER_POS_Clientes_Agregar @ClienteID = 77;
	
	
 
*/

IF OBJECT_ID(N'dbo.USER_POS_Clientes_Agregar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.USER_POS_Clientes_Agregar;
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.USER_POS_Clientes_Agregar
    @ClienteID INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @ClienteID IS NULL OR @ClienteID <= 0
        RETURN;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Clientes AS c
        WHERE c.ClienteID = @ClienteID
		AND c.VendedorID not in (107,37,91)
    )
        RETURN;

    IF EXISTS (
        SELECT 1
        FROM dbo.ProcesosSpertaHubSpot AS p
        WHERE p.Destino = N'HubSpot'
          AND p.TipoEntidad = N'Cliente'
          AND p.Identificador = @ClienteID
          AND p.Estado = 0
		  
    )
        RETURN;

    INSERT INTO dbo.ProcesosSpertaHubSpot (
        TenantId,
        EmpresaId,
        Destino,
        TipoEntidad,
        TipoOperacion,
        Identificador,
        Estado,
        Intentos,
        MensajeUltimoError,
        FechaCreacion,
        FechaInicioProceso,
        FechaFinProceso
    )
    VALUES (
        N'MS',
        1,
        N'HubSpot',
        N'Cliente',
        N'Alta/Modificacion',
        @ClienteID,
        0,
        0,
        NULL,
        GETDATE(),
        NULL,
        NULL
    );
END
GO
