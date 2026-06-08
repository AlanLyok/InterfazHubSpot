# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

**InterfazHubSpot** — .NET Framework 4.5.2 async batch + ASP.NET MVC console that syncs ERP Mastersoft → HubSpot CRM **directly against the database** (no SpertaAPI in the runtime path). Pipeline:

1. Read pending rows from process table `dbo.ProcesosSpertaHubSpot`.
2. Resolve client/company/contact data via **stored procedures** against MSGestion.
3. Query HubSpot CRM v3 (Private App Token) to find existing company by `mastersoft_id_`.
4. Create or update company/contact in HubSpot; mark queue row `Procesado` or `Error`.

Two flows:
- **2A** — Outbox queue rows (Destino=HubSpot) → per-row company/contact upsert (`HubSpotIntegracionRunner`).
- **2B** — Paginated cuenta-corriente snapshot → batch update 100 companies/page in HubSpot.

> **The `ISpertaApiClient` / `HttpSpertaApiClient` / `…Sperta…Job` classes in this repo are legacy scaffolding, not the production path.** Don't extend them; the real data source is SP calls on MSGestion.

PRD: `docs/PRD_Integracion_HubSpot_2A_2B.md`. GSD planning state: `.planning/ROADMAP.md`, `.planning/STATE.md`.

## Commands

Run from repo root. PowerShell wrappers handle nuget restore + MSBuild discovery (env vars `SPERTA_MSBUILD`, `MSBUILD_EXE`, `SPERTA_NUGET_EXE`).

**Windows shell:** default is Windows PowerShell 5.1 — do **not** use `&&`/`||` (PS 7+ only). See `.cursor/rules/powershell-windows.mdc`. Primary project skill: `.cursor/skills/interfaz-hubspot-dev/SKILL.md`.

| Action | Command |
|---|---|
| Full build | `pwsh -NoProfile -File InterfazHubSpot/Scripts/agent/Build-InterfazHubSpot.ps1` |
| Libraries only (skip MVC site) | `pwsh -NoProfile -File InterfazHubSpot/Scripts/agent/Build-InterfazHubSpot.ps1 -LibrariesOnly` |
| Tests (xUnit, excludes `Category=Live`) | `pwsh -NoProfile -File InterfazHubSpot/Scripts/agent/Test-InterfazHubSpot.ps1` |
| Build + tests + legacy-grep check | `pwsh -NoProfile -File InterfazHubSpot/Scripts/agent/Verify-InterfazHubSpot.ps1` |

Single test: pass `dotnet test` filter via the script (e.g. `... -Filter "FullyQualifiedName~HttpSpertaApiClient"`), or run `dotnet test InterfazHubSpot.Tests.Unit\InterfazHubSpot.Tests.Unit.csproj --filter "<expr>"` directly.

The MVC web project imports `Microsoft.WebApplication.targets` — open `InterfazHubSpot.sln` in Visual Studio for full IDE builds.

## Architecture

Solution layers (classic Mastersoft layout — keep dependencies one-way top→down):

```
InterfazHubSpot                  # ASP.NET MVC (internal console — no login — with manual job buttons)
  ├─ InterfazHubSpot.BatchProcess   # IScheduler host (jobs)
  ├─ InterfazHubSpot.Business       # Managers, ClienteIntegracionManager, HubSpot/ runners, integration queue
  ├─ InterfazHubSpot.Mapping        # AutoMapper profiles
  ├─ InterfazHubSpot.Entities       # EF6 entities (MSGestion)
  └─ InterfazHubSpot.Interfaces
Componentes/                       # Mastersoft framework DLLs (binary, required to compile)
```

**One connection string** in `Web.config`/`App.config`:
- `MSGestion` — ERP DB; EF6 context **and** the host for all data SPs the integration calls (queue `dbo.ProcesosSpertaHubSpot`, log `dbo.ProcesosSpertaHubSpotLog`, `InterfazHubSpot_Cliente_Obtener` 004, `InterfazHubSpot_Clientes_Contactos_Obtener` 005, `InterfazHubSpot_CuentaCorriente_Pagina` 006, función `InterfazHubSpot_ManejoCuentaCorriente_Texto` 007, `USER_POS_Clientes_Agregar`, etc.). SQL deploy canónico en `scriptsSQL/` (`000_Deploy_All.sql` + 001–007); copias versionadas en `sql/`.

`MSFwk` is no longer required — the MVC site is an internal tool with no user authentication.

**HubSpot runners** (`InterfazHubSpot.Business/HubSpot/`) talk to HubSpot CRM v3 with a Private App Token. Config: `HubSpot:PrivateAppToken` (required unless `HubSpot:UseDevelopmentMock=true`), `HubSpot:BaseUrl`, `HubSpot:PropertyMastersoftId` (default `mastersoft_id_`), `HubSpot:PropertyManejoCuentaCorriente` (default `manejo_cuenta_corriente`), `HubSpot:DelayMillisecondsBetweenCalls` (default 120), `HubSpot:CuentaCorrientePageSize` (default 500). Rate-limit rules baked into runners: 120ms delay between calls, 429 backoff (max 3 retries), **stop on 401**.

**Jobs** (`IScheduler` in `InterfazHubSpot.BatchProcess`): `ProcesarColaIntegracionesHubSpotJob` (2A) and `HubSpotSincronizarCuentaCorrienteJob` (2B) are the live jobs. `GrabarEmailError` is a diagnostic template. Manually triggerable from MVC Home — `POST /Home/ProcesarColaHubSpot`, `POST /Home/HubSpotCuentaCorrienteBatch`, plus incremental traza endpoints (`…TrazaCola`, `…TrazaCliente?clienteId=n`, `TrazaHubSpotUpsertEmpresa`, `TrazaHubSpotSincronizarContactos`, `…Traza`) that return step-by-step JSON for debugging.

**Queue contract** (outbox written by ERP WinForms): see PRD § outbox (`docs/PRD_Integracion_HubSpot_2A_2B.md`). Identifier column is `Identificador`. 2A flow: claim Pending → mark `EnProceso` → SP 004 + HubSpot company upsert → SP 005 + contact upsert → mark `Ok` or `Error`. **Errors are NOT auto-retried.**

## Critical rules

- **.NET Framework 4.5.2 only.** Follow `.cursor/skills/dotnet-best-practices` (InterfazHubSpot stack) — no .NET 8 / ASP.NET Core patterns.
- Never commit secrets. `Web.config` and `App.config` are gitignored; use `Web.config.example`. `HubSpot:PrivateAppToken` must never be versioned.
- Locked naming: `InterfazHubSpot`, `ProcesosSpertaHubSpot`, `HubSpot` (capital S). Wave 0–2 refactors unified these — see recent commits before introducing variants.
- WinForms gate before any `sp_rename` on the queue table: deploy `sql/002` and verify SP `USER_POS_Clientes_Agregar` is active in production (PRD §5.1).

## Tests

- `InterfazHubSpot.Tests.Unit` — xUnit. Covers HubSpot internals with mocked HTTP, diagnostics, queue constants.
- `InterfazHubSpot.IntegrationTests` — xUnit smoke/compile against `Business`. Tests tagged `Category=Live` (require live DB/API) are excluded by default.
