/*
  Listado Sperta — cola HubSpot (un solo result set).
  XML: scriptsSQL/listados xml/ListadoHubSpotProcesosCola.xml

  Parametros (orden XML):
    @EmpresaID, @FechaDesde, @FechaHasta

  Estados: 0=Pendiente, 1=EnProceso, 2=Ok, 3=Error

  EXEC dbo.ListadoHubSpotProcesosCola_Buscar 1, '20260101', '20261231'
*/

IF OBJECT_ID(N'dbo.InterfazHubSpot_Procesos_Listar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.InterfazHubSpot_Procesos_Listar;
GO

IF OBJECT_ID(N'dbo.ListadoHubSpotProcesosCola_Buscar', N'P') IS NOT NULL
    DROP PROCEDURE dbo.ListadoHubSpotProcesosCola_Buscar;
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE PROCEDURE dbo.ListadoHubSpotProcesosCola_Buscar
    @EmpresaID   INT,
    @FechaDesde  DATETIME,
    @FechaHasta  DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FechaDesdeEfectiva DATETIME2;
    DECLARE @FechaHastaEfectiva DATETIME2;
    DECLARE @FechaHastaFin DATETIME2;
    DECLARE @EmpresaIdFiltro INT;

    IF @FechaDesde IS NOT NULL AND @FechaDesde >= CONVERT(DATETIME, '17530101', 112)
        SET @FechaDesdeEfectiva = @FechaDesde;

    IF @FechaHasta IS NOT NULL AND @FechaHasta >= CONVERT(DATETIME, '17530101', 112)
        SET @FechaHastaEfectiva = @FechaHasta;

    SET @FechaHastaFin = CASE
                             WHEN @FechaHastaEfectiva IS NULL THEN NULL
                             ELSE DATEADD(DAY, 1, CAST(CAST(@FechaHastaEfectiva AS DATE) AS DATETIME2))
                         END;

    SET @EmpresaIdFiltro = CASE WHEN @EmpresaID IS NULL OR @EmpresaID = 0 THEN NULL ELSE @EmpresaID END;

    SELECT TOP 500
        p.ProcesoId,
        EstadoTexto = CASE p.Estado
                            WHEN 0 THEN N'Pendiente'
                            WHEN 1 THEN N'EnProceso'
                            WHEN 2 THEN N'Ok'
                            WHEN 3 THEN N'Error'
                            ELSE N'Desconocido'
                        END,
        p.FechaCreacion,
        p.Identificador,
        ClienteCodigo = CASE WHEN p.TipoEntidad = N'Cliente' THEN c.CodCli ELSE NULL END,
        ClienteRazonSocial = CASE WHEN p.TipoEntidad = N'Cliente' THEN c.RazonSocial ELSE NULL END,
        ClienteNombreFantasia = CASE WHEN p.TipoEntidad = N'Cliente' THEN c.ApeyNom ELSE NULL END,
        p.TipoOperacion,
        p.Intentos,
        MensajeUltimoError = CONVERT(NVARCHAR(4000), p.MensajeUltimoError),
        p.FechaInicioProceso,
        p.FechaFinProceso,
        DuracionSegundos = DATEDIFF(SECOND, p.FechaInicioProceso, p.FechaFinProceso),
        CantidadLogs = ISNULL(logAgg.CantidadLogs, 0),
        CantidadLogsError = ISNULL(logAgg.CantidadLogsError, 0),
        UltimaFaseLog = ultLog.Fase,
        UltimaLogFecha = ultLog.FechaGrabacion,
        UltimoLogDetalle = CONVERT(NVARCHAR(4000), ultLog.Detalle)
    FROM dbo.ProcesosSpertaHubSpot AS p
    LEFT JOIN dbo.Clientes AS c
        ON p.TipoEntidad = N'Cliente'
       AND c.ClienteID = p.Identificador
    LEFT JOIN (
        SELECT
            lg.ProcesoId,
            CantidadLogs = COUNT(*),
            CantidadLogsError = SUM(CASE WHEN lg.Exito = 0 THEN 1 ELSE 0 END)
        FROM dbo.ProcesosSpertaHubSpotLog AS lg
        WHERE lg.ProcesoId IS NOT NULL
        GROUP BY lg.ProcesoId
    ) AS logAgg
        ON logAgg.ProcesoId = p.ProcesoId
    OUTER APPLY (
        SELECT TOP 1
            lg.Fase,
            lg.Detalle,
            lg.FechaGrabacion
        FROM dbo.ProcesosSpertaHubSpotLog AS lg
        WHERE lg.ProcesoId = p.ProcesoId
        ORDER BY lg.FechaGrabacion DESC, lg.LogId DESC
    ) AS ultLog
    WHERE p.Destino = N'HubSpot'
      AND (@EmpresaIdFiltro IS NULL OR p.EmpresaId = @EmpresaIdFiltro)
      AND (@FechaDesdeEfectiva IS NULL OR p.FechaCreacion >= @FechaDesdeEfectiva)
      AND (@FechaHastaFin IS NULL OR p.FechaCreacion < @FechaHastaFin)
    ORDER BY p.FechaCreacion DESC;
END
GO
