# HubSpot CRM v3 — referencia runtime

**Tipo:** Reference.  
**Implementación:** `InterfazHubSpot.Business/HubSpot/HubSpotCrmClient.cs`, `HubSpotConfiguration`.

---

## Autenticación

```http
Authorization: Bearer {HubSpot:PrivateAppToken}
Content-Type: application/json
```

401 → `HubSpotAuthException` → **fail-fast** (sin reintentos). Job 2B se detiene; email auth.

---

## Endpoints usados

| Operación | Método | Ruta |
|-----------|--------|------|
| Buscar company por ERP id | POST | `/crm/v3/objects/companies/search` |
| Crear company | POST | `/crm/v3/objects/companies` |
| Actualizar company | PATCH | `/crm/v3/objects/companies/{id}` |
| Buscar contact por email | POST | `/crm/v3/objects/contacts/search` |
| Crear contact | POST | `/crm/v3/objects/contacts` |
| Actualizar contact | PATCH | `/crm/v3/objects/contacts/{id}` |
| Asociar contact→company | PUT | `/crm/v3/objects/contacts/{cid}/associations/companies/{coId}/contact_to_company` |
| Batch update companies (2B) | POST | `/crm/v3/objects/companies/batch/update` |

Base URL: `HubSpot:BaseUrl` (default `https://api.hubapi.com`).

---

## Propiedades custom Calzetta

| Config | Default | Uso |
|--------|---------|-----|
| `HubSpot:PropertyCuitCuilUnica` | `cuitcuil_unica` | Correlación ERP ↔ company (búsqueda y upsert). Valor: solo dígitos, sin `-` ni `.` (ej. `30547981029`, `13018824`). |
| `HubSpot:PropertyMastersoftId` | `mastersoft_id_` | ClienteId ERP en payload (informativo) |
| `HubSpot:PropertyManejoCuentaCorriente` | `manejo_cuenta_corriente` | Texto saldo CC (2B) |

Otras propiedades company/contact vienen del mapper SP→payload (validar enums/options en portal HubSpot).

---

## Política HTTP

| Regla | Valor |
|-------|-------|
| Delay entre calls | `HubSpot:DelayMillisecondsBetweenCalls` (default 120 ms) |
| Reintentos | 429, 500, 502, 503, 504 — hasta `MaxHttpRetries` (3) |
| Backoff | `HttpRetryBackoffMilliseconds` (1000 ms) |
| 401 | Sin reintento; excepción auth |
| TLS | `Tls12` forzado en `HubSpotCrmClient` static ctor (servicio Windows) |

Intentos cola 2A: reporter `IHubSpotColaIntentosReporter` incrementa en fallos HTTP reintentables.

---

## Mock desarrollo

`HubSpot:UseDevelopmentMock=true` → `DevelopmentHubSpotStubHandler` responde a las rutas anteriores sin red.

**No usar en producción.**

Config completa: [`configuracion.md`](configuracion.md).
