#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Warns when a src/ project targets a framework no test project exercises.

.DESCRIPTION
    Guard 3 of the test-matrix regression guards. Walks the ProjectReference
    graph from every test project the way the build does: for each test TFM,
    the referenced src project contributes the single asset NuGet/MSBuild would
    select for that consumer (exact TFM match, else the nearest asset of the
    consumer's own family that is not newer than it, else the nearest
    netstandard the consumer can load; a platform consumer such as
    net10.0-android is matched through its base TFM), and that selected TFM is
    what carries on to the src
    project's own references. A src TFM that is never the selected asset for
    any test TFM is reported - it is built and shipped but never loaded by a
    test.

    Evaluation goes through `dotnet msbuild -getProperty` / `-getItem` with
    Configuration=Release and, for references, the consumer's TargetFramework,
    so inherited, conditional and per-TFM values are all seen. Test projects
    are every *.csproj / *.vbproj / *.fsproj under tests/, as in pr.yaml.

    Emits GitHub `::warning` annotations (one per src project) and exits 0 on
    findings - a platform TFM may legitimately have no test story yet; the
    point is that the gap is visible, not silent. Exits 1 only when the
    evaluation itself fails. Run from the repository root; pr.yaml runs it on
    the Windows stage and build-pr.ps1 mirrors it locally.

.EXAMPLE
    pwsh ./scripts/tfm-parity.ps1
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$projectFilter = @('*.csproj', '*.vbproj', '*.fsproj')

function Invoke-MsBuildEval {
    param([string]$Project, [string[]]$Arguments)
    $out = & dotnet msbuild $Project -noLogo -p:Configuration=Release @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet msbuild evaluation failed for $Project ($($Arguments -join ' ')): $($out -join ' ')"
    }
    return $out
}

function Get-Tfms([string]$Project) {
    $raw = (Invoke-MsBuildEval $Project @('-getProperty:TargetFrameworks') |
        Where-Object { $_ -and "$_".Trim() } | Select-Object -Last 1)
    if (-not $raw) {
        $raw = (Invoke-MsBuildEval $Project @('-getProperty:TargetFramework') |
            Where-Object { $_ -and "$_".Trim() } | Select-Object -Last 1)
    }
    $raw = ("$raw" -replace '^TargetFrameworks?[=:]\s*', '') -replace '\s', ''
    return @($raw -split ';' | Where-Object { $_ })
}

# ProjectReference items as the project sees them when built for $Tfm - a
# reference conditioned on $(TargetFramework) only shows up for that TFM.
function Get-ReferencedProjects([string]$Project, [string]$Tfm) {
    $json = (Invoke-MsBuildEval $Project @('-getItem:ProjectReference', "-p:TargetFramework=$Tfm") | Out-String)
    if (-not $json.Trim()) { return @() }
    $dir = Split-Path -Parent (Resolve-Path $Project)
    $items = (ConvertFrom-Json $json).Items.ProjectReference
    return @($items | ForEach-Object { [System.IO.Path]::GetFullPath((Join-Path $dir $_.Identity)) })
}

# A TFM's family and version: 'core' (netcoreappX.Y and netX.Y from net5.0 on), 'fx'
# (.NET Framework, net462 -> 4.6.2) or 'ns' (netstandard). $null for anything else
# (e.g. a platform TFM such as net10.0-android, which only matches exactly).
function Get-TfmInfo([string]$Tfm) {
    if ($Tfm -match '^netcoreapp(\d+)\.(\d+)$') { return @{ Family = 'core'; Version = [version]"$($Matches[1]).$($Matches[2])" } }
    if ($Tfm -match '^net(\d+)\.(\d+)$' -and [int]$Matches[1] -ge 5) { return @{ Family = 'core'; Version = [version]"$($Matches[1]).$($Matches[2])" } }
    if ($Tfm -match '^net(\d)(\d)(\d)?$') {
        $v = if ($Matches[3]) { "$($Matches[1]).$($Matches[2]).$($Matches[3])" } else { "$($Matches[1]).$($Matches[2])" }
        return @{ Family = 'fx'; Version = [version]$v }
    }
    if ($Tfm -match '^netstandard(\d+)\.(\d+)$') { return @{ Family = 'ns'; Version = [version]"$($Matches[1]).$($Matches[2])" } }
    return $null
}

