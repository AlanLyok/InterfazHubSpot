# Índice para agentes AI

**Objetivo:** enrutar a **un solo documento** por tarea — ahorro de tokens, sin duplicar PRD ni README.

**Índice humano completo:** [`../README.md`](../README.md).

---

## Entrada rápida por tarea

| Quiero… | Leer primero | Luego si hace falta |
|---------|--------------|---------------------|
| Entender el proyecto en 30 s | [`../../AGENTS.md`](../../AGENTS.md) | [`../explanation/arquitectura.md`](../explanation/arquitectura.md) |
| Compilar / test / verify | [`../../AGENTS.md`](../../AGENTS.md) § comandos | [`../../scriptsPS1/README.md`](../../scriptsPS1/README.md), [`../TESTING.md`](../TESTING.md) |
| Requisitos funcionales cola/2A/2B | [`../PRD_Integracion_HubSpot_2A_2B.md`](../PRD_Integracion_HubSpot_2A_2B.md) | — |
| Arquitectura / capas | [`../explanation/arquitectura.md`](../explanation/arquitectura.md) | [`../reference/mapas-codigo.md`](../reference/mapas-codigo.md) |
| Dominio (entidades, eventos) | [`../explanation/dominio.md`](../explanation/dominio.md) | [`../explanation/flujos-2a-2b.md`](../explanation/flujos-2a-2b.md) |
| Flujo datos 2A o 2B | [`../explanation/flujos-2a-2b.md`](../explanation/flujos-2a-2b.md) | PRD |
| Config Web.config / App.config | [`../reference/configuracion.md`](../reference/configuracion.md) | `Web.config.example` |
| SPs, tablas, deploy SQL | [`../reference/base-datos.md`](../reference/base-datos.md) | `scriptsSQL/` |
| HubSpot API, rate limit, 401 | [`../reference/hubspot-crm.md`](../reference/hubspot-crm.md) | Código `HubSpotCrmClient.cs` |
| Endpoints MVC traza | [`../reference/consola-mvc.md`](../reference/consola-mvc.md) | [`../how-to/debug-integracion.md`](../how-to/debug-integracion.md) |
| Servicio Windows / Config.xml | [`../BatchProcess_Desarrollo_e_Implementacion.md`](../BatchProcess_Desarrollo_e_Implementacion.md) | [`../how-to/desplegar-servicio-windows.md`](../how-to/desplegar-servicio-windows.md) |
| Bug job / sync | [`../how-to/debug-integracion.md`](../how-to/debug-integracion.md) | Skill `systematic-debugging` |
| Fase GSD / roadmap | [`.planning/ROADMAP.md`](../../.planning/ROADMAP.md) | [`.planning/STATE.md`](../../.planning/STATE.md) |

---

## Reglas bloqueadas (no re-leer PRD entero)

| Regla | Fuente |
|-------|--------|
| Stack .NET **4.5.2** — no .NET 8 | [`../../AGENTS.md`](../../AGENTS.md) |
| Nombres: `InterfazHubSpot`, `ProcesosSpertaHubSpot`, `HubSpot` | [`.cursor/rules/interfaz-hubspot.mdc`](../../.cursor/rules/interfaz-hubspot.mdc) |
| PowerShell: no `&&` en PS 5.1 | [`.cursor/rules/powershell-windows.mdc`](../../.cursor/rules/powershell-windows.mdc) |
| Errores cola 2A → `Error`, sin auto-retry | PRD + [`../reference/hubspot-crm.md`](../reference/hubspot-crm.md) |
| No versionar tokens / Web.config | [`../reference/configuracion.md`](../reference/configuracion.md) |
| Legacy SpertaAPI: no extender | [`../explanation/arquitectura.md`](../explanation/arquitectura.md) |

---

## Skills Cursor (repo)

| Skill | Cuándo invocar |
|-------|----------------|
| `.cursor/skills/interfaz-hubspot-dev` | **Siempre** — tarea en este repo |
| `.cursor/skills/dotnet-best-practices` | C#, csproj, MVC, managers |
| `.cursor/skills/documentation-writer` | Crear/editar docs Diátaxis |
| `systematic-debugging` | Fallos jobs / HubSpot / SQL |
| `verification-before-completion` | Antes de declarar hecho |

---

## MCP SQL

1. Listar tools: `mcps/project-0-INTERFAZHUBSPOT-mssql-mcp-msgestion-CALZETTA/tools/`
2. Servidor MSGestion: cola, SPs, datos ERP
3. Config: `.cursor/mcp.mssql-mcp-server.example.json`

**No** asumir nombres SP legacy `USER_HS_*` — canónicos: `InterfazHubSpot_*` ([`../reference/base-datos.md`](../reference/base-datos.md)).

---

## Comandos canónicos (copiar tal cual)

```powershell
pwsh -NoProfile -File scriptsPS1/Build-InterfazHubSpot.ps1
pwsh -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1
pwsh -NoProfile -File scriptsPS1/Measure-TestCoverage.ps1
pwsh -NoProfile -File scriptsPS1/Verify-InterfazHubSpot.ps1
pwsh -NoProfile -File implementacion/Deploy-ServicioHubSpot.ps1
```

---

## Qué NO cargar por defecto

- [`../RequerimientosIniciales/`](../RequerimientosIniciales/) — histórico
- [`.planning/research/`](../../.planning/research/) — verificar contra código
- PRD completo si solo necesitas config o un endpoint → usar `reference/`
