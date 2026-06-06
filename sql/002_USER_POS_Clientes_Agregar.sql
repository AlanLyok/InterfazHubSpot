/*
  Copia versionada alineada con scriptsSQL/USER_POS_Clientes_Agregar.sql
  INSERT en dbo.ProcesosSpertaHubSpot (nombre unificado post-refactor).

  Desplegar ANTES del sp_rename en sql/001 (gate WinForms — ver comentario en 001).
*/

IF OBJECT_ID(N'dbo.USER_POS_Clientes_Agregar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.USER_POS_Clientes_Agregar;
GO

CREATE PROCEDURE dbo.USER_POS_Clientes_Agregar
    @ClienteID INT
AS
BEGIN
    SET NOCOUNT ON;

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