# Which asset of a multi-targeted project a consumer built for $ConsumerTfm
# loads, following NuGet's nearest-framework rule: the exact TFM if the project
# has it; otherwise the highest asset of the consumer's OWN family that is not
# newer than the consumer (a net8.0 consumer loads net6.0 before netstandard2.1,
# a net48 consumer loads net462 before netstandard2.0, #623); otherwise the
# newest netstandard the consumer can reference. Returns $null when nothing is
# compatible.
# A platform consumer (net10.0-android) can load its base TFM's assets (net10.0, and from
# there the same fallbacks), so everything after the exact match uses the base TFM. A
# platform-specific CANDIDATE still matches only exactly: Get-TfmInfo returns $null for it.
function Select-Asset([string]$ConsumerTfm, [string[]]$CandidateTfms) {
    if ($CandidateTfms -contains $ConsumerTfm) { return $ConsumerTfm }
    $baseTfm = $ConsumerTfm -replace '-.*$', ''
    if ($baseTfm -ne $ConsumerTfm -and $CandidateTfms -contains $baseTfm) { return $baseTfm }
    $consumer = Get-TfmInfo $baseTfm
    if ($null -eq $consumer) { return $null }

    $sameFamily = @($CandidateTfms |
        ForEach-Object { $info = Get-TfmInfo $_; if ($info -and $info.Family -eq $consumer.Family -and $info.Version -le $consumer.Version) { [pscustomobject]@{ Tfm = $_; Version = $info.Version } } } |
        Sort-Object Version -Descending)
    if ($sameFamily.Count -gt 0) { return $sameFamily[0].Tfm }

    $canLoad21 = $baseTfm -match '^(netcoreapp3\.\d|net[5-9]\.0|net[1-9]\d\.0|netstandard2\.1)'
    $canLoad20 = $canLoad21 -or $baseTfm -match '^(net4(6[1-9]|[7-9]\d*)|netcoreapp2\.\d|netstandard2\.0)'
    if ($canLoad21 -and $CandidateTfms -contains 'netstandard2.1') { return 'netstandard2.1' }
    if ($canLoad20 -and $CandidateTfms -contains 'netstandard2.0') { return 'netstandard2.0' }
    return $null
}

$srcProjects = @(Get-ChildItem -Path src -Recurse -File -Include $projectFilter -ErrorAction SilentlyContinue)
$testProjects = @(Get-ChildItem -Path tests -Recurse -File -Include $projectFilter -ErrorAction SilentlyContinue)
if ($srcProjects.Count -eq 0 -or $testProjects.Count -eq 0) {
    Write-Host "No src/ or tests/ projects - skipping TFM parity check."
    exit 0
}

$srcSet = [System.Collections.Generic.HashSet[string]]::new([string[]]$srcProjects.FullName, [System.StringComparer]::OrdinalIgnoreCase)
$srcTfms = @{}
foreach ($src in $srcProjects) { $srcTfms[$src.FullName] = Get-Tfms $src.FullName }

# selected[src project] = the set of that project's TFMs some test actually loads.
$selected = @{}
foreach ($src in $srcProjects) { $selected[$src.FullName] = [System.Collections.Generic.HashSet[string]]::new() }
$refCache = @{}
function Get-RefsCached([string]$Project, [string]$Tfm) {
    $key = "$Project|$Tfm"
    if (-not $refCache.ContainsKey($key)) { $refCache[$key] = Get-ReferencedProjects $Project $Tfm }
    return $refCache[$key]
}

foreach ($test in $testProjects) {
    foreach ($testTfm in (Get-Tfms $test.FullName)) {
        # Breadth-first over (project, consumer TFM); each src project contributes
        # the one asset the consumer selects, and that asset's TFM is the consumer
        # TFM for the project's own references.
        $queue = [System.Collections.Generic.Queue[object]]::new()
        $seen = [System.Collections.Generic.HashSet[string]]::new()
        foreach ($ref in (Get-RefsCached $test.FullName $testTfm)) { $queue.Enqueue(@($ref, $testTfm)) }
        while ($queue.Count -gt 0) {
            $project, $consumerTfm = $queue.Dequeue()
            if (-not $srcSet.Contains($project)) { continue }
            $asset = Select-Asset $consumerTfm $srcTfms[$project]
            if (-not $asset) { continue }
            if (-not $seen.Add("$project|$asset")) { continue }
            [void]$selected[$project].Add($asset)
            foreach ($next in (Get-RefsCached $project $asset)) { $queue.Enqueue(@($next, $asset)) }
        }
    }
}

$gaps = 0
foreach ($src in $srcProjects) {
    $rel = [System.IO.Path]::GetRelativePath((Get-Location).Path, $src.FullName) -replace '\\', '/'
    $covered = @($selected[$src.FullName] | Sort-Object)
    $all = $srcTfms[$src.FullName]
    if ($covered.Count -eq 0) {
        Write-Host "::warning file=$rel::No test project reaches this src project - none of its TFMs ($($all -join ', ')) are tested."
        $gaps++
        continue
    }
    $uncovered = @($all | Where-Object { $covered -notcontains $_ })
    if ($uncovered.Count -gt 0) {
        Write-Host "::warning file=$rel::TFM(s) $($uncovered -join ', ') are never the asset a test loads (tests load: $($covered -join ', ')) - that build is shipped untested."
        $gaps++
    }
    else {
        Write-Host "OK  $rel [$($all -join ', ')] <- tests load [$($covered -join ', ')]"
    }
}
if ($gaps -eq 0) { Write-Host "Every src TFM is loaded by at least one test TFM." }
else { Write-Host "$gaps src project(s) with TFM gaps - see warnings above." }
exit 0
