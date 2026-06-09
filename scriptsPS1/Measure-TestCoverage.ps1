#Requires -Version 5.1
param(
    [int]$Threshold = 0,
    [switch]$IncludeLive,
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'
$scriptsDir = $PSScriptRoot
$repoRoot = (Resolve-Path (Join-Path $scriptsDir '..\SolucionInterfazHubSpot')).Path
$coverageRoot = Join-Path $scriptsDir 'coverage'
$scopesPath = Join-Path $scriptsDir 'coverage-scopes.json'

if (-not (Test-Path $scopesPath)) {
    throw "No se encontro $scopesPath"
}

$scopesConfig = Get-Content -LiteralPath $scopesPath -Raw | ConvertFrom-Json

function Get-CoverageFilter {
    param(
        [string]$CategoryName,
        [bool]$Live
    )

    switch ($CategoryName) {
        'unit' { return 'Category!=Live&Category!=Integration' }
        'security' { return 'Category=Security' }
        'integration' {
            if ($Live) { return 'Category=Integration|Category=Live' }
            return 'Category=Integration'
        }
        default { throw "Categoria desconocida: $CategoryName" }
    }
}

function Get-TestProject {
    param([string]$CategoryName)

    if ($CategoryName -eq 'integration') {
        return Join-Path $repoRoot 'InterfazHubSpot.IntegrationTests\InterfazHubSpot.IntegrationTests.csproj'
    }
    return Join-Path $repoRoot 'InterfazHubSpot.Tests.Unit\InterfazHubSpot.Tests.Unit.csproj'
}

function Find-CoberturaFile {
    param([string]$SearchRoot)

    if (-not (Test-Path $SearchRoot)) { return $null }

    $found = Get-ChildItem -Path $SearchRoot -Recurse -Filter 'coverage.cobertura.xml' -File -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    if ($found) { return $found.FullName }

    $foundAlt = Get-ChildItem -Path $SearchRoot -Recurse -Filter '*.cobertura.xml' -File -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    if ($foundAlt) { return $foundAlt.FullName }

    return $null
}

function Test-ClassInScope {
    param(
        $Scope,
        [string]$ClassName,
        [string]$FileName
    )

    $shortName = ($ClassName -split '\.')[-1]

    if ($Scope.includeClasses) {
        foreach ($inc in $Scope.includeClasses) {
            if ($ClassName -like "*.$inc" -or $shortName -eq $inc) {
                return $true
            }
        }
    }

    if ($Scope.includePaths) {
        $normalizedFile = ($FileName -replace '\\', '/')
        foreach ($path in $Scope.includePaths) {
            $pathNorm = ($path -replace '\\', '/').TrimEnd('/')
            if ($normalizedFile -like "*$pathNorm*") {
                return $true
            }
            $segment = ($pathNorm -split '/')[-1]
            if ($ClassName -like "*.$segment*" -or $ClassName -like "*.$segment") {
                return $true
            }
        }
    }

    return $false
}

function Get-ScopedCoverage {
    param(
        [string]$CoberturaPath,
        $Scope
    )

    [xml]$xml = Get-Content -LiteralPath $CoberturaPath
    $linesValid = 0
    $linesCovered = 0

    foreach ($pkg in $xml.coverage.packages.package) {
        $asmName = [string]$pkg.name
        $asmMatch = $false
        foreach ($a in $Scope.assemblies) {
            if ($asmName -like "*$a*") { $asmMatch = $true; break }
        }
        if (-not $asmMatch) { continue }

        foreach ($cls in $pkg.classes.class) {
            $className = [string]$cls.name
            $fileName = [string]$cls.filename
            if (-not (Test-ClassInScope -Scope $Scope -ClassName $className -FileName $fileName)) {
                continue
            }

            if ($cls.lines -and $cls.lines.line) {
                foreach ($line in $cls.lines.line) {
                    $linesValid++
                    if ([int]$line.hits -gt 0) {
                        $linesCovered++
                    }
                }
            }
        }
    }

    if ($linesValid -eq 0) {
        return [pscustomobject]@{
            LinesValid   = 0
            LinesCovered = 0
            Percent      = 0.0
        }
    }

    $pct = [math]::Round(100.0 * $linesCovered / $linesValid, 2)
    return [pscustomobject]@{
        LinesValid   = $linesValid
        LinesCovered = $linesCovered
        Percent      = $pct
    }
}

function Invoke-CategoryCoverage {
    param(
        [string]$CategoryName,
        $Scope,
        [bool]$Live,
        [bool]$BuildFirst
    )

    if ($BuildFirst) {
        & (Join-Path $scriptsDir 'Build-InterfazHubSpot.ps1') -LibrariesOnly
        $BuildFirst = $false
    }

    $outDir = Join-Path $coverageRoot $CategoryName
    if (Test-Path $outDir) {
        Remove-Item -LiteralPath $outDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $outDir -Force | Out-Null

    $project = Get-TestProject -CategoryName $CategoryName
    $filter = Get-CoverageFilter -CategoryName $CategoryName -Live:$Live
    $threshold = if ($Threshold -gt 0) { $Threshold } else { [int]$Scope.threshold }

    Write-Host ""
    Write-Host "=== Cobertura: $CategoryName (umbral ${threshold}%) ===" -ForegroundColor Cyan
    Write-Host "Proyecto: $project"
    Write-Host "Filtro:   $filter"

    & dotnet restore $project 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "dotnet restore fallo para categoria $CategoryName" }

    $coverletBase = Join-Path $outDir 'coverage'
    & dotnet test $project `
        --filter $filter `
        "/p:CollectCoverage=true" `
        "/p:CoverletOutput=$coverletBase" `
        "/p:CoverletOutputFormat=cobertura" `
        "/p:Include=[InterfazHubSpot.Business]*" 2>&1 | Out-Null

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test fallo para categoria $CategoryName"
    }

    $cobertura = Find-CoberturaFile -SearchRoot $outDir
    if (-not $cobertura) {
        $projDir = Split-Path $project -Parent
        $cobertura = Find-CoberturaFile -SearchRoot $projDir
    }
    if (-not $cobertura) {
        throw "No se encontro coverage.cobertura.xml para categoria $CategoryName"
    }

    $stats = Get-ScopedCoverage -CoberturaPath $cobertura -Scope $Scope
    $passed = $stats.Percent -ge $threshold

    return [pscustomobject]@{
        Category     = $CategoryName
        LinesValid   = $stats.LinesValid
        LinesCovered = $stats.LinesCovered
        Percent      = $stats.Percent
        Threshold    = $threshold
        Passed       = $passed
    }
}

if (-not $SkipBuild) {
    & (Join-Path $scriptsDir 'Build-InterfazHubSpot.ps1') -LibrariesOnly
}

$results = @()
$buildDone = $true

foreach ($cat in @('unit', 'security', 'integration')) {
    $scope = $scopesConfig.$cat
    $results += Invoke-CategoryCoverage -CategoryName $cat -Scope $scope -Live:$IncludeLive -BuildFirst:$false
}

Write-Host ""
Write-Host 'Resumen de cobertura por categoria:' -ForegroundColor Cyan
$results | Format-Table Category, LinesCovered, LinesValid, Percent, Threshold, Passed -AutoSize

$failed = @($results | Where-Object { -not $_.Passed })
if ($failed.Count -gt 0) {
    foreach ($f in $failed) {
        Write-Host "FAIL $($f.Category): $($f.Percent)% < $($f.Threshold)%" -ForegroundColor Red
    }
    throw "Measure-TestCoverage: $($failed.Count) categoria(s) bajo umbral."
}

Write-Host 'Measure-TestCoverage: OK (todas las categorias >= umbral)' -ForegroundColor Green
