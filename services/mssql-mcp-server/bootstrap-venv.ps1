#Requires -Version 5.1
<#
.SYNOPSIS
    Crea .venv con Python 3.11+ e instala mssql-mcp-server segun requirements.txt.
.DESCRIPTION
    Ejecutar desde services/mssql-mcp-server/. Requiere Python >= 3.11 (PyPI metadata del paquete).
    Proximo paso: copiar .env.example a .env y MCP en Cursor.
#>
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$serviceRoot = $PSScriptRoot
Set-Location $serviceRoot

function Test-PythonVersionLine {
    param([Parameter(Mandatory)][string]$VersionLine)

    try {
        if ($VersionLine -match '^(\d+)\.(\d+)') {
            $major = [int]$Matches[1]
            $minor = [int]$Matches[2]
            return ($major -eq 3 -and $minor -ge 11)
        }
    }
    catch { }
    return $false
}

function Get-PythonExe311Plus {
    $found = $null

    if (Get-Command py.exe -ErrorAction SilentlyContinue) {
        foreach ($tag in @('3.14', '3.13', '3.12', '3.11')) {
            $combined = py "-$tag" -c "import sys
if sys.version_info < (3, 11):
    sys.exit(1)
sys.stdout.write(sys.executable)
" 2>$null

            if ($LASTEXITCODE -ne 0) { continue }
            $combined = ([string]$combined).Trim()
            if ($combined -and (Test-Path -LiteralPath $combined)) {
                $found = $combined
                break
            }
        }
    }

    if (-not $found) {
        $pyPaths = @(
            Join-Path $env:LOCALAPPDATA 'Programs\Python\Python314\python.exe',
            Join-Path $env:LOCALAPPDATA 'Programs\Python\Python313\python.exe',
            Join-Path $env:LOCALAPPDATA 'Programs\Python\Python312\python.exe',
            Join-Path $env:LOCALAPPDATA 'Programs\Python\Python311\python.exe',
            Join-Path $env:ProgramFiles 'Python311\python.exe',
            Join-Path $env:ProgramFiles 'Python312\python.exe',
            Join-Path $env:ProgramFiles 'Python313\python.exe'
        )
        foreach ($p in $pyPaths) {
            if (-not (Test-Path -LiteralPath $p)) { continue }
            $verLine = & $p -c "import sys; print('{}.{}'.format(sys.version_info.major, sys.version_info.minor))" 2>$null
            if ($LASTEXITCODE -ne 0) { continue }
            if (-not (Test-PythonVersionLine -VersionLine $verLine.Trim())) { continue }
            $found = $p
            break
        }
    }

    if (-not $found -and (Get-Command python.exe -ErrorAction SilentlyContinue)) {
        $verLine = & python.exe -c "import sys; print('{}.{}'.format(sys.version_info.major, sys.version_info.minor))" 2>$null
        if ($LASTEXITCODE -eq 0 -and (Test-PythonVersionLine -VersionLine $verLine.Trim())) {
            $exeLine = & python.exe -c "import sys; print(sys.executable)" 2>$null
            if ($LASTEXITCODE -eq 0) { $found = $exeLine.Trim() }
        }
    }

    return $found
}

$pyExe = Get-PythonExe311Plus
if (-not $pyExe) {
    $msg = @'
No se encontro Python 3.11 o superior.

El paquete mssql-mcp-server lo exige (ver https://pypi.org/project/mssql-mcp-server/).

Instalacion en Windows (elige una):

  winget install Python.Python.3.11

Instalador oficial:

  https://www.python.org/downloads/

Marca la opcion "Add python.exe to PATH" y el py launcher cuando el instalador lo ofrezca.

Cierra la terminal abre una nueva y prueba:

  py -3.11 --version

Vuelve a ejecutar bootstrap-venv.ps1
'@
    Write-Error $msg
    exit 1
}

$venvPath = Join-Path $serviceRoot '.venv'
& $pyExe -m venv $venvPath
if ($LASTEXITCODE -ne 0) {
    Write-Error "python -m venv fallo (codigo de salida $LASTEXITCODE)."
    exit 1
}

$pip = Join-Path $venvPath 'Scripts/pip.exe'
# pip puede escribir WARNING en stderr; con ErrorActionPreference=Stop algunos hosts lo interpretan como error.
$savedEap = $ErrorActionPreference
$ErrorActionPreference = 'Continue'
try {
    & $pip install --upgrade pip
    if ($LASTEXITCODE -ne 0) {
        Write-Error "pip install --upgrade pip fallo (codigo $LASTEXITCODE)."
        exit 1
    }

    & $pip install -r (Join-Path $serviceRoot 'requirements.txt')
    if ($LASTEXITCODE -ne 0) {
        Write-Error "pip install -r requirements.txt fallo (codigo $LASTEXITCODE)."
        exit 1
    }
}
finally {
    $ErrorActionPreference = $savedEap
}
Write-Host 'Listo.' -ForegroundColor Green
Write-Host "  Python : $pyExe"
Write-Host "  venv   : $venvPath"
Write-Host '  Proximo paso: copy .env.example .env ; ver docs/how-to/mcp-mssql-desarrollo-cursor.md'
