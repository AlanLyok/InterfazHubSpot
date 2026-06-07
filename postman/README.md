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
   - `mastersoftId`, `hubspotCompanyId`, `hubspotContactId` según el caso de prueba.

> **No versionar** tokens reales. Los valores secretos quedan solo en tu Postman local.

## Orden sugerido de prueba (HubSpot directo)

1. **0 — Auth y conexión → POST Verificar PAT (scopes)** — confirma token y permisos.
2. **0 — Auth y conexión → GET Propiedades de compañía** — smoke mínimo (200 = OK).
3. **Flujo 2A — Compañías → POST Buscar por mastersoft_id_**
4. **POST Crear compañía** (o PATCH si ya existe) — guarda `hubspotCompanyId` automáticamente.
5. **Flujo 2A — Contactos** — buscar → crear → asociar a compañía.
6. **Flujo 2B — Batch update** — actualizar `manejo_cuenta_corriente`.

## Consola MVC (local)

Carpeta **Consola MVC (local)** — requiere sitio corriendo y `Web.config` con `MSGestion` + token (o `UseDevelopmentMock=true`).

| Request | Qué hace |
|---------|----------|
| POST Procesar Cola HubSpot | Job 2A completo |
| POST Traza: ver estado de cola | Solo lectura cola SQL |
| POST Traza: SP datos cliente | SP sin HubSpot |
| POST Traza: corrida completa 2A | SP + HubSpot con JSON paso a paso |
| POST Batch Cuenta Corriente | Job 2B |

## Autenticación

Todas las llamadas **salientes** a `api.hubapi.com` usan:

```http
Authorization: Bearer {{privateAppToken}}
```

El **Client Secret** no va en ese header; sirve para validar webhooks **entrantes** (Flujo 1, no implementado en este repo).

## Referencia

- PRD: `docs/PRD_Integracion_HubSpot_2A_2B.md`
- PDF integración v3: `docs/Integracion_HubSpot_Mastersoft_v3.docx.pdf`
