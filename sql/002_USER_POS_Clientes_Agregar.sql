/*
  Copia versionada alineada con scriptsSQL/003_USER_POS_Clientes_Agregar.sql
  Desplegar ANTES del sp_rename en sql/001 (gate WinForms).
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
