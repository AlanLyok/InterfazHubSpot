# AGENTS.md — Integración HubSpot BatchSpertaAPI

Guía para agentes AI (Cursor) trabajando en este repositorio.

## Proyecto

**Integración HubSpot Flujos 2A y 2B** — Batch que sincroniza ERP Mastersoft → HubSpot CRM.  
**PRD:** `docs/PRD_Integracion_HubSpot_2A_2B.md`  
**Planificación GSD:** `.planning/`

## Stack (este repo)

| Componente | Tecnología |
|------------|------------|
| Framework | **.NET Framework 4.5.2** (NO .NET 8) |
| Web | ASP.NET MVC |
| Batch | `BatchSpertaAPI.BatchProcess` / IScheduler |
| Datos HubSpot | SQL Server — **solo stored procedures** |
| API externa | HubSpot CRM v3 — Private App Token |
| Tests | xUnit (`BatchSpertaAPI.Tests.Unit`, `IntegrationTests`) |

> El skill `.cursor/skills/dotnet-best-practices` describe **OrdenTrabajoApi (.NET 8)**. No aplicar esos patrones aquí salvo migración futura explícita.

## Skills a usar

| Skill | Cuándo |
|-------|--------|
| `.cursor/skills/get-shit-done` | Planificación, fases, ejecución GSD |
| `.cursor/skills/documentation-writer` | Documentación técnica |
| `.cursor/skills/cursor-best-practices` | Workflow Cursor |
| `systematic-debugging` | Bugs y fallos de jobs |
| `verification-before-completion` | Antes de declarar fase completa |

## Workflow GSD

1. Leer `.planning/STATE.md` y fase actual en `ROADMAP.md`
2. `/gsd-discuss-phase N` → `/gsd-plan-phase N` → `/gsd-execute-phase N`
3. Verificar con tests: `pwsh -NoProfile -File .\scripts\Run-Tests-BatchSpertaAPI.ps1`
4. Build: `pwsh -NoProfile -File .\scripts\Build-BatchSpertaAPI.ps1 -LibrariesOnly`

## Reglas críticas

- **Nunca** versionar `HubSpot:PrivateAppToken` en Web.config
- **No** usar `HttpSpertaApiClient` en código nuevo HubSpot — usar `ISqlDataAccess`
- Errores cola 2A: marcar `Error`, **no reintentar** automáticamente
- Rate limit HubSpot: delay 120ms; backoff 429 (máx. 3); detener en 401
- Confirmar con Calzetta los 3 puntos abiertos del PRD §14 antes de cerrar SPs

## Estructura clave

```
BatchSpertaAPI.sln
├── InterfazHubSpot/          # Runners 2A y 2B
├── BatchSpertaAPI.Business/  # SqlDataAccess, EmailsManager
├── BatchSpertaAPI.BatchProcess/  # Jobs scheduler
├── sql/                      # Scripts SP (crear según PRD)
└── docs/PRD_Integracion_HubSpot_2A_2B.md
```

## MCP SQL (desarrollo)

Servidores MCP disponibles: `user-mssql-mcp-msgestion`, `user-mssql-mcp-msfwk`, `user-mssql-mcp-msordentrabajo`. Usar para validar tablas y probar SPs en desarrollo.

## Fase actual

**Phase 1: SQL y capa de datos** — Ver `.planning/ROADMAP.md`
