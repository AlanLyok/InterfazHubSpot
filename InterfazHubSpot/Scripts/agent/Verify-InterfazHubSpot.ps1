#Requires -Version 5.1
param(
    [switch]$LibrariesOnly,
    [switch]$SkipBuild,
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
$agentDir = $PSScriptRoot

if (-not $SkipBuild) {
    $buildArgs = @()
    if ($LibrariesOnly) { $buildArgs += '-LibrariesOnly' }
    & (Join-Path $agentDir 'Build-InterfazHubSpot.ps1') @buildArgs
}

if (-not $SkipTests) {
    & (Join-Path $agentDir 'Test-InterfazHubSpot.ps1')
}

$patterns = @('BatchSpertaAPI', 'ProcesosSpertaAPI', 'Hubspot')
$extensions = @('*.cs', '*.sql', '*.md')
$hits = @()

foreach ($ext in $extensions) {
    Get-ChildItem -Path $repoRoot -Recurse -Filter $ext -File |
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

Write-Host 'Verify-InterfazHubSpot: OK (build + tests + grep legacy=0)'
