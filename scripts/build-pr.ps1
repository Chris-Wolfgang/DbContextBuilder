#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs the same checks as the Windows section of pr.yaml locally.

.DESCRIPTION
    Replicates the PR workflow's Windows stage locally so you can verify
    your changes will pass before pushing. Runs in order:
      1. Restore and build (Release)
      2. Run all tests across all target frameworks
      3. Generate coverage report and enforce threshold
      4. Run DevSkim security scan
      5. Run gitleaks secrets scan

.PARAMETER SkipTests
    Skip test execution (build only).

.PARAMETER SkipCoverage
    Skip coverage report generation and threshold enforcement.

.PARAMETER SkipSecurity
    Skip DevSkim and gitleaks scans.

.PARAMETER CoverageThreshold
    Minimum line coverage for PRODUCTION assemblies (anything not built from tests/).
    Defaults to 95. Mirrors CODECOV_MINIMUM in pr.yaml.

.PARAMETER TestCoverageThreshold
    Minimum line coverage for TEST assemblies (projects under tests/). Defaults to 100:
    test code that never executes has no purpose. Mirrors CODECOV_TEST_MINIMUM in pr.yaml.

.EXAMPLE
    pwsh ./scripts/build-pr.ps1
    pwsh ./scripts/build-pr.ps1 -SkipSecurity
    pwsh ./scripts/build-pr.ps1 -CoverageThreshold 80
#>
param(
    [switch]$SkipTests,
    [switch]$SkipCoverage,
    [switch]$SkipSecurity,
    [int]$CoverageThreshold = 95,
    [int]$TestCoverageThreshold = 100
)

$ErrorActionPreference = 'Stop'
$failed = @()

function Write-Step($message) {
    Write-Host ""
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host $message -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Cyan
}

function Write-Pass($message) {
    Write-Host $message -ForegroundColor Green
}

function Write-Fail($message) {
    Write-Host $message -ForegroundColor Red
}


# Restores the tools pinned in .config/dotnet-tools.json once per run. Local tools
# resolve through the repo's manifest, so nothing needs ~/.dotnet/tools on PATH.
$script:localToolsRestored = $false
function Restore-LocalTools {
    if ($script:localToolsRestored) { return $true }
    dotnet tool restore | Out-Host
    if ($LASTEXITCODE -ne 0) {
        Write-Fail "dotnet tool restore failed (see .config/dotnet-tools.json)"
        return $false
    }
    $script:localToolsRestored = $true
    return $true
}

# ============================================================================
# STEP 1: Restore and Build
# ============================================================================
Write-Step "Step 1: Restore and Build (Release)"

dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Fail "Restore failed"
    $failed += "Restore"
}
else {
    dotnet build --no-restore --configuration Release
    if ($LASTEXITCODE -ne 0) {
        Write-Fail "Build failed"
        $failed += "Build"
    }
    else {
        Write-Pass "Build succeeded"
    }
}

# ============================================================================
# STEP 2: Run Tests
# ============================================================================
if (-not $SkipTests -and $failed.Count -eq 0) {
    Write-Step "Step 2: Run Tests (all target frameworks)"

    # Mirrors pr.yaml's Stage 2 TFM parity check (guard 3). Findings are
    # warnings (exit 0); a non-zero exit means the evaluation itself broke and
    # is a failure here exactly as it is in CI.
    if (Test-Path './scripts/tfm-parity.ps1') {
        & pwsh -NoProfile -File './scripts/tfm-parity.ps1'
        if ($LASTEXITCODE -ne 0) {
            Write-Fail "TFM parity guard failed to run (exit $LASTEXITCODE)"
            $failed += "TFM parity"
        }
    }

    $testProjects = @(Get-ChildItem -Path './tests' -Recurse -File -Include '*.csproj', '*.vbproj', '*.fsproj' -ErrorAction SilentlyContinue)

    # Mirrors pr.yaml's Stage 2 ledger: the assembly of every project that ran a
    # coverage-collecting framework. Step 3 requires each of them to have a row in
    # the coverage report, so a dropped collector fails instead of passing quietly.
    $ranAssemblies = @()

    if ($testProjects.Count -eq 0) {
        Write-Host "No test projects found in ./tests — skipping"
    }
    else {
        foreach ($testProj in $testProjects) {
            Write-Host ""
            Write-Host "Testing: $($testProj.FullName)" -ForegroundColor White

            $content = Get-Content $testProj.FullName -Raw
            $tfmMatch = [regex]::Match($content, '<TargetFramework[s]?>([^<]+)</TargetFramework[s]?>')

            if (-not $tfmMatch.Success) {
                Write-Host "  No target frameworks found — skipping" -ForegroundColor Yellow
                continue
            }

            $frameworks = $tfmMatch.Groups[1].Value -split ';' |
                ForEach-Object { $_.Trim() } |
                Where-Object { $_ -match '^net(5\.0|6\.0|7\.0|8\.0|9\.0|10\.0|462|47|471|472|48|481|coreapp3\.1)$' }

            if ($frameworks.Count -eq 0) {
                Write-Host "  No compatible frameworks — skipping" -ForegroundColor Yellow
                continue
            }

            Write-Host "  Frameworks: $($frameworks -join ', ')"

            $collectedCoverage = $false
            foreach ($fw in $frameworks) {
                Write-Host "  Testing: $fw" -ForegroundColor Yellow

                $testArgs = @(
                    $testProj.FullName,
                    '--configuration', 'Release',
                    '--framework', $fw,
                    '--logger', 'console;verbosity=normal'
                )

                if ($fw -match '^net([5-9]|[1-9][0-9]+)\.') {
                    $testArgs += '--collect:XPlat Code Coverage'
                    $testArgs += '--results-directory'
                    $testArgs += './TestResults'
                    if (Test-Path 'coverlet.runsettings') {
                        $testArgs += '--settings'
                        $testArgs += 'coverlet.runsettings'
                    }
                    $collectedCoverage = $true
                }

                dotnet test @testArgs

                if ($LASTEXITCODE -ne 0) {
                    Write-Fail "  Tests failed for $fw"
                    $failed += "Tests ($fw)"
                    break
                }
            }

            if ($failed.Count -gt 0) { break }

            if ($collectedCoverage) {
                $asmMatch = [regex]::Match($content, '<AssemblyName>([^<]+)</AssemblyName>')
                $ranAssemblies += if ($asmMatch.Success) { $asmMatch.Groups[1].Value.Trim() } else { $testProj.BaseName }
            }
        }

        if ($failed.Count -eq 0) {
            Write-Pass "All tests passed"
        }
    }
}

