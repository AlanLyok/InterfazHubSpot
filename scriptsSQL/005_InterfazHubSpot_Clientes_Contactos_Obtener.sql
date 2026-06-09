/*
  InterfazHubSpot_Clientes_Contactos_Obtener  (v2)
  ─────────────────────────────────────────────────────────────────────────────
  Devuelve Contactos (dbo.Clientes_Contactos).

  EXEC dbo.InterfazHubSpot_Clientes_Contactos_Obtener @ClienteId = 77
*/

IF OBJECT_ID(N'dbo.InterfazHubSpot_Clientes_Contactos_Obtener', N'P') IS NOT NULL
    DROP PROCEDURE dbo.InterfazHubSpot_Clientes_Contactos_Obtener;
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.InterfazHubSpot_Clientes_Contactos_Obtener
    @ClienteId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ClienteId         = cc.ClienteID,
        ApellidoYNombre   = cc.ApeyNom,
        CorreoElectronico = cc.Email,
        Telefono          = cc.Telefono,
        Sector            = ISNULL(sec.Descripcion, N'')
    FROM dbo.Clientes_Contactos AS cc
    LEFT JOIN dbo.SectoresDeContactosClientes AS sec
        ON sec.SectorID = cc.SectorID
    INNER JOIN dbo.Clientes AS cl
        ON  cl.ClienteID    = cc.ClienteID
        AND cl.inhabilitado = 0
        AND EXISTS (
              SELECT 1
              FROM dbo.InterfazHubSpot_VendedoresHabilitados AS v
              WHERE v.VendedorID = cl.VendedorID
            )
    WHERE cc.ClienteID = @ClienteId;
END
GO
