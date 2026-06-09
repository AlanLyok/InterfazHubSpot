# Desplegar servicio Windows

**Tipo:** How-to.  
**Guía completa (3 capas, Config.xml, TLS, troubleshooting):** [`../BatchProcess_Desarrollo_e_Implementacion.md`](../BatchProcess_Desarrollo_e_Implementacion.md).  
**Comandos `sc.exe`:** [`../../implementacion/README.md`](../../implementacion/README.md).

---

## Resumen

El batch en producción **no** es un exe del repo: es `MSScheduler452Service.exe` (Mastersoft) que carga `InterfazHubSpot.BatchProcess.dll` según `Config.xml`.

---

## Pasos

### 1. Build Release

```powershell
powershell.exe -NoProfile -File scriptsPS1/Build-InterfazHubSpot.ps1
```

Salida DLL: `SolucionInterfazHubSpot/InterfazHubSpot.BatchProcess/bin/Release/net452/`.

### 2. Copiar binarios

```powershell
powershell.exe -NoProfile -File implementacion/Deploy-ServicioHubSpot.ps1
# Elevado + reinicio:
powershell.exe -NoProfile -File implementacion/Deploy-ServicioHubSpot.ps1 -RestartService
```

Destino típico Calzetta: `implementacion/ServicioInterfazHubSpot_Implementacion/` (o ruta servidor documentada en BatchProcess doc).

### 3. Configurar `MSScheduler452Service.exe.config`

Desde [`App.config.example`](../../implementacion/ServicioInterfazHubSpot_Implementacion/App.config.example):

- `MSGestion` connection string
- `HubSpot:PrivateAppToken` (producción: **mock=false**)
- Emails error (`EmailErrPara`, etc.)

**No versionar** el `.config` real.

### 4. Verificar `Config.xml`

Jobs 2A/2B con `assembly` + `clase` correctos. Cron según operación Calzetta.

### 5. Instalar / reiniciar servicio

```powershell
sc.exe query MastersoftInterfazHubSpot
sc.exe stop MastersoftInterfazHubSpot
sc.exe start MastersoftInterfazHubSpot
```

Nombre servicio y `binPath`: [`../../implementacion/README.md`](../../implementacion/README.md).

---

## Checklist post-deploy

- [ ] TLS 1.2 (Global.asax en MVC; static ctor en `HubSpotCrmClient` para servicio)
- [ ] Token HubSpot válido y scopes
- [ ] `Templates/error_template.html` presente
- [ ] Log servicio / `ProcesosSpertaHubSpotLog` tras primera corrida 2B
