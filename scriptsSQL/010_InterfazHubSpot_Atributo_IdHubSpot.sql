/*
  Atributo variable id_hubspot en CLIENTES + SP de persistencia (Flujo 2A).
  Idempotente: no recrea el atributo ni el SP en re-deploy.

  EXEC dbo.InterfazHubSpot_Cliente_GuardarIdHubSpot @ClienteId = 77, @IdHubSpot = '12345678901'
*/

-- ── A) Definición del atributo en MSAtributos ────────────────────────────────
IF NOT EXISTS (
    SELECT 1
    FROM dbo.MSAtributos
    WHERE Entidad = 'CLIENTES'
      AND CodAtrib = 'id_hubspot'
)
BEGIN
    EXEC dbo.MSAtributos_Agregar
         @Entidad     = 'CLIENTES',
         @CodAtrib    = 'id_hubspot',
         @Descripcion = 'ID HubSpot CRM',
         @Tipo        = 'TEXTO',
         @Decimales   = NULL,
         @MaxLength   = 50;

    PRINT 'Atributo variable creado: CLIENTES.id_hubspot';
END
ELSE
BEGIN
    PRINT 'Atributo variable CLIENTES.id_hubspot ya existe — sin cambios.';
END
GO

-- ── B) SP idempotente ────────────────────────────────────────────────────────
IF OBJECT_ID(N'dbo.InterfazHubSpot_Cliente_GuardarIdHubSpot', N'P') IS NOT NULL
    DROP PROCEDURE dbo.InterfazHubSpot_Cliente_GuardarIdHubSpot;
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.InterfazHubSpot_Cliente_GuardarIdHubSpot
    @ClienteId  INT,
    @IdHubSpot  VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF @ClienteId IS NULL OR @ClienteId <= 0
    BEGIN
        RAISERROR('InterfazHubSpot_Cliente_GuardarIdHubSpot: @ClienteId debe ser mayor a cero.', 16, 1);
        RETURN;
    END

    SET @IdHubSpot = LTRIM(RTRIM(ISNULL(@IdHubSpot, '')));

    IF @IdHubSpot = ''
    BEGIN
        RAISERROR('InterfazHubSpot_Cliente_GuardarIdHubSpot: @IdHubSpot no puede estar vacío.', 16, 1);
        RETURN;
    END

    DECLARE @AtributoID INT;

    SELECT @AtributoID = AtributoID
    FROM dbo.MSAtributos
    WHERE Entidad = 'CLIENTES'
      AND CodAtrib = 'id_hubspot';

    IF @AtributoID IS NULL
    BEGIN
        RAISERROR('InterfazHubSpot_Cliente_GuardarIdHubSpot: atributo CLIENTES.id_hubspot no existe. Ejecutar script 010.', 16, 1);
        RETURN;
    END

    DECLARE @ValorActual VARCHAR(255);

    SELECT @ValorActual = Valor
    FROM dbo.MSAtributosValores
    WHERE AtributoID = @AtributoID
      AND RegistroID = @ClienteId;

    IF LTRIM(RTRIM(ISNULL(@ValorActual, ''))) = @IdHubSpot
        RETURN;

    EXEC dbo.MSValorAtributo_Agregar
         @AtributoID = @AtributoID,
         @RegistroId = @ClienteId,
         @Valor      = @IdHubSpot;
END
GO

PRINT 'SP creado: dbo.InterfazHubSpot_Cliente_GuardarIdHubSpot';
GO
