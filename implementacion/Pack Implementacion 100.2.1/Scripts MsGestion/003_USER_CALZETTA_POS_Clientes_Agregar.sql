   
/*---------------------------------------------------------------------------------------------------     
Creado por : Santiago GruÒeiro    
Fecha : 17/08/2023    
Actividad : (17562) Instalacion Primera Parte CRM Prospectos    
DescripciÛn : Se crea stored    
---------------------------------------------------------------------------------------------------   
Modificado por    : Alan Lipshitz       
Fecha             : 29/06/2026  
Incidente/Actividadù      : (19512)  
Descripcion       : Se crea el store para la InterfazHubSpot  
---------------------------------------------------------------------------------------------------*/    
  
CREATE OR ALTER  PROCEDURE [dbo].[USER_CALZETTA_POS_Clientes_Agregar]      
@ClienteID INT       
AS       
 BEGIN     SET NOCOUNT ON;      IF @ClienteID IS NULL OR @ClienteID <= 0         RETURN;     
  
------------------ SECCION CRM Prospectos  -------------------------------  
      
declare @razonsocial varchar(120),      
 @nombreapellido varchar(70),      
@cuit varchar(20),      
@emailPara varchar(500),      
@emailCc varchar(500),      
@telefono varchar(50),      
@direccion varchar(500),      
@localidad varchar(100),      
@provincia varchar(200),      
@vendedorId int,      
@InHabilitado bit      
      
      
select       
@razonsocial='('+CLI.CodCli+')'+ ' '+ CLI.RazonSocial,      
@nombreapellido=CLI.ApeyNom,      
@cuit=CLI.NroDocumento,      
@telefono=CLI.Telefono,      
@InHabilitado=cast(ISNULL(CLI.InHabilitado,0) as bit),      
@direccion=CLI.Calle +' ' +cast(CLI.Puerta AS varchar(6)),      
@localidad=CLI.Localidad,      
@provincia=PRO.Descripcion,      
@vendedorId=CLI.VendedorId      
from Clientes CLI      
INNER JOIN Provincias PRO ON CLI.ProvinciaID=PRO.ProvinciaID      
where ClienteID=@ClienteID      
      
set @emailCc=(SELECT valor from ParametriaEmails where Nombre = 'EmailCcHabilitacionCliente')      
      
set @emailPara=(SELECT case when ( mail not like '%@%') then      
     @emailCc      
    else      
     Mail      
    end      
    FROM Vendedores WHERE VendedorID =@vendedorId)      
        
     
      
DECLARE @Plantilla  varchar(max)       
DECLARE @Asunto  varchar(100)       
SET @Asunto = 'Cliente Habilitado ' +@razonsocial      
SELECT @Plantilla=valor from ParametriaEmails where Nombre = 'PlantillaBienvenidaProspecto'        
      
      
set @Plantilla=replace(@Plantilla,'#razonsocial#',ISNULL(@razonsocial,'No Informado'))       
set @Plantilla=replace(@Plantilla,'#nombreapellido#',ISNULL(@nombreapellido,'No Informado'))       
set @Plantilla=replace(@Plantilla,'#cuit#',ISNULL(@cuit,'No Informado'))       
set @Plantilla=replace(@Plantilla,'#email#',ISNULL(@emailPara,'No Informado'))       
set @Plantilla=replace(@Plantilla,'#telefono#',ISNULL(@telefono,'No Informado'))       
set @Plantilla=replace(@Plantilla,'#direccion#',ISNULL(@direccion,'No Informado'))       
set @Plantilla=replace(@Plantilla,'#localidad#',ISNULL(@localidad,'No Informado'))       
set @Plantilla=replace(@Plantilla,'#provincia#',ISNULL(@provincia,'No Informado'))       
        
declare @EmailDe varchar (200)      
select @EmailDe=valor from ParametriaEmails where Nombre='EmailDe'      
      
if (@InHabilitado=CAST(0 AS bit))      
      
BEGIN      
UPDATE ClientesProspecto      
 SET EstadoClientesProspectoId=4      
 WHERE ClienteID=@ClienteID and EstadoClientesProspectoId=3      
 IF EXISTS (select * from ClientesProspecto where ClienteID=@ClienteID and isnull(MailEnviado,0)=0)      
   BEGIN      
   INSERT INTO Emails (Sistema,Enviado,De,Para,Cc,Asunto,EsHtml,Mensaje,FechaEmail,NombreOrigen,CampoOrigen,ValorOrigen)        
     values        
   ('SPERTA','N',@EmailDe,@emailPara,@emailCc,@Asunto,'S',@Plantilla,GETDATE(),'BienvenidaProspecto','ClienteID',@ClienteID)      
      
      
    UPDATE ClientesProspecto SET MailEnviado= cast(1 as bit) where ClienteID=@ClienteID      
      
      
    END  
 END   
   
 ---------------------SECCION INTERFAZ HUBSPOT ----------------------------  
 
    -- SÛlo encolar clientes activos con vendedor habilitado para HubSpot.
 
    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Clientes AS c
        INNER JOIN dbo.InterfazHubSpot_VendedoresHabilitados AS v
            ON v.VendedorID = c.VendedorID
        WHERE c.ClienteID    = @ClienteID
          AND c.InHabilitado = 0
    )
        RETURN;

    -- No encolar si ya hay una entrada activa (Pendiente o EnProceso) para este cliente.
    IF EXISTS (
        SELECT 1
        FROM dbo.ProcesosSpertaHubSpot AS p
        WHERE p.Destino       = N'HubSpot'
          AND p.TipoEntidad   = N'Cliente'
          AND p.Identificador = @ClienteID
          AND p.Estado IN (0, 1)
    )
        RETURN;

    INSERT INTO dbo.ProcesosSpertaHubSpot (
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
 ;  