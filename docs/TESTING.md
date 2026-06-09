# Testing — InterfazHubSpot

Referencia de suites xUnit, categorías y gate de cobertura automatizada.

> **Índice:** [`README.md`](README.md)

## Proyectos

| Proyecto | Ubicación | Rol |
|----------|-----------|-----|
| `InterfazHubSpot.Tests.Unit` | `SolucionInterfazHubSpot/` | Lógica de negocio con HTTP/SQL mockeado o sin I/O |
| `InterfazHubSpot.IntegrationTests` | `SolucionInterfazHubSpot/` | Contratos de integración sin BD y tests Live opcionales |

## Categorías xUnit (`Trait("Category", ...)`)

| Categoría | Filtro default | Qué cubre |
|-----------|----------------|-----------|
| **Unit** | `Category!=Live&Category!=Security&Category!=Integration` | HubSpot CRM client, mapper, cola, diagnósticos |
| **Security** | `Category=Security` | Token HubSpot, 401 fail-fast, sanitizado SQL, MSSecurity, notificaciones auth |
| **Integration** | `Category=Integration` | Mapper SP→DTO, proyección cola, contratos Managers estáticos |
| **Live** | `Category=Live` (excluido por defecto) | Managers EF6 contra MSGestion real |

Los tests **Live** están en `IntegrationTests/Managers/*LiveTests.cs` con `Fact(Skip=...)` hasta tener `App.config` con MSGestion y referencia Mastersoft activa.

## Comandos

Desde la raíz del repo. Detalle de parámetros: [`scriptsPS1/README.md`](../scriptsPS1/README.md).

```powershell
# Unitarios (default)
powershell.exe -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1

# Por categoría
powershell.exe -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1 -Category Security
powershell.exe -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1 -Category Integration
powershell.exe -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1 -Category All

# Filtro xUnit libre
powershell.exe -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1 -Filter "FullyQualifiedName~HubSpotHttp"

# Incluir Live (requiere BD)
powershell.exe -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1 -IncludeLive
```

## Cobertura (coverlet)

El script `Measure-TestCoverage.ps1` ejecuta cada categoría con **coverlet.msbuild** y valida **line-rate ≥ 90%** en ámbitos definidos en [`scriptsPS1/coverage-scopes.json`](../scriptsPS1/coverage-scopes.json).

```powershell
powershell.exe -NoProfile -File scriptsPS1/Measure-TestCoverage.ps1
powershell.exe -NoProfile -File scriptsPS1/Verify-InterfazHubSpot.ps1
```

| Categoría | Ámbito medido (clases Business) |
|-----------|----------------------------------|
| unit | `HubSpotCrmClient`, `HubSpotConfiguration`, integración cola/mapper, diagnósticos, excepciones HTTP |
| security | `HubSpotConfiguration`, `IntegracionErrorNotifier`, `MSSecurity`, `HubSpotAuthException` |
| integration | `ClienteIntegracionMapper`, `ColaIntegracionPendienteMuestra` |

Salida XML en `scriptsPS1/coverage/` (gitignored). El gate falla si alguna categoría queda bajo el umbral.

### Medición unit vs ejecución unit

La medición de cobertura **unit** usa filtro `Category!=Live&Category!=Integration` (incluye tests Security) para reflejar código compartido. La ejecución `-Category Unit` excluye Security e Integration para corridas rápidas aisladas.

## CI / agentes

Verificación canónica antes de merge:

```powershell
powershell.exe -NoProfile -File scriptsPS1/Verify-InterfazHubSpot.ps1 -LibrariesOnly
```

Incluye grep de nombres legacy bloqueados (ver regla `.cursor/rules/interfaz-hubspot.mdc` y script `Verify-InterfazHubSpot.ps1`).

## Estructura de carpetas de tests

```
InterfazHubSpot.Tests.Unit/
  HubSpot/           # CRM client, retry, runner payload
  Integration/       # Cola, error notifier
  Diagnostics/       # ErpConnectivityProbe, reporters
  Managers/          # EmailsManager, ClienteIntegracionMapper
  Security/          # Token, MSSecurity, auth exceptions

InterfazHubSpot.IntegrationTests/
  Managers/          # Contratos mapper, proyección cola, Live (skipped)
  BusinessAssemblySmokeTests.cs
```
