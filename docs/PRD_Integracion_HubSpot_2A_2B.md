# PRD — Integración HubSpot · Flujos 2A y 2B
**Proyecto:** InterfazHubSpot (limpio) — Solo HubSpot + SQL  
**Ticket:** #116367  
**Cliente:** Calzetta  
**Versión:** 1.0  
**Fecha:** Junio 2026  

> **Nota de implementación (jun 2026):** El runtime resuelve datos via SPs canónicos en MSGestion:
> `InterfazHubSpot_Cliente_Obtener` (004), `InterfazHubSpot_Clientes_Contactos_Obtener` (005),
> `InterfazHubSpot_CuentaCorriente_Pagina` (006), `InterfazHubSpot_Cliente_GuardarIdHubSpot` (010).
> No hay HTTP a SpertaAPI. Los nombres `USER_HS_*` y `USP_Integracion_*` en este documento son **históricos**;
> ver §5.3 mapeo y [`reference/base-datos.md`](reference/base-datos.md).

---

## 1. Contexto y Objetivo

Desarrollar, sobre la estructura existente de `InterfazHubSpot`, una versión limpia que implemente exclusivamente los dos flujos de sincronización de Mastersoft ERP hacia HubSpot CRM. Se elimina toda dependencia de otras APIs (SpertaAPI, Mercado Libre, MKP, APPro) y se reemplaza por consultas directas a SQL Server mediante stored procedures.

**Flujo 2A** — Cuando se crea o modifica un cliente en el ERP (WinForms), sincronizar la Compañía y sus Contactos en HubSpot.

**Flujo 2B** — Una vez al día, actualizar el campo `manejo_cuenta_corriente` de todas las compañías en HubSpot con el saldo de deuda vigente.

---

## 2. Stack y Restricciones Técnicas

| Componente | Decisión |
|---|---|
| Framework | .NET Framework 4.5.2 (igual al proyecto existente) |
| Scheduler | `IScheduler` (Quartz.NET o similar, ya presente en InterfazHubSpot) |
| Web dev console | ASP.NET MVC — login + botones manuales en Home |
| Datos | SQL Server — acceso exclusivo por Stored Procedures |
| API externa | HubSpot CRM v3 — autenticación con Private App Token |
| Sin dependencias | No SpertaAPI, no DLLs Mastersoft, no OAuth flow |

---

## 3. Autenticación HubSpot

Se utiliza **Private App Token** (PAT), ya disponible. No se implementa OAuth flow.

Todas las llamadas HTTP salientes a HubSpot llevan el header:

```
Authorization: Bearer {HubSpot:PrivateAppToken}
Content-Type: application/json
```

El token se configura en `Web.config` bajo la clave `HubSpot:PrivateAppToken`. **Nunca se versiona en el repositorio.** En desarrollo se puede usar `HubSpot:UseDevelopmentMock=true` para interceptar llamadas sin token real.

---

## 4. Estructura del Proyecto (limpio)

```
InterfazHubSpot.sln
├── InterfazHubSpot/                  # MVC — consola web (login + Home con botones)
├── InterfazHubSpot.Business/         # Managers + HubSpotClient + SqlDataAccess + EmailsManager
├── InterfazHubSpot.Entities/         # DTOs: ClienteHS, ContactoHS, CuentaCorrienteHS
├── InterfazHubSpot.Interfaces/       # IHubSpotClient, ISqlDataAccess
├── InterfazHubSpot.BatchProcess/     # IScheduler — jobs 2A y 2B
├── InterfazHubSpot/                 # Lógica de negocio HubSpot (runners 2A y 2B)
└── sql/                             # Scripts SQL (tablas + stored procedures)
```

Se eliminan o vacían: referencias a `HttpSpertaApiClient`, integraciones MKP/MeLi/APPro.  
Se conserva: `EmailsManager` (notificaciones de error en ambos flujos).

---

## 5. Base de Datos

### 5.1 Tabla `dbo.ProcesosSpertaHubSpot`

