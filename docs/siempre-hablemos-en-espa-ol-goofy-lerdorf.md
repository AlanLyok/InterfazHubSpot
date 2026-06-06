# Plan: Eliminar legacy SpertaAPI + MSFwk + OAuth, dejar solo HubSpot ⇄ SPs sobre MSGestion

## Contexto

El runtime de InterfazHubSpot quedó con tres capas de scaffolding heredado que **no se usan en producción** y desorientan a quien lee el código:

1. **Cliente SpertaAPI HTTP** (`ISpertaApiClient` + `HttpSpertaApiClient` + `TracingSpertaApiClient`). El runner `HubSpotIntegracionRunner` lo inyecta y lo llama en `SincronizarClienteColaAsync` (flujo 2A) y `EjecutarSincronizacionCuentaCorriente` (flujo 2B). Diseño real: ambos datos deben salir de **stored procedures sobre MSGestion**.
2. **Autenticación contra MSFwk** (`SpertaFwkUsuarioAuthenticator`, `MsgestionTablasCompartidasLookup`, `OAuthProvider`, connection string `MSFwk`, claves `FrameworkCNPrefix`). Login OAuth heredado de SpertaAPI. El sitio MVC es una consola interna; no necesita login.
3. **Configuración `SpertaAPI*`** en `Web.config` / `App.config` (BaseUrl, UserName, Password, CompanyId, ReciboRelativePath).

**Outcome esperado:** el repo describe lo que realmente hace — leer cola `dbo.ProcesosSpertaHubSpot`, resolver datos por SP sobre MSGestion, autenticar **solo contra HubSpot** y crear/actualizar compañías + contactos. README, AGENTS.md y configs reflejan eso. Build y tests verdes.

Decisiones tomadas con el usuario:
- **Login MVC:** se borra completo (no MSFwk, no OAuth, no Windows Auth). El sitio queda como herramienta interna sin autenticación.
- **SPs:** los escribo yo en `sql/` (revisión humana antes de aplicar en BD). Las columnas que devuelven se derivan del consumo actual del runner.
- **Tests:** borro los tests Sperta y agrego cobertura unitaria al nuevo manager SP.

## Alcance del cambio

### A. Borrar enteros (archivos completos)

- `InterfazHubSpot.Business/Integration/ISpertaApiClient.cs`
- `InterfazHubSpot.Business/Integration/HttpSpertaApiClient.cs`
- `InterfazHubSpot.Business/Integration/TracingSpertaApiClient.cs`
- `InterfazHubSpot.BatchProcess/EjemploSpertaApiJob.cs`
- `InterfazHubSpot.Tests.Unit/HttpSpertaApiClientTests.cs`
- `InterfazHubSpot.Tests.Unit/TracingSpertaApiClientTests.cs`
- `InterfazHubSpot/Security/SpertaFwkUsuarioAuthenticator.cs`
- `InterfazHubSpot/Security/SpertaFwkUsuarioAuthFailureKind.cs`
- `InterfazHubSpot/Security/MsgestionTablasCompartidasLookup.cs`
- `InterfazHubSpot/Providers/OAuthProvider.cs` *(y cualquier wiring de OWIN/OAuth en `Startup.Auth.cs` / `App_Start`)*

Recordar quitar cada `<Compile Include="…" />` correspondiente en los `.csproj` (.NET Framework 4.5.2 no usa SDK-style; los includes son explícitos).

### B. Crear nuevo: `InterfazHubSpot.Business/Managers/ClienteIntegracionManager.cs`

Manager EF6 sobre `MSContext` que reemplaza las dos llamadas HTTP del runner. Devuelve **POCOs**, no JSON envelope.

```csharp
public sealed class ClienteIntegracionManager
{
    public ClienteIntegracionManager(MSContext ctx);

    // Reemplaza GetIntegracionesClienteAsync(int) — flujo 2A
    ClienteIntegracionDto ObtenerClienteParaHubSpot(int clienteId);

    // Reemplaza GetIntegracionesHubSpotCuentaCorrienteAsync(cursor, pageSize) — flujo 2B
    PaginaCuentaCorriente ObtenerPaginaCuentaCorriente(int cursor, int pageSize);
}
```

