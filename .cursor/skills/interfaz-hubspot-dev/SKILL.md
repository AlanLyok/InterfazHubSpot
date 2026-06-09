---
name: interfaz-hubspot-dev
description: >-
  Experto en desarrollo del proyecto InterfazHubSpot (.NET Framework 4.5.2, ASP.NET MVC, batch
  HubSpot, SQL Server MSGestion, Windows, MCP, skills y rules de Cursor). Usar en cualquier tarea
  de código, build, test, debug, SQL, integración HubSpot o configuración de agentes en este
  repositorio INTERFAZHUBSPOT.
---

# InterfazHubSpot — desarrollo experto

Skill principal de este repo. Cargar contexto antes de implementar, depurar o ejecutar comandos.

## Contexto del proyecto

Integración ERP Mastersoft → HubSpot CRM (flujos **2A** cola outbox y **2B** cuenta corriente).  
Datos vía **stored procedures** en MSGestion; runtime **no** usa SpertaAPI.

| Recurso | Ubicación |
|---------|-----------|
| Guía agentes | `AGENTS.md`, `CLAUDE.md` |
| PRD | `docs/PRD_Integracion_HubSpot_2A_2B.md` |
| BatchProcess dev/deploy | `docs/BatchProcess_Desarrollo_e_Implementacion.md` |
| Planificación | `.planning/ROADMAP.md`, `.planning/STATE.md` |
| Reglas nombres/build | `.cursor/rules/interfaz-hubspot.mdc` |
| PowerShell Windows | `.cursor/rules/powershell-windows.mdc` |
| Stack C# | `.cursor/skills/dotnet-best-practices/SKILL.md` |

## Checklist antes de codear

- [ ] Stack: **.NET Framework 4.5.2** — no .NET 8 / Core
- [ ] Nombres: `InterfazHubSpot`, `ProcesosSpertaHubSpot`, `HubSpot` (S mayúscula)
- [ ] Runners en `InterfazHubSpot.Business/HubSpot/`
- [ ] Config en `Web.config`/`App.config` — sin secretos en git
- [ ] Comandos vía scripts PS1 canónicos (`pwsh -NoProfile -File ...`)
- [ ] Shell: sintaxis **PowerShell** (no `&&` en PS 5.1)

## Comandos canónicos

```powershell
pwsh -NoProfile -File SolucionInterfazHubSpot/InterfazHubSpot/Scripts/agent/Build-InterfazHubSpot.ps1
pwsh -NoProfile -File SolucionInterfazHubSpot/InterfazHubSpot/Scripts/agent/Build-InterfazHubSpot.ps1 -LibrariesOnly
pwsh -NoProfile -File SolucionInterfazHubSpot/InterfazHubSpot/Scripts/agent/Test-InterfazHubSpot.ps1
pwsh -NoProfile -File SolucionInterfazHubSpot/InterfazHubSpot/Scripts/agent/Verify-InterfazHubSpot.ps1
pwsh -NoProfile -File implementacion/Deploy-ServicioHubSpot.ps1
```

Verificar antes de declarar trabajo completo (`verification-before-completion`).

## Arquitectura rápida

```
Cola dbo.ProcesosSpertaHubSpot
  → ProcesarColaIntegracionesHubSpotJob (2A) / HubSpotSincronizarCuentaCorrienteJob (2B)
  → HubSpotIntegracionRunner
  → ClienteIntegracionManager (SPs MSGestion)
  → HubSpotCrmClient (CRM v3)
```

Traza debug MVC: `POST /Home/ProcesarColaHubSpot`, `…TrazaCola`, `…TrazaCliente?clienteId=n`.

## MCP SQL (desarrollo)

Antes de llamar herramientas MCP, leer schema en `mcps/<server>/tools/`.

| Servidor | Uso |
|----------|-----|
| `project-0-INTERFAZHUBSPOT-mssql-mcp-msgestion-CALZETTA` | Cola, SPs integración, datos ERP |
| `project-0-INTERFAZHUBSPOT-mssql-mcp-msfwk-CALZETTA` | MSFwk (si aplica) |

Config ejemplo: `.cursor/mcp.mssql-mcp-server.example.json`, bootstrap `services/mssql-mcp-server/`.

## Reglas de integración HubSpot

- Errores cola 2A → estado `Error`; **no** reintentar automáticamente
- Rate limit: 120 ms entre calls; 429 backoff máx. 3; parar en 401
- Mock dev: `HubSpot:UseDevelopmentMock=true`
- Gate WinForms antes de `sp_rename` tabla cola (PRD §5.1)

## Skills complementarios

| Skill | Cuándo |
|-------|--------|
| `dotnet-best-practices` | C#, csproj, MVC, managers, runners |
| `get-shit-done` | Fases GSD, planes, ejecución |
| `documentation-writer` | Docs técnicas Diátaxis |
| `systematic-debugging` | Fallos de jobs / HubSpot / SQL |
| `cursor-best-practices` | Rules, skills, MCP en Cursor |

## Cursor / agentes en este repo

- **Rules** (`.cursor/rules/`): contexto estático siempre activo (`alwaysApply: true` en reglas clave).
- **Skills** (`.cursor/skills/`): capacidades dinámicas; este skill es el hub del proyecto.
- **AGENTS.md**: índice para cualquier agente AI del repo.
- Al crear reglas nuevas: concisas, referenciar archivos, sin copiar PRD entero.
- PowerShell: seguir `powershell-windows.mdc` — error típico `&&` no válido en PS 5.1.

## Qué evitar

- Patrones OrdenTrabajoApi / .NET 8 del skill antiguo mal aplicados
- `dotnet build` como sustituto de MSBuild en verificación formal
- Bash-ismos en terminal Windows (`&&`, heredoc `<<EOF`, `export`)
- Reintroducir nombres legacy de solución/tabla/cola (ver `.cursor/rules/interfaz-hubspot.mdc`)
