---
name: dotnet-best-practices
description: >-
  Asegura que el C# y la solución sigan el stack real InterfazHubSpot (.NET Framework 4.5.2,
  ASP.NET MVC, EF6, Web.config/App.config, MSBuild, Newtonsoft.Json, Mastersoft DLLs).
  Usar al tocar *.cs, *.csproj, *.sln, controllers MVC, managers, runners HubSpot, jobs batch,
  o al compilar/verificar este repositorio. No recomendar .NET 8, ASP.NET Core, appsettings.json
  ni dotnet CLI como build principal.
---

# Buenas prácticas .NET — InterfazHubSpot (.NET Framework 4.5.2)

## Ámbito

- **Solución:** `InterfazHubSpot.sln` — **.NET Framework 4.5.2** (`TargetFrameworkVersion` v4.5.2).
- **Web:** ASP.NET **MVC** clásico (`InterfazHubSpot/`) — `Web.config`, no `appsettings.json`.
- **Batch:** `InterfazHubSpot.BatchProcess` — jobs `IScheduler` (Mastersoft.Scheduler452).
- **Negocio:** `InterfazHubSpot.Business` — managers `*Manager`, runners en `Business/HubSpot/`.
- **Datos:** EF6 (`EntityFramework 6.1.3`) + SPs vía `ClienteIntegracionManager` contra MSGestion.
- **Framework:** DLLs en `Componentes/` (`Mastersoft.Framework.*`) — no reimplementar reglas ERP.
- **Fuente de verdad:** `AGENTS.md`, `CLAUDE.md`, `docs/PRD_Integracion_HubSpot_2A_2B.md`.

**Prohibido** importar patrones de .NET 8 / ASP.NET Core / EF Core salvo migración futura explícita.

## Estructura y namespaces

```
InterfazHubSpot              # MVC (consola interna)
InterfazHubSpot.BatchProcess # Jobs scheduler
InterfazHubSpot.Business     # Managers + HubSpot/
InterfazHubSpot.Mapping      # AutoMapper
InterfazHubSpot.Entities     # EF6 MSGestion
InterfazHubSpot.Interfaces
```

- Runners HubSpot: namespace `InterfazHubSpot.Business.HubSpot`.
- Dependencias **unidireccionales** top→down (MVC → Business → Entities/Interfaces).
- Managers con sufijo `*Manager`; cola: `ProcesosSpertaHubSpotManager`.

## Configuración

- `Web.config` / `App.config` + `ConfigurationManager.AppSettings`.
- Connection string única: **`MSGestion`** (cola, SPs integración, EF6).
- HubSpot: `HubSpot:PrivateAppToken`, `HubSpot:BaseUrl`, `HubSpot:UseDevelopmentMock`, etc.
- **Nunca** versionar tokens; usar `Web.config.example`.
- No sustituir por `IConfiguration` / `appsettings.json`.

## JSON y HTTP

- **Newtonsoft.Json** (`JObject`, `JToken`) en clientes HubSpot y DTOs.
- `System.Net.Http.HttpClient` con `async`/`await` (disponible en 4.5.2).
- HubSpot CRM **v3** — Private App Token en header `Authorization: Bearer`.

## Datos y SQL

- Cola outbox: `dbo.ProcesosSpertaHubSpot` (nunca nombres legacy de cola; ver regla Cursor).
- SPs integración: `dbo.USP_Integracion_HubSpot_Cliente_Obtener`, `dbo.USP_Integracion_HubSpot_CuentaCorriente_Pagina`.
- SPs legacy HubSpot datos: prefijo `USER_HS_*`.
- Migraciones SQL en `sql/` y `scriptsSQL/`.
- Explorar esquema/datos dev: MCP `project-0-INTERFAZHUBSPOT-mssql-mcp-msgestion-CALZETTA`.

## Flujos de negocio (2A / 2B)

- **2A:** cola → claim Pending → `EnProceso` → SP cliente → upsert HubSpot → `Procesado`/`Error`. **Sin reintento automático** en Error.
- **2B:** paginación cuenta corriente → batch update companies en HubSpot.
- Rate limit: delay 120 ms; backoff 429 (máx. 3); **detener en 401**.

## Nomenclatura bloqueada

- Marca: `HubSpot` (S mayúscula). Prohibido casing incorrecto de la marca.
- Prohibido reintroducir nombres legacy de solución/tabla (ver `.cursor/rules/interfaz-hubspot.mdc`).
- Columna identificador cola: `Identificador`.

## Estilo C# (4.5.2)

- Sin nullable reference types ni `record`/`init` (C# moderno post-7).
- `async`/`await` en runners y clientes HTTP; managers legacy pueden ser síncronos — seguir patrón existente.
- Priorizar **consistencia con código existente** sobre modernización gratuita.
- AutoMapper en `InterfazHubSpot.Mapping` para perfiles existentes.

## Compilación y verificación

Desde raíz del repo (PowerShell — ver regla `powershell-windows`):

```powershell
pwsh -NoProfile -File InterfazHubSpot/Scripts/agent/Build-InterfazHubSpot.ps1
pwsh -NoProfile -File InterfazHubSpot/Scripts/agent/Test-InterfazHubSpot.ps1
pwsh -NoProfile -File InterfazHubSpot/Scripts/agent/Verify-InterfazHubSpot.ps1
```

- Build: **MSBuild** + `nuget restore` (no `dotnet build` como camino principal).
- Tests: xUnit en `InterfazHubSpot.Tests.Unit` (excluir `Category=Live` por defecto).
- Proyecto MVC requiere `Microsoft.WebApplication.targets` — build completo IDE/VS si falla MSBuild web.

Variables: `SPERTA_MSBUILD`, `MSBUILD_EXE`, `SPERTA_NUGET_EXE`.

## Qué NO hacer

- ASP.NET Core, minimal APIs, `ControllerBase` con `[ApiController]`.
- `dotnet run`, `WebApplicationFactory`, EF Core, `Microsoft.Data.SqlClient` en código nuevo 4.5.2.
- JWT Bearer / `appsettings.json` / Swashbuckle como stack de este repo.
- Extender `ISpertaApiClient` / jobs SpertaAPI — son legacy; datos reales vienen de SPs MSGestion.
- `sp_rename` cola sin gate WinForms (PRD §5.1, `sql/002`).
