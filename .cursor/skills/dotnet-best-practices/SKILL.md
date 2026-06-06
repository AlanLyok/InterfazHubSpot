---
name: dotnet-best-practices
description: >-
  Asegura que el C# y la API Web sigan el stack real OrdenTrabajoApi (.NET 8, ASP.NET Core,
  JWT Bearer, appsettings.json, Newtonsoft.Json, Swashbuckle, Mastersoft.FrameworkCore DLLs).
  Usar al tocar *.cs, *.csproj, *.sln, controllers, DTOs, ApiEngine, POST api/token,
  Mastersoft.ApiEngine.Consultas, verificación dotnet build. No aplicar patrones de
  .NET Framework 4.5.2 / Web API 2 / OWIN salvo en código legacy pendiente de migrar.
---

# Buenas prácticas .NET — ApiEngine (stack real)

## Ámbito y versiones

- **Host API:** `ApiEngineWeb/ApiEngineWeb.csproj` — **.NET 8**, **ASP.NET Core**, Kestrel.
- **Capas:** `Mastersoft.ApiEngine.{Business,Entities,Interfaces,Mapping}` — **net8.0**.
- **Consultas dinámicas:** `Mastersoft.ApiEngine.Consultas` — objetivo **net8.0** (migración desde net452).
- **ERP interno:** ensamblados **`Mastersoft.FrameworkCore.*`** en `Componentes/`; no reimplementar reglas ERP.
- **Fuente de verdad:** `AGENTS.md`, `.planning/PROJECT.md`, `.planning/research/STACK.md`.

**Política:** no upgrade a .NET 9+ ni majors nuevas de EF/JWT salvo corrección mínima para migración ApiEngine.Consultas.

## Estructura y namespaces

- Namespace web: **`ApiEngineWeb`** (Controllers, Core, Model).
- Capas negocio: **`Mastersoft.ApiEngine.*`**.
- Consultas dinámicas: **`Mastersoft.ApiEngine.Consultas`** — sin referencia circular a Business.
- Controladores heredan **`ControllerBase`** con `[ApiController]`.
- Managers con sufijo **`*Manager`** registrados en `Program.cs` vía **`NetCore.AutoRegisterDi`**.

## JSON y contratos HTTP

- Web usa **System.Text.Json** con **PascalCase** (`PropertyNamingPolicy = null`).
- Librería ApiEngine.Consultas y partes legacy usan **Newtonsoft.Json 13.0.2** — unificar versión al migrar.
- Respuestas auth y stores: **`ApiResponse`** (`HayError`, `Datos`, `ListaErrores`, `TimeStamp`) serializado con Newtonsoft.Json en login y pipeline Stores.

## Autenticación y multiempresa

- **JWT Bearer:** `POST api/token` con body `ParUsuariosWeb`.
- Claims: `CompanyId`, `UsuarioId`, ejercicios, perfil, `CNPrefix`.
- Endpoints Stores: **empresa y usuario desde claims JWT**, no confiar solo en query/body.
- Config JWT en **`appsettings.json`** sección `Jwt:*`.

## Consultas dinámicas (ApiEngine)

- Única vía segura: taxonomía en ruta + **`IdentificadorStore`** + fila **`dbo.ApiEngine`**.
- Ejecutor: **`CompactDynamicStoreExecutor`** — SP parametrizado, metadatos `sys.parameters`, lista negra SP.
- Resolver whitelist: **`StoreWhitelistAdoResolver`**.
- Parámetros reservados de query no se reenvían al SP.
- **No** inventar rutas GET estáticas para maestros sin registro en BD.

## Configuración

- **`appsettings.json`** + **`appsettings.Development.json`** + variables de entorno.
- Connection strings: `ConnectionStrings:ApiEngineWeb` (EF/host).
- Dynamic stores: sección **`DynamicStores:*`**; alinear nombre con `DynamicStores:DefaultConnectionStringName`.
- Preferir **`IConfiguration`** / **`DynamicStoreExecutorSettings`** sobre `ConfigurationManager`.

## Datos y SQL

- EF Core 7 para entidades del proyecto (`CAIM_MsOrdenTrabajoContext`).
- Stores dinámicos: **`Microsoft.Data.SqlClient`** (no `System.Data.SqlClient` en código net8).
- No concatenar SQL con input de usuario.

## Errores y pipeline

- Middleware global: **`ApiEngineWeb.Core.ExceptionHandler`**.
- Stores: **`DynamicStoreException`** con códigos HTTP conocidos.
- No manejo ad hoc por controlador si el pipeline central ya cubre el caso.

## Swagger

- **Swashbuckle.AspNetCore** en Development.
- Documentar seguridad JWT cuando se expongan endpoints protegidos.

## Async y estilo C#

- Host net8 soporta async/await; managers Mastersoft pueden ser síncronos — seguir patrón existente.
- Nullable reference types habilitado en proyectos SDK — respetar warnings nuevos.
- Priorizar **consistencia con código existente** sobre modernización gratuita.

## DLL-only entre soluciones

- Satélites (OT, FlowCRM, Admin, Worker.Core): `*DllReferences.props` + `Componentes/ApiEngine/`; **prohibido** `ClienteMonorepo.props` y `ProjectReference` al motor.
- Tests cat. **A** motor: `ProjectReference` interno OK. Cat. **B/C**: `*.TestSupport` local en la solución del producto.
- Regla Cursor: [`.cursor/rules/dll-only-deps.mdc`](../../.cursor/rules/dll-only-deps.mdc).

## Compilación y verificación

```powershell
.\scripts\build.ps1
.\scripts\test.ps1 -Scope Unit
```

Tests de integración (HTTP siempre; SQL solo con env vars):

```powershell
.\scripts\test.ps1 -Scope All
.\scripts\test-integration.ps1
```

Ejecutar la API:

```powershell
dotnet run --project SolucionApiEngine/ApiEngineWeb
```

Proyectos de test: `tests/ApiEngine.UnitTests`, `tests/OrdenTrabajoApi.IntegrationTests` (xUnit + WebApplicationFactory).

## Qué NO hacer

- Patrones OWIN / `Web.config` / `ApiController` Web API 2 en el host net8.
- Sustituir Mastersoft.FrameworkCore por DI genérico sin DLLs.
- Upgrade de stack más allá de .NET 8 sin aprobación explícita.
- SQL dinámico fuera del modelo ApiEngine whitelist.
