# Stack Research — Integración HubSpot InterfazHubSpot

**Researched:** 2026-06-06  
**Confidence:** HIGH (definido en PRD y código existente)

## Recommended Stack

| Componente | Tecnología | Notas |
|-----------|------------|-------|
| Runtime | .NET Framework 4.5.2 | Mantener compatibilidad con InterfazHubSpot existente |
| Web | ASP.NET MVC | Login + Home con botones manuales |
| Scheduler | IScheduler (Quartz.NET) | Jobs 2A (5 min) y 2B (3:00 AM) |
| Datos | SQL Server + ADO.NET/EF6 | **Solo SPs** para datos HubSpot |
| API externa | HubSpot CRM v3 REST | Private App Token Bearer |
| HTTP client | HttpClient / WebRequest existente | Reutilizar patrón `InterfazHubSpot` |
| Tests | xUnit (`Tests.Unit`, `IntegrationTests`) | HTTP mockeado para HubSpot |
| Build | MSBuild + nuget restore | Scripts `scripts/Build-InterfazHubSpot.ps1` |

## Qué NO usar

- **SpertaAPI / HttpSpertaApiClient** — fuera de alcance PRD
- **.NET 8 / ASP.NET Core** — skill `dotnet-best-practices` del repo apunta a otro producto; este proyecto es Framework 4.5.2
- **OAuth HubSpot** — PAT suficiente
- **DLLs Mastersoft adicionales** — solo `Componentes/` mínimas para compilar

## Configuración crítica (`Web.config`)

```xml
HubSpot:PrivateAppToken          — NO versionar
HubSpot:BaseUrl                  — https://api.hubapi.com
HubSpot:PropertyMastersoftId     — mastersoft_id_
HubSpot:PropertyManejoCuentaCorriente — manejo_cuenta_corriente
HubSpot:DelayMillisecondsBetweenCalls — 120
HubSpot:ColaBatchSize            — 50
HubSpot:UseDevelopmentMock       — true (dev) / false (prod)
```

## Connection strings

- `MSGestion` — cola, clientes, cuenta corriente, HubSpotCompanyId
- `MSFwk` — framework/usuarios si aplica login MVC
