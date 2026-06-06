#Requires -Version 5.1
param(
    [switch]$IncludeLive
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path

$filter = if ($IncludeLive) { '' } else { '--filter "Category!=Live"' }

$projects = @(
    'InterfazHubSpot.Tests.Unit\InterfazHubSpot.Tests.Unit.csproj',
    'InterfazHubSpot.IntegrationTests\InterfazHubSpot.IntegrationTests.csproj'
)

foreach ($p in $projects) {
    $path = Join-Path $repoRoot $p
    if ($filter) {
        dotnet test $path $filter --no-restore
    } else {
        dotnet test $path --no-restore
    }
    if ($LASTEXITCODE -ne 0) { throw "dotnet test falló: $p" }
}

Write-Host 'Test-InterfazHubSpot: OK'
