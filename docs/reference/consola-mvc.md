# Consola MVC — endpoints

**Tipo:** Reference.  
**Código:** `SolucionInterfazHubSpot/InterfazHubSpot/Controllers/HomeController.cs`, vista `Views/Home/Index.cshtml`.

Consola interna **sin login**. Todos los endpoints abajo son accesibles vía POST desde la Home o herramientas HTTP.

---

## Jobs completos

| Método | Ruta | Job / acción |
|--------|------|----------------|
| POST | `/Home/ProcesarColaHubSpot` | `ProcesarColaIntegracionesHubSpotJob` (2A completo) |
| POST | `/Home/HubSpotCuentaCorrienteBatch` | `HubSpotSincronizarCuentaCorrienteJob` (2B) |

---

## Trazas 2A (JSON paso a paso)

| Método | Ruta | Qué hace |
|--------|------|----------|
| POST | `/Home/ProcesarColaHubSpotTrazaCola` | Vista previa filas cola pendientes |
| POST | `/Home/ProcesarColaHubSpotTrazaCliente?clienteId={n}` | SP 004 solo (sin HubSpot) |
| POST | `/Home/TrazaHubSpotBuscarEmpresa?clienteId={n}` | Search company por `mastersoft_id_` |
| POST | `/Home/TrazaHubSpotUpsertEmpresa?clienteId={n}` | SP 004 + create/patch company |
| POST | `/Home/TrazaHubSpotBuscarContacto?email={e}` | Search contact por email |
| POST | `/Home/TrazaHubSpotSincronizarContactos?clienteId={n}&hubCompanyId={id}` | SP 005 + upsert/asociar contactos |
| POST | `/Home/ProcesarColaHubSpotTraza` | Corrida 2A completa con steps JSON |

---

## Respuesta

JSON con pasos, correlación (`correlacionId`) y errores capturados. Logs texto en `PathLog` (`Web.config`).

Guía de uso: [`../how-to/debug-integracion.md`](../how-to/debug-integracion.md).

---

## Publicar MVC

Perfil `FolderProfile.pubxml` → `publish/` (raíz repo, gitignored). Ver [`../../SolucionInterfazHubSpot/README.md`](../../SolucionInterfazHubSpot/README.md).
