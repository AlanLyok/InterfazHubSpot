select TOp 10 * from Emails order by id desc


Select * from ProcesosSpertaHubSpot
Select * from ProcesosSpertaHubSpotLog

Select * from Vendedores where VendedorID in (107,37,91)
Select * from Vendedores where Descripcion like '%01%'

select Distinct(CC.Descripcion) from clientes CL
LEFT JOIN CategClientes CC ON CC.CategClienteID = CL.CategClienteID
where VendedorID in (107,37,91) order by Descripcion

select ClienteId, CodCli, RazonSocial, NroDocumento=REPLACE(REPLACE(NroDocumento, '-',''),'.',''),CategCliente= CC.Descripcion, * from clientes CL
LEFT JOIN CategClientes CC ON CC.CategClienteID = CL.CategClienteID
where VendedorID in (107,37,91)
--AND CL.CategClienteID IS NULL


Select * from CategClientes order by Descripcion
Select * from Vendedores where VendedorID in (107,37,91)

select CategClienteID, * from clientes where clienteid = 77

-- update clientes set CategClienteID = 6 where clienteid = 77
Select * from CondVenta 

select Distinct(CC.Descripcion) from clientes CL
LEFT JOIN CategClientes CC ON CC.CategClienteID = CL.CategClienteID
where VendedorID in (107,37,91) 
And clienteid = 77
order by Descripcion

--update clientes set CategClienteID = 5 where clienteid = 77

SELECT ClienteID, CodCLi, REPLACE(REPLACE(NroDocumento, '-',''),'.','') as NroDocumento, RazonSocial
FROM dbo.Clientes AS c
INNER JOIN dbo.InterfazHubSpot_VendedoresHabilitados AS v
    ON v.VendedorID = c.VendedorID
WHERE c.InHabilitado = 0 and clienteid = 26349


select inhabilitado, vendedorid,REPLACE(REPLACE(NroDocumento, '-',''),'.','') as NroDocumento, RazonSocial, ApeYnom from clientes where CodCli = 90368

select inhabilitado, vendedorid,REPLACE(REPLACE(NroDocumento, '-',''),'.','') as NroDocumento, RazonSocial, ApeYnom from clientes where ClienteID = 90368


create table ProcesosSpertaHubSpotEstados

0 -- Sin procesar
1 -- En proceso
2 -- Procesado ok
3 -- Procesado con error
 

Select * from ProcesosSpertaHubSpot
Select * from ProcesosSpertaHubSpotLog
select TOp 10 * from Emails order by id desc


-- 1 
/*
  Post-grabación WinForms — encola cliente para sincronización HubSpot (flujo 2A).
  Uso:
    EXEC dbo.USER_POS_Clientes_Agregar @ClienteID = 77;
*/
-- 2
/*
  InterfazHubSpot_Cliente_Obtener  (v2 — sin función escalar)
  ─────────────────────────────────────────────────────────────────────────────
  Devuelve 2 result sets para ClienteIntegracionManager / ClienteIntegracionMapper:
    1. Cabecera cliente (dbo.Clientes + lookups + ManejoCuentaCorriente)
    2. Direcciones de entrega TOP 3 (dbo.DireccionEntregas)

  EXEC dbo.InterfazHubSpot_Cliente_Obtener @ClienteId = 77
*/
-- 3
/*
  InterfazHubSpot_Clientes_Contactos_Obtener  (v2)
  ─────────────────────────────────────────────────────────────────────────────
  Devuelve Contactos (dbo.Clientes_Contactos).

  EXEC dbo.InterfazHubSpot_Clientes_Contactos_Obtener @ClienteId = 77
*/


-- 4
/*
  InterfazHubSpot_CuentaCorriente_Pagina  (v3 — filtro vendedores)
  ─────────────────────────────────────────────────────────────────────────────
  Paginación keyset de clientes activos con texto preformateado para HubSpot
  (propiedad manejo_cuenta_corriente — flujo 2B).

  RESULTADO POR CLIENTE
  ─────────────────────
  Con deuda:                               
    Cuenta actualizada al 07/06/2026         
                                           
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
*/



select * from MSAtributosValores MAV
inner join MSAtributos MA ON MA.AtributoID = MAV.AtributoID
where RegistroID=  77

select * from clientes where clienteid = 77