Cola outbox. El ERP inserta filas vía `USER_POS_Clientes_Agregar`; el batch las consume (flujo 2A).

```sql
CREATE TABLE dbo.ProcesosSpertaHubSpot (
    ProcesoId              BIGINT IDENTITY(1,1) NOT NULL,
    EmpresaId              INT NULL,
    Destino                NVARCHAR(50) NOT NULL,   -- 'HubSpot'
    TipoEntidad            NVARCHAR(50) NOT NULL,   -- 'Cliente'
    TipoOperacion          NVARCHAR(20) NOT NULL,   -- 'Alta/Modificacion'
    Identificador          INT NOT NULL,            -- PK maestro (ClienteID)
    Estado                 TINYINT NOT NULL,          -- 0=Pendiente, 1=EnProceso, 2=Ok, 3=Error
    Intentos               INT NOT NULL,
    MensajeUltimoError     NVARCHAR(MAX) NULL,
    FechaCreacion          DATETIME2 NOT NULL DEFAULT GETDATE(),
    FechaInicioProceso     DATETIME2 NULL,
    FechaFinProceso        DATETIME2 NULL,
    CONSTRAINT PK_ProcesosSpertaHubSpot PRIMARY KEY CLUSTERED (ProcesoId)
);
```

### 5.2 Tabla `dbo.ProcesosSpertaHubSpotLog`

Auditoría por corrida de integración (flujos 2A y 2B). Una fila por fase/resultado.

```sql
CREATE TABLE dbo.ProcesosSpertaHubSpotLog (
    LogId          BIGINT IDENTITY(1,1) NOT NULL,
    ProcesoId      BIGINT NULL,
    Destino        NVARCHAR(50) NOT NULL,
    Identificador  INT NULL,              -- PK maestro según contexto (p. ej. ClienteID)
    Fase           NVARCHAR(80) NOT NULL,
    Exito          BIT NOT NULL,
    Detalle        NVARCHAR(MAX) NULL,
    FechaGrabacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_ProcesosSpertaHubSpotLog PRIMARY KEY CLUSTERED (LogId)
);
```

### 5.3 Stored Procedures — nombres canónicos (implementados)

**Deploy:** `scriptsSQL/000_Deploy_All.sql`. Referencia: [`reference/base-datos.md`](reference/base-datos.md).

#### Mapeo histórico → implementación

| Nombre en borrador PRD | SP / mecanismo actual | Script |
|------------------------|----------------------|--------|
| `USER_HS_Cliente_ObtenerDatos` | `InterfazHubSpot_Cliente_Obtener` | 004 |
| `USER_HS_ClienteContactos_Buscar` | `InterfazHubSpot_Clientes_Contactos_Obtener` | 005 |
| `USER_HS_CuentaCorriente_ObtenerTodos` | `InterfazHubSpot_CuentaCorriente_Pagina` (paginado keyset) | 006 |
| `USER_HS_Cola_ActualizarEstado` | `ProcesosSpertaHubSpotManager` (EF6 sobre cola) | — |
| `USER_HS_Cliente_GuardarHubSpotId` | `InterfazHubSpot_Cliente_GuardarIdHubSpot` | 010 |
| `USP_Integracion_HubSpot_*` | Alias históricos en `sql/` | ver §13 |

#### SP 1 — `USER_POS_Clientes_Agregar` (post-grabación WinForms)

Se ejecuta desde el ERP WinForms cada vez que se crea o modifica un cliente. Inserta una fila en la cola solo si no hay ya una fila `Pendiente` para el mismo cliente (evita duplicados en cola).

**Parámetros de entrada:**
- `@ClienteID INT`

**Lógica:**
```sql
IF NOT EXISTS (
    SELECT 1 FROM dbo.ProcesosSpertaHubSpot
    WHERE Destino = N'HubSpot'
      AND TipoEntidad = N'Cliente'
      AND Identificador = @ClienteID
      AND Estado = 0
)
BEGIN
    INSERT INTO dbo.ProcesosSpertaHubSpot (
        Destino, TipoEntidad, TipoOperacion, Identificador, Estado, Intentos, FechaCreacion
    )
    VALUES (N'HubSpot', N'Cliente', N'Alta/Modificacion', @ClienteID, 0, 0, GETDATE())
END
```

