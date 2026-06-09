# CLAUDE.md

Contexto para Claude Code. **Documentación indexada:** [`docs/README.md`](docs/README.md) · **Agentes:** [`docs/agents/INDEX.md`](docs/agents/INDEX.md).

## Project

**InterfazHubSpot** — .NET 4.5.2 batch + ASP.NET MVC console syncs ERP Mastersoft → HubSpot CRM via **MSGestion SPs** (no SpertaAPI runtime).

- **2A:** Queue `dbo.ProcesosSpertaHubSpot` → company/contact upsert (`HubSpotIntegracionRunner`)
- **2B:** Paginated cuenta corriente → batch company update

PRD: [`docs/PRD_Integracion_HubSpot_2A_2B.md`](docs/PRD_Integracion_HubSpot_2A_2B.md). Architecture: [`docs/explanation/arquitectura.md`](docs/explanation/arquitectura.md).

> Legacy `ISpertaApiClient` / `HttpSpertaApiClient` — do not extend.

## Commands

From repo root. PS 5.1: no `&&` — see [`.cursor/rules/powershell-windows.mdc`](.cursor/rules/powershell-windows.mdc).

| Action | Command |
|---|---|
| Full build | `pwsh -NoProfile -File scriptsPS1/Build-InterfazHubSpot.ps1` |
| Libraries only | `pwsh -NoProfile -File scriptsPS1/Build-InterfazHubSpot.ps1 -LibrariesOnly` |
| Tests | `pwsh -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1` |
| Coverage ≥90% | `pwsh -NoProfile -File scriptsPS1/Measure-TestCoverage.ps1` |
| Verify all | `pwsh -NoProfile -File scriptsPS1/Verify-InterfazHubSpot.ps1` |
| Deploy service | `pwsh -NoProfile -File implementacion/Deploy-ServicioHubSpot.ps1` |

Skill: [`.cursor/skills/interfaz-hubspot-dev/SKILL.md`](.cursor/skills/interfaz-hubspot-dev/SKILL.md).

## Architecture (pointer)

Layers: MVC → BatchProcess (`IScheduler`) → Business (`HubSpot/`) → Entities/Interfaces/Mapping.

- Config keys: [`docs/reference/configuracion.md`](docs/reference/configuracion.md)
- SQL/SPs: [`docs/reference/base-datos.md`](docs/reference/base-datos.md)
- HubSpot HTTP: [`docs/reference/hubspot-crm.md`](docs/reference/hubspot-crm.md)
- MVC traces: [`docs/reference/consola-mvc.md`](docs/reference/consola-mvc.md)
- Code map: [`docs/reference/mapas-codigo.md`](docs/reference/mapas-codigo.md)

## Critical rules

- .NET Framework 4.5.2 only — no .NET 8 patterns
- Never commit secrets / `Web.config` / `App.config`
- Locked naming: `InterfazHubSpot`, `ProcesosSpertaHubSpot`, `HubSpot`
- Queue 2A errors: mark `Error`, no auto-retry
- WinForms gate before queue table rename (PRD §5.1)

## Tests

[`docs/TESTING.md`](docs/TESTING.md) — xUnit, categories Unit/Security/Integration/Live, coverlet gate.

GSD: [`.planning/ROADMAP.md`](.planning/ROADMAP.md), [`.planning/STATE.md`](.planning/STATE.md).
