/*
  Despliegue ordenado — HubSpot / MSGestion (Calzetta)
  Ejecutar en SSMS contra la base MSGestion o via sqlcmd:

    sqlcmd -S <server> -d <MSGestion> -i scriptsSQL/000_Deploy_All.sql

  Orden:
    0. Cleanup legacy (IntegracionEjecucionLog, esquemas *Utc)
    1. Tabla cola
    2. Tabla log
    3. SP post-grabación WinForms
    4. SP datos cliente (empresa + direcciones)
    5. SP contactos cliente
    6. SP cuenta corriente paginada (2B)
    7. Función texto cuenta corriente (compartida 2A/2B)
*/

:r 000_Cleanup_Legacy.sql
:r 001_ProcesosSpertaHubSpot.sql
:r 002_ProcesosSpertaHubSpotLog.sql
:r 003_USER_CALZETTA_POS_Clientes_Agregar.sql
:r 007_InterfazHubSpot_ManejoCuentaCorriente_Texto.sql
:r 004_InterfazHubSpot_Cliente_Obtener.sql
:r 005_InterfazHubSpot_Clientes_Contactos_Obtener.sql
:r 006_InterfazHubSpot_CuentaCorriente_Pagina.sql

PRINT 'Deploy HubSpot MSGestion completado.';
