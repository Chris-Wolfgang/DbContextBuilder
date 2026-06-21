window.BENCHMARK_DATA = {
  "lastUpdate": 1782003353328,
  "repoUrl": "https://github.com/Chris-Wolfgang/DbContextBuilder",
  "entries": {
    "BenchmarkDotNet": [
      {
        "commit": {
          "author": {
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com",
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "f6ca7e65aeb5d6f2424c2f63b941382a208f0353",
          "message": "Release v0.6.2: canonical maintenance round + AssemblyVersion fix (#242)\n\n## Summary\n\nCuts v0.6.2 across all seven packages. Supersedes the stacked\n`canonical-protected` (#233) and `protected/d8-verify-docs-build-fleet`\n(#240) PRs — both are folded into this single vNext-→-main merge along\nwith the `fix/restore-assemblyversion` work (#241, already merged into\nvNext).\n\nThe library binary's public API and runtime behavior are unchanged\nfrom v0.6.1; this is canonical CI/docs/metadata work plus the C4\nbinding-stability fix.\n\n## Scope folded into v0.6.2\n\n### Added\n- **D8** verify-docs-build job in `release.yaml`\n- **A1** PublicApiAnalyzers scaffolding + baselined\n`PublicAPI.Shipped.txt` for all 7 src csprojs\n- **CI3** canonical NuGet metadata (Authors/Copyright/SourceLink/snupkg)\n- **T3** Stryker mutation-testing workflow\n- **T1** coverage report published to docs\n- **S1** CodeQL security-extended query pack\n- **D6** versions.json preservation guard\n- **D7** docs build cache hygiene\n- `docs/DOCFX-VERSION-PICKER.md`\n\n### Changed\n- **C1** fleet template-drift sync\n- `<Nullable>enable</Nullable>` consolidated into\n`Directory.Build.props`\n- **CI2** Dependabot `github-actions` ecosystem\n- **D3** script hardening\n\n### Fixed\n- **C4** explicit `<AssemblyVersion>1.0.0.0</AssemblyVersion>` +\nprerelease-safe `<FileVersion>` restored across all 7 src csprojs\n(binding stability for .NET Framework consumers)\n\n## Multi-project pack flow\n\nAll 7 `<Version>` values bumped 0.6.1 → 0.6.2 in lockstep. Each csproj\nproduces its own `.nupkg`; `release.yaml` validates the tag against\n**any** csproj's `<Version>`, so the single `v0.6.2` tag covers all\nseven packages.\n\n## Post-merge\n\n1. Tag `v0.6.2` and publish a GitHub Release (release.yaml then fires)\n2. Verify 7 .nupkg uploads to NuGet (~5 min for index)\n3. Delete vNext branch\n4. Close maintenance issues this release delivers\n## Closes (canonical work delivered)\n\n- Closes #204 (C1 template drift)\n- Closes #207 (C4 version metadata)\n- Closes #214 (T1 coverage on docs)\n- Closes #216 (T3 Stryker)\n- Closes #217 (D — Standardize README)\n- Closes #218 (D — CHANGELOG exists)\n- Closes #222 (D8 verify docs build at release)\n- Closes #223 (D6 versions.json preservation)\n- Closes #224 (D — version selector)\n- Closes #225 (S1 CodeQL security-extended)\n- Closes #226 (S2 GitHub Actions hardening)\n- Closes #228 (A1 PublicApiAnalyzers)\n- Closes #229 (CI3 NuGet package quality)\n- Closes #230 (CI2 Dependabot github-actions)",
          "timestamp": "2026-05-31T17:20:34-04:00",
          "tree_id": "4187bcbf9bd077708c2d605002aac739f7bc4fb9",
          "url": "https://github.com/Chris-Wolfgang/DbContextBuilder/commit/f6ca7e65aeb5d6f2424c2f63b941382a208f0353"
        },
        "date": 1780262524088,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_NoSeed(SeedCount: 1)",
            "value": 84933.63818359375,
            "unit": "ns",
            "range": "± 6517.311631609682"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_SeedWith(SeedCount: 1)",
            "value": 102244.68896484375,
            "unit": "ns",
            "range": "± 4493.18985229103"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_SeedWithRandom(SeedCount: 1)",
            "value": 268361.49202473956,
            "unit": "ns",
            "range": "± 13679.40926898906"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_NoSeed(SeedCount: 10)",
            "value": 82087.29996744792,
            "unit": "ns",
            "range": "± 1553.199533229167"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_SeedWith(SeedCount: 10)",
            "value": 149103.88997395834,
            "unit": "ns",
            "range": "± 11459.429089246389"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_SeedWithRandom(SeedCount: 10)",
            "value": 447258.7734375,
            "unit": "ns",
            "range": "± 54381.50477916145"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_NoSeed(SeedCount: 100)",
            "value": 79784.27799479167,
            "unit": "ns",
            "range": "± 1287.6105967079461"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_SeedWith(SeedCount: 100)",
            "value": 469187.3528645833,
            "unit": "ns",
            "range": "± 9487.40138324399"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_SeedWithRandom(SeedCount: 100)",
            "value": 2577390.0052083335,
            "unit": "ns",
            "range": "± 499007.2037780411"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com",
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "1385841fd8e36c53067bf2c4f89769b370794dbc",
          "message": "release: 0.7.0 (#333)\n\nRelease **0.7.0** — merges the accumulated `vNext` work to `main`.\n\n## Version\nAll seven `Wolfgang.DbContextBuilder-*` packages: **0.6.2 → 0.7.0**\n(MINOR — additive public API since 0.6.2).\n\n## Highlights since 0.6.2\n**Added**\n- `Wolfgang.DbContextBuilderCore.Assertions` — fluent `DbSet<T>`\nassertions (`HaveCount`, `BeEmpty`, `Contain`, `AllSatisfy`, …). (#106)\n- `SeedWith<TEntity>(TEntity)` singleton overload (avoids one-element\narray allocation).\n- DocFX content pages + README Target Frameworks / API surface section.\n\n**Changed / perf**\n- `SqliteModelCustomizer` delegate fields ctor-initialized (thread-safe;\nremoves a race).\n- `OverrideTableRenaming` default lambda no longer allocates a\nper-entity probe string.\n- Internal `DbContextActivator<T>` caches a compiled factory delegate\n(removes `Activator.CreateInstance` reflection cost per build).\n- Core + EF6 XML-doc and contract-documentation sweep.\n\n**Fixed**\n- EF6 Build/BuildAsync no longer leak the seed-time `DbContext`.\n- `SeedWith` singleton rejects string elements per-item.\n\n**Security**\n- Pinned `SQLitePCLRaw.lib.e_sqlite3` to `3.50.3`, above the `<= 2.1.11`\nrange affected by\n[GHSA-2m69-gcr7-jv3q](https://github.com/advisories/GHSA-2m69-gcr7-jv3q).\n\n**Tests / hygiene**\n- AdventureWorks EF8/9/10 scaffolds + test rewire to ProjectReference\n(closes #41/#42/#43, #235).\n- Deterministic `DbContext` disposal in unit tests (#329); empty-assert\n+ naming fixes (#330).\n\nFull detail in CHANGELOG `[0.7.0]`.\n\n## Closes\nCloses #235, #329, #330 (will fire on merge to `main`).\n\n---\n⚠️ Held for your review/merge per the no-auto-merge-to-main rule. After\nthis merges, I'll create the `v0.7.0` GitHub Release (`--target main`)\nso `release.yaml` publishes the packages.",
          "timestamp": "2026-06-20T20:54:24-04:00",
          "tree_id": "7937a0d8313254fe9210150f36c22e76f7ba6b2b",
          "url": "https://github.com/Chris-Wolfgang/DbContextBuilder/commit/1385841fd8e36c53067bf2c4f89769b370794dbc"
        },
        "date": 1782003352677,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_NoSeed(SeedCount: 1)",
            "value": 100439.87504069011,
            "unit": "ns",
            "range": "± 2229.994450202619"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_SeedWith(SeedCount: 1)",
            "value": 125058.88948567708,
            "unit": "ns",
            "range": "± 2604.9992078832247"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_SeedWithRandom(SeedCount: 1)",
            "value": 254662.19287109375,
            "unit": "ns",
            "range": "± 18915.061815580117"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_NoSeed(SeedCount: 10)",
            "value": 98535.30631510417,
            "unit": "ns",
            "range": "± 1872.5185486469647"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_SeedWith(SeedCount: 10)",
            "value": 177164.85872395834,
            "unit": "ns",
            "range": "± 19809.70970217595"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_SeedWithRandom(SeedCount: 10)",
            "value": 494634.8987630208,
            "unit": "ns",
            "range": "± 19759.319108755386"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_NoSeed(SeedCount: 100)",
            "value": 98043.27164713542,
            "unit": "ns",
            "range": "± 448.36250390589925"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_SeedWith(SeedCount: 100)",
            "value": 535268.5813802084,
            "unit": "ns",
            "range": "± 46176.456268075766"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_SeedWithRandom(SeedCount: 100)",
            "value": 2612791.6458333335,
            "unit": "ns",
            "range": "± 323536.4125628964"
          }
        ]
      }
    ]
  }
}