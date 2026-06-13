# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

Documentación indexada: [`docs/README.md`](docs/README.md) · Agentes: [`docs/agents/INDEX.md`](docs/agents/INDEX.md).

## Project

**InterfazHubSpot** — .NET 4.5.2 batch + ASP.NET MVC console syncs ERP Mastersoft → HubSpot CRM via **MSGestion SPs** (no SpertaAPI runtime).

- **2A:** Queue `dbo.ProcesosSpertaHubSpot` → company/contact upsert (`HubSpotIntegracionRunner`)
- **2B:** Paginated cuenta corriente → batch company update (100/call)

PRD: [`docs/PRD_Integracion_HubSpot_2A_2B.md`](docs/PRD_Integracion_HubSpot_2A_2B.md). Architecture: [`docs/explanation/arquitectura.md`](docs/explanation/arquitectura.md).

> Legacy `ISpertaApiClient` / `HttpSpertaApiClient` — do not extend.

## Commands

Use `pwsh -NoProfile -File` (PS 7+). PS 5.1 has no `&&` — see [`.cursor/rules/powershell-windows.mdc`](.cursor/rules/powershell-windows.mdc).

| Action | Command |
|---|---|
| Full build | `pwsh -NoProfile -File scriptsPS1/Build-InterfazHubSpot.ps1` |
| Libraries only | `pwsh -NoProfile -File scriptsPS1/Build-InterfazHubSpot.ps1 -LibrariesOnly` |
| Tests (unit, default) | `pwsh -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1` |
| Tests by category | `pwsh -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1 -Category Security` |
| Single test / filter | `pwsh -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1 -Filter "FullyQualifiedName~ClassName"` |
| Coverage ≥90% | `pwsh -NoProfile -File scriptsPS1/Measure-TestCoverage.ps1` |
| Verify all | `pwsh -NoProfile -File scriptsPS1/Verify-InterfazHubSpot.ps1` |
| Deploy service | `pwsh -NoProfile -File implementacion/Deploy-ServicioHubSpot.ps1` |

Optional build env vars: `SPERTA_MSBUILD` / `MSBUILD_EXE`, `SPERTA_NUGET_EXE`.

Skill: [`.cursor/skills/interfaz-hubspot-dev/SKILL.md`](.cursor/skills/interfaz-hubspot-dev/SKILL.md).

## Architecture

Layers: MVC → BatchProcess (`IScheduler`) → Business (`HubSpot/`) → Entities/Interfaces/Mapping.

| Layer | Project | Key classes |
|-------|---------|-------------|
| Dev console | `InterfazHubSpot` (MVC) | Home controller, JSON traces |
| Jobs | `InterfazHubSpot.BatchProcess` | `ProcesarColaIntegracionesHubSpotJob`, `HubSpotSincronizarCuentaCorrienteJob` |
| Business | `InterfazHubSpot.Business` | `HubSpotIntegracionRunner`, `HubSpotCrmClient`, `ClienteIntegracionManager` |
| Data | `InterfazHubSpot.Entities` | EF6 over MSGestion |
| Contracts | `InterfazHubSpot.Interfaces` | Shared interfaces |
| Mapping | `InterfazHubSpot.Mapping` | AutoMapper |

**Two execution modes** — same `IScheduler` classes, same Business DLL:
- **Dev:** IIS Express + MVC (buttons trigger jobs, step-by-step JSON traces)
- **Prod:** `MSScheduler452Service.exe` Windows service + `Config.xml` cron

**One connection string** `MSGestion` (queue, log tables, SPs, EF6). No `MSFwk`, no multitenancy.

**HubSpot HTTP** — `HubSpotCrmClient` uses Private App Token (`Authorization: Bearer`). Set `HubSpot:UseDevelopmentMock=true` in `Web.config` to stub all HTTP calls via `DevelopmentHubSpotStubHandler` without a real token.

Key config keys (full list: [`docs/reference/configuracion.md`](docs/reference/configuracion.md)):

| Key | Notes |
|-----|-------|
| `HubSpot:PrivateAppToken` | Required when mock=false |
| `HubSpot:UseDevelopmentMock` | `true` for local dev |
| `HubSpot:DelayMillisecondsBetweenCalls` | Default 120 ms |
| `HubSpot:MaxHttpRetries` | Default 3 (429/5xx only) |

Config templates: `Web.config.example`, `App.config.example` in solution root and `implementacion/`.

Reference docs: [Config](docs/reference/configuracion.md) · [SQL/SPs](docs/reference/base-datos.md) · [HubSpot HTTP](docs/reference/hubspot-crm.md) · [MVC traces](docs/reference/consola-mvc.md) · [Code map](docs/reference/mapas-codigo.md).

## Tests

[`docs/TESTING.md`](docs/TESTING.md) — xUnit, two projects: `InterfazHubSpot.Tests.Unit` and `InterfazHubSpot.IntegrationTests`.

Test categories (`Trait("Category", ...)`):

| Category | Default run | Covers |
|----------|-------------|--------|
| **Unit** | yes | HubSpot client, mapper, queue, diagnostics |
| **Security** | excluded | Token, 401 fail-fast, SQL sanitization |
| **Integration** | excluded | Mapper SP→DTO, queue projections |
| **Live** | excluded (skipped) | EF6 against real MSGestion — needs `App.config` + Mastersoft refs |

```powershell
# Run by category
pwsh -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1 -Category Security
pwsh -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1 -Category Integration
pwsh -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1 -Category All

# Run a single test class
pwsh -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1 -Filter "FullyQualifiedName~HubSpotCrmClientTests"
```

Pre-merge canonical check: `pwsh -NoProfile -File scriptsPS1/Verify-InterfazHubSpot.ps1 -LibrariesOnly` (includes grep for blocked legacy names).

## Critical rules

- .NET Framework 4.5.2 only — no .NET 8 patterns
- Never commit secrets / `Web.config` / `App.config`
- Locked naming: `InterfazHubSpot`, `ProcesosSpertaHubSpot`, `HubSpot` (capital S). Prohibited: `Hubspot`, `BatchSpertaAPI`, `ProcesosSpertaAPI`
- Queue 2A errors: mark `Error`, no auto-retry
- WinForms gate before queue table rename (PRD §5.1)
- HubSpot runners live in `InterfazHubSpot.Business/HubSpot/` (namespace `InterfazHubSpot.Business.HubSpot`)

GSD: [`.planning/ROADMAP.md`](.planning/ROADMAP.md), [`.planning/STATE.md`](.planning/STATE.md).
