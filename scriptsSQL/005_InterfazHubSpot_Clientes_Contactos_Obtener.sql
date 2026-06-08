/*
  InterfazHubSpot_Clientes_Contactos_Obtener
  Devuelve Contactos (dbo.Clientes_Contactos)
  

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
        ClienteId          = cc.ClienteID,
        ApellidoYNombre    = cc.ApeyNom,
        CorreoElectronico  = cc.Email,
        Telefono           = cc.Telefono,
        Sector           = ISNULL(sec.Descripcion, N'')
    FROM dbo.Clientes_Contactos AS cc
    LEFT JOIN dbo.SectoresDeContactosClientes AS sec
        ON sec.SectorID = cc.SectorID
	INNER JOIN Clientes CL ON CL.ClienteID = cc.ClienteID AND CL.VendedorID in (107,37,91)
    WHERE cc.ClienteID = @ClienteId
		AND CL.inhabilitado = 0


END
GO