DTOs (`InterfazHubSpot.Entities/Integration/` o `InterfazHubSpot.Business/Integration/Dtos/`) con exactamente los campos que hoy lee el runner — `BuildCompanyProperties` (`HubSpotIntegracionRunner.cs:424-471`) y `BuildContactProperties` (`HubSpotIntegracionRunner.cs:473-484`) son la fuente de verdad de la forma esperada:

- `ClienteIntegracionDto`: `ClienteId`, `CodigoCliente`, y un sub-objeto `Cliente` con `RazonSocial`, `ApellidoYNombre`, `Contacto`, `NumeroDocumento`, `Calle`, `Puerta`, `Localidad`, `CodigoPostal`, `CodigoProvinciaCliente`, `CodigoPais`, `ZonaId`, `VendedorId`, `ResponsableCuentaId`, `ListaPreciosId`, `CondicionVentaId`, `DiasParaDeuda`, `LimiteCredito`, `CategoriaClienteId`, `ListaClientesContactos[]` (`ApellidoYNombre`, `CorreoElectronico`, `Telefono`, `SectorId`), `ListaDireccionEntregas[]` (`Domicilio` o `Descripcion`, `CodigoPostal`, `Localidad`, `ProvinciaId`, máx. 3).
- `PaginaCuentaCorriente`: `Items[] { ClienteId, ManejoCuentaCorriente }`, `HayMas`, `SiguienteCursor`.

Ejecución de SPs: usar `_ctx.Database.SqlQuery<T>("EXEC dbo.NombreSp @p", new SqlParameter("@p", v))` (patrón EF6 ya presente en el resto de managers). Para resultados de múltiples columnas + listas anidadas, llamar SP que devuelve **múltiples result sets** (`ObjectContext.Translate`) o dos SPs separados (cabecera + detalle). Prefiero **un SP con 3 result sets** para 2A (cabecera, contactos, direcciones) — menor latencia y atomicidad.

### C. Crear scripts SQL en `sql/`

- `sql/003_USP_Integracion_HubSpot_Cliente_Obtener.sql` — SP `dbo.USP_Integracion_HubSpot_Cliente_Obtener(@ClienteId INT)` con 3 SELECTs: cabecera cliente, contactos, direcciones de entrega (TOP 3).
- `sql/004_USP_Integracion_HubSpot_CuentaCorriente_Pagina.sql` — SP `dbo.USP_Integracion_HubSpot_CuentaCorriente_Pagina(@Cursor INT, @PageSize INT)` que devuelve `ClienteId`, `ManejoCuentaCorriente` desde `@Cursor` exclusive con `TOP @PageSize+1` para inferir `HayMas` y `SiguienteCursor`.

Los SPs leen las tablas reales del ERP (`Clientes`, `ClientesContactos`, `DireccionEntregas`, vistas/SP de CC existentes). En el script SQL dejo los `JOIN`/`SELECT` con comentarios marcando cada columna a confirmar contra el esquema MSGestion antes de aplicar — el usuario los revisa.

### D. Refactor de acoplamiento