#### SP 2 — `InterfazHubSpot_Cliente_Obtener` *(borrador: USER_HS_Cliente_ObtenerDatos)*

Devuelve los datos de la compañía para sincronizar en HubSpot dado un `ClienteId`.

**Parámetros de entrada:**
- `@ClienteId INT`

**Result set — Compañía (1 fila):**

| Campo SQL | Campo HubSpot (interno) | Notas |
|---|---|---|
| RazonSocial | `name` | |
| NombreFantasia | `nombre_fantasia` | |
| CUIT / NroDocumento | `cuitcuil_unica` | Normalizado (sin `-`, `.`, `,`) — clave de correlación |
| NroCliente | `nro_cliente` | |
| ClienteId | `mastersoft_id_` | Como string (informativo) |
| HubSpotCompanyId | — | ID HubSpot almacenado en Mastersoft; NULL si no existe aún |
| Calle | `address` | |
| Puerta | `puerta` | |
| Localidad | `city` | |
| CodigoPostal | `zip` | |
| Provincia | `state` | |
| Pais | `Country` | |
| ZonaVta | `zona_vta` | |
| Vendedor | `vendedor` | |
| ResponsableCuenta | `responsable_de_cuenta` | |
| ListaPrecios | `lista_de_precios` | |
| CondicionVenta | `condicion_de_venta` | |
| DiasParaDeuda | `dias_para_deuda` | |
| LimiteCredito | `limite_de_credito` | |
| CategoriaCliente | `categoria_cliente` | |
| Dir1Domicilio | `direccion_1_domicilio` | |
| Dir1CP | `direccion_1_cp` | |
| Dir1Localidad | `direccion_1_localidad` | |
| Dir1Provincia | `direccion_1_provincia` | |
| Dir2Domicilio | `direccion_2_domicilio` | nullable |
| Dir2CP | `direccion_2_cp` | nullable |
| Dir2Localidad | `direccion_2_localidad` | nullable |
| Dir3Domicilio | `direccion_3_domicilio` | nullable |
| Dir3CP | `direccion_3_cp` | nullable |
| Dir3Localidad | `direccion_3_localidad` | nullable |

#### SP 3 — `InterfazHubSpot_Clientes_Contactos_Obtener` *(borrador: USER_HS_ClienteContactos_Buscar)*

Devuelve todos los contactos asociados a un cliente.

**Parámetros de entrada:**
- `@ClienteId INT`

**Result set — Contactos (N filas):**

| Campo SQL | Campo HubSpot (interno) | Notas |
|---|---|---|
| Nombre | `firstname` | |
| Sector | `sector` | |
| Telefono | `phone` | |
| Email | `email` | Clave de deduplicación en HubSpot |

#### SP 4 — `InterfazHubSpot_CuentaCorriente_Pagina` *(borrador: USER_HS_CuentaCorriente_ObtenerTodos)*

Paginación keyset (`@Cursor`, `@PageSize`). El batch C# itera páginas hasta agotar clientes; no carga todo en memoria.

**Parámetros de entrada:**
- `@Cursor INT` — último `ClienteID` procesado (0 = primera página)
- `@PageSize INT` — tamaño página (caller pasa N+1 para detectar más datos)

**Result set (una fila por cliente en la página):**

| Campo SQL | Descripción |
|---|---|
| ClienteId | ID Mastersoft |
| NumeroDocumento | NroDocumento normalizado (sin `-`, `.`, `,`) — búsqueda HubSpot `cuitcuil_unica` |
| ManejoCuentaCorriente | Texto preformateado para propiedad HubSpot |

El job 2B envía el texto en batch de 100 companies vía `POST /crm/v3/objects/companies/batch/update`.

