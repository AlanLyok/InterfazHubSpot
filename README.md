# BatchSpertaAPI (batch neutro)

Aplicación **.NET Framework 4.5.2** para procesos en segundo plano que se integran con **SpertaAPI** (`api/v100`), más **consola web MVC** de desarrollo para lanzar jobs manualmente. **No** incluye dominio Mercado Libre, MKP, APPro ni otros conectores de producto retirados.

---

## Stack

| Componente | Tecnología |
|------------|------------|
| Batch | `BatchSpertaAPI.BatchProcess` (`IScheduler`) |
| Web dev | ASP.NET MVC (login, ABMs existentes, botones de prueba en Home) |
| Datos | EF6 + SQL Server (usuarios, errores, combos de empresa/perfil) |
| API | `HttpSpertaApiClient` — OAuth password + rutas documentadas en el repo principal |

---

## Estructura

```
BatchSpertaAPI.sln
├── BatchSpertaAPI/                 # MVC
├── BatchSpertaAPI.Business/        # Managers + HttpSpertaApiClient + cola integraciones
├── BatchSpertaAPI.Entities/
├── BatchSpertaAPI.Interfaces/
├── BatchSpertaAPI.Mapping/
├── BatchSpertaAPI.BatchProcess/    # IScheduler (jobs + HubSpot)
├── InterfazHubSpot/                # Conector CRM HubSpot (2A cola + 2B cuenta corriente)
├── sql/                            # Scripts SQL (cola `ProcesosSpertaAPI`)
├── BatchSpertaAPI.IntegrationTests/
└── Componentes/                    # DLL Mastersoft mínimas para compilar
```

---

## Base de datos (cola neutra)

Ejecutar en la misma base MSGestion / contexto que usa el batch para errores y EF:

- [`sql/001_ProcesosSpertaAPI.sql`](sql/001_ProcesosSpertaAPI.sql) — tablas `dbo.ProcesosSpertaAPI` (columna **`Identificador`**) y `dbo.IntegracionEjecucionLog`.
- [`sql/002_ProcesosSpertaAPI_identificador.sql`](sql/002_ProcesosSpertaAPI_identificador.sql) — migración desde esquemas con `ClienteId`/`PayloadJson` + SP **`USER_POS_Clientes_Agregar`**.

Desde el ERP WinForms se insertan filas pendientes en la cola; contrato de columnas: [`docs/how-to/cola-integraciones-outbox-winforms.md`](../../docs/how-to/cola-integraciones-outbox-winforms.md).

---

## `connectionStrings`: MSFwk y MSGestion

Como **SpertaAPI**, el `Web.config` del MVC batch debe declarar **`MSFwk`** (framework: validación de usuario en `POST /token` OAuth) y **`MSGestion`** (datos ERP vía EF: cola `dbo.ProcesosSpertaAPI`, `UsuariosWeb`, errores, etc.). El cliente HTTP debe configurar **`SpertaAPICompanyId`** (cabecera `CompanyId` en OAuth y llamadas Bearer).

El login por pantalla (`UsuariosWeb` en gestión) puede seguir usando tablas en MSGestion; no reemplaza al OAuth salvo que el front use el mismo flujo.

---

## Cliente SpertaAPI (`HttpSpertaApiClient`)

Configuración en `Web.config` / `App.config`:

| Clave | Uso |
|--------|-----|
| `SpertaAPIBaseUrl` | Origen (ej. `https://host/`) |
| `SpertaAPIUserName` / `SpertaAPIPassword` | Grant `password` |
| `SpertaAPICompanyId` | **Obligatorio**: empresa ERP enviada como cabecera `CompanyId` en `POST /token` y en requests autenticadas (mismo valor que `CodEmpre` / contexto del token). |
| `SpertaAPIReciboRelativePath` | Opcional; ruta relativa si el despliegue expone POST de recibos (el núcleo documentado usa `clientes/grabar` y `pedidos/grabar`) |
| `FrameworkCNPrefix` | Prefijo de cadena Mastersoft para `MSContext` (default lógico: `BatchSpertaAPI`) |
| `EmailErrDE` / `EmailErrPara` / `EmailErrCc` | Destinatarios para `EmailsManager` (cola `Emails_Agregar`) |

Métodos principales: `GetHealthAsync`, `PostClientesGrabarAsync`, `PostPedidosGrabarAsync`, `PostReciboAsync` (solo si hay `SpertaAPIReciboRelativePath`), **`GetIntegracionesClienteAsync`**, **`GetIntegracionesHubSpotCuentaCorrienteAsync`** (lecturas Bearer para HubSpot).

---

## HubSpot (`InterfazHubSpot`)

Configuración en el mismo `Web.config` del sitio MVC batch (o `App.config` si ejecutan jobs desde otro host):

