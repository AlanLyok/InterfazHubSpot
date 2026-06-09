# SolucionInterfazHubSpot — código fuente .NET

Solución Visual Studio **InterfazHubSpot.sln** (.NET Framework 4.5.2): consola MVC, librerías de negocio, jobs batch (`IScheduler`) y tests.

El resto del repositorio (SQL, documentación, scripts, despliegue del servicio Windows) vive en la **raíz** del repo.

**Documentación:** [`../docs/README.md`](../docs/README.md)

## Proyectos

| Proyecto | Rol |
|----------|-----|
| `InterfazHubSpot/` | ASP.NET MVC — consola interna, trazas, botones manuales |
| `InterfazHubSpot.BatchProcess/` | Jobs `ProcesarColaIntegracionesHubSpotJob` (2A), `HubSpotSincronizarCuentaCorrienteJob` (2B) |
| `InterfazHubSpot.Business/` | Runners HubSpot, managers, cola, `EmailsManager` |
| `InterfazHubSpot.Entities/` | EF6 MSGestion |
| `InterfazHubSpot.Interfaces/` | Contratos |
| `InterfazHubSpot.Mapping/` | AutoMapper |
| `InterfazHubSpot.Tests.Unit/` | xUnit — unitarios y seguridad |
| `InterfazHubSpot.IntegrationTests/` | xUnit — integración y Live |
| `Componentes/` | DLLs Mastersoft (`Mastersoft.Scheduler452.Intefaces.dll`, Framework, etc.) |

## Build y tests

Desde la **raíz del repo**:

```powershell
powershell.exe -NoProfile -File scriptsPS1/Build-InterfazHubSpot.ps1
powershell.exe -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1
powershell.exe -NoProfile -File scriptsPS1/Verify-InterfazHubSpot.ps1
```

Detalle de categorías, cobertura y parámetros: [`docs/TESTING.md`](../docs/TESTING.md) y [`scriptsPS1/README.md`](../scriptsPS1/README.md).

## Configuración local

| Archivo | Uso |
|---------|-----|
| `Web.config.example` | Plantilla → copiar a `InterfazHubSpot/Web.config` (no versionado) |
| `InterfazHubSpot.BatchProcess/App.config.example` | Referencia para `MSScheduler452Service.exe.config` |

## Despliegue servicio Windows

```powershell
powershell.exe -NoProfile -File ../implementacion/Deploy-ServicioHubSpot.ps1
```

Guía completa: [docs/BatchProcess_Desarrollo_e_Implementacion.md](../docs/BatchProcess_Desarrollo_e_Implementacion.md).

## Publicar MVC

Perfil: `InterfazHubSpot/Properties/PublishProfiles/FolderProfile.pubxml` → salida en `../publish/` (raíz del repo).
