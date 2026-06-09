#Requires -Version 5.1
param(
    [switch]$LibrariesOnly
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\SolucionInterfazHubSpot')).Path
$sln = Join-Path $repoRoot 'InterfazHubSpot.sln'

function Get-MsBuildExe {
    if ($env:SPERTA_MSBUILD -and (Test-Path $env:SPERTA_MSBUILD)) { return $env:SPERTA_MSBUILD }
    if ($env:MSBUILD_EXE -and (Test-Path $env:MSBUILD_EXE)) { return $env:MSBUILD_EXE }
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $msb = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
        if ($msb -and (Test-Path $msb)) { return $msb }
    }
    throw 'MSBuild no encontrado. Configure SPERTA_MSBUILD o MSBUILD_EXE.'
}

function Invoke-NuGetRestore {
    $nuget = $env:SPERTA_NUGET_EXE
    if (-not $nuget -or -not (Test-Path $nuget)) {
        $nuget = Get-Command nuget -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
    }
    if ($nuget) {
        & $nuget restore $sln
        if ($LASTEXITCODE -ne 0) { throw "nuget restore fallo ($LASTEXITCODE)" }
    } else {
        Write-Warning 'nuget no encontrado; omitiendo restore.'
    }
}

Invoke-NuGetRestore
$msbuild = Get-MsBuildExe

$schedulerDll = Join-Path $repoRoot 'Componentes\Mastersoft.Scheduler452.Intefaces.dll'
$batchProcessAvailable = Test-Path $schedulerDll
if (-not $batchProcessAvailable) {
    Write-Warning "Omitiendo InterfazHubSpot.BatchProcess: falta $schedulerDll"
}

if ($LibrariesOnly) {
    $projects = @(
        'InterfazHubSpot.Business\InterfazHubSpot.Business.csproj',
        'InterfazHubSpot.Entities\InterfazHubSpot.Entities.csproj',
        'InterfazHubSpot.Interfaces\InterfazHubSpot.Interfaces.csproj',
        'InterfazHubSpot.Mapping\InterfazHubSpot.Mapping.csproj',
        'InterfazHubSpot.Tests.Unit\InterfazHubSpot.Tests.Unit.csproj',
        'InterfazHubSpot.IntegrationTests\InterfazHubSpot.IntegrationTests.csproj'
    )
    if ($batchProcessAvailable) {
        $projects = @(
            'InterfazHubSpot.BatchProcess\InterfazHubSpot.BatchProcess.csproj'
        ) + $projects
    }
    foreach ($p in $projects) {
        $path = Join-Path $repoRoot $p
        & $msbuild $path /p:Configuration=Debug /v:m
        if ($LASTEXITCODE -ne 0) { throw "MSBuild fallo: $p" }
    }
} else {
    if ($batchProcessAvailable) {
        & $msbuild $sln /p:Configuration=Debug /v:m
        if ($LASTEXITCODE -ne 0) { throw 'MSBuild fallo en solucion' }
    } else {
        Write-Warning 'Build sin BatchProcess: copie DLLs Mastersoft en Componentes/ para compilar jobs IScheduler.'
        $projects = @(
            'InterfazHubSpot.Business\InterfazHubSpot.Business.csproj',
            'InterfazHubSpot.Entities\InterfazHubSpot.Entities.csproj',
            'InterfazHubSpot.Interfaces\InterfazHubSpot.Interfaces.csproj',
            'InterfazHubSpot.Mapping\InterfazHubSpot.Mapping.csproj',
            'InterfazHubSpot\InterfazHubSpot.csproj',
            'InterfazHubSpot.Tests.Unit\InterfazHubSpot.Tests.Unit.csproj',
            'InterfazHubSpot.IntegrationTests\InterfazHubSpot.IntegrationTests.csproj'
        )
        foreach ($p in $projects) {
            $path = Join-Path $repoRoot $p
            & $msbuild $path /p:Configuration=Debug /v:m
            if ($LASTEXITCODE -ne 0) { throw "MSBuild fallo: $p" }
        }
    }
}

Write-Host 'Build-InterfazHubSpot: OK'