#### SP 5 — Actualización estado cola *(borrador: USER_HS_Cola_ActualizarEstado)*

**Implementación:** `ProcesosSpertaHubSpotManager` (EF6) — métodos `MarcarOk`, `MarcarError`, claim `EnProceso`. No hay SP dedicado.

#### SP 6 — `InterfazHubSpot_Cliente_GuardarIdHubSpot` *(borrador: USER_HS_Cliente_GuardarHubSpotId)*

Persiste el `HubSpotCompanyId` devuelto por HubSpot en la tabla de clientes de Mastersoft, para no tener que buscarlo en cada ejecución futura.

**Parámetros de entrada:**
- `@ClienteId INT`
- `@IdHubSpot VARCHAR(50)` — ID company devuelto por HubSpot

Persiste en atributo variable `CLIENTES.id_hubspot` (script 010). Si no existe company en HubSpot aún, 2A busca por `cuitcuil_unica` (NroDocumento normalizado) antes de create/patch.

---

## 6. Flujo 2A — Mastersoft → HubSpot (Sincronización Clientes/Contactos)

### 6.1 Trigger

El ERP WinForms llama a `USER_POS_Clientes_Agregar @ClienteId` tras crear o modificar un cliente. Esto inserta una fila `Pendiente` en `dbo.ProcesosSpertaHubSpot`.

### 6.2 Job: `ProcesarColaIntegracionesHubSpotJob`

**Frecuencia:** cada 5 minutos (configurable en `Config.xml` / `IScheduler`).

**Paso a paso:**

1. Consultar `dbo.ProcesosSpertaHubSpot` WHERE `Destino='HubSpot'` AND `Estado=0` (Pendiente) ORDER BY `FechaCreacion ASC`. Tomar hasta N registros por ciclo (configurable, default 50).

2. Para cada fila:
   a. Marcar `Estado=1` (EnProceso) vía `ProcesosSpertaHubSpotManager`.
   b. Ejecutar `InterfazHubSpot_Cliente_Obtener @ClienteId` → cabecera + direcciones; incluye `HubSpotCompanyId` / `id_hubspot` si existe.
   c. **Buscar/crear compañía en HubSpot:**
      - Si no hay ID conocido → buscar por `cuitcuil_unica` via `POST /crm/v3/objects/companies/search` (valor = NroDocumento normalizado del SP 004).
        - Si existe → `PATCH /crm/v3/objects/companies/{id}`.
        - Si no existe → `POST /crm/v3/objects/companies`.
      - Si hay ID HubSpot → `PATCH` directo.
      - Tras create: `InterfazHubSpot_Cliente_GuardarIdHubSpot @ClienteId, @IdHubSpot`.
   d. Ejecutar `InterfazHubSpot_Clientes_Contactos_Obtener @ClienteId` → lista de contactos.
   e. Para cada contacto:
      - Buscar por email via `POST /crm/v3/objects/contacts/search`.
      - Si existe → `PATCH /crm/v3/objects/contacts/{id}`.
      - Si no existe → `POST /crm/v3/objects/contacts` → luego `PUT` asociación contacto-compañía.
   f. Si todo OK → `ProcesosSpertaHubSpotManager.MarcarOk`.
   g. Si cualquier paso falla → `MarcarError` con detalle. **No reintentar** la fila automáticamente.

### 6.3 Endpoints HubSpot utilizados (Flujo 2A)

| Paso | Endpoint | Método |
|---|---|---|
| Buscar compañía | `/crm/v3/objects/companies/search` | POST |
| Crear compañía | `/crm/v3/objects/companies` | POST |
| Actualizar compañía | `/crm/v3/objects/companies/{id}` | PATCH |
| Buscar contacto | `/crm/v3/objects/contacts/search` | POST |
| Crear contacto | `/crm/v3/objects/contacts` | POST |
| Actualizar contacto | `/crm/v3/objects/contacts/{id}` | PATCH |
| Asociar contacto | `/crm/v3/objects/contacts/{id}/associations/companies/{companyId}/contact_to_company` | PUT |

