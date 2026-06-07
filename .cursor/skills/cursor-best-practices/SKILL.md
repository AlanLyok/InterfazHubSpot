---
name: cursor-best-practices
description: >-
  Best practices for Cursor agents working on InterfazHubSpot — rules, skills, MCP SQL, PowerShell
  on Windows, and .NET Framework 4.5.2 workflows. Use when configuring agents, extending rules/skills,
  or optimizing the development workflow in this repository.
---

# Cursor Best Practices — InterfazHubSpot

## Core Principles

- **Start with plans** — Plan Mode for multi-file or architectural work
- **Rules = static** — always-on context in `.cursor/rules/` (`alwaysApply: true` for stack/shell)
- **Skills = dynamic** — load on relevance; hub: `interfaz-hubspot-dev`
- **Verify before done** — `Verify-InterfazHubSpot.ps1`, not assumed success

## Extension model (this repo)

| Tipo | Ubicación | Ejemplos |
|------|-----------|----------|
| Rules | `.cursor/rules/*.mdc` | `interfaz-hubspot`, `powershell-windows`, `codacy` |
| Skills | `.cursor/skills/*/SKILL.md` | `interfaz-hubspot-dev`, `dotnet-best-practices` |
| Agent index | `AGENTS.md`, `CLAUDE.md` | Stack, MCP, comandos |

**Nuevo skill:** seguir `.cursor/skills/create-skill` pattern — `name`, `description` en tercera persona, omitir `disable-model-invocation` si debe auto-dispararse.

## MCP SQL (MSGestion)

1. Leer descriptor en `mcps/project-0-INTERFAZHUBSPOT-mssql-mcp-msgestion-CALZETTA/tools/` antes de invocar.
2. Servidor principal: **`project-0-INTERFAZHUBSPOT-mssql-mcp-msgestion-CALZETTA`** (cola, SPs, ERP).
3. Wiring: `.cursor/mcp.mssql-mcp-server.example.json`, `services/mssql-mcp-server/bootstrap-venv.ps1`.
4. Coordinar con `dotnet-best-practices` (SPs, tabla `ProcesosSpertaHubSpot`).

## PowerShell on Windows

Agents run in **Windows PowerShell 5.1** by default. Full guide: [`.cursor/rules/powershell-windows.mdc`](../../rules/powershell-windows.mdc).

- No `&&` / `||` unless explicitly using `pwsh` 7+
- Use canonical build scripts, not raw `msbuild` chains
- Git commit bodies: PowerShell here-strings, not bash heredoc

## Key Workflows

- **Build/test** — `InterfazHubSpot/Scripts/agent/*.ps1` via `pwsh -NoProfile -File`
- **HubSpot debug** — MVC traza endpoints + `HubSpot:UseDevelopmentMock`
- **SQL exploration** — MCP msgestion + migrations in `sql/`
- **GSD phases** — `.cursor/skills/get-shit-done`, `.planning/`

## References

- `references/planning.md` — Plan mode
- `references/context.md` — Context management
- `references/extending.md` — Rules vs Skills
- `references/workflows.md` — TDD, git
- `references/reviewing.md` — Code review
- `references/parallel-agents.md` — Parallel agents
