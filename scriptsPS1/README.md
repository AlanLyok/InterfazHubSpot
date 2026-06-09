# Scripts PowerShell — InterfazHubSpot

Comandos canónicos desde la **raíz del repo**. Requieren **PowerShell 7+** (`pwsh`).

## Build

```powershell
pwsh -NoProfile -File scriptsPS1/Build-InterfazHubSpot.ps1
pwsh -NoProfile -File scriptsPS1/Build-InterfazHubSpot.ps1 -LibrariesOnly
```

| Parámetro | Descripción |
|-----------|-------------|
| `-LibrariesOnly` | Omite el sitio MVC; compila Business, tests y BatchProcess si hay DLL Scheduler |

Variables opcionales: `SPERTA_MSBUILD`, `MSBUILD_EXE`, `SPERTA_NUGET_EXE`.

## Tests

```powershell
pwsh -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1
pwsh -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1 -Category Security
pwsh -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1 -Category Integration
pwsh -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1 -Category All
pwsh -NoProfile -File scriptsPS1/Test-InterfazHubSpot.ps1 -Filter "FullyQualifiedName~HubSpotHttp"
```

| Parámetro | Descripción |
|-----------|-------------|
| `-Category` | `Unit` (default), `Security`, `Integration`, `All` |
| `-Filter` | Expresión xUnit pasada a `dotnet test` (ambos proyectos) |
| `-IncludeLive` | Incluye tests `Category=Live` (requiere MSGestion real) |
| `-SkipBuild` | No compila antes de testear |

## Cobertura (coverlet)

```powershell
pwsh -NoProfile -File scriptsPS1/Measure-TestCoverage.ps1
pwsh -NoProfile -File scriptsPS1/Measure-TestCoverage.ps1 -Threshold 90
```

Mide line-rate por categoría según [`coverage-scopes.json`](coverage-scopes.json). Salida en `scriptsPS1/coverage/` (gitignored). Falla si alguna categoría queda bajo el umbral.

| Parámetro | Descripción |
|-----------|-------------|
| `-Threshold` | Override global; default = valor en JSON por categoría (90) |
| `-IncludeLive` | Incluye Live en la corrida de integración |
| `-SkipBuild` | Omite build previo |

## Verificación completa

```powershell
pwsh -NoProfile -File scriptsPS1/Verify-InterfazHubSpot.ps1
pwsh -NoProfile -File scriptsPS1/Verify-InterfazHubSpot.ps1 -LibrariesOnly -SkipCoverage
```

Build + tests (Unit + Security + Integration) + cobertura ≥90% + grep legacy bloqueado.

| Parámetro | Descripción |
|-----------|-------------|
| `-LibrariesOnly` | Build sin MVC |
| `-SkipBuild` / `-SkipTests` / `-SkipCoverage` | Saltar pasos |
| `-IncludeLive` | Tests y cobertura Live |

Documentación de suites: [`docs/TESTING.md`](../docs/TESTING.md). Índice: [`docs/README.md`](../docs/README.md). Inicio rápido: [`docs/QUICKSTART.md`](../docs/QUICKSTART.md).
