# Postman — InterfazHubSpot / HubSpot CRM v3

Colección y environment para probar los endpoints HubSpot (2A, 2B) y la consola MVC local sin depender del batch.

## Importar en Postman

1. **File → Import** (o arrastrar los JSON).
2. Seleccionar:
   - `InterfazHubSpot-HubSpot.postman_collection.json`
   - `InterfazHubSpot-Dev.postman_environment.json`
3. Activar el environment **InterfazHubSpot — Dev (Calzetta)** (esquina superior derecha).
4. Editar variables del environment (icono ojo):
   - `privateAppToken` → tu PAT `pat-na1-...` (**obligatorio** para llamadas a HubSpot).
   - `clientSecret` → opcional; solo si probás webhooks Flujo 1.
   - `localUrl` → puerto donde corre el sitio MVC.
   - `mastersoftId` → ClienteId ERP para SP 004/005 y trazas MVC.
   - `hubspotCompanyId` → se completa en Paso 4 o manualmente.
   - `useDevelopmentMock` → informativo; alinear con `Web.config` si usás mock.

> **No versionar** tokens reales. Los valores secretos quedan solo en tu Postman local.

## Prerrequisito SQL (trazas MVC)

Antes de ejecutar requests de **Consola MVC (local)**, desplegar en MSGestion:

```text
scriptsSQL/000_Deploy_All.sql
```

Incluye cola `ProcesosSpertaHubSpot`, SP 004 `InterfazHubSpot_Cliente_Obtener`, SP 005 `InterfazHubSpot_Clientes_Contactos_Obtener`, etc.

## Orden sugerido de prueba (HubSpot directo)

1. **0 — Auth y conexión → POST Verificar PAT (scopes)** — confirma token y permisos.
2. **0 — Auth y conexión → GET Propiedades de compañía** — smoke mínimo (200 = OK).
3. **Flujo 2A — Compañías → POST Buscar por mastersoft_id_**
4. **POST Crear compañía** (o PATCH si ya existe) — guarda `hubspotCompanyId` automáticamente. Body incluye `manejo_cuenta_corriente`, `puerta` y `direccion_1_*`.
5. **Flujo 2A — Contactos** — buscar → crear → asociar a compañía.
6. **Flujo 2B — Batch update** — actualizar `manejo_cuenta_corriente`.

## Consola MVC (local)

Carpeta **Consola MVC (local)** — requiere sitio corriendo, `Web.config` con `MSGestion` + token (o `HubSpot:UseDevelopmentMock=true`).

**Flujo 2A dividido:** SP 004 (empresa + direcciones) → upsert compañía → SP 005 (contactos).

### Orden smoke sugerido (MVC)

1. **Paso 1 — Traza cola** — estado de `dbo.ProcesosSpertaHubSpot`.
2. **Paso 2 — Traza SP 004** — solo datos ERP (sin HubSpot ni contactos).
3. **Paso 4 — TrazaHubSpotUpsertEmpresa** — SP 004 + upsert; guarda `hubspotCompanyId`.
4. **Paso 6 — TrazaHubSpotSincronizarContactos** — SP 005 + contactos (usa `hubspotCompanyId`).
5. **Corrida completa 2A — Traza** — flujo end-to-end con cola.

(Paso 3 buscar empresa y Paso 5 buscar contacto son opcionales para diagnóstico aislado.)

### Endpoints MVC (todos POST, sin auth)

| Request Postman | Ruta | Qué hace |
|-----------------|------|----------|
| Paso 1 — Traza cola | `/Home/ProcesarColaHubSpotTrazaCola` | Lectura cola SQL |
| Paso 2 — Traza SP 004 | `/Home/ProcesarColaHubSpotTrazaCliente?clienteId=` | SP 004: empresa + direcciones (no contactos) |
| Paso 3 — TrazaHubSpotBuscarEmpresa | `/Home/TrazaHubSpotBuscarEmpresa?clienteId=` | Search compañía por `mastersoft_id_` |
| Paso 4 — TrazaHubSpotUpsertEmpresa | `/Home/TrazaHubSpotUpsertEmpresa?clienteId=` | SP 004 + crear/actualizar compañía |
| Paso 5 — TrazaHubSpotBuscarContacto | `/Home/TrazaHubSpotBuscarContacto?email=` | Search contacto por email |
| Paso 6 — TrazaHubSpotSincronizarContactos | `/Home/TrazaHubSpotSincronizarContactos?clienteId=&hubCompanyId=` | SP 005 + upsert/asociar contactos |
| Corrida completa 2A | `/Home/ProcesarColaHubSpotTraza` | Job 2A con JSON paso a paso |
| Job silencioso 2A | `/Home/ProcesarColaHubSpot` | Job 2A sin traza |
| Job silencioso 2B | `/Home/HubSpotCuentaCorrienteBatch` | Batch cuenta corriente |
| Grabar email | `/Home/GrabarEmailError` | Diagnóstico email |

En la **corrida completa 2A**, el JSON de traza debe mostrar el paso `bd.sp.contactos_obtener` **después** de `destinoexterno.hubspot.company_upsert` (orden del flujo dividido SP 004 → HubSpot → SP 005).

## Autenticación

Todas las llamadas **salientes** a `api.hubapi.com` usan:

```http
Authorization: Bearer {{privateAppToken}}
```

El **Client Secret** no va en ese header; sirve para validar webhooks **entrantes** (Flujo 1, no implementado en este repo).

Los requests **MVC** usan `auth: noauth` (consola interna sin login).

## Referencia

- PRD: `docs/PRD_Integracion_HubSpot_2A_2B.md`
- Deploy SQL: `scriptsSQL/000_Deploy_All.sql`
- PDF integración v3: `docs/Integracion_HubSpot_Mastersoft_v3.docx.pdf`
