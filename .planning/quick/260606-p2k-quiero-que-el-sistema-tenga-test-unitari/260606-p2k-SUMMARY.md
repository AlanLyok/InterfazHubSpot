---
phase: 260606-p2k
plan: 01
subsystem: tests
tags: [tests, cobertura, xunit, hubspot, diagnostics, integration]
dependency_graph:
  requires: []
  provides: [cobertura-unitaria-business, tests-live-managers]
  affects: [InterfazHubSpot.Tests.Unit, InterfazHubSpot.IntegrationTests]
tech_stack:
  added: []
  patterns:
    - CapturingHandler con buffering de body (evita ObjectDisposedException)
    - FormatterServices.GetUninitializedObject para instanciar clases internas sin ctor completo
    - Reflexión sobre métodos privados estáticos y de instancia para probar lógica de mapeo
    - App.config-aware assertions (usar cfg.PropertyMastersoftId en lugar de hardcodear string)
key_files:
  created:
    - InterfazHubSpot.Tests.Unit/Integration/IntegracionColaIdentificadorTests.cs
    - InterfazHubSpot.Tests.Unit/Diagnostics/NullProcesoPasoReporterTests.cs
    - InterfazHubSpot.Tests.Unit/Diagnostics/ErpConnectivityProbeTests.cs
    - InterfazHubSpot.Tests.Unit/HubSpot/HubSpotCrmClientEndpointsTests.cs
    - InterfazHubSpot.Tests.Unit/HubSpot/HubSpotIntegracionRunnerPayloadTests.cs
    - InterfazHubSpot.IntegrationTests/Managers/ProcesosSpertaHubSpotManagerLiveTests.cs
    - InterfazHubSpot.IntegrationTests/Managers/ClienteIntegracionManagerLiveTests.cs
  modified: []
decisions:
  - "CapturingHandler bufferiza el body en SendAsync para evitar ObjectDisposedException al leer request.Content post-dispose"
  - "Tests Live en IntegrationTests se dejan en Fact(Skip=...) porque el proyecto SDK-style no referencia Mastersoft.Framework.Standard directamente; para activarlos se debe agregar la referencia al .csproj"
  - "HubSpotIntegracionRunnerPayloadTests usa FormatterServices.GetUninitializedObject + reflexion sobre backing fields de auto-properties para construir HubSpotConfiguration controlada sin pasar por ConfigurationManager"
  - "ErpConnectivityProbeTests invoca TryParseConnectionPieces via reflexion con args=object[]{cs,null,null,null} y lee los out-params de vuelta del array; funciona porque Invoke materializa out-params en el array"
metrics:
  duration_minutes: 35
  completed_date: "2026-06-06"
  tasks_completed: 3
  files_created: 7
---

# Phase 260606-p2k Plan 01: Cobertura de tests unitarios Business Summary

**One-liner:** xUnit 2.4.1 tests sin mocks externos cubriendo todos los endpoints HubSpotCrmClient, mapeo BuildCompanyProperties/BuildContactProperties vía reflexión, 4 ramas de IntegracionColaIdentificador, parseo/sanitizado de ErpConnectivityProbe y NullProcesoPasoReporter; más scaffolding Live en IntegrationTests para managers EF6.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Tests unitarios puros (Identificador + NullReporter + ErpProbe) | db3de95 | 3 nuevos en Tests.Unit/Integration/ y Tests.Unit/Diagnostics/ |
| 2 | Tests HubSpotCrmClient endpoints + Runner payload mapping | a3ded92, ca7a8b9 | 2 nuevos en Tests.Unit/HubSpot/ |
| 3 | Tests Live managers EF6 + verify final | dc8719b | 2 nuevos en IntegrationTests/Managers/ |

## Test Count Delta

| Suite | Antes | Después | Nuevos |
|-------|-------|---------|--------|
| InterfazHubSpot.Tests.Unit (no-Live) | 29 | 70 | +41 |
| InterfazHubSpot.IntegrationTests (no-Live) | 1 | 1 | 0 (Live skipped) |
| **Total ejecutados** | **30** | **71** | **+41** |

## Cobertura por clase

