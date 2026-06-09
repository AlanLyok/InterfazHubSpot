#Requires -Version 5.1
param(
    [switch]$IncludeLive
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path

$filterArgs = if ($IncludeLive) { @() } else { @('--filter', 'Category!=Live') }

$projects = @(
    'InterfazHubSpot.Tests.Unit\InterfazHubSpot.Tests.Unit.csproj',
    'InterfazHubSpot.IntegrationTests\InterfazHubSpot.IntegrationTests.csproj'
)

foreach ($p in $projects) {
    $path = Join-Path $repoRoot $p
    & dotnet test $path @filterArgs --no-restore
    if ($LASTEXITCODE -ne 0) { throw "dotnet test falló: $p" }
}

Write-Host 'Test-InterfazHubSpot: OK'
