#Requires -Version 5.1
param(
    [switch]$LibrariesOnly,
    [switch]$SkipBuild,
    [switch]$SkipTests,
    [switch]$SkipCoverage,
    [switch]$IncludeLive
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\SolucionInterfazHubSpot')).Path
$scriptsDir = $PSScriptRoot
$schedulerDll = Join-Path $repoRoot 'Componentes\Mastersoft.Scheduler452.Intefaces.dll'

if (-not $SkipBuild) {
    $useLibrariesOnly = $LibrariesOnly -or -not (Test-Path $schedulerDll)
    if (-not (Test-Path $schedulerDll)) {
        Write-Warning 'Componentes sin Scheduler DLL; verify usa -LibrariesOnly.'
    }
    if ($useLibrariesOnly) {
        & (Join-Path $scriptsDir 'Build-InterfazHubSpot.ps1') -LibrariesOnly
    } else {
        & (Join-Path $scriptsDir 'Build-InterfazHubSpot.ps1')
    }
}

if (-not $SkipTests) {
    $testArgs = @{ Category = 'All' }
    if ($IncludeLive) { $testArgs['IncludeLive'] = $true }
    $testArgs['SkipBuild'] = $true
    & (Join-Path $scriptsDir 'Test-InterfazHubSpot.ps1') @testArgs
}

if (-not $SkipCoverage) {
    $covArgs = @{ SkipBuild = $true }
    if ($IncludeLive) { $covArgs['IncludeLive'] = $true }
    & (Join-Path $scriptsDir 'Measure-TestCoverage.ps1') @covArgs
}

$patterns = @('BatchSpertaAPI', 'ProcesosSpertaAPI', 'Hubspot')
$extensions = @('*.cs', '*.sql', '*.md')
$hits = @()

foreach ($ext in $extensions) {
    Get-ChildItem -Path (Join-Path $PSScriptRoot '..') -Recurse -Filter $ext -File |
        Where-Object {
            $_.FullName -notmatch '\\\.git\\' -and
            $_.FullName -notmatch '\\obj\\' -and
            $_.FullName -notmatch '\\bin\\' -and
            $_.FullName -notmatch '\\packages\\' -and
            $_.FullName -notmatch '\\\.cursor\\plans\\' -and
            $_.Name -ne '001_ProcesosSpertaHubSpot.sql'
        } |
        ForEach-Object {
            $content = Get-Content -LiteralPath $_.FullName -Raw -ErrorAction SilentlyContinue
            if (-not $content) { return }
            foreach ($pat in $patterns) {
                $matched = if ($pat -eq 'Hubspot') {
                    $content -cmatch 'Hubspot'
                } else {
                    $content -match [regex]::Escape($pat)
                }
                if ($matched) {
                    $hits += [pscustomobject]@{ File = $_.FullName; Pattern = $pat }
                }
            }
        }
}

if ($hits.Count -gt 0) {
    $hits | Format-Table -AutoSize
    throw "Verify-InterfazHubSpot: $($hits.Count) referencias legacy encontradas."
}

$patternesBlockedCs = @(
    'SpertaApi',
    'HttpSpertaApiClient',
    'ISpertaApiClient',
    'TracingSpertaApiClient',
    'MSFwk',
    'SpertaFwk',
    'EjemploSpertaApiJob'
)
$thisScript = $MyInvocation.MyCommand.Path
$csHits = @()

Get-ChildItem -Path $repoRoot -Recurse -Filter '*.cs' -File |
    Where-Object {
        $_.FullName -notmatch '\\\.git\\' -and
        $_.FullName -notmatch '\\obj\\' -and
        $_.FullName -notmatch '\\bin\\' -and
        $_.FullName -notmatch '\\packages\\' -and
        $_.FullName -notmatch '\\sql\\' -and
        $_.FullName -notmatch '\\docs\\' -and
        $_.FullName -ne $thisScript
    } |
    ForEach-Object {
        $lines = Get-Content -LiteralPath $_.FullName -ErrorAction SilentlyContinue
        if (-not $lines) { return }
        $lineNum = 0
        foreach ($line in $lines) {
            $lineNum++
            foreach ($pat in $patternesBlockedCs) {
                if ($line -cmatch [regex]::Escape($pat)) {
                    $csHits += [pscustomobject]@{
                        File    = $_.FullName
                        Line    = $lineNum
                        Pattern = $pat
                        Content = $line.Trim()
                    }
                }
            }
        }
    }

if ($csHits.Count -gt 0) {
    Write-Host 'ERROR: Referencias bloqueadas encontradas en codigo fuente C#:' -ForegroundColor Red
    $csHits | Format-Table -AutoSize
    throw "Verify-InterfazHubSpot: $($csHits.Count) referencias bloqueadas (SpertaApi/MSFwk/SpertaFwk) en archivos .cs."
}

Write-Host 'OK Sin referencias legacy Sperta/MSFwk en archivos .cs' -ForegroundColor Green
Write-Host 'Verify-InterfazHubSpot: OK (build + tests + coverage + grep legacy=0)'