### 6.4 Rate Limiting

- Rate limit HubSpot: 100 requests / 10 segundos por token.
- Configurar delay entre llamadas: `HubSpot:DelayMillisecondsBetweenCalls` (default 120ms).
- Para el proceso de contactos en un cliente con muchos contactos, el delay aplica entre cada llamada individual.

---

## 7. Flujo 2B — Mastersoft → HubSpot (Cuenta Corriente Diaria)

### 7.1 Trigger

Job programado diario. **Hora propuesta: 3:00 AM** (antes del inicio de jornada laboral). Configurable en `IScheduler`.

### 7.2 Job: `HubSpotSincronizarCuentaCorrienteJob`

**Paso a paso:**

1. Registrar inicio en `dbo.ProcesosSpertaHubSpotLog` (fase de corrida 2B).
2. Iterar páginas con `InterfazHubSpot_CuentaCorriente_Pagina @Cursor, @PageSize` hasta agotar clientes activos.
3. Por cada fila de página: resolver company HubSpot por `cuitcuil_unica` (= `NumeroDocumento` del SP); omitir si falta documento o no hay company.
4. Dividir en grupos de 100 compañías (límite HubSpot batch).
5. Por cada grupo: `POST /crm/v3/objects/companies/batch/update`.
6. Delay configurable entre calls/batches (`HubSpot:DelayMillisecondsBetweenCalls`, default 120 ms).
7. Registrar fin, totales y errores en `dbo.ProcesosSpertaHubSpotLog`.

### 7.3 Formato del campo `manejo_cuenta_corriente`

```
// Cliente CON deuda:
15/03/2026 --- $125.400,00
02/04/2026 --- $89.200,50
18/04/2026 --- $210.000,00

// Cliente SIN deuda:
Cuenta Corriente al 06/06/2026. Deuda: $0
```

### 7.4 Endpoint HubSpot utilizado (Flujo 2B)

| Paso | Endpoint | Método |
|---|---|---|
| Batch update compañías | `/crm/v3/objects/companies/batch/update` | POST |

### 7.5 Consideraciones

- El proceso es **idempotente**: ejecutarlo múltiples veces produce el mismo estado final.
- Se procesan **siempre todos** los clientes activos, no solo los que tienen deuda, para que los que pagaron queden actualizados.
- Si un cliente activo no tiene `HubSpotCompanyId` en Mastersoft, se omite y se registra en el log como advertencia (no se interrumpe el proceso).

---

## 8. Configuración (`Web.config`)

Las claves ya existentes en el proyecto se conservan tal cual. A continuación el estado final relevante para esta integración:

