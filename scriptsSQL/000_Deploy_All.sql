/*
  Despliegue ordenado — HubSpot / MSGestion (Calzetta)
  Ejecutar en SSMS contra la base MSGestion o via sqlcmd:

    sqlcmd -S <server> -d <MSGestion> -i scriptsSQL/000_Deploy_All.sql

  Orden:
    1. Tabla cola
    2. Tabla log
    3. SP post-grabación WinForms
    4. SP datos cliente
    5. SP cuenta corriente paginada
*/

:r 001_ProcesosSpertaHubSpot.sql
:r 002_IntegracionEjecucionLog.sql
:r 003_USER_POS_Clientes_Agregar.sql
:r 004_USP_Integracion_HubSpot_Cliente_Obtener.sql
:r 005_USP_Integracion_HubSpot_CuentaCorriente_Pagina.sql

PRINT 'Deploy HubSpot MSGestion completado.';
