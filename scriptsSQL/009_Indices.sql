/*
  Índices de performance para SPs HubSpot (004, 006).
  Idempotente: IF NOT EXISTS por nombre de índice.
*/

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Clientes')
      AND name = N'IX_Clientes_HubSpot_Pagina'
)
BEGIN
    CREATE INDEX IX_Clientes_HubSpot_Pagina
        ON dbo.Clientes (InHabilitado, ClienteID)
        INCLUDE (VendedorID);
    PRINT 'Indice IX_Clientes_HubSpot_Pagina creado.';
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.VeCtasCtes')
      AND name = N'IX_VeCtasCtes_HubSpot'
)
BEGIN
    CREATE INDEX IX_VeCtasCtes_HubSpot
        ON dbo.VeCtasCtes (ClienteID, DebeHaber)
        INCLUDE (CodCompID, Sucursal, NroComp, ValorizacionID, FechaEMI, EmpresaID);
    PRINT 'Indice IX_VeCtasCtes_HubSpot creado.';
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.VeComprobantes')
      AND name = N'IX_VeComprobantes_HubSpot'
)
BEGIN
    CREATE INDEX IX_VeComprobantes_HubSpot
        ON dbo.VeComprobantes (CodCompID, Sucursal, NroComp)
        INCLUDE (ComprobantesID);
    PRINT 'Indice IX_VeComprobantes_HubSpot creado.';
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.VeComprobantesVto')
      AND name = N'IX_VeComprobantesVto_HubSpot'
)
BEGIN
    CREATE INDEX IX_VeComprobantesVto_HubSpot
        ON dbo.VeComprobantesVto (ComprobantesID)
        INCLUDE (FechaVto);
    PRINT 'Indice IX_VeComprobantesVto_HubSpot creado.';
END
GO