```xml
<appSettings>
  <!-- App -->
  <add key="Title"                          value="InterfazHubSpot" />
  <add key="EmpresaId"                      value="1" />
  <add key="EnableErrorLogInDataBase"       value="true" />
  <add key="ErrorLogConnectionName"         value="InterfazHubSpot" />
  <add key="PathLog"                        value="C:\...\InterfazHubSpot.txt" />

  <!-- Email (ya configurado) -->
  <add key="smtpserver"                     value="smtp.office365.com" />
  <add key="smtpserverport"                 value="587" />
  <add key="enablessl"                      value="S" />
  <add key="usedefaultcredentials"          value="N" />
  <add key="credentialusername"             value="Notificaciones_Mastersoft@mastersoft.com.ar" />
  <add key="credentialpassword"             value="(encriptado)" />
  <add key="EmailDe"                        value="Notificaciones_Mastersoft@mastersoft.com.ar" />
  <add key="EmailErrDE"                     value="" />  <!-- completar en deploy -->
  <add key="EmailErrPara"                   value="" />  <!-- completar en deploy -->
  <add key="EmailErrCc"                     value="" />  <!-- completar en deploy -->

  <!-- HubSpot — descomentar y completar en servidor; NO versionar el token -->
  <!-- <add key="HubSpot:PrivateAppToken"               value="pat-na1-..." /> -->
  <!-- <add key="HubSpot:BaseUrl"                       value="https://api.hubapi.com" /> -->
  <!-- <add key="HubSpot:PropertyMastersoftId"          value="mastersoft_id_" /> -->
  <!-- <add key="HubSpot:PropertyCuitCuilUnica"       value="cuitcuil_unica" /> -->
  <!-- <add key="HubSpot:PropertyManejoCuentaCorriente" value="manejo_cuenta_corriente" /> -->
  <!-- <add key="HubSpot:DelayMillisecondsBetweenCalls" value="120" /> -->
  <!-- <add key="HubSpot:CuentaCorrientePageSize"       value="500" /> -->
  <!-- <add key="HubSpot:ColaBatchSize"                 value="50" /> -->
  <add key="HubSpot:UseDevelopmentMock"             value="true" />  <!-- false en producción -->

  <!-- Diagnóstico traza (opcional) -->
  <!-- <add key="Diagnostics:PasoDatosMaxChars"         value="8192" /> -->
</appSettings>

<connectionStrings>
  <add name="MSGestion"
       connectionString="Server=...;Database=...;..."
       providerName="System.Data.SqlClient" />
  <add name="MSFwk"
       connectionString="Server=...;Database=...;..."
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

> **Nota de deploy:** antes de pasar a producción, descomentar las claves `HubSpot:*`, completar `EmailErrPara` y `EmailErrCc`, y cambiar `HubSpot:UseDevelopmentMock` a `false`.

---

## 9. Consola Web MVC (Home)

Botones manuales para testing y operaciones de soporte:

| Botón | Acción | Endpoint MVC |
|---|---|---|
| Procesar Cola HubSpot | Ejecuta `ProcesarColaIntegracionesHubSpotJob` | `POST /Home/ProcesarColaHubSpot` |
| Ver Cola (traza) | Muestra conteos y muestra Pendientes | `POST /Home/ProcesarColaHubSpotTrazaCola` |
| Procesar Cliente (traza) | SP 004 para un ClienteId | `POST /Home/ProcesarColaHubSpotTrazaCliente?clienteId=N` |
| Cuenta Corriente Batch | Ejecuta `HubSpotSincronizarCuentaCorrienteJob` | `POST /Home/HubSpotCuentaCorrienteBatch` |

Lista completa trazas: [`reference/consola-mvc.md`](reference/consola-mvc.md).

---

## 10. EmailsManager — Notificaciones de Error

Se conserva el `EmailsManager` existente. Se dispara en **ambos flujos ante cualquier error**, usando las claves SMTP ya configuradas en `Web.config`.

### Cuándo se envía

| Situación | Flujo | Asunto sugerido |
|---|---|---|
| Fallo al procesar un cliente de la cola (cualquier paso) | 2A | `[HubSpot 2A] Error al sincronizar cliente {ClienteId}` |
| Error de autenticación HubSpot (401) | 2A y 2B | `[HubSpot] Error de autenticación — job detenido` |
| Error de rate limit agotado (429/5xx, tras reintentos HTTP) | 2A y 2B | `[HubSpot 2A] Error ...` / `[HubSpot 2B] Error batch lote N` |
| Fallo de un batch en Flujo 2B | 2B | `[HubSpot 2B] Error en batch cuenta corriente — lote N` |
| Fallo total del job (excepción no controlada) | 2A y 2B | `[HubSpot] Fallo crítico en job {NombreJob}` |

### Contenido del email

```
Proceso : {NombreJob}
Fecha   : {Timestamp}
Cliente : {ClienteId} (si aplica)
Error   : {Mensaje de excepción sanitizado}
Detalle : {Stack trace o contexto adicional}
```

### Configuración

Las claves `EmailErrDE`, `EmailErrPara` y `EmailErrCc` ya existen en `Web.config` y deben completarse en el deploy. Si `EmailErrPara` está vacío, el `EmailsManager` no envía (comportamiento ya existente en el proyecto).

---

## 11. Manejo de Errores — Resumen

| Situación | Comportamiento |
|---|---|
| Error en Flujo 2A (cualquier paso) | Marcar registro cola como `Error` con detalle. No reintentar automáticamente. |
| Cliente sin `HubSpotCompanyId` en Flujo 2B | Omitir, registrar en log como advertencia. |
| Error en batch de Flujo 2B | Registrar batch fallido en `ProcesosSpertaHubSpotLog`. Continuar con el siguiente batch. |
| Error de autenticación HubSpot (401) | Loguear, email `[HubSpot] Error autenticación`, detener job 2B. En 2A marcar fila Error y continuar siguiente ítem. |
| Error HTTP reintentable (429/500/502/503/504) | Reintentar hasta `HubSpot:MaxHttpRetries` (default 3). Cada fallo reintentable en 2A incrementa `Intentos`. Agotados → Error + email. |

### Configuración HTTP HubSpot (Web.config / App.config)

| Clave | Default | Descripción |
|---|---|---|
| `HubSpot:MaxHttpRetries` | 3 | Reintentos ante 429/5xx (401 nunca reintenta) |
| `HubSpot:HttpRetryBackoffMilliseconds` | 1000 | Espera entre reintentos HTTP |
| `HubSpot:DelayMillisecondsBetweenCalls` | 120 | Throttle entre llamadas CRM |

### Semántica `Intentos` (cola 2A)

| Evento | Incremento |
|---|---|
| Reclamar (Pendiente→EnProceso) | Intentos++ |
| HTTP reintentable fallido (429/5xx) | Intentos++ vía `IncrementarIntentos` |
| 401 | Solo incremento del reclamo |

---

## 12. Entidades (DTOs)

```csharp
// ClienteHubSpotDto — datos de compañía a enviar a HubSpot
public class ClienteHubSpotDto {
    public int    ClienteId         { get; set; }
    public string HubSpotCompanyId  { get; set; } // null si no sincronizado aún
    public string RazonSocial       { get; set; }
    public string NombreFantasia    { get; set; }
    public string CUIT              { get; set; }
    public string NroCliente        { get; set; }
    public string Calle             { get; set; }
    public string Puerta            { get; set; }
    public string Localidad         { get; set; }
    public string CodigoPostal      { get; set; }
    public string Provincia         { get; set; }
    public string Pais              { get; set; }
    public string ZonaVta           { get; set; }
    public string Vendedor          { get; set; }
    public string ResponsableCuenta { get; set; }
    public string ListaPrecios      { get; set; }
    public string CondicionVenta    { get; set; }
    public int?   DiasParaDeuda     { get; set; }
    public decimal? LimiteCredito   { get; set; }
    public string CategoriaCliente  { get; set; }
    // Domicilios entrega (hasta 3)
    public string Dir1Domicilio     { get; set; }
    public string Dir1CP            { get; set; }
    public string Dir1Localidad     { get; set; }
    public string Dir1Provincia     { get; set; }
    public string Dir2Domicilio     { get; set; }
    public string Dir2CP            { get; set; }
    public string Dir2Localidad     { get; set; }
    public string Dir3Domicilio     { get; set; }
    public string Dir3CP            { get; set; }
    public string Dir3Localidad     { get; set; }
}