# ============================================================================
# STEP 3: Coverage Report and Threshold
# ============================================================================
if (-not $SkipTests -and -not $SkipCoverage -and $failed.Count -eq 0) {
    Write-Step "Step 3: Coverage Report (threshold: ${CoverageThreshold}% src, ${TestCoverageThreshold}% tests)"

    $coverageFiles = Get-ChildItem -Path TestResults -Recurse -Filter coverage.cobertura.xml -ErrorAction SilentlyContinue

    $ran = @($ranAssemblies | Where-Object { $_ } | Sort-Object -Unique)

    if (-not $coverageFiles -and $ran.Count -gt 0) {
        # Tests ran with the collector, so no coverage files means collection broke.
        # Skipping here is how a dropped coverlet.collector used to report green.
        Write-Fail "Tests ran but produced no coverage files — the coverage gate cannot be evaluated. Test assemblies that ran: $($ran -join ', ')"
        $failed += "Coverage"
    }
    elseif (-not $coverageFiles) {
        Write-Host "No coverage files found and no test assembly collected coverage — skipping"
    }
    elseif (-not (Restore-LocalTools)) {
        # ReportGenerator is pinned in .config/dotnet-tools.json, exactly as pr.yaml
        # uses it. Run it as a local tool: no global install, no PATH dependency, so
        # it works from any shell or account that has `dotnet` on PATH.
        $failed += "Coverage"
    }
    else {
        dotnet reportgenerator `
            -reports:"TestResults/**/coverage.cobertura.xml" `
            -targetdir:"CoverageReport" `
            -reporttypes:"Html;TextSummary;MarkdownSummaryGithub;CsvSummary"

        if (Test-Path "CoverageReport/Summary.txt") {
            Write-Host ""
            Get-Content "CoverageReport/Summary.txt"
            Write-Host ""

            # Every test assembly that ran must have a row: the gate below can only
            # judge modules that appear in the report. Module rows start in column 1;
            # -notcontains is an exact match, so X.Tests is never taken for X.Tests.Unit.
            $rows = @(Get-Content "CoverageReport/Summary.txt" |
                Where-Object { $_ -match '^\S' } |
                ForEach-Object { ($_ -split '\s+')[0] })
            $missing = @($ran | Where-Object { $rows -notcontains $_ })
            if ($ran.Count -eq 0) {
                Write-Fail "Coverage files exist but no test assembly was recorded as having collected coverage"
                $failed += "Coverage"
            }
            elseif ($missing.Count -gt 0) {
                Write-Fail "These test assemblies ran but produced no coverage row: $($missing -join ', '). Is coverlet.collector still referenced?"
                $failed += "Coverage"
            }
            else {
                Write-Pass "All $($ran.Count) test assemblies that collected coverage have a row in the report"
            }

            # Same classification as pr.yaml: a test assembly is one built from a project
            # under tests/, identified by LOCATION, never by name (a shipped package can
            # contain "Test" in its name and must still be held to the src threshold).
            $testAssemblies = @($testProjects | ForEach-Object {
                $m = [regex]::Match((Get-Content $_.FullName -Raw), '<AssemblyName>([^<]+)</AssemblyName>')
                if ($m.Success) { $m.Groups[1].Value.Trim() } else { $_.BaseName }
            })

            $failedProjects = @()
            foreach ($line in (Get-Content "CoverageReport/Summary.txt")) {
                if ($line -match '^\s*(\S+)\s+(\d+(?:\.\d+)?)%\s*$' -and $line -notmatch '^\s*Summary') {
                    $module = $Matches[1]
                    $percent = [int][math]::Floor([double]$Matches[2])
                    $applies = if ($testAssemblies -contains $module) { $TestCoverageThreshold } else { $CoverageThreshold }

                    if ($percent -lt $applies) {
                        Write-Fail "  $module — ${percent}% (below ${applies}%)"
                        $failedProjects += "$module (${percent}%, needs ${applies}%)"
                    }
                    else {
                        Write-Pass "  $module — ${percent}%"
                    }
                }
            }

            if ($failedProjects.Count -gt 0) {
                Write-Fail "Coverage threshold FAILED: $($failedProjects -join ', ')"
                $failed += "Coverage"
            }
            else {
                Write-Pass "Coverage threshold passed"
            }
        }
        else {
            Write-Fail "Coverage report not generated (reportgenerator exit code $LASTEXITCODE) — the coverage gate cannot be evaluated"
            $failed += "Coverage"
        }
    }
}

# ============================================================================
# STEP 4: DevSkim Security Scan
# ============================================================================
if (-not $SkipSecurity) {
    Write-Step "Step 4: DevSkim Security Scan"

    # Pinned in .config/dotnet-tools.json and run as a local tool, as pr.yaml does.
    # A global `devskim` lookup used to fail silently when ~/.dotnet/tools was not on
    # PATH: analyze never ran, no results file was written, and the step reported
    # "No security issues found". Any failure to run is now a failure.
    $devskimRan = $false
    $devskimExit = $null
    if (Restore-LocalTools) {
        dotnet devskim analyze `
            --source-code . `
            --file-format text `
            --output-file devskim-results.txt `
            --ignore-rule-ids DS176209 `
            --ignore-globs "**/api/**,**/CoverageReport/**,**/TestResults/**"
        # Mirror pr.yaml, where a non-zero exit fails the DevSkim step.
        $devskimExit = $LASTEXITCODE
        $devskimRan = ($devskimExit -eq 0)
    }

    if (-not $devskimRan) {
        if (Test-Path "devskim-results.txt") { Get-Content "devskim-results.txt" -Raw | Write-Host }
        Write-Fail "DevSkim did not complete successfully (exit code $devskimExit)"
        $failed += "DevSkim"
        Remove-Item "devskim-results.txt" -ErrorAction SilentlyContinue
    }
    elseif (Test-Path "devskim-results.txt") {
        $results = Get-Content "devskim-results.txt" -Raw
        if ($results -and $results -match '(?i)(error|critical|high)') {
            Write-Host $results
            Write-Fail "DevSkim found security issues"
            $failed += "DevSkim"
        }
        else {
            Write-Pass "No critical security issues found"
        }
        Remove-Item "devskim-results.txt" -ErrorAction SilentlyContinue
    }
    else {
        Write-Pass "No security issues found"
    }
}

