/*
  Despliegue ordenado — HubSpot / MSGestion (Calzetta)
  Ejecutar en SSMS contra la base MSGestion o via sqlcmd:

    sqlcmd -S <server> -d <MSGestion> -i scriptsSQL/000_Deploy_All.sql

  Orden:
    0. Cleanup legacy (IntegracionEjecucionLog, función CC obsoleta)
    1. Tabla cola
    2. Tabla log
    3. Tabla vendedores habilitados (requerida por SPs 003–006)
    4. Índices de performance HubSpot
    5. SP post-grabación WinForms
    6. SP datos cliente (empresa + direcciones)
    7. SP contactos cliente
    8. SP cuenta corriente paginada (2B)
    9. Atributo variable id_hubspot + SP guardar ID HubSpot en cliente (2A)
   10. Indices listado cola HubSpot
   11. SP listado Sperta cola HubSpot (un result set)
*/

:r 000_Cleanup_Legacy.sql
:r 001_ProcesosSpertaHubSpot.sql
:r 002_ProcesosSpertaHubSpotLog.sql
:r 008_InterfazHubSpot_VendedoresHabilitados.sql
:r 009_Indices.sql
:r 013_Indices_ProcesosHubSpot_Listado.sql
:r 003_USER_CALZETTA_POS_Clientes_Agregar.sql
:r 004_InterfazHubSpot_Cliente_Obtener.sql
:r 005_InterfazHubSpot_Clientes_Contactos_Obtener.sql
:r 006_InterfazHubSpot_CuentaCorriente_Pagina.sql
:r 010_InterfazHubSpot_Atributo_IdHubSpot.sql
:r 012_ListadoHubSpotProcesosCola_Buscar.sql

PRINT 'Deploy HubSpot MSGestion completado.';