// ContactoHubSpotDto
public class ContactoHubSpotDto {
    public string Nombre   { get; set; }
    public string Sector   { get; set; }
    public string Telefono { get; set; }
    public string Email    { get; set; } // clave de deduplicación
}

// CuentaCorrienteItemDto — una fila del SP de CC
public class CuentaCorrienteItemDto {
    public int     ClienteId        { get; set; }
    public string  HubSpotCompanyId { get; set; }
    public string  FechaVencimiento { get; set; } // "DD/MM/YYYY"
    public decimal? Monto           { get; set; } // null = sin deuda
}
```

---

## 13. Entregables y Scripts SQL

| Archivo | Descripción |
|---|---|
| `scriptsSQL/000_Deploy_All.sql` | Orquestador de despliegue (orden canónico) |
| `scriptsSQL/001_ProcesosSpertaHubSpot.sql` | Tabla cola outbox |
| `scriptsSQL/002_ProcesosSpertaHubSpotLog.sql` | Tabla log de ejecuciones |
| `scriptsSQL/003_USER_CALZETTA_POS_Clientes_Agregar.sql` | SP post-grabación WinForms (`USER_POS_Clientes_Agregar`) |
| `scriptsSQL/004_InterfazHubSpot_Cliente_Obtener.sql` | SP datos cliente — cabecera + direcciones (2A) |
| `scriptsSQL/005_InterfazHubSpot_Clientes_Contactos_Obtener.sql` | SP contactos cliente (2A) |
| `scriptsSQL/006_InterfazHubSpot_CuentaCorriente_Pagina.sql` | SP cuenta corriente paginada (2B) |
| `scriptsSQL/008_InterfazHubSpot_VendedoresHabilitados.sql` | Filtro vendedores en SP 004/006 |
| `scriptsSQL/009_Indices.sql` | Índices performance SPs |
| `scriptsSQL/010_InterfazHubSpot_Atributo_IdHubSpot.sql` | Atributo `id_hubspot` + SP `InterfazHubSpot_Cliente_GuardarIdHubSpot` |
| `scriptsSQL/012_ListadoHubSpotProcesosCola_Buscar.sql` | Listado cola (consola / soporte) |
| `scriptsSQL/013_Indices_ProcesosHubSpot_Listado.sql` | Índices listado cola |
| `sql/*` | Copias versionadas / alias históricos `USP_*` — preferir `scriptsSQL/` |

---

## 15. Convenciones de nombres y tooling para agentes

| Tema | Convención |
|------|------------|
| Solución | `InterfazHubSpot.sln` — 8 proyectos, sin proyecto conector suelto |
| Runners HubSpot | `InterfazHubSpot.Business/HubSpot/` → namespace `InterfazHubSpot.Business.HubSpot` |
| Tabla cola | `dbo.ProcesosSpertaHubSpot` (EF `ToTable`, SP `USER_POS_Clientes_Agregar`) |
| Prohibido | nombres legacy de solución/tabla/cola y variante HubSpot con s minúscula (ver regla Cursor) |

### Scripts canónicos (`scriptsPS1/`)

| Script | Uso |
|--------|-----|
| `Build-InterfazHubSpot.ps1` | `nuget restore` + MSBuild; `-LibrariesOnly` omite sitio MVC |
| `Test-InterfazHubSpot.ps1` | `dotnet test` por categoría Unit/Security/Integration; excluye `Category=Live` por defecto |
| `Measure-TestCoverage.ps1` | coverlet + gate line-rate ≥90% por categoría (`coverage-scopes.json`) |
| `Verify-InterfazHubSpot.ps1` | Build + Test + cobertura + grep legacy → 0 hits en `.cs`, `.sql`, `.md` |

### Gate WinForms (deploy BD)

Antes de `sp_rename` / migración `sql/001`: desplegar `sql/002_USER_POS_Clientes_Agregar.sql`, verificar SP activo en WinForms Calzetta, coordinar ventana de mantenimiento. Ver §5.1 y comentarios en `sql/001`.

Reglas Cursor: `.cursor/rules/interfaz-hubspot.mdc` (always-on). Comandos resumidos en `AGENTS.md`.

---

## 14. Puntos Abiertos / Pendientes de Confirmación

| # | Punto | Estado |
|---|---|---|
| 1 | Persistencia `HubSpotCompanyId` en ERP | **Implementado** — atributo `id_hubspot` + SP 010 `InterfazHubSpot_Cliente_GuardarIdHubSpot` |
| 2 | Campos SP datos cliente | **Implementado** — ver script 004 y mapper C# |
| 3 | Estructura cuenta corriente / paginación 2B | **Implementado** — script 006 con keyset pagination |
| 4 | Token PAT ambiente productivo vs sandbox | Confirmar con operaciones Calzetta / HubSpot portal |
