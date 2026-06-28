# Base de datos — MSGestion

**Tipo:** Reference.  
**Contrato cola y estados:** [`../PRD_Integracion_HubSpot_2A_2B.md`](../PRD_Integracion_HubSpot_2A_2B.md) §5.  
**Deploy:** [`../../scriptsSQL/000_Deploy_All.sql`](../../scriptsSQL/000_Deploy_All.sql) (orquestador).

Copias versionadas en [`../../sql/`](../../sql/) — **canónico:** `scriptsSQL/`.

---

## Deploy

```text
scriptsSQL/000_Deploy_All.sql  →  ejecutar contra MSGestion (SSMS o sqlcmd)
```

| Script | Objeto | Rol |
|--------|--------|-----|
| `001_ProcesosSpertaHubSpot.sql` | `dbo.ProcesosSpertaHubSpot` | Cola outbox flujo 2A |
| `002_ProcesosSpertaHubSpotLog.sql` | `dbo.ProcesosSpertaHubSpotLog` | Log batch 2B / auditoría |
| `003_USER_CALZETTA_POS_Clientes_Agregar.sql` | `USER_POS_Clientes_Agregar` | ERP inserta en cola |
| `004_InterfazHubSpot_Cliente_Obtener.sql` | `InterfazHubSpot_Cliente_Obtener` | Empresa + direcciones (2A) |
| `005_InterfazHubSpot_Clientes_Contactos_Obtener.sql` | `InterfazHubSpot_Clientes_Contactos_Obtener` | Contactos cliente (2A) |
| `006_InterfazHubSpot_CuentaCorriente_Pagina.sql` | `InterfazHubSpot_CuentaCorriente_Pagina` | Paginación CC (2B); devuelve `ClienteId`, `NumeroDocumento`, `ManejoCuentaCorriente` |
| `008_InterfazHubSpot_VendedoresHabilitados.sql` | `InterfazHubSpot_VendedoresHabilitados` | Filtro vendedores SP 004/006 |
| `009_Indices.sql` | Índices | Performance SPs 004/006 |
| `010_InterfazHubSpot_Atributo_IdHubSpot.sql` | `InterfazHubSpot_Cliente_GuardarIdHubSpot` + atributo `id_hubspot` | Persistir ID company en ERP |
| `012_ListadoHubSpotProcesosCola_Buscar.sql` | Listado cola | Consola / soporte |
| `013_Indices_ProcesosHubSpot_Listado.sql` | Índices listado | Performance listado cola |

---

## Cola `dbo.ProcesosSpertaHubSpot`

Consumida por `ProcesarColaIntegracionesHubSpotJob`. Columna clave de negocio: **`Identificador`** (id cliente ERP).

Estados relevantes (detalle PRD): `Pendiente` → `EnProceso` → `Ok` | `Error`.

**Gate WinForms:** antes de `sp_rename` de tabla, desplegar `002` y verificar `USER_POS_Clientes_Agregar` activo en producción.

---

## Acceso desde C#

| Manager / componente | SP / tabla |
|---------------------|------------|
| `ClienteIntegracionManager` | 004, 005, 006 |
| Cola claim/update | `ProcesosSpertaHubSpot` via `ProcesosSpertaHubSpotManager` (EF6) |
| Persistir id HubSpot en cliente | `InterfazHubSpot_Cliente_GuardarIdHubSpot` (010) |
| `IntegracionErrorNotifier` | `MSEMails_Agregar` (framework email) |

Código managers: `InterfazHubSpot.Business/Managers/`.

---

## MCP SQL (desarrollo Cursor)

Servidor: `project-0-INTERFAZHUBSPOT-mssql-mcp-msgestion-CALZETTA`.  
Schema tools: `mcps/<server>/tools/`. Config ejemplo: `.cursor/mcp.mssql-mcp-server.example.json`.

Ver [`../agents/INDEX.md`](../agents/INDEX.md) § MCP.
