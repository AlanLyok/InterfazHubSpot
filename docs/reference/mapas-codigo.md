# Mapa de código fuente

**Tipo:** Reference.  
**Proyectos y build:** [`../../SolucionInterfazHubSpot/README.md`](../../SolucionInterfazHubSpot/README.md).

---

## Solución

`SolucionInterfazHubSpot/InterfazHubSpot.sln` — .NET Framework **4.5.2**.

---

## HubSpot — núcleo integración

| Archivo | Rol |
|---------|-----|
| `Business/HubSpot/HubSpotIntegracionRunner.cs` | Orquestación 2A y 2B |
| `Business/HubSpot/HubSpotCrmClient.cs` | HTTP CRM v3 + `HubSpotConfiguration` |
| `Business/HubSpot/HubSpotHttpExceptions.cs` | Excepciones HTTP tipadas |
| `Business/HubSpot/HubSpotColaIntentosReporter.cs` | Intentos cola en reintentos |
| `Business/HubSpot/DevelopmentHubSpotStubHandler.cs` | Mock dev |

---

## Jobs batch (`IScheduler`)

| Clase | Archivo |
|-------|---------|
| `ProcesarColaIntegracionesHubSpotJob` | `BatchProcess/ProcesarColaIntegracionesHubSpotJob.cs` |
| `HubSpotSincronizarCuentaCorrienteJob` | `BatchProcess/HubSpotSincronizarCuentaCorrienteJob.cs` |
| `GrabarEmailError` | Plantilla diagnóstico email |

Host producción: `implementacion/MSScheduler452Service.exe` + `Config.xml`.

---

## Datos ERP

| Componente | Ubicación |
|------------|-----------|
| `ClienteIntegracionManager` | `Business/Managers/` |
| Mapper SP→DTO | `Business/Managers/ClienteIntegracionMapper.cs` |
| Cola claim/update | `Business/` (integración cola) |
| EF6 entities | `InterfazHubSpot.Entities/` |

SPs: [`base-datos.md`](base-datos.md).

---

## Notificaciones

| Componente | Rol |
|------------|-----|
| `EmailsManager` | Encolar emails vía framework |
| `IntegracionErrorNotifier` | Errores 2A/2B → template HTML |

Template: `implementacion/Templates/error_template.html`.

---

## Tests

| Proyecto | Enfoque |
|----------|---------|
| `InterfazHubSpot.Tests.Unit` | HTTP mock, config, cola, security |
| `InterfazHubSpot.IntegrationTests` | Contratos mapper, Live skipped |

Detalle: [`../TESTING.md`](../TESTING.md).

---

## Legacy (no productivo)

`HttpSpertaApiClient`, `ISpertaApiClient`, jobs `*Sperta*` — no extender. Ver [`../explanation/arquitectura.md`](../explanation/arquitectura.md).

---

## DLLs Mastersoft

`SolucionInterfazHubSpot/Componentes/` — binarios requeridos para compilar (Scheduler452, Framework).
