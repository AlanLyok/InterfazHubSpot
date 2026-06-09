# SolucionInterfazHubSpot — código fuente .NET

Solución Visual Studio **InterfazHubSpot.sln** (.NET Framework 4.5.2): consola MVC, librerías de negocio, jobs batch (`IScheduler`) y tests.

El resto del repositorio (SQL, documentación, despliegue del servicio Windows) vive en la **raíz** del repo.

## Proyectos

| Proyecto | Rol |
|----------|-----|
| `InterfazHubSpot/` | ASP.NET MVC — consola interna, trazas, botones manuales |
| `InterfazHubSpot.BatchProcess/` | Jobs `ProcesarColaIntegracionesHubSpotJob` (2A), `HubSpotSincronizarCuentaCorrienteJob` (2B) |
| `InterfazHubSpot.Business/` | Runners HubSpot, managers, cola, `EmailsManager` |
| `InterfazHubSpot.Entities/` | EF6 MSGestion |
| `InterfazHubSpot.Interfaces/` | Contratos |
| `InterfazHubSpot.Mapping/` | AutoMapper |
| `InterfazHubSpot.Tests.Unit/` | xUnit |
| `InterfazHubSpot.IntegrationTests/` | Humo / Live |
| `Componentes/` | DLLs Mastersoft (`Mastersoft.Scheduler452.Intefaces.dll`, Framework, etc.) |

## Build y tests

Desde la **raíz del repo** (o desde esta carpeta con rutas relativas):

```powershell
# PowerShell 7+ (recomendado)
pwsh -NoProfile -File SolucionInterfazHubSpot/InterfazHubSpot/Scripts/agent/Build-InterfazHubSpot.ps1
pwsh -NoProfile -File SolucionInterfazHubSpot/InterfazHubSpot/Scripts/agent/Test-InterfazHubSpot.ps1

# Solo librerías + BatchProcess (iteración jobs)
pwsh -NoProfile -File SolucionInterfazHubSpot/InterfazHubSpot/Scripts/agent/Build-InterfazHubSpot.ps1 -LibrariesOnly
```

Sin `pwsh`, MSBuild directo:

```powershell
$msb = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
& $msb "SolucionInterfazHubSpot\InterfazHubSpot.BatchProcess\InterfazHubSpot.BatchProcess.csproj" /p:Configuration=Release
```

Output batch: `InterfazHubSpot.BatchProcess/bin/Release/net452/`.

## Configuración local

| Archivo | Uso |
|---------|-----|
| `Web.config.example` | Plantilla → copiar a `InterfazHubSpot/Web.config` (no versionado) |
| `InterfazHubSpot.BatchProcess/App.config.example` | Referencia para `MSScheduler452Service.exe.config` |

## Despliegue servicio Windows

Los binarios compilados se copian a [`implementacion/`](../implementacion/) (paquete en repo) y al servidor (`ServicioFinalImple` en Calzetta).

```powershell
pwsh -NoProfile -File ..\implementacion\Deploy-ServicioHubSpot.ps1
```

Guía completa: [docs/BatchProcess_Desarrollo_e_Implementacion.md](../docs/BatchProcess_Desarrollo_e_Implementacion.md).

## Publicar MVC

Perfil: `InterfazHubSpot/Properties/PublishProfiles/FolderProfile.pubxml` → salida en `../publish/` (raíz del repo).
