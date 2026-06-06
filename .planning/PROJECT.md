# Integración HubSpot — Flujos 2A y 2B

## What This Is

Versión limpia de **InterfazHubSpot** para Calzetta que sincroniza datos del ERP Mastersoft hacia **HubSpot CRM** mediante dos flujos batch: **2A** (clientes y contactos en tiempo casi real vía cola) y **2B** (cuenta corriente diaria). Reemplaza las lecturas vía SpertaAPI por **stored procedures directos** a SQL Server y conserva la consola web MVC para operación manual y soporte.

**Ticket:** #116367 · **Cliente:** Calzetta · **PRD:** `docs/PRD_Integracion_HubSpot_2A_2B.md`

## Core Value

Cuando un cliente se crea o modifica en el ERP, su compañía y contactos deben quedar correctamente reflejados en HubSpot; y cada día el campo `manejo_cuenta_corriente` de todas las compañías activas debe reflejar la deuda vigente o $0.

## Requirements

### Validated

- ✓ Solución `InterfazHubSpot.sln` con proyectos MVC, Business, Entities, Interfaces, BatchProcess e `InterfazHubSpot` — existente
- ✓ Tablas `dbo.ProcesosSpertaHubSpot` y `dbo.IntegracionEjecucionLog` — existentes (verificar estructura)
- ✓ `IScheduler` con jobs HubSpot (`ProcesarColaIntegracionesHubSpotJob`, `HubSpotSincronizarCuentaCorrienteJob`) — existente
- ✓ Consola MVC con botones de prueba HubSpot en Home — existente
- ✓ `EmailsManager` para notificaciones de error — existente
- ✓ Modo mock `HubSpot:UseDevelopmentMock` — existente

### Active

- [ ] Migrar acceso a datos de SpertaAPI HTTP a stored procedures SQL (`USER_HS_*`)
- [ ] Implementar/ajustar los 6 SPs del PRD (cola, datos cliente, contactos, cuenta corriente, estado cola, guardar HubSpotCompanyId)
- [ ] Flujo 2A: cola → compañía HubSpot (search/create/patch) → contactos (search/create/patch + asociación)
- [ ] Flujo 2B: cuenta corriente masiva → agrupación en memoria → batch update 100 compañías
- [ ] Rate limiting HubSpot (120ms entre llamadas, backoff 429, detener en 401)
- [ ] Persistir `HubSpotCompanyId` en Mastersoft vía SP
- [ ] Eliminar dependencias de `HttpSpertaApiClient` y otras APIs (MeLi, MKP, APPro) del alcance HubSpot
- [ ] Configuración `Web.config` HubSpot documentada (token no versionado)
- [ ] Tests unitarios/integración para flujos críticos

### Out of Scope

- OAuth flow HubSpot — se usa Private App Token (PAT) según PRD
- SpertaAPI, Mercado Libre, MKP, APPro — eliminados del alcance de esta integración
- Reintentos automáticos en errores de cola 2A — marcar `Error` y notificar; no reintentar
- Migración a .NET 8 / ASP.NET Core — mantener .NET Framework 4.5.2

## Context

**Estado actual (brownfield):** El repo ya contiene una implementación HubSpot que consume datos vía `HttpSpertaApiClient` (SpertaAPI). El PRD define la **versión objetivo**: acceso directo a SQL Server por SPs, sin DLLs Mastersoft adicionales ni OAuth.

**Flujo 2A:** El ERP WinForms llama `USER_POS_Clientes_Agregar` → fila `Pendiente` en cola → job cada 5 min procesa hasta N registros.

**Flujo 2B:** Job diario (3:00 AM) obtiene todos los clientes activos con facturas, construye texto `manejo_cuenta_corriente` y envía batches de 100 a HubSpot.

**Puntos abiertos (Calzetta):**
1. Tabla donde persistir `HubSpotCompanyId` en Mastersoft
2. Campos origen para `USER_HS_Cliente_ObtenerDatos`
3. Estructura tabla cuenta corriente/facturas para `USER_HS_CuentaCorriente_ObtenerTodos`
4. Ambiente del token PAT (productivo vs sandbox)

## Constraints

- **Tech stack:** .NET Framework 4.5.2, ASP.NET MVC, Quartz/IScheduler, SQL Server, HubSpot CRM v3 — decisión PRD
- **Datos:** Solo stored procedures; sin concatenación SQL con input de usuario
- **Seguridad:** `HubSpot:PrivateAppToken` nunca en repositorio; mock solo en desarrollo
- **HubSpot rate limit:** 100 req/10s; delay configurable 120ms; máx. 3 reintentos en 429
- **Compatibilidad:** Conservar `EmailsManager`, `Web.config` existente y estructura de proyectos

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Private App Token (sin OAuth) | Token ya disponible; simplifica despliegue | — Pending |
| SQL directo vs SpertaAPI | Eliminar dependencia intermedia; PRD explícito | — Pending |
| No reintentar errores cola 2A | Evitar loops; operador corrige manualmente | — Pending |
| Batch 100 compañías en 2B | Límite API HubSpot batch update | — Pending |
| `HubSpot:UseDevelopmentMock=true` en dev | Desarrollo sin token real | — Pending |

## Evolution

Este documento evoluciona en transiciones de fase y cierre de milestone.

**Después de cada fase** (`/gsd-transition`):
1. Requisitos invalidados → Out of Scope con razón
2. Requisitos validados → Validated con referencia de fase
3. Nuevos requisitos → Active
4. Decisiones → Key Decisions
5. Actualizar "What This Is" si hubo deriva

**Después de cada milestone** (`/gsd-complete-milestone`):
1. Revisión completa de secciones
2. Verificar Core Value
3. Auditar Out of Scope

---
*Last updated: 2026-06-06 after initialization from PRD*
