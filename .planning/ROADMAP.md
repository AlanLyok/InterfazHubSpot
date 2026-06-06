# Roadmap: Integración HubSpot 2A/2B

**Project:** Integración HubSpot — Flujos 2A y 2B  
**Phases:** 5  
**Requirements:** 35 v1 (100% mapped)  
**PRD:** `docs/PRD_Integracion_HubSpot_2A_2B.md`

---

## Phase Overview

| # | Phase | Goal | Requirements | Success Criteria |
|---|-------|------|--------------|------------------|
| 1 | SQL y capa de datos | SPs + DTOs + SqlDataAccess | SQL-*, DATA-* | 4 |
| 2 | HubSpot Client | Cliente REST CRM v3 con rate limit y mock | HS-* | 4 |
| 3 | Flujo 2A | Cola clientes/contactos ERP → HubSpot | 2A-* | 5 |
| 4 | Flujo 2B | Cuenta corriente diaria masiva | 2B-* | 5 |
| 5 | Integración y entrega | MVC, limpieza SpertaAPI, tests, deploy | MVC-*, CFG-*, TST-* | 5 |

---

### Phase 1: SQL y capa de datos
**Goal:** Tener acceso a datos HubSpot vía stored procedures y DTOs, sin SpertaAPI.  
**Depends on:** Confirmación Calzetta (tabla HubSpotCompanyId, campos origen, CC)  
**Requirements:** SQL-01..07, DATA-01..03

**Success Criteria:**
1. Los 6 SPs del PRD existen en `sql/` y ejecutan sin error en BD de desarrollo
2. `ISqlDataAccess` / `SqlDataAccess` invocan todos los SPs con parámetros correctos
3. DTOs mapean todos los campos del PRD §12
4. `USER_POS_Clientes_Agregar` no duplica filas Pendiente para el mismo cliente

**Deliverables:**
- `sql/001`..`008` según PRD §13
- `InterfazHubSpot.Interfaces/ISqlDataAccess.cs`
- `InterfazHubSpot.Business/SqlDataAccess.cs`
- DTOs en `InterfazHubSpot.Entities`

**UI hint:** no

---

### Phase 2: HubSpot Client
**Goal:** Cliente HTTP HubSpot CRM v3 production-ready con PAT, rate limit y mock dev.  
**Depends on:** Phase 1 (DTOs)  
**Requirements:** HS-01..06

**Success Criteria:**
1. Search/create/patch companies y contacts funcionan contra API o mock
2. Asociación contacto-compañía implementada
3. Batch update 100 compañías operativo
4. 401 detiene operación; 429 reintenta hasta 3 veces con backoff

**Deliverables:**
- `IHubSpotClient` + implementación en `InterfazHubSpot` o `Business`
- Configuración `HubSpot:*` documentada en Web.config (token comentado)

**UI hint:** no

---

### Phase 3: Flujo 2A — Cola clientes/contactos
**Goal:** Job procesa cola Pendiente y sincroniza compañía + contactos en HubSpot.  
**Depends on:** Phase 1, Phase 2  
**Requirements:** 2A-01..07

**Success Criteria:**
1. Job corre cada 5 min (configurable) y procesa hasta N registros por ciclo
2. Compañía creada/actualizada y `HubSpotCompanyId` persistido en Mastersoft
3. Contactos sincronizados por email con asociación a compañía
4. Errores marcan cola `Error` sin reintento; email enviado si `EmailErrPara` configurado
5. Flujo idempotente: re-ejecutar mismo cliente produce mismo estado HubSpot

**Deliverables:**
- `ProcesarColaHubSpotJob` / runner en `InterfazHubSpot`
- Integración `EmailsManager` para errores 2A

**UI hint:** no

---

### Phase 4: Flujo 2B — Cuenta corriente diaria
**Goal:** Job diario actualiza `manejo_cuenta_corriente` de todas las compañías activas.  
**Depends on:** Phase 1, Phase 2  
**Requirements:** 2B-01..07

**Success Criteria:**
1. Job programado 3:00 AM (configurable) ejecuta proceso completo
2. Formato texto correcto para clientes con y sin deuda (PRD §7.3)
3. Batches de 100 con delay 200ms; errores de batch no detienen siguientes lotes
4. `IntegracionEjecucionLog` registra inicio, fin, totales y errores
5. Clientes sin `HubSpotCompanyId` omitidos con advertencia en log

**Deliverables:**
- `HubSpotSincronizarCuentaCorrienteJob` + formateador CC
- Tests unitarios formateador (TST-01)

**UI hint:** no

---

### Phase 5: Integración, consola MVC y entrega
**Goal:** Operación manual, limpieza SpertaAPI, tests y listo para deploy.  
**Depends on:** Phase 3, Phase 4  
**Requirements:** MVC-01..04, CFG-01..03, TST-02..03

**Success Criteria:**
1. Cuatro botones MVC del PRD §9 operativos con trazas
2. Cero referencias a `HttpSpertaApiClient` en código HubSpot
3. `dotnet test` / script `Run-Tests-InterfazHubSpot.ps1` pasa
4. Web.config listo para deploy (mock=false, emails, HubSpot keys documentadas)
5. README actualizado reflejando arquitectura SQL directa

**Deliverables:**
- Home controller + views
- Limpieza dependencias SpertaAPI en flujos HubSpot
- Suite tests actualizada

**UI hint:** yes

---

## Execution Order

```
Phase 1 ──► Phase 2 ──► Phase 3 ──┐
              │                    ├──► Phase 5
              └──► Phase 4 ────────┘
```

Fases 3 y 4 pueden planificarse en paralelo tras Fase 2.

---
*Roadmap created: 2026-06-06*
