select * from Empresas

-- update empresas set DESCRIP = 'TST-'+ DESCRIP

select * from Clientes where InHabilitado = 0 and clienteid = 1007

-- update clientes set RazonSocial = 'TEST-'+ LEFT(RazonSocial,110), ApeyNom = 'TEST-'+ LEFT(ApeyNom,60)

select ApeyNom = 'TEST-' + CON.ApeyNom ,CON.*  from Clientes_Contactos CON
inner join Clientes CL ON CL.ClienteID = CON.ClienteID
Where CL.InHabilitado = 0 and isnull(CON.Email, '') <> '' and CON.clienteid = 1007

-- update Clientes_Contactos set ApeyNom = 'TEST-' + ApeyNom
-- update Clientes_Contactos set Email = 'test' + RIGHT(Email,95)


-- update Clientes_Contactos set Telefono = '555-1234' where clienteid = 1007