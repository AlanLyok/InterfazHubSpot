# Architecture Research — Integración HubSpot

**Researched:** 2026-06-06

## Componentes

```
ERP WinForms
    │ USER_POS_Clientes_Agregar
    ▼
dbo.ProcesosSpertaHubSpot (cola)
    │
    ▼
InterfazHubSpot.BatchProcess (IScheduler)
    │ ProcesarColaHubSpotJob (2A, cada 5 min)
    │ HubSpotSincronizarCuentaCorrienteJob (2B, diario 3AM)
    ▼
InterfazHubSpot (runners 2A / 2B)
    │
    ├── SqlDataAccess ──► SQL Server (SPs USER_HS_*)
    └── HubSpotClient ──► HubSpot CRM v3 API
              │
              ▼
InterfazHubSpot.Business (EmailsManager, managers)
InterfazHubSpot.Entities (DTOs)
InterfazHubSpot.Interfaces (IHubSpotClient, ISqlDataAccess)
InterfazHubSpot (MVC consola manual)
```

## Flujo de datos 2A

1. Cola `Pendiente` → marcar `EnProceso`
2. `USER_HS_Cliente_ObtenerDatos` → DTO compañía
3. HubSpot: search by `mastersoft_id_` o PATCH por ID conocido
4. `USER_HS_Cliente_GuardarHubSpotId` → persistir ID
5. `USER_HS_ClienteContactos_Buscar` → N contactos
6. Por contacto: search email → PATCH/POST → asociar a compañía
7. Marcar `Ok` o `Error` vía `USER_HS_Cola_ActualizarEstado`

## Flujo de datos 2B

1. Log inicio en `IntegracionEjecucionLog`
2. `USER_HS_CuentaCorriente_ObtenerTodos` → filas factura
3. Agrupar por `HubSpotCompanyId`, formatear texto
4. Chunks de 100 → `POST /crm/v3/objects/companies/batch/update`
5. Log fin con totales

## Orden de construcción recomendado

1. SQL scripts + DTOs + interfaces
2. SqlDataAccess + HubSpotClient
3. Runner 2A + job scheduler
4. Runner 2B + job scheduler
5. MVC botones + limpieza SpertaAPI + tests
