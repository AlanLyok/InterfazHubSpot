# AGENTS.md — Integración HubSpot InterfazHubSpot

Guía mínima para agentes AI. **Índice completo:** [`docs/README.md`](docs/README.md) · **Enrutamiento por tarea:** [`docs/agents/INDEX.md`](docs/agents/INDEX.md).

## Proyecto

Batch **2A** (cola outbox → HubSpot company/contact) y **2B** (cuenta corriente batch). Datos: SPs MSGestion. Stack: **.NET Framework 4.5.2**.

| Necesito… | Documento |
|-----------|-----------|
| Requisitos funcionales | [`docs/PRD_Integracion_HubSpot_2A_2B.md`](docs/PRD_Integracion_HubSpot_2A_2B.md) |
| Arquitectura | [`docs/explanation/arquitectura.md`](docs/explanation/arquitectura.md) |
| Config / SQL / HubSpot API / MVC | [`docs/reference/`](docs/reference/) |
| Batch + servicio Windows | [`docs/BatchProcess_Desarrollo_e_Implementacion.md`](docs/BatchProcess_Desarrollo_e_Implementacion.md) |
| Planificación GSD | [`.planning/ROADMAP.md`](.planning/ROADMAP.md) |

## Comandos canónicos

Desde raíz del repo (`pwsh` o `powershell.exe`):

```powershell
pwsh -NoProfile -File scriptsPS1/Build-InterfazHubSpot.ps1
pwsh -NoProfile -File scriptsPS1/Build-InterfazHubSpot.ps1 -LibrariesOnly
pwsh -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1
pwsh -NoProfile -File scriptsPS1/Measure-TestCoverage.ps1
pwsh -NoProfile -File scriptsPS1/Verify-InterfazHubSpot.ps1
pwsh -NoProfile -File implementacion/Deploy-ServicioHubSpot.ps1
```

Parámetros: [`scriptsPS1/README.md`](scriptsPS1/README.md). Tests: [`docs/TESTING.md`](docs/TESTING.md).

Variables MSBuild: `SPERTA_MSBUILD`, `MSBUILD_EXE`, `SPERTA_NUGET_EXE`.

## PowerShell Windows

Shell default: **PS 5.1** — **no usar `&&`**. Regla: [`.cursor/rules/powershell-windows.mdc`](.cursor/rules/powershell-windows.mdc).

## Reglas críticas

- **Nunca** versionar `HubSpot:PrivateAppToken` ni `Web.config`/`App.config` reales
- Errores cola 2A → `Error`; **no** reintentar fila automáticamente
- Rate limit: 120 ms; 429 backoff (máx. 3); **401 fail-fast**
- Nombres bloqueados: `InterfazHubSpot`, `ProcesosSpertaHubSpot`, `HubSpot` (S mayúscula)
- Legacy `ISpertaApiClient` / jobs Sperta: **no extender**
- Gate WinForms antes de `sp_rename` cola (PRD §5.1)

## Skills y rules

| Recurso | Cuándo |
|---------|--------|
| `.cursor/skills/interfaz-hubspot-dev` | Siempre en este repo |
| `.cursor/skills/dotnet-best-practices` | C# / .NET 4.5.2 |
| `.cursor/rules/interfaz-hubspot.mdc` | Nombres, build |
| `.cursor/rules/powershell-windows.mdc` | Terminal |

## MCP SQL

- `project-0-INTERFAZHUBSPOT-mssql-mcp-msgestion-CALZETTA` — MSGestion
- Leer schema en `mcps/<server>/tools/` antes de invocar

## Fase actual

[`.planning/STATE.md`](.planning/STATE.md)
