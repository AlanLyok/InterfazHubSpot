# Project Research Summary

**Project:** Integración HubSpot — Flujos 2A y 2B  
**Domain:** ERP-to-CRM batch integration (.NET Framework brownfield)  
**Researched:** 2026-06-06  
**Confidence:** HIGH

## Executive Summary

El proyecto es una refactorización brownfield de BatchSpertaAPI: conservar la arquitectura de jobs, consola MVC y conector HubSpot existente, pero **reemplazar SpertaAPI por stored procedures SQL**. Los dos flujos (cola de clientes/contactos y cuenta corriente diaria) están bien definidos en el PRD con endpoints HubSpot CRM v3, manejo de errores y rate limiting.

El mayor riesgo no es técnico sino de **datos**: tres puntos abiertos con Calzetta (tabla HubSpotCompanyId, campos origen cliente, estructura cuenta corriente) deben resolverse en la Fase 1 antes de cerrar los SPs.

## Key Findings

### Recommended Stack

.NET Framework 4.5.2 + ASP.NET MVC + IScheduler + SQL Server SPs + HubSpot CRM v3 PAT. No migrar a .NET 8 en este milestone.

### Must Have (table stakes)

- Cola + job 2A + job 2B + 6 SPs + HubSpotClient + EmailsManager + consola MVC

### Architecture

Capas existentes (`InterfazHubSpot`, `Business`, `BatchProcess`) con nueva `ISqlDataAccess` sustituyendo llamadas HTTP a SpertaAPI.

### Critical Pitfalls

1. Puntos abiertos BD Calzetta  
2. Rate limit 429  
3. Token no versionado  
4. Clientes sin HubSpotCompanyId en 2B  
5. Limpieza incompleta SpertaAPI

## Roadmap Implications

- **Fase 1** debe incluir validación con Calzetta de esquema BD  
- **Fases 2-4** pueden paralelizarse parcialmente (client vs runners) tras Fase 1  
- **Fase 5** verificación y tests obligatoria antes de deploy
