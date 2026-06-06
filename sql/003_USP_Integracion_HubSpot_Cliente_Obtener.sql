/*
  USP_Integracion_HubSpot_Cliente_Obtener
  Devuelve 3 result sets para hidratar ClienteIntegracionDto en la capa Business:
    1. Cabecera cliente        -> ClienteIntegracionDto (scalar)
    2. Contactos               -> ListaClientesContactos
    3. Direcciones de entrega  -> ListaDireccionEntregas (TOP 3)

  Uso: EXEC dbo.USP_Integracion_HubSpot_Cliente_Obtener @ClienteId = 12345

  NOTA: todos los nombres de tabla y columna están marcados con -- CONFIRMAR.
        Revisar contra el esquema real de MSGestion antes de poner en producción.
*/

IF OBJECT_ID('dbo.USP_Integracion_HubSpot_Cliente_Obtener', 'P') IS NOT NULL
    DROP PROCEDURE dbo.USP_Integracion_HubSpot_Cliente_Obtener;
GO

CREATE PROCEDURE dbo.USP_Integracion_HubSpot_Cliente_Obtener
    @ClienteId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- =========================================================
    -- Result set 1: Cabecera cliente
    -- Tabla origen: dbo.Clientes -- CONFIRMAR: nombre de tabla real en MSGestion
    -- =========================================================
    SELECT
        c.ClienteId,                    -- CONFIRMAR: columna PK en Clientes
        c.CodigoCliente,                -- CONFIRMAR: código alfanumérico del cliente
        c.RazonSocial,                  -- CONFIRMAR: razón social / nombre empresa
        c.ApellidoYNombre,              -- CONFIRMAR: nombre completo persona física
        c.Contacto,                     -- CONFIRMAR: nombre de contacto principal (campo libre)
        c.NumeroDocumento,              -- CONFIRMAR: CUIT / DNI
        c.Calle,                        -- CONFIRMAR: calle del domicilio fiscal
        c.Puerta,                       -- CONFIRMAR: número de puerta / altura
        c.Localidad,                    -- CONFIRMAR: localidad del domicilio fiscal
        c.CodigoPostal,                 -- CONFIRMAR: código postal fiscal
        c.CodigoProvinciaCliente,       -- CONFIRMAR: FK a tabla de provincias o código directo
        c.CodigoPais,                   -- CONFIRMAR: FK a tabla de países o código ISO
        c.ZonaId,                       -- CONFIRMAR: FK a tabla de zonas comerciales
        c.VendedorId,                   -- CONFIRMAR: FK a tabla de vendedores / empleados
        c.ResponsableCuentaId,          -- CONFIRMAR: FK a responsable de cuenta (puede ser null)
        c.ListaPreciosId,               -- CONFIRMAR: FK a tabla de listas de precios
        c.CondicionVentaId,             -- CONFIRMAR: FK a condiciones de venta
        c.DiasParaDeuda,                -- CONFIRMAR: días de gracia antes de considerar deuda
        c.LimiteCredito,                -- CONFIRMAR: límite de crédito en moneda local
        c.CategoriaClienteId            -- CONFIRMAR: FK a categoría de cliente (A/B/C u otro)
    FROM
        dbo.Clientes AS c               -- CONFIRMAR: nombre de tabla real en MSGestion
    WHERE
        c.ClienteId = @ClienteId;       -- CONFIRMAR: nombre de columna PK

    -- =========================================================
    -- Result set 2: Contactos del cliente
    -- Tabla origen: dbo.ClientesContactos -- CONFIRMAR: nombre de tabla real en MSGestion
    -- =========================================================
    SELECT
        cc.ClienteId,                   -- CONFIRMAR: FK a Clientes
        cc.ApellidoYNombre,             -- CONFIRMAR: nombre completo del contacto
        cc.CorreoElectronico,           -- CONFIRMAR: email del contacto
        cc.Telefono,                    -- CONFIRMAR: teléfono del contacto
        cc.SectorId                     -- CONFIRMAR: FK a sector/área del contacto (puede ser null)
    FROM
        dbo.ClientesContactos AS cc     -- CONFIRMAR: nombre de tabla real en MSGestion
    WHERE
        cc.ClienteId = @ClienteId;      -- CONFIRMAR: nombre de columna FK

    -- =========================================================
    -- Result set 3: Direcciones de entrega (TOP 3)
    -- Tabla origen: dbo.DireccionEntregas -- CONFIRMAR: nombre de tabla real en MSGestion
    -- =========================================================
    SELECT TOP 3
        de.ClienteId,                   -- CONFIRMAR: FK a Clientes
        de.Domicilio,                   -- CONFIRMAR: descripción o calle de la dirección de entrega
        de.CodigoPostal,                -- CONFIRMAR: código postal de la dirección de entrega
        de.Localidad,                   -- CONFIRMAR: localidad de la dirección de entrega
        de.ProvinciaId                  -- CONFIRMAR: FK a tabla de provincias o código directo
    FROM
        dbo.DireccionEntregas AS de     -- CONFIRMAR: nombre de tabla real en MSGestion
    WHERE
        de.ClienteId = @ClienteId       -- CONFIRMAR: nombre de columna FK
    ORDER BY
        de.ClienteId ASC;               -- CONFIRMAR: criterio de ordenamiento para elegir las 3 primeras
END
GO
