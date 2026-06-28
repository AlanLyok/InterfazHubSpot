# Configuración

**Tipo:** Reference.  
**Plantillas:** [`../../SolucionInterfazHubSpot/Web.config.example`](../../SolucionInterfazHubSpot/Web.config.example), [`../../SolucionInterfazHubSpot/InterfazHubSpot.BatchProcess/App.config.example`](../../SolucionInterfazHubSpot/InterfazHubSpot.BatchProcess/App.config.example), [`../../implementacion/ServicioInterfazHubSpot_Implementacion/App.config.example`](../../implementacion/ServicioInterfazHubSpot_Implementacion/App.config.example).

> **Nunca versionar** `Web.config`, `App.config`, `MSScheduler452Service.exe.config` ni tokens.

---

## connectionStrings

| Nombre | Uso |
|--------|-----|
| `MSGestion` | Única BD: cola, log, SPs integración, EF6 |

Calzetta: conexión directa; **no** requiere `TenantId` ni `MSFwk`.

---

## appSettings — HubSpot

Leídas por `HubSpotConfiguration` (`InterfazHubSpot.Business/HubSpot/HubSpotCrmClient.cs`).

| Clave | Default | Descripción |
|-------|---------|-------------|
| `HubSpot:PrivateAppToken` | *(vacío)* | PAT HubSpot. Obligatorio si `UseDevelopmentMock=false` |
| `HubSpot:UseDevelopmentMock` | `false` | `true` → stub HTTP sin llamadas reales |
| `HubSpot:BaseUrl` | `https://api.hubapi.com` | Base API |
| `HubSpot:PropertyMastersoftId` | `mastersoft_id_` | Propiedad company informativa (ClienteId ERP) |
| `HubSpot:PropertyCuitCuilUnica` | `cuitcuil_unica` | Correlación ERP ↔ company (NroDocumento normalizado) |
| `HubSpot:PropertyManejoCuentaCorriente` | `manejo_cuenta_corriente` | Propiedad texto flujo 2B |
| `HubSpot:DelayMillisecondsBetweenCalls` | `120` | Pausa entre REST |
| `HubSpot:CuentaCorrientePageSize` | `500` | Tamaño página SP 006 |
| `HubSpot:MaxHttpRetries` | `3` | Reintentos HTTP reintentables |
| `HubSpot:HttpRetryBackoffMilliseconds` | `1000` | Backoff base (429/5xx) |

---

## appSettings — aplicación / batch

| Clave | Uso |
|-------|-----|
| `FrameworkCNPrefix` | Prefijo Mastersoft (`InterfazHubSpot`) |
| `EmpresaId` | Id empresa Calzetta (ej. `1`) |
| `PathLog` | Archivo log texto MVC |
| `EmailDe`, `EmailErrDE`, `EmailErrPara`, `EmailErrCc` | `IntegracionErrorNotifier` → `MSEMails_Agregar` |
| `ErrorLogConnectionName` | Nombre lógico conexión errores |

Servicio Windows: mismas claves en `MSScheduler452Service.exe.config`. Ver [`../BatchProcess_Desarrollo_e_Implementacion.md`](../BatchProcess_Desarrollo_e_Implementacion.md) § config host.

---

## Scopes HubSpot requeridos (Private App)

Mínimo para 2A/2B:

- `crm.objects.companies.read` / `crm.objects.companies.write`
- `crm.objects.contacts.read` / `crm.objects.contacts.write`
- Batch update companies (incluido en write companies)

Errores 403 `MISSING_SCOPES`: ampliar scopes en el portal HubSpot.

---

## Variables entorno build (opcionales)

| Variable | Uso |
|----------|-----|
| `SPERTA_MSBUILD` / `MSBUILD_EXE` | Ruta MSBuild |
| `SPERTA_NUGET_EXE` | Ruta nuget.exe |

Ver [`../../scriptsPS1/README.md`](../../scriptsPS1/README.md).
