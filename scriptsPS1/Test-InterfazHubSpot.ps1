#Requires -Version 5.1
param(
    [ValidateSet('Unit', 'Security', 'Integration', 'All')]
    [string]$Category = 'Unit',
    [string]$Filter,
    [switch]$IncludeLive,
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\SolucionInterfazHubSpot')).Path
$scriptsDir = $PSScriptRoot

if (-not $SkipBuild) {
    & (Join-Path $scriptsDir 'Build-InterfazHubSpot.ps1') -LibrariesOnly
}

function Get-CategoryFilter {
    param(
        [string]$Cat,
        [bool]$Live
    )

    switch ($Cat) {
        'Unit' { return 'Category!=Live&Category!=Security&Category!=Integration' }
        'Security' { return 'Category=Security' }
        'Integration' {
            if ($Live) { return 'Category=Integration|Category=Live' }
            return 'Category=Integration'
        }
        default { return $null }
    }
}

function Invoke-TestProject {
    param(
        [string]$ProjectRelativePath,
        [string]$TestFilter
    )

    $path = Join-Path $repoRoot $ProjectRelativePath
    $args = @('test', $path, '--no-restore')
    if ($TestFilter) {
        $args += @('--filter', $TestFilter)
    }

    Write-Host "dotnet test $ProjectRelativePath --filter '$TestFilter'"
    & dotnet @args
    if ($LASTEXITCODE -ne 0) { throw "dotnet test fallo: $ProjectRelativePath" }
}

$unitProject = 'InterfazHubSpot.Tests.Unit\InterfazHubSpot.Tests.Unit.csproj'
$integrationProject = 'InterfazHubSpot.IntegrationTests\InterfazHubSpot.IntegrationTests.csproj'

if ($Filter) {
    Invoke-TestProject -ProjectRelativePath $unitProject -TestFilter $Filter
    Invoke-TestProject -ProjectRelativePath $integrationProject -TestFilter $Filter
    Write-Host 'Test-InterfazHubSpot: OK'
    return
}

switch ($Category) {
    'Unit' {
        $f = Get-CategoryFilter -Cat 'Unit' -Live:$IncludeLive
        Invoke-TestProject -ProjectRelativePath $unitProject -TestFilter $f
    }
    'Security' {
        $f = Get-CategoryFilter -Cat 'Security' -Live:$IncludeLive
        Invoke-TestProject -ProjectRelativePath $unitProject -TestFilter $f
    }
    'Integration' {
        $f = Get-CategoryFilter -Cat 'Integration' -Live:$IncludeLive
        Invoke-TestProject -ProjectRelativePath $integrationProject -TestFilter $f
    }
    'All' {
        $unitFilter = if ($IncludeLive) { 'Category!=Security&Category!=Integration' } else { 'Category!=Live&Category!=Security&Category!=Integration' }
        Invoke-TestProject -ProjectRelativePath $unitProject -TestFilter $unitFilter
        Invoke-TestProject -ProjectRelativePath $unitProject -TestFilter 'Category=Security'
        $integrationFilter = if ($IncludeLive) { 'Category=Integration|Category=Live' } else { 'Category=Integration' }
        Invoke-TestProject -ProjectRelativePath $integrationProject -TestFilter $integrationFilter
    }
}

Write-Host 'Test-InterfazHubSpot: OK'
