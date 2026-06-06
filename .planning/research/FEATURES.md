# Features Research — Integración HubSpot

**Researched:** 2026-06-06

## Table Stakes (v1 — obligatorio)

| Feature | Complejidad | Dependencias |
|---------|-------------|--------------|
| Cola outbox `ProcesosSpertaAPI` | Baja | Tabla existente |
| SP post-grabación WinForms | Baja | `USER_POS_Clientes_Agregar` |
| Job 2A — procesar cola | Alta | SPs + HubSpotClient |
| Sync compañía HubSpot (search/create/patch) | Media | CRM v3 companies |
| Sync contactos + asociación | Media | CRM v3 contacts |
| Job 2B — cuenta corriente diaria | Alta | SP masivo + batch update |
| Formato `manejo_cuenta_corriente` | Media | Agrupación en memoria |
| Rate limiting y backoff 429 | Media | Config delay |
| EmailsManager en errores | Baja | Existente |
| Consola MVC botones manuales | Baja | Home controller |
| Modo desarrollo mock | Baja | Existente |

## Differentiators (no requeridos v1)

- Dashboard de métricas HubSpot
- Reintentos automáticos inteligentes cola
- UI de monitoreo cola en tiempo real

## Anti-features (no construir)

- OAuth flow HubSpot
- Dependencia SpertaAPI para datos HubSpot
- Reintentos automáticos en error 2A
- Sincronización bidireccional HubSpot → ERP
