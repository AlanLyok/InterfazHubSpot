# AGENTS.md — Integración HubSpot InterfazHubSpot

Guía para agentes AI (Cursor) trabajando en este repositorio.

## Proyecto

**Integración HubSpot Flujos 2A y 2B** — Batch que sincroniza ERP Mastersoft → HubSpot CRM.  
**PRD:** `docs/PRD_Integracion_HubSpot_2A_2B.md`  
**BatchProcess (desarrollo + servicio Windows):** `docs/BatchProcess_Desarrollo_e_Implementacion.md`  
**Planificación GSD:** `.planning/`

## Stack (este repo)

| Componente | Tecnología |
|------------|------------|
| Framework | **.NET Framework 4.5.2** (NO .NET 8) |
| Web | ASP.NET MVC (`SolucionInterfazHubSpot/InterfazHubSpot/`) |
| Batch | `SolucionInterfazHubSpot/InterfazHubSpot.BatchProcess` / IScheduler |
| HubSpot runners | `SolucionInterfazHubSpot/InterfazHubSpot.Business/HubSpot/` |
| Servicio Windows | `implementacion/ServicioInterfazHubSpot_Implementacion/` |
| Datos cola | SQL Server — tabla `dbo.ProcesosSpertaHubSpot` |
| Datos ERP | SPs en MSGestion — `ClienteIntegracionManager` (`dbo.InterfazHubSpot_Cliente_Obtener` 004, `dbo.InterfazHubSpot_Clientes_Contactos_Obtener` 005, `dbo.InterfazHubSpot_CuentaCorriente_Pagina` 006) |
| API externa | HubSpot CRM v3 — Private App Token |
| Tests | xUnit (`InterfazHubSpot.Tests.Unit`, `InterfazHubSpot.IntegrationTests`) |

> Stack bloqueado: **.NET Framework 4.5.2**. No recomendar .NET 8, ASP.NET Core ni `dotnet build` como camino principal.

## PowerShell en Windows

Shell por defecto: **Windows PowerShell 5.1**. Regla obligatoria: [`.cursor/rules/powershell-windows.mdc`](.cursor/rules/powershell-windows.mdc).

| Error típico | Causa | Solución |
|--------------|-------|----------|
| `El token '&&' no es un separador válido` | `&&` solo en PS 7+ | Usar `;` + `$?` / `$LASTEXITCODE`, o `pwsh` explícito |
| Heredoc `<<EOF` falla | Sintaxis bash | Here-string PowerShell `@" "@` o `-m` múltiple en git |
| `%VAR%` no expande | Sintaxis CMD | `$env:VAR` |

Preferir scripts canónicos con `pwsh -NoProfile -File ...` en lugar de encadenar comandos a mano.

## Comandos canónicos

| Acción | Comando |
|--------|---------|
| Build completo | `pwsh -NoProfile -File SolucionInterfazHubSpot/InterfazHubSpot/Scripts/agent/Build-InterfazHubSpot.ps1` |
| Solo librerías | `pwsh -NoProfile -File SolucionInterfazHubSpot/InterfazHubSpot/Scripts/agent/Build-InterfazHubSpot.ps1 -LibrariesOnly` |
| Tests | `pwsh -NoProfile -File SolucionInterfazHubSpot/InterfazHubSpot/Scripts/agent/Test-InterfazHubSpot.ps1` |
| Verificar todo | `pwsh -NoProfile -File SolucionInterfazHubSpot/InterfazHubSpot/Scripts/agent/Verify-InterfazHubSpot.ps1` |
| Deploy servicio | `pwsh -NoProfile -File implementacion/Deploy-ServicioHubSpot.ps1` |

Variables MSBuild: `SPERTA_MSBUILD`, `MSBUILD_EXE`, `SPERTA_NUGET_EXE`.

## Skills y reglas (Cursor)

| Recurso | Cuándo |
|---------|--------|
| `.cursor/skills/interfaz-hubspot-dev` | **Siempre** — hub experto del proyecto (stack, MCP, build, HubSpot) |
| `.cursor/skills/dotnet-best-practices` | C#, csproj, MVC, managers — **.NET Framework 4.5.2** |
| `.cursor/rules/interfaz-hubspot.mdc` | Nombres bloqueados y comandos build/test |
| `.cursor/rules/powershell-windows.mdc` | Cualquier comando en terminal Windows |
| `.cursor/skills/get-shit-done` | Planificación, fases, ejecución GSD |
| `.cursor/skills/documentation-writer` | Documentación técnica |
| `.cursor/skills/cursor-best-practices` | Rules vs skills, MCP, flujo agente |
| `systematic-debugging` | Bugs y fallos de jobs |
| `verification-before-completion` | Antes de declarar fase completa |

## Reglas críticas

- **Nunca** versionar `HubSpot:PrivateAppToken` en Web.config/App.config (usar `Web.config.example` y `InterfazHubSpot.BatchProcess/App.config.example`)
- Errores cola 2A: marcar `Error`, **no reintentar** automáticamente
- Rate limit HubSpot: delay 120ms; backoff 429 (máx. 3); detener en 401
- **Gate WinForms** antes de `sp_rename` tabla en BD: desplegar `sql/002` y verificar SP activo (PRD §5.1)
- Convención bloqueada: `InterfazHubSpot`, `ProcesosSpertaHubSpot`, `HubSpot` (S mayúscula)

## Estructura clave

```
INTERFAZHUBSPOT/                      # Raíz repo
├── SolucionInterfazHubSpot/           # Código .NET (sln + proyectos)
│   ├── InterfazHubSpot.sln
│   ├── InterfazHubSpot/                # MVC
│   ├── InterfazHubSpot.Business/HubSpot/
│   ├── InterfazHubSpot.BatchProcess/
│   └── InterfazHubSpot/Scripts/agent/ # Build, Test, Verify PS1
├── implementacion/                   # Paquete servicio Windows + Deploy-ServicioHubSpot.ps1
├── docs/                             # PRD, guía BatchProcess
├── scriptsSQL/                       # Deploy canónico MSGestion
└── sql/                              # Copias versionadas scriptsSQL
```

## MCP SQL (desarrollo)

Servidores MCP (leer schema en `mcps/` antes de invocar):

- `project-0-INTERFAZHUBSPOT-mssql-mcp-msgestion-CALZETTA` — MSGestion (cola, SPs integración)
- `project-0-INTERFAZHUBSPOT-mssql-mcp-msfwk-CALZETTA` — MSFwk

Config: `.cursor/mcp.mssql-mcp-server.example.json`, bootstrap en `services/mssql-mcp-server/`.

## Fase actual

Ver `.planning/ROADMAP.md` y `.planning/STATE.md`.
