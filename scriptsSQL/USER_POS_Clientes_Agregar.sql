/*
-- Ejemplos (descomentar en SSMS)

EXEC dbo.USER_POS_Clientes_Agregar @ClienteID = 12345, @TipoOperacion = N'Alta';

EXEC dbo.USER_POS_Clientes_Agregar   @ClienteID = 5

EXEC dbo.USER_POS_Clientes_Agregar
    @ClienteID = 999,
    @TipoOperacion = N'alta',
    @Destino = N'HubSpot';

	Select * from clientes order by clienteid 
*/


IF OBJECT_ID(N'dbo.USER_POS_Clientes_Agregar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.USER_POS_Clientes_Agregar;
GO

CREATE PROCEDURE dbo.USER_POS_Clientes_Agregar
    @ClienteID INT
AS
BEGIN
    SET NOCOUNT ON;


    INSERT INTO dbo.ProcesosSpertaAPI (
        TenantId,
        EmpresaId,
        Destino,
        TipoEntidad,
        TipoOperacion,
        Identificador,
        Estado,
        Intentos,
        MensajeUltimoError,
        FechaCreacionUtc,
        FechaInicioProcesoUtc,
        FechaFinProcesoUtc
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
        SYSUTCDATETIME(),
        NULL,
        NULL
    );
END
GO