**`InterfazHubSpot.Business/HubSpot/HubSpotIntegracionRunner.cs`**
- Quitar campo `_api`, parámetro `apiClient` del ctor, parámetro `instrumentarClienteHttpSpertaApi`, y todos los `using InterfazHubSpot.Business.Integration` que queden huérfanos.
- Inyectar `ClienteIntegracionManager` en el ctor (o construirlo a partir de `_msCtx`).
- `SincronizarClienteColaAsync` (línea 293): reemplazar `await _api.GetIntegracionesClienteAsync(clienteIdCola)` + parsing `JObject` por `_cli.ObtenerClienteParaHubSpot(clienteIdCola)`. `BuildCompanyProperties` y `BuildContactProperties` siguen igual pero leen del DTO en vez de `JObject` — toca cambiar los `ms["X"]?.ToString()` por `dto.Cliente.X`. La estructura de pasos (`_pasos.RegistrarPaso`) se mantiene; cambia `muestraDatosTruncada` para serializar el DTO con `JsonConvert.SerializeObject`.
- `EjecutarSincronizacionCuentaCorriente` (línea 230): reemplazar el bucle que llama `_api.GetIntegracionesHubSpotCuentaCorrienteAsync` por `_cli.ObtenerPaginaCuentaCorriente(cursor, tam)`; eliminar parseo `JObject` envelope y el chequeo `HayError`. La lógica de batching de 100, `SearchCompanyIdByMastersoftIdAsync` y `BatchUpdateCompaniesManejoCcAsync` queda intacta.
- Borrar el helper `LeerPrimerError` (solo servía al envelope SpertaAPI).
- El método ya no necesita ser `async`/`Task` arriba — pero como llama a `_hub.*` que sí son `async`, mantener `RunSync` como hoy.

**`InterfazHubSpot.BatchProcess/ProcesarColaIntegracionesHubSpotJob.cs`**
- Borrar las llamadas `HttpSpertaApiClient.SetOAuthDiagnosticsListener` / `ClearOAuthDiagnosticsListener` (líneas ~87-93) y el `try/finally` asociado.
- Cambiar `new HubSpotIntegracionRunner(Contexto, null, rep, instrumentarHttp)` → `new HubSpotIntegracionRunner(Contexto, rep)`.

**`InterfazHubSpot/Controllers/HomeController.cs`**
- Borrar action `EjemploSpertaApi` (líneas 29-35).
- En `ProcesarColaHubSpotTrazaCliente` y `ProcesarColaHubSpotTraza`: quitar cualquier referencia a `TracingSpertaApiClient` y al appSetting `SpertaAPIBaseUrl`. La traza pasa a reportar el SP ejecutado en vez del request HTTP (categoría `ProcesoPasoCategoria` ya tiene `Origen`/`Bd` apropiados).
- Quitar atributo `[Authorize]` y cualquier `[AllowAnonymous]` ya no necesario tras borrar OAuth. Revisar `Web.config` `<authentication mode>` → poner `None`. Borrar `App_Start/Startup.Auth.cs` si existe.

**`InterfazHubSpot/Global.asax.cs` / `Startup.cs`**
- Remover registro OWIN OAuth si está; el sitio queda sin pipeline de auth.

### E. Limpieza de configuración

**`Web.config.example`** (y `Web.config` real localmente — el archivo está gitignored, solo documentar):
- Borrar `<connectionStrings>` entry `MSFwk`.
- Borrar appSettings: `SpertaAPIBaseUrl`, `SpertaAPIUserName`, `SpertaAPIPassword`, `SpertaAPICompanyId`, `SpertaAPIReciboRelativePath`, `FrameworkCNPrefix`.
- Mantener: `MSGestion` connection string, todas las `HubSpot:*`, `EmailErr*`.
- `<system.web><authentication mode="Forms"/></system.web>` → `<authentication mode="None"/>` y borrar `<authorization>` salvo lo mínimo.

**`App.config`** en `InterfazHubSpot.Business`, `InterfazHubSpot.Mapping`, `InterfazHubSpot.Tests.Unit`, `InterfazHubSpot.IntegrationTests`, `InterfazHubSpot.BatchProcess`:
- Mismas remociones (cuando apliquen). Tests probablemente solo necesitan `MSGestion`.

### F. Tests

**Borrar:** `HttpSpertaApiClientTests.cs`, `TracingSpertaApiClientTests.cs` y cualquier fixture de helper Sperta asociado.

**Agregar:** `InterfazHubSpot.Tests.Unit/Managers/ClienteIntegracionManagerTests.cs` con cobertura del shape de DTO. Como EF6 + SQL Server no se puede mockear limpio sin InMemory, dos opciones — elijo (b) por simplicidad:

