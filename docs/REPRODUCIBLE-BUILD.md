# Reproducible build — third-party verification

This library builds deterministically, so anyone can rebuild a released version
from source and confirm the compiled assemblies match what was published to NuGet.
This page is the **consumer-side** procedure: how *you* independently verify the
claim, without trusting our CI.

Tracks issue #319. Pairs with the internal reproducible-build check
(`.github/workflows/reproducible-build.yaml`, #310), which proves our own builds
are byte-stable when the same commit is built twice on the reference runner.

## What makes the build reproducible

`Directory.Build.props` sets the determinism knobs that remove machine- and
time-dependent inputs from the compiled output:

- Deterministic compilation (the .NET SDK default) — no timestamps / random
  GUIDs in the assembly.
- `ContinuousIntegrationBuild=true` — set automatically in CI when `$(CI)` is
  `true`; normalizes embedded source paths. Pass it explicitly when rebuilding
  locally (see below).
- `<EmbedUntrackedSources>true</EmbedUntrackedSources>` + `Microsoft.SourceLink.GitHub`
  — source references resolve to the exact commit rather than local paths.

The **managed IL** in each assembly is therefore expected to be byte-for-byte
identical given the same source commit and a matching compiler (SDK) version.

## Tooling

| Tool | Version |
| --- | --- |
| .NET SDK | 10.0.x (latest patch — the released packages are packed with the 10.0 SDK) |
| OS | **Windows** is the reference environment: the released packages are packed on `windows-latest` (`release.yaml`). Other operating systems may produce different bytes; see [What is guaranteed](#what-is-guaranteed). |

There is no `global.json` SDK pin, so use the latest **.NET 10** SDK. The Roslyn
compiler ships with the SDK, so a different SDK *major* can change the emitted IL;
stay on 10.0.x to reproduce a package built during the current release cycle.

## Verify a release

1. **Clone the source at the exact release tag** (replace `v0.9.0`):

   ```bash
   git clone --depth 1 --branch v0.9.0 https://github.com/Chris-Wolfgang/DbContextBuilder.git
   cd DbContextBuilder
   ```

2. **Pack in Release with the CI determinism flag** (this repo ships 9 packages):

   ```bash
   dotnet pack src/Wolfgang.DbContextBuilder-Core-EF6/Wolfgang.DbContextBuilder-Core-EF6.csproj -c Release -p:ContinuousIntegrationBuild=true
   dotnet pack src/Wolfgang.DbContextBuilder-Core-EF7/Wolfgang.DbContextBuilder-Core-EF7.csproj -c Release -p:ContinuousIntegrationBuild=true
   dotnet pack src/Wolfgang.DbContextBuilder-Core-EF8/Wolfgang.DbContextBuilder-Core-EF8.csproj -c Release -p:ContinuousIntegrationBuild=true
   dotnet pack src/Wolfgang.DbContextBuilder-Core-EF9/Wolfgang.DbContextBuilder-Core-EF9.csproj -c Release -p:ContinuousIntegrationBuild=true
   dotnet pack src/Wolfgang.DbContextBuilder-Core-EF10/Wolfgang.DbContextBuilder-Core-EF10.csproj -c Release -p:ContinuousIntegrationBuild=true
   dotnet pack src/Wolfgang.DbContextBuilder-EF6/Wolfgang.DbContextBuilder-EF6.csproj -c Release -p:ContinuousIntegrationBuild=true
   dotnet pack src/Wolfgang.DbContextBuilder.Abstractions/Wolfgang.DbContextBuilder.Abstractions.csproj -c Release -p:ContinuousIntegrationBuild=true
   dotnet pack src/Wolfgang.DbContextBuilder.AutoFixture/Wolfgang.DbContextBuilder.AutoFixture.csproj -c Release -p:ContinuousIntegrationBuild=true
   dotnet pack src/Wolfgang.DbContextBuilder.Bogus/Wolfgang.DbContextBuilder.Bogus.csproj -c Release -p:ContinuousIntegrationBuild=true
   ```

   `Wolfgang.DbContextBuilder-Core` is not in this list — it stopped shipping as
   of #362 (deprecated on nuget.org, not unlisted; the last version it published
   remains verifiable the same way).

3. **Compare against the published hashes.** Each release attaches a
   `reproducible-build-manifest.json` listing the expected SHA-256 of every
   published `.nupkg`. Download it from the release page and compare:

   ```bash
   # your local package hash
   sha256sum src/Wolfgang.DbContextBuilder-Core-EF10/bin/Release/*.nupkg
   # expected hash from the release
   cat reproducible-build-manifest.json
   ```

   For the most robust check, compare the **compiled assemblies** rather than the
   `.nupkg` wrapper (the wrapper's zip framing can vary by SDK patch even when the
   IL is identical). Extract both packages and diff the `.dll` hashes:

   ```bash
   unzip -o your.nupkg -d local && unzip -o published.nupkg -d published
   diff <(cd local && find . -name '*.dll' -exec sha256sum {} + | sort) \
        <(cd published && find . -name '*.dll' -exec sha256sum {} + | sort)
   ```

## What is guaranteed

The guarantee is **same commit, same SDK, same kind of environment, same bytes**.
The assemblies (`.dll`) and symbols (`.pdb`) you rebuild on Windows with the SDK
the release used are byte-identical to the ones inside the published packages.

That is what the `Build reproducibility` workflow checks every week: it builds
every `src/` project, all target frameworks, from two checkouts in different
directories on `windows-latest`, and fails on any `.dll`/`.pdb` difference. A manual
run with a `verify-tag` input packs a release tag and compares the assemblies and
symbols inside every package against the ones published on that GitHub release,
which is the check you would run yourself.

**Not guaranteed:**

- **Identical bytes across operating systems.** Linux, macOS and Windows may
  produce different files from the same commit. The workflow also builds on
  `ubuntu-latest` and reports a difference as a warning only. Today they match
  because `Directory.Build.targets` rewrites the SDK-generated sources to LF
  and `Directory.Build.props` pins `Microsoft.NETFramework.ReferenceAssemblies`
  for net4x builds, which also stops a machine's installed targeting pack from
  leaking into the PDB. Neither is a promise, and verification does not depend on
  them: rebuild on Windows.
- **Package files.** NuGet stamps the pack time into every `.nupkg`/`.snupkg` zip
  entry, so two packs of one commit on one machine differ. Compare the assemblies
  inside the packages, as shown above.
- **A different SDK.** A different SDK patch can legitimately change the output.
  There is no `global.json` pin, so use the SDK version shown in the release's
  `release.yaml` run log.

A mismatch in a `.dll` built from the same commit with the same SDK on Windows is
the signal that warrants a report.

## Report a discrepancy

If a rebuilt assembly's IL does not match the published package built with the
same SDK and commit, **do not** open a public issue with details first — follow
the [security policy](../SECURITY.md) and report privately via the repository's
Security tab. Include: the release tag, your exact `dotnet --version`, your OS,
and the differing file hashes.

## Independent attestations

Third parties are encouraged to publish verification attestations using
[Reproducible Builds](https://reproducible-builds.org/) project conventions (or a
service such as [vouchsafe.io](https://vouchsafe.io/)), pointing at the release
tag and the `reproducible-build-manifest.json` they verified against.
