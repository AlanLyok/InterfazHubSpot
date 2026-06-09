# Inicio rápido — InterfazHubSpot

Guía mínima para clonar, compilar y verificar el proyecto en Windows.

## Requisitos

- Windows con **MSBuild** (Visual Studio 2019+ o Build Tools)
- **PowerShell 7+** (`pwsh`) recomendado; también funciona **Windows PowerShell 5.1** con `powershell.exe`
- **.NET SDK** (para `dotnet test`)
- SQL Server con `MSGestion` y scripts en [`scriptsSQL/`](../scriptsSQL/) (solo para runtime Live/MVC con BD real)
- Token HubSpot Private App — **no versionar**; usar plantillas `Web.config.example`

## Pasos

### 1. Clonar y configurar

```powershell
git clone https://github.com/AlanLyok/InterfazHubSpot.git
cd InterfazHubSpot
copy SolucionInterfazHubSpot\Web.config.example SolucionInterfazHubSpot\InterfazHubSpot\Web.config
# Editar Web.config: connectionString MSGestion + HubSpot:PrivateAppToken
# En dev local: HubSpot:UseDevelopmentMock=true evita llamadas reales a HubSpot
```

### 2. Compilar

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scriptsPS1/Build-InterfazHubSpot.ps1
```

Solo librerías y tests (sin sitio MVC):

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scriptsPS1/Build-InterfazHubSpot.ps1 -LibrariesOnly
```

### 3. Tests

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scriptsPS1/Test-InterfazHubSpot.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scriptsPS1/Test-InterfazHubSpot.ps1 -Category Security
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scriptsPS1/Test-InterfazHubSpot.ps1 -Category Integration
```

### 4. Verificación completa (recomendado antes de PR)

Build + tests + cobertura ≥90% + grep legacy:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scriptsPS1/Verify-InterfazHubSpot.ps1
```

### 5. Consola MVC (desarrollo)

Abrir `SolucionInterfazHubSpot/InterfazHubSpot.sln` en Visual Studio, ejecutar el proyecto web y usar la Home para jobs manuales y trazas JSON.

| Acción | Endpoint |
|--------|----------|
| Job 2A | `POST /Home/ProcesarColaHubSpot` |
| Job 2B | `POST /Home/HubSpotCuentaCorrienteBatch` |
| Traza cola | `POST /Home/ProcesarColaHubSpotTrazaCola` |

## Servicio Windows (producción)

Ver [`implementacion/README.md`](../implementacion/README.md) y [`docs/BatchProcess_Desarrollo_e_Implementacion.md`](BatchProcess_Desarrollo_e_Implementacion.md).

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File implementacion/Deploy-ServicioHubSpot.ps1
```

## Más información

**Índice maestro:** [`docs/README.md`](README.md)

| Documento | Contenido |
|-----------|-----------|
| [`README.md`](../README.md) | Visión general del repo |
| [`docs/TESTING.md`](TESTING.md) | Suites, categorías xUnit, cobertura coverlet |
| [`scriptsPS1/README.md`](../scriptsPS1/README.md) | Referencia de parámetros PS1 |
| [`docs/PRD_Integracion_HubSpot_2A_2B.md`](PRD_Integracion_HubSpot_2A_2B.md) | Requisitos funcionales |
| [`docs/agents/INDEX.md`](agents/INDEX.md) | Enrutamiento para agentes AI |

Variables opcionales de build: `SPERTA_MSBUILD`, `MSBUILD_EXE`, `SPERTA_NUGET_EXE`.