- (a) Wrapear `ClienteIntegracionManager` detrás de una interfaz `IClienteIntegracionFuente` que el runner consume → tests del runner mockean la interfaz, tests del manager usan integración real.
- (b) **Extraer la traducción `DataReader → DTO` a un método estático puro** (`ClienteIntegracionMapper.Mapear(...)`) y testear ese mapper con fixtures `DataTable` / objetos anónimos. Cobertura del SP en sí queda para `IntegrationTests` (`Category=Live`).

También agregar tests de regresión a `BuildCompanyProperties` / `BuildContactProperties` ahora que toman DTO en vez de `JObject`, si no existen.

### G. Documentación

- `README.md`: reescribir secciones `## Stack`, `## connectionStrings: MSFwk y MSGestion`, `## Cliente SpertaAPI (HttpSpertaApiClient)` y los puntos de los jobs `Ejemplo*`. Dejar solo: stack (sin SpertaAPI), connection string única `MSGestion`, claves `HubSpot:*`, claves `EmailErr*`, jobs reales, comandos PS1.
- `AGENTS.md`: actualizar tabla "Stack" (quitar "API externa SpertaAPI", reemplazar por "Datos ERP vía SP sobre MSGestion"); quitar línea "**Nunca** versionar `HubSpot:PrivateAppToken`" y dejar también la regla equivalente; quitar referencia a `MSFwk`.
- `docs/PRD_Integracion_HubSpot_2A_2B.md`: revisar y reemplazar menciones de SpertaAPI por SPs. Si el PRD describe el diseño original con SpertaAPI, agregar nota al inicio de que **el runtime usa SPs directos** y que las referencias a endpoints SpertaAPI son históricas.
- `CLAUDE.md` (ya creado en esta sesión): borrar la nota sobre "MSFwk para login OAuth" y dejar solo `MSGestion`.
- `.cursor/rules/interfaz-hubspot.mdc`: revisar y limpiar menciones a SpertaAPI / MSFwk.
- Scripts PS1 (`InterfazHubSpot/Scripts/agent/*.ps1`): revisar el grep legacy de `Verify-InterfazHubSpot.ps1` — debe **fallar** si alguien re-introduce `SpertaApi`, `MSFwk`, `HttpSpertaApiClient`. Agregar esos patrones a la lista bloqueada (si no están).

### H. Orden de ejecución sugerido (commits atómicos)

1. Agregar `sql/003_*.sql` y `sql/004_*.sql` (sin aplicar a BD).
2. Agregar DTOs + `ClienteIntegracionManager` + mapper estático + tests del mapper.
3. Refactor `HubSpotIntegracionRunner` para consumir el manager (sin borrar aún `ISpertaApiClient` — coexisten).
4. Refactor `ProcesarColaIntegracionesHubSpotJob` y `HomeController` para no construir/usar Sperta.
5. Borrar archivos Sperta + tests Sperta + entries de `.csproj`.
6. Borrar Security MSFwk + OAuthProvider + cambiar `Web.config.example` a `authentication mode="None"`.
7. Limpiar appSettings `SpertaAPI*`, connection string `MSFwk`.
8. Actualizar docs (README, AGENTS, PRD, CLAUDE.md, .cursor/rules).
9. Actualizar grep legacy en `Verify-InterfazHubSpot.ps1`.

Cada commit debe compilar y pasar tests (excepto el paso 1 que es solo SQL).

## Archivos críticos

**Modificar:**
- `InterfazHubSpot.Business/HubSpot/HubSpotIntegracionRunner.cs` (corazón del refactor)
- `InterfazHubSpot.BatchProcess/ProcesarColaIntegracionesHubSpotJob.cs`
- `InterfazHubSpot/Controllers/HomeController.cs`
- `InterfazHubSpot/Web.config.example` (+ `Web.config` local)
- `InterfazHubSpot.Business/InterfazHubSpot.Business.csproj`, `InterfazHubSpot/InterfazHubSpot.csproj`, `InterfazHubSpot.Tests.Unit/*.csproj`, `InterfazHubSpot.BatchProcess/*.csproj` (remover `<Compile Include="…Sperta…" />`)
- `README.md`, `AGENTS.md`, `CLAUDE.md`, `docs/PRD_Integracion_HubSpot_2A_2B.md`, `.cursor/rules/interfaz-hubspot.mdc`
- `InterfazHubSpot/Scripts/agent/Verify-InterfazHubSpot.ps1`

