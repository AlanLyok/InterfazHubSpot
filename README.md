# InterfazHubSpot

Aplicación **.NET Framework 4.5.2** para procesos en segundo plano que sincronizan datos ERP Mastersoft → HubSpot CRM, más **consola web MVC** de desarrollo para lanzar jobs manualmente.

La fuente de datos es **SQL Server vía stored procedures** sobre `MSGestion` — no hay llamada HTTP a SpertaAPI en el runtime. El acceso a HubSpot usa **Private App Token** (CRM v3).

---

## Stack

| Componente | Tecnología |
|------------|------------|
| Framework | .NET Framework 4.5.2 |
| Web dev | ASP.NET MVC 5 (consola interna, sin login) |
| Batch | `InterfazHubSpot.BatchProcess` (`IScheduler`) |
| Datos ERP | EF6 + SQL Server — cola `dbo.ProcesosSpertaHubSpot` + SPs en MSGestion (`ClienteIntegracionManager`) |
| API HubSpot | HubSpot CRM v3 — Private App Token |
| Tests | xUnit (`InterfazHubSpot.Tests.Unit`, `InterfazHubSpot.IntegrationTests`) |

---

## Estructura

```
InterfazHubSpot.sln
├── InterfazHubSpot/                 # MVC
├── InterfazHubSpot.Business/        # Managers + ClienteIntegracionManager + cola integraciones
├── InterfazHubSpot.Business/HubSpot/  # Runners CRM HubSpot (2A cola + 2B cuenta corriente)
├── InterfazHubSpot.Entities/
├── InterfazHubSpot.Interfaces/
├── InterfazHubSpot.Mapping/
├── InterfazHubSpot.BatchProcess/    # IScheduler (jobs + HubSpot)
├── sql/                            # Scripts SQL (cola `ProcesosSpertaHubSpot`)
├── InterfazHubSpot.IntegrationTests/
└── Componentes/                    # DLL Mastersoft mínimas para compilar
```

---

## Base de datos (cola neutra)

Ejecutar en `MSGestion` (la misma base que usa el batch):

- [`sql/001_ProcesosSpertaHubSpot.sql`](sql/001_ProcesosSpertaHubSpot.sql) — tablas `dbo.ProcesosSpertaHubSpot` (columna **`Identificador`**) y `dbo.IntegracionEjecucionLog`.
- [`sql/002_ProcesosSpertaHubSpot_identificador.sql`](sql/002_ProcesosSpertaHubSpot_identificador.sql) — migración desde esquemas con `ClienteId`/`PayloadJson` + SP **`USER_POS_Clientes_Agregar`**.

Desde el ERP WinForms se insertan filas pendientes en la cola; contrato de columnas: [`docs/how-to/cola-integraciones-outbox-winforms.md`](../../docs/how-to/cola-integraciones-outbox-winforms.md).

---

## `connectionStrings`

El `Web.config` / `App.config` declara una única connection string:

- **`MSGestion`** — ERP DB; contexto EF6, cola `dbo.ProcesosSpertaHubSpot`, y host de todos los SPs que llama la integración (`dbo.USP_Integracion_HubSpot_Cliente_Obtener`, `dbo.USP_Integracion_HubSpot_CuentaCorriente_Pagina`, `USER_POS_Clientes_Agregar`, etc.). SQL migrations en `sql/`.

No se requiere `MSFwk`; el sitio MVC es una consola interna sin autenticación de usuario.

---

## HubSpot (`InterfazHubSpot`)

Configuración en `Web.config` del sitio MVC batch (o `App.config` si los jobs corren en otro host):

| Clave | Uso |
|--------|-----|
| `HubSpot:PrivateAppToken` | Obligatorio para jobs HubSpot **salvo** `HubSpot:UseDevelopmentMock=true` (solo dev); token private app (no versionar). |
| `HubSpot:BaseUrl` | Opcional; default `https://api.hubapi.com`. |
| `HubSpot:PropertyMastersoftId` | Propiedad company en HubSpot para id ERP (default `mastersoft_id_`). |
| `HubSpot:PropertyManejoCuentaCorriente` | Propiedad texto para resumen CC en batch 2B (default `manejo_cuenta_corriente`). |
| `HubSpot:DelayMillisecondsBetweenCalls` | Pausa entre llamadas REST (default `120`). |
| `HubSpot:CuentaCorrientePageSize` | Tamaño de página al consultar cuenta corriente en 2B (default `500`, acotado en código). |
| `HubSpot:UseDevelopmentMock` | Opcional (`true`/`false`): **solo desarrollo**. Evita token real y intercepta llamadas CRM v3 con respuestas mínimas; no usar en producción. |

---

## Jobs (`IScheduler`)

- **`GrabarEmailError`** — Encola un correo de prueba vía `EmailsManager`.
- **`ProcesarColaIntegracionesHubSpotJob`** — Toma filas `Destino=HubSpot` pendientes, resuelve datos de cliente vía SP (`ClienteIntegracionManager`) y sincroniza compañía/contactos vía `HubSpotIntegracionRunner` (flujo **2A**).
- **`HubSpotSincronizarCuentaCorrienteJob`** — Pagina datos de cuenta corriente vía SP (`dbo.USP_Integracion_HubSpot_CuentaCorriente_Pagina`) y envía batch update de 100 compañías en HubSpot (flujo **2B**).

Desde la Home MVC:

- Acciones estándar: **`POST /Home/ProcesarColaHubSpot`** (silencioso) y **`POST /Home/HubSpotCuentaCorrienteBatch`**.
- Traza incremental (respuesta JSON con `pasos`):  
  **`POST /Home/ProcesarColaHubSpotTrazaCola`** — vista previa tabla `ProcesosSpertaHubSpot` (conteos + muestra Pendiente sin reclamar).  
  **`POST /Home/ProcesarColaHubSpotTrazaCliente?clienteId=n`** — consulta datos cliente vía SP y muestra resolución sin secretos.  
  **`POST /Home/ProcesarColaHubSpotTraza`** — corrida completa reclamo + sincronización HubSpot (marcar pendientes como EnProceso). Los botones equivalentes están en `Views/Home/Index.cshtml`.

---

## Tests

| Proyecto | Rol |
|----------|-----|
| `InterfazHubSpot.Tests.Unit` | xUnit: HubSpot internals (HTTP mockeado), diagnósticos y constantes de cola. |
| `InterfazHubSpot.IntegrationTests` | xUnit humo/compilación frente a `Business` (+ futuras `Category=Live` con BD/API). |

Ejecución automatizada desde la raíz del repo:

- `pwsh -NoProfile -File .\InterfazHubSpot\Scripts\agent\Test-InterfazHubSpot.ps1` — `dotnet test` en ambos proyectos; excluye `Category=Live` por defecto.
- `pwsh -NoProfile -File .\InterfazHubSpot\Scripts\agent\Verify-InterfazHubSpot.ps1` — build + tests + verificación grep legacy.

Compilar solución:

- `pwsh -NoProfile -File .\InterfazHubSpot\Scripts\agent\Build-InterfazHubSpot.ps1` — **nuget restore** + MSBuild (`SPERTA_MSBUILD` / `MSBUILD_EXE`).
- Solo librerías (sin sitio MVC): `pwsh -NoProfile -File .\InterfazHubSpot\Scripts\agent\Build-InterfazHubSpot.ps1 -LibrariesOnly`.
- Alternativa: abrir `InterfazHubSpot.sln` en Visual Studio (el proyecto web importa `Microsoft.WebApplication.targets`).
