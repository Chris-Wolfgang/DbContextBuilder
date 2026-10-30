#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Fails when an EF Core version wrapper does not link every shared source file.

.DESCRIPTION
    The -Core-EF6..EF10 packages compile -Core's source through
    <Compile Include="..\Wolfgang.DbContextBuilder-Core\..." Link="..." /> items
    (docs/adr/0001-ef-version-wrappers-link-shared-source.md), and the
    Core.Tests.Unit-EF7..EF10 test wrappers link Core.Tests.Unit's tests the same
    way. A file added to the shared project and not linked into a wrapper
    compiles in one package and silently not in the others. This is the
    build-time check ADR-0001 lists as a follow-up (#738).

    For each wrapper, the set of linked files (paths resolved and normalised) must
    equal the set of tracked *.cs files in the shared project (git ls-files, so
    obj/ and bin/ never count). Missing and extra files are both reported as
    GitHub ::error annotations, and the script exits 1. Run from the repository
    root.

.EXAMPLE
    pwsh ./scripts/compile-link-parity.ps1
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

# Each shared project and the wrappers that link it.
$groups = @(
    @{
        Shared   = 'src/Wolfgang.DbContextBuilder-Core'
        Wrappers = @(Get-ChildItem -Path 'src' -Directory -Filter 'Wolfgang.DbContextBuilder-Core-EF*')
    },
    @{
        Shared   = 'tests/Wolfgang.DbContextBuilder-Core.Tests.Unit'
        Wrappers = @(Get-ChildItem -Path 'tests' -Directory -Filter 'Wolfgang.DbContextBuilder-Core.Tests.Unit-EF*')
    }
)

$root = (Get-Location).Path
$failures = 0

function ConvertTo-RepoPath {
    param([string]$Path)
    $full = [System.IO.Path]::GetFullPath($Path)
    return [System.IO.Path]::GetRelativePath($root, $full).Replace('\', '/')
}

foreach ($group in $groups) {
    $shared = $group.Shared
    $expected = @(git ls-files -- "$shared/*.cs" "$shared/**/*.cs" | Sort-Object -Unique)
    if ($LASTEXITCODE -ne 0) { throw "git ls-files failed for $shared" }
    if ($expected.Count -eq 0) { throw "No tracked .cs files under $shared" }
    if ($group.Wrappers.Count -eq 0) { throw "No wrapper projects found for $shared" }

    foreach ($wrapper in $group.Wrappers) {
        $csproj = Get-ChildItem -Path $wrapper.FullName -Filter '*.csproj' | Select-Object -First 1
        if (-not $csproj) { throw "No .csproj in $($wrapper.FullName)" }

        [xml]$xml = Get-Content -Raw -LiteralPath $csproj.FullName
        $linked = @(
            $xml.SelectNodes('//*[local-name()="Compile"][@Include]') |
                ForEach-Object { $_.GetAttribute('Include') } |
                ForEach-Object { ConvertTo-RepoPath (Join-Path $csproj.DirectoryName ($_ -replace '\\', '/')) } |
                Where-Object { $_.StartsWith("$shared/", [System.StringComparison]::OrdinalIgnoreCase) } |
                Sort-Object -Unique
        )

        $missing = @($expected | Where-Object { $linked -notcontains $_ })
        $extra = @($linked | Where-Object { $expected -notcontains $_ })
        $name = ConvertTo-RepoPath $csproj.FullName

        foreach ($file in $missing) {
            Write-Host "::error file=$name::$file is in $shared but not linked by this project."
            $failures++
        }
        foreach ($file in $extra) {
            Write-Host "::error file=$name::links $file, which is not a tracked file in $shared."
            $failures++
        }
        if ($missing.Count -eq 0 -and $extra.Count -eq 0) {
            Write-Host "OK  $name links all $($expected.Count) files of $shared"
        }
    }
}

if ($failures -gt 0) {
    Write-Host "$failures compile-link mismatch(es) - see the errors above."
    exit 1
}
exit 0