**Crear:**
- `InterfazHubSpot.Business/Managers/ClienteIntegracionManager.cs`
- `InterfazHubSpot.Business/Integration/Dtos/ClienteIntegracionDto.cs` (+ subtipos contacto/dirección + página CC)
- `InterfazHubSpot.Business/Integration/ClienteIntegracionMapper.cs` (estático, puro, testeable)
- `sql/003_USP_Integracion_HubSpot_Cliente_Obtener.sql`
- `sql/004_USP_Integracion_HubSpot_CuentaCorriente_Pagina.sql`
- `InterfazHubSpot.Tests.Unit/Managers/ClienteIntegracionMapperTests.cs`

**Borrar:** ver sección A.

## Reúso

- `ProcesosSpertaHubSpotManager` (`InterfazHubSpot.Business/Managers/`) — sigue siendo el dueño de la cola; no se toca.
- `IntegracionEjecucionLogManager`, `ErroresManager`, `EmailsManager` — intactos.
- `HubSpotCrmClient` y `HubSpotConfiguration` — intactos; siguen siendo la capa de auth + HTTP contra HubSpot.
- `_pasos` (`IProcesoPasoReporter`) — intacto; solo cambian categorías/códigos al reportar pasos de SP en vez de HTTP.
- `RunSync`, `IntegracionColaIdentificador.TryGetClienteId`, `IntegracionDestinos.HubSpot` — intactos.

## Verificación end-to-end

1. **Compilación full** desde la raíz:
   `pwsh -NoProfile -File InterfazHubSpot/Scripts/agent/Build-InterfazHubSpot.ps1`
   Verde sin warnings nuevos por using huérfanos.
2. **Tests unitarios**:
   `pwsh -NoProfile -File InterfazHubSpot/Scripts/agent/Test-InterfazHubSpot.ps1`
   - Los nuevos tests del mapper pasan.
   - No quedan referencias a `Sperta*` en árbol de tests.
3. **Grep legacy reforzado**:
   `pwsh -NoProfile -File InterfazHubSpot/Scripts/agent/Verify-InterfazHubSpot.ps1`
   El verify falla si alguien re-introduce `SpertaApi`, `HttpSpertaApiClient`, `MSFwk`, `SpertaFwk`.
4. **Aplicación de SPs en BD de dev** (manual): `sqlcmd -i sql/003_*.sql -i sql/004_*.sql` contra la base MSGestion de dev. `EXEC dbo.USP_Integracion_HubSpot_Cliente_Obtener 1234` devuelve 3 result sets con la forma esperada. `EXEC dbo.USP_Integracion_HubSpot_CuentaCorriente_Pagina 0, 500` devuelve datos paginables.
5. **Smoke MVC** local con `HubSpot:UseDevelopmentMock=true`:
   - Sitio levanta sin pedir login (modo anónimo).
   - `POST /Home/ProcesarColaHubSpotTrazaCliente?clienteId=<id-real>` ejecuta el SP, mapea al DTO, llama al stub HubSpot y devuelve JSON con pasos `bd.sp.cliente_obtener`, `mapeo.hubspot.company_properties`, `destinoexterno.hubspot.*`.
   - `POST /Home/HubSpotCuentaCorrienteBatch` itera páginas reales sin colgarse.
6. **Smoke contra HubSpot real** (manual, fuera de CI): mismas acciones con `UseDevelopmentMock=false` y un private app token de sandbox.

## Memoria a guardar al salir de plan mode

- `feedback`: "El usuario siempre quiere comunicación en español." (motivo: instrucción explícita 2026-06-06.)
- Actualizar `feedback_no_spertaapi_runtime.md` con el resultado de este refactor cuando se ejecute.