# ============================================================================
# STEP 5: Gitleaks Secrets Scan
# ============================================================================
if (-not $SkipSecurity) {
    Write-Step "Step 5: Gitleaks Secrets Scan"

    $gitleaks = Get-Command gitleaks -ErrorAction SilentlyContinue
    if (-not $gitleaks) {
        Write-Host "gitleaks not found — installing..."
        $version = "8.24.0"
        if ($IsWindows -or $env:OS -match 'Windows') {
            $archive = "gitleaks_${version}_windows_x64.zip"
            $url = "https://github.com/gitleaks/gitleaks/releases/download/v${version}/$archive"
            $dest = Join-Path $env:LOCALAPPDATA "gitleaks"
            New-Item -ItemType Directory -Force -Path $dest | Out-Null
            $zip = Join-Path $env:TEMP $archive
            Invoke-WebRequest -Uri $url -OutFile $zip -UseBasicParsing
            Expand-Archive -Path $zip -DestinationPath $dest -Force
            Remove-Item $zip -ErrorAction SilentlyContinue
            $env:PATH = "$dest;$env:PATH"
        }
        else {
            $archive = "gitleaks_${version}_linux_x64.tar.gz"
            $url = "https://github.com/gitleaks/gitleaks/releases/download/v${version}/$archive"
            curl -sSfL $url | tar xz -C /usr/local/bin gitleaks
        }
    }

    gitleaks detect --source . --verbose --redact
    if ($LASTEXITCODE -ne 0) {
        Write-Fail "Gitleaks found secrets"
        $failed += "Gitleaks"
    }
    else {
        Write-Pass "No secrets detected"
    }
}

# ============================================================================
# Summary
# ============================================================================
Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "SUMMARY" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

if ($failed.Count -gt 0) {
    Write-Fail "FAILED: $($failed -join ', ')"
    exit 1
}
else {
    Write-Pass "All checks passed"
    exit 0
}
