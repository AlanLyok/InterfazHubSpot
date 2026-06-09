# Servicio Windows — InterfazHubSpot Batch

Carpeta de **implementación** del host Mastersoft Scheduler452 para los jobs HubSpot (flujos 2A y 2B).

## Contenido

| Archivo / carpeta | Rol |
|-------------------|-----|
| `MSScheduler452Service.exe` | Ejecutable del servicio Windows (host Mastersoft Scheduler452) |
| `MSScheduler452Service.exe.config` | Config runtime local (**no versionar** — ver `.gitignore`) |
| `App.config.example` | Plantilla sin secretos para crear `MSScheduler452Service.exe.config` |
| `Config.xml` | Programación de procesos (`IScheduler`) |
| `Templates/error_template.html` | Plantilla HTML para emails de error (`MSEMails_Agregar`) |
| `Mastersoft.Scheduler452.*.dll` | Motor del scheduler |
| `InterfazHubSpot.*.dll` | Jobs y lógica HubSpot (copiar tras cada build Release) |

## Documentación completa

Ver **[docs/BatchProcess_Desarrollo_e_Implementacion.md](../docs/BatchProcess_Desarrollo_e_Implementacion.md)** — arquitectura, desarrollo de jobs, despliegue, verificación y troubleshooting.

## Instalar el servicio Windows (Calzetta)

`sc create` **no inicia** el servicio; después hay que ejecutar `sc start`.

```powershell
sc.exe create MastersoftInterfazHubSpot binPath= "C:\MsDna\InterfazHubSpot\ServicioFinalImple\MSScheduler452Service.exe" start= auto DisplayName= "Mastersoft Interfaz HubSpot"
sc.exe start MastersoftInterfazHubSpot
sc.exe query MastersoftInterfazHubSpot
```

```powershell
sc.exe stop MastersoftInterfazHubSpot
sc.exe start MastersoftInterfazHubSpot
```

`binPath` debe apuntar a **`MSScheduler452Service.exe`**. Todos los DLL, `Config.xml` y `Templates/` deben estar en la misma carpeta.

## Actualizar binarios tras un build

Script canónico (repo + servidor `ServicioFinalImple` si existe):

```powershell
# Desde la raíz del repo
powershell -NoProfile -File implementacion\Deploy-ServicioHubSpot.ps1

# Con reinicio del servicio (consola elevada como Administrador)
powershell -NoProfile -File implementacion\Deploy-ServicioHubSpot.ps1 -RestartService
```

Origen del build: `SolucionInterfazHubSpot/InterfazHubSpot.BatchProcess/bin/Release/net452/`.

Luego reiniciar manualmente si no usaste `-RestartService`:

```powershell
sc.exe stop MastersoftInterfazHubSpot
sc.exe start MastersoftInterfazHubSpot
```
