# Depurar integración HubSpot

**Tipo:** How-to.  
**Endpoints:** [`../reference/consola-mvc.md`](../reference/consola-mvc.md).  
**Flujos:** [`../explanation/flujos-2a-2b.md`](../explanation/flujos-2a-2b.md).

---

## Prerrequisitos

1. MVC corriendo con `Web.config` válido ([`configurar-desarrollo-local.md`](configurar-desarrollo-local.md)).
2. Scripts SQL desplegados en MSGestion ([`../reference/base-datos.md`](../reference/base-datos.md)).
3. Para HubSpot real: `HubSpot:UseDevelopmentMock=false` + PAT con scopes.

---

## Secuencia recomendada (2A)

Ejecutar en orden desde Home o POST manual:

1. **`ProcesarColaHubSpotTrazaCola`** — ¿hay filas `Pendiente`?
2. **`ProcesarColaHubSpotTrazaCliente?clienteId=N`** — ¿SP 004 devuelve empresa/direcciones?
3. **`TrazaHubSpotBuscarEmpresa?clienteId=N`** — ¿existe company en HubSpot?
4. **`TrazaHubSpotUpsertEmpresa?clienteId=N`** — ¿create/patch OK? (revisar enums propiedades)
5. **`TrazaHubSpotSincronizarContactos?clienteId=N&hubCompanyId=...`** — contactos + asociación
6. **`ProcesarColaHubSpotTraza`** — corrida completa con steps JSON

Si un paso falla, **no avanzar** hasta corregir (datos SP, propiedades HubSpot, scopes).

---

## Errores frecuentes

| Síntoma | Causa probable | Acción |
|---------|----------------|--------|
| HTTP 400 `PROPERTY_DOESNT_EXIST` | Nombre propiedad distinto al portal | Alinear mapper / portal HubSpot |
| HTTP 400 opciones enum | Valor ERP no está en lista HubSpot | Corregir dato o ampliar enum en HubSpot |
| HTTP 403 `MISSING_SCOPES` | PAT sin write contacts | Ampliar Private App |
| HTTP 401 | Token inválido/expirado | Renovar PAT; revisar config servicio |
| Cola en `Error` | Fallo en paso anterior | Ver log MVC (`PathLog`); no auto-retry |

Política HTTP: [`../reference/hubspot-crm.md`](../reference/hubspot-crm.md).

---

## Flujo 2B

1. Ejecutar `HubSpotCuentaCorrienteBatch` desde Home.
2. Revisar `ProcesosSpertaHubSpotLog` en MSGestion.
3. Si 401: job se detiene — corregir token antes de reintentar.

---

## Tests automatizados

```powershell
powershell.exe -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1 -Filter "FullyQualifiedName~HubSpot"
```

Ver [`../TESTING.md`](../TESTING.md).
