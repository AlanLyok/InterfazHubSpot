# Project State

**Project:** Integración HubSpot — Flujos 2A y 2B  
**Initialized:** 2026-06-06  
**Current Phase:** 1 (not started)  
**Status:** Planning complete — ready for `/gsd-discuss-phase 1`

## Project Reference

See: `.planning/PROJECT.md` (updated 2026-06-06)

**Core value:** Clientes ERP en HubSpot (2A) + cuenta corriente diaria (2B)  
**Current focus:** Phase 1 — SQL y capa de datos

## Progress

| Phase | Name | Status |
|-------|------|--------|
| 1 | SQL y capa de datos | Pending |
| 2 | HubSpot Client | Pending |
| 3 | Flujo 2A | Pending |
| 4 | Flujo 2B | Pending |
| 5 | Integración y entrega | Pending |

## Blockers

| # | Blocker | Owner | Phase |
|---|---------|-------|-------|
| 1 | Tabla `HubSpotCompanyId` en Mastersoft | Calzetta | 1 |
| 2 | Campos origen `USER_HS_Cliente_ObtenerDatos` | Calzetta | 1 |
| 3 | Estructura CC/facturas para SP masivo | Calzetta | 1 |
| 4 | Ambiente token PAT | Alan/Dayana | 2 |

## Artifacts

| Artifact | Path |
|----------|------|
| PRD | `docs/PRD_Integracion_HubSpot_2A_2B.md` |
| Project | `.planning/PROJECT.md` |
| Requirements | `.planning/REQUIREMENTS.md` |
| Roadmap | `.planning/ROADMAP.md` |
| Research | `.planning/research/SUMMARY.md` |

## Session Notes

- Repo brownfield: InterfazHubSpot ya tiene jobs HubSpot vía SpertaAPI; objetivo es migrar a SQL directo.
- Git inicializado 2026-06-06.
- Skills del proyecto: `.cursor/skills/get-shit-done`, `dotnet-best-practices` (aplica a otro stack — este proyecto es .NET 4.5.2).

---
*Last updated: 2026-06-06*