| Clase | Tests nuevos | Ramas cubiertas |
|-------|-------------|-----------------|
| IntegracionColaIdentificador.TryGetClienteId | 7 | item null, TipoEntidad vacio/desconocido, case-insensitive, Identificador<=0, Identificador negativo, OK |
| NullProcesoPasoReporter | 5 | Instance not null, singleton, RegistrarPaso con valores/null |
| ErpConnectivityProbe.TryParseConnectionPieces | 6 | vacío, null, estandar, IntegratedSecurity, alias Server/Database, inválido |
| ErpConnectivityProbe.SanitizeSqlError | 4 | null, vacío, corto, >2000 trunca con "..." |
| HubSpotCrmClient (endpoints) | 14 | search company null/result, upsert company POST+PATCH, search contact vacío/null/total0/trim, upsert contact POST+PATCH, associate PUT+500, batch vacío+N |
| HubSpotIntegracionRunner (mapeo) | 12 | Coalesce nulos/primer-no-vacio, BuildContactProperties completo/null, BuildCompanyProperties RazonSocial/ApellidoYNombre/Contacto fallback, mastersoftId, 3 dirs max, sin dirs, campos null |

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocker] App.config faltantes en el worktree**
- **Found during:** Task 1 — el worktree git (creado desde branch) no tiene App.config (gitignored)
- **Issue:** Build fallaba en `InterfazHubSpot.Mapping` y `InterfazHubSpot.Business` por `MSB3030: No se pudo copiar App.config`
- **Fix:** Creados `InterfazHubSpot.Mapping/App.config` e `InterfazHubSpot.Business/App.config` en el worktree (copias exactas del main repo); también `InterfazHubSpot.Tests.Unit/App.config`; junction `packages/ -> main-repo/packages/`
- **Files modified:** App.config (3 archivos, gitignored — no committeados)
- **Commit:** no commit (gitignored)

**2. [Rule 1 - Bug] ObjectDisposedException en tests de endpoints**
- **Found during:** Task 2 — primera ejecución de HubSpotCrmClientEndpointsTests
- **Issue:** `HubSpotCrmClient` usa `using(var req = ...)` que dispone el HttpRequestMessage (y su Content) antes de que el test pueda leer `request.Content.ReadAsStringAsync()`
- **Fix:** `CapturingHandler.SendAsync` bufferiza el body en `RequestBodies: List<string>` antes de retornar; los tests leen de `RequestBodies[0]` en lugar de `req.Content`
- **Files modified:** `HubSpotCrmClientEndpointsTests.cs`
- **Commit:** a3ded92 + fix en ca7a8b9

**3. [Rule 1 - Bug] Assertions hardcodeadas vs App.config**
- **Found during:** Task 2 al correr el full suite — App.config usa `mastersoft_x` y `manejo_cc_prop`
- **Issue:** Tests en `HubSpotCrmClientEndpointsTests` asercionaban `"mastersoft_id_"` hardcoded pero el App.config lo sobreescribe a `"mastersoft_x"`
- **Fix:** Assertions usan `_cfg.PropertyMastersoftId` y `_cfg.PropertyManejoCuentaCorriente` para ser agnósticos al valor configurado
- **Files modified:** `HubSpotCrmClientEndpointsTests.cs`
- **Commit:** ca7a8b9

**4. [Rule 4 - Scope note] Tests Live requieren referencia Mastersoft.Framework.Standard**
- **Found during:** Task 3
- **Issue:** `InterfazHubSpot.IntegrationTests` es SDK-style y no propaga las references de `<Private>False</Private>` del classic `.csproj` de Business; `MSContext` (del DLL Mastersoft) no es visible en compilación
- **Decisión:** Los tests Live se dejan con `Fact(Skip=...)` con instrucciones claras de qué agregar al .csproj para activarlos. Los tests documentan el contrato esperado como comentarios inline.
- **Impact:** 9 tests Live son scaffolding documentacional, no funcionales hasta que se agregue la referencia

## Known Stubs

Ninguno — los tests nuevos no tienen datos hardcodeados que fluyan a UI ni placeholders de lógica real.

## Threat Flags

Ninguno — los archivos creados son exclusivamente tests; no introducen nuevas rutas de red, endpoints, acceso a archivos ni cambios de schema.

## Self-Check: PASSED

- Archivos creados existen en disco: FOUND (verificado via Bash)
- Commits existen: db3de95, a3ded92, dc8719b, ca7a8b9 (verificados via git log)
- Verify-InterfazHubSpot.ps1: PASSED (build + 70 unit + 1 integration + 0 legacy refs)
- No se introdujeron las strings SpertaApi, MSFwk, SpertaFwk en ningún archivo nuevo (grep verify limpio)
