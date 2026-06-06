# Requirements: Integración HubSpot 2A/2B

**Defined:** 2026-06-06  
**Core Value:** Clientes ERP sincronizados en HubSpot (2A) y cuenta corriente actualizada diariamente (2B)

## v1 Requirements

### Base de Datos (SQL)

- [ ] **SQL-01**: Verificar estructura de `dbo.ProcesosSpertaHubSpot` y `dbo.IntegracionEjecucionLog`
- [ ] **SQL-02**: Implementar `USER_POS_Clientes_Agregar` con deduplicación de filas Pendiente
- [ ] **SQL-03**: Implementar `USER_HS_Cliente_ObtenerDatos` con todos los campos del PRD
- [ ] **SQL-04**: Implementar `USER_HS_ClienteContactos_Buscar`
- [ ] **SQL-05**: Implementar `USER_HS_CuentaCorriente_ObtenerTodos`
- [ ] **SQL-06**: Implementar `USER_HS_Cola_ActualizarEstado`
- [ ] **SQL-07**: Implementar `USER_HS_Cliente_GuardarHubSpotId`

### Capa de Datos (.NET)

- [ ] **DATA-01**: Interface `ISqlDataAccess` con métodos para los 6 SPs
- [ ] **DATA-02**: Implementación `SqlDataAccess` con connection string MSGestion
- [ ] **DATA-03**: DTOs `ClienteHubSpotDto`, `ContactoHubSpotDto`, `CuentaCorrienteItemDto`

### HubSpot Client

- [ ] **HS-01**: `IHubSpotClient` con search/create/patch companies y contacts
- [ ] **HS-02**: Asociación contacto-compañía (PUT association)
- [ ] **HS-03**: Batch update companies (2B)
- [ ] **HS-04**: Rate limiting configurable (`DelayMillisecondsBetweenCalls`)
- [ ] **HS-05**: Backoff en 429 (máx. 3 intentos); detener job en 401
- [ ] **HS-06**: Modo desarrollo mock (`HubSpot:UseDevelopmentMock`)

### Flujo 2A — Cola Clientes/Contactos

- [ ] **2A-01**: Job `ProcesarColaHubSpotJob` cada 5 min, batch size configurable (default 50)
- [ ] **2A-02**: Procesar cola: EnProceso → sync compañía → sync contactos → Ok/Error
- [ ] **2A-03**: Buscar compañía por `mastersoft_id_` si `HubSpotCompanyId` es NULL
- [ ] **2A-04**: Persistir `HubSpotCompanyId` tras crear/encontrar compañía
- [ ] **2A-05**: Deduplicar contactos por email en HubSpot
- [ ] **2A-06**: No reintentar automáticamente registros en Error
- [ ] **2A-07**: Email de error vía `EmailsManager` en fallos

### Flujo 2B — Cuenta Corriente

- [ ] **2B-01**: Job diario 3:00 AM `HubSpotSincronizarCuentaCorrienteJob`
- [ ] **2B-02**: Obtener todos los clientes activos vía SP
- [ ] **2B-03**: Formatear `manejo_cuenta_corriente` (con/sin deuda según PRD)
- [ ] **2B-04**: Enviar batches de 100 compañías con delay 200ms entre batches
- [ ] **2B-05**: Registrar ejecución en `IntegracionEjecucionLog`
- [ ] **2B-06**: Omitir clientes sin `HubSpotCompanyId` con advertencia en log
- [ ] **2B-07**: Email de error en fallos de batch o job crítico

### Consola MVC y Operación

- [ ] **MVC-01**: Botón `POST /Home/ProcesarColaHubSpot` — ejecutar job 2A manual
- [ ] **MVC-02**: Botón `POST /Home/TrazaCola` — conteos y muestra cola Pendiente
- [ ] **MVC-03**: Botón `POST /Home/TrazaCliente?clienteId=N` — traza flujo 2A un cliente
- [ ] **MVC-04**: Botón `POST /Home/CuentaCorrienteBatch` — ejecutar job 2B manual

### Limpieza y Configuración

- [ ] **CFG-01**: Eliminar uso de `HttpSpertaApiClient` en flujos HubSpot
- [ ] **CFG-02**: Documentar claves `HubSpot:*` en Web.config (token comentado)
- [ ] **CFG-03**: Scripts SQL versionados en `sql/` según PRD §13

### Tests

- [ ] **TST-01**: Tests unitarios formateador cuenta corriente
- [ ] **TST-02**: Tests unitarios HubSpotClient con HTTP mockeado
- [ ] **TST-03**: Tests unitarios SqlDataAccess (mockeado o integración)

## v2 Requirements

### Monitoreo

- **MON-01**: Dashboard estado cola y últimas ejecuciones 2B
- **MON-02**: Reprocesamiento manual de registros en Error desde MVC

## Out of Scope

| Feature | Reason |
|---------|--------|
| OAuth HubSpot | PRD: PAT suficiente |
| SpertaAPI / MeLi / MKP / APPro | PRD: eliminados del alcance |
| Reintentos automáticos cola 2A | PRD §11: marcar Error, no reintentar |
| Migración .NET 8 | Mantener Framework 4.5.2 |
| Sync HubSpot → ERP | Solo ERP → HubSpot |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| SQL-01 | Phase 1 | Pending |
| SQL-02 | Phase 1 | Pending |
| SQL-03 | Phase 1 | Pending |
| SQL-04 | Phase 1 | Pending |
| SQL-05 | Phase 1 | Pending |
| SQL-06 | Phase 1 | Pending |
| SQL-07 | Phase 1 | Pending |
| DATA-01 | Phase 1 | Pending |
| DATA-02 | Phase 1 | Pending |
| DATA-03 | Phase 1 | Pending |
| HS-01 | Phase 2 | Pending |
| HS-02 | Phase 2 | Pending |
| HS-03 | Phase 2 | Pending |
| HS-04 | Phase 2 | Pending |
| HS-05 | Phase 2 | Pending |
| HS-06 | Phase 2 | Pending |
| 2A-01 | Phase 3 | Pending |
| 2A-02 | Phase 3 | Pending |
| 2A-03 | Phase 3 | Pending |
| 2A-04 | Phase 3 | Pending |
| 2A-05 | Phase 3 | Pending |
| 2A-06 | Phase 3 | Pending |
| 2A-07 | Phase 3 | Pending |
| 2B-01 | Phase 4 | Pending |
| 2B-02 | Phase 4 | Pending |
| 2B-03 | Phase 4 | Pending |
| 2B-04 | Phase 4 | Pending |
| 2B-05 | Phase 4 | Pending |
| 2B-06 | Phase 4 | Pending |
| 2B-07 | Phase 4 | Pending |
| MVC-01 | Phase 5 | Pending |
| MVC-02 | Phase 5 | Pending |
| MVC-03 | Phase 5 | Pending |
| MVC-04 | Phase 5 | Pending |
| CFG-01 | Phase 5 | Pending |
| CFG-02 | Phase 5 | Pending |
| CFG-03 | Phase 5 | Pending |
| TST-01 | Phase 5 | Pending |
| TST-02 | Phase 5 | Pending |
| TST-03 | Phase 5 | Pending |

**Coverage:**
- v1 requirements: 35 total
- Mapped to phases: 35
- Unmapped: 0 ✓

---
*Requirements defined: 2026-06-06*  
*Last updated: 2026-06-06 after roadmap creation*