| Clave | Uso |
|--------|-----|
| `HubSpot:PrivateAppToken` | Obligatorio para jobs HubSpot **salvo** `HubSpot:UseDevelopmentMock=true` (solo dev); token private app (no versionar). |
| `HubSpot:BaseUrl` | Opcional; default `https://api.hubapi.com`. |
| `HubSpot:PropertyMastersoftId` | Propiedad company en HubSpot para id ERP (default `mastersoft_id_`). |
| `HubSpot:PropertyManejoCuentaCorriente` | Propiedad texto para resumen CC en batch 2B (default `manejo_cuenta_corriente`). |
| `HubSpot:DelayMillisecondsBetweenCalls` | Pausa entre llamadas REST (default `120`). |
| `HubSpot:CuentaCorrientePageSize` | Tamaño de página al pedir datos a SpertaAPI en 2B (default `500`, acotado en código). |
| `HubSpot:UseDevelopmentMock` | Opcional (`true`/`false`): **solo desarrollo**. Evita token real y intercepta llamadas CRM v3 con respuestas mínimas; no usar en producción sin consenso. |

En **SpertaAPI** (`Web.config` del API), el endpoint de cuenta corriente masiva requiere las claves `HubSpotCc:*` (reportes activos + impagos); ver comentarios en ese archivo y [`AGENTS.md`](../../AGENTS.md).

---

## Jobs (`IScheduler`)

- **`EjemploSpertaApiJob`** — Plantilla: llama a `GET api/v100/health` y registra en log; en error usa `ErroresManager` + `EmailsManager`.
- **`GrabarEmailError`** — Encola un correo de prueba vía `EmailsManager`.
- **`ProcesarColaIntegracionesHubSpotJob`** — Toma filas `Destino=HubSpot` pendientes, sincroniza compañía/contactos vía `HubSpotIntegracionRunner` (flujo **2A**).
- **`HubSpotSincronizarCuentaCorrienteJob`** — Pagina `GET …/sperta/integraciones/hubspot/cuenta-corriente-clientes` y envía batch update de 100 compañías en HubSpot (flujo **2B**).

Desde la Home MVC:

- Acciones estándar: **`POST /Home/ProcesarColaHubSpot`** (silencioso) y **`POST /Home/HubSpotCuentaCorrienteBatch`**.
- Traza incremental (respuesta JSON con `pasos`):  
  **`POST /Home/ProcesarColaHubSpotTrazaCola`** — vista previa tabla `ProcesosSpertaAPI` (conteos + muestra Pendiente sin reclamar).  
  **`POST /Home/ProcesarColaHubSpotTrazaCliente?clienteId=n`** — GET integraciones cliente vía `TracingSpertaApiClient` (pregunta/resolución OAuth + solicitud/resumen respuesta sin secretos largos duplicados).  
  **`POST /Home/ProcesarColaHubSpotTraza`** — corrida completa reclamo + sincronización HubSpot (marcar pendientes como EnProceso). Los botones equivalentes están en `Views/Home/Index.cshtml`.

---

## Tests

| Proyecto | Rol |
|----------|-----|
| `BatchSpertaAPI.Tests.Unit` | xUnit: cliente HTTP, trazas, HubSpot internals (HTTP mockeado), diagnósticos y constantes de cola. |
| `BatchSpertaAPI.IntegrationTests` | xUnit humo/compilación frente a `Business` (+ futuras `Category=Live` con BD/API). |

Ejecución automatizada desde la raíz del repo:

- `pwsh -NoProfile -File .\scripts\Run-Tests-BatchSpertaAPI.ps1` — build `-LibrariesOnly` (incluye **nuget restore** de la solución) + `dotnet test` en ambos proyectos. Por defecto excluye pruebas con trait `Category=Live` (`-IncludeLiveTraits` si en el futuro hay casos contra SQL/API reales).

Compilar solución:

- Desde la **raíz del repo**: [`scripts/Build-BatchSpertaAPI.ps1`](../../scripts/Build-BatchSpertaAPI.ps1) ejecuta antes **`nuget restore BatchSpertaAPI.sln`** y luego **MSBuild de Visual Studio** (`SPERTA_MSBUILD` / `MSBUILD_EXE` / `-NuGetExe` / `SPERTA_NUGET_EXE`). Ejemplo: `pwsh -NoProfile -File .\scripts\Build-BatchSpertaAPI.ps1`.
- Sin instalación web ASP.NET / solo SDK **dotnet**: `pwsh -NoProfile -File .\scripts\Build-BatchSpertaAPI.ps1 -LibrariesOnly` compila bibliotecas, `BatchProcess`, `InterfazHubSpot`, proyectos de tests (`Tests.Unit`, `IntegrationTests`) **sin** el sitio MVC `BatchSpertaAPI`.
- Alternativa: abrir `BatchSpertaAPI.sln` en Visual Studio (el proyecto web importa `Microsoft.WebApplication.targets`).
