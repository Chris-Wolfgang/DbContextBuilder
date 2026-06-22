window.BENCHMARK_DATA = {
  "lastUpdate": 1782156090814,
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
          "id": "ef4505a47cfbb316beb8784be10409801caf0363",
          "message": "chore(deps): bump the github-actions group with 2 updates (#340)\n\nBumps the github-actions group with 2 updates:\n[actions/checkout](https://github.com/actions/checkout) and\n[softprops/action-gh-release](https://github.com/softprops/action-gh-release).\n\nUpdates `actions/checkout` from 6 to 7\n<details>\n<summary>Release notes</summary>\n<p><em>Sourced from <a\nhref=\"https://github.com/actions/checkout/releases\">actions/checkout's\nreleases</a>.</em></p>\n<blockquote>\n<h2>v7.0.0</h2>\n<h2>What's Changed</h2>\n<ul>\n<li>block checking out fork pr for pull_request_target and workflow_run\nby <a href=\"https://github.com/aiqiaoy\"><code>@​aiqiaoy</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2454\">actions/checkout#2454</a></li>\n<li>Bump actions/publish-immutable-action from 0.0.3 to 0.0.4 in the\nminor-actions-dependencies group across 1 directory by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2458\">actions/checkout#2458</a></li>\n<li>Bump flatted from 3.3.1 to 3.4.2 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2460\">actions/checkout#2460</a></li>\n<li>Bump js-yaml from 4.1.0 to 4.2.0 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2461\">actions/checkout#2461</a></li>\n<li>Bump <code>@​actions/core</code> and\n<code>@​actions/tool-cache</code> and Remove uuid by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2459\">actions/checkout#2459</a></li>\n<li>upgrade module to esm and update dependencies by <a\nhref=\"https://github.com/aiqiaoy\"><code>@​aiqiaoy</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2463\">actions/checkout#2463</a></li>\n<li>Bump the minor-npm-dependencies group across 1 directory with 3\nupdates by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2462\">actions/checkout#2462</a></li>\n<li>getting ready for checkout v7 release by <a\nhref=\"https://github.com/aiqiaoy\"><code>@​aiqiaoy</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2464\">actions/checkout#2464</a></li>\n<li>update error wording by <a\nhref=\"https://github.com/aiqiaoy\"><code>@​aiqiaoy</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2467\">actions/checkout#2467</a></li>\n</ul>\n<h2>New Contributors</h2>\n<ul>\n<li><a href=\"https://github.com/aiqiaoy\"><code>@​aiqiaoy</code></a> made\ntheir first contribution in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2454\">actions/checkout#2454</a></li>\n</ul>\n<p><strong>Full Changelog</strong>: <a\nhref=\"https://github.com/actions/checkout/compare/v6.0.3...v7.0.0\">https://github.com/actions/checkout/compare/v6.0.3...v7.0.0</a></p>\n<h2>v6.0.3</h2>\n<h2>What's Changed</h2>\n<ul>\n<li>Update changelog by <a\nhref=\"https://github.com/ericsciple\"><code>@​ericsciple</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2357\">actions/checkout#2357</a></li>\n<li>fix: expand merge commit SHA regex and add SHA-256 test cases by <a\nhref=\"https://github.com/yaananth\"><code>@​yaananth</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2414\">actions/checkout#2414</a></li>\n<li>Fix checkout init for SHA-256 repositories by <a\nhref=\"https://github.com/yaananth\"><code>@​yaananth</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2439\">actions/checkout#2439</a></li>\n<li>Update changelog for v6.0.3 by <a\nhref=\"https://github.com/yaananth\"><code>@​yaananth</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2446\">actions/checkout#2446</a></li>\n</ul>\n<h2>New Contributors</h2>\n<ul>\n<li><a href=\"https://github.com/yaananth\"><code>@​yaananth</code></a>\nmade their first contribution in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2414\">actions/checkout#2414</a></li>\n</ul>\n<p><strong>Full Changelog</strong>: <a\nhref=\"https://github.com/actions/checkout/compare/v6...v6.0.3\">https://github.com/actions/checkout/compare/v6...v6.0.3</a></p>\n<h2>v6.0.2</h2>\n<h2>What's Changed</h2>\n<ul>\n<li>Add orchestration_id to git user-agent when ACTIONS_ORCHESTRATION_ID\nis set by <a\nhref=\"https://github.com/TingluoHuang\"><code>@​TingluoHuang</code></a>\nin <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2355\">actions/checkout#2355</a></li>\n<li>Fix tag handling: preserve annotations and explicit fetch-tags by <a\nhref=\"https://github.com/ericsciple\"><code>@​ericsciple</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2356\">actions/checkout#2356</a></li>\n</ul>\n<p><strong>Full Changelog</strong>: <a\nhref=\"https://github.com/actions/checkout/compare/v6.0.1...v6.0.2\">https://github.com/actions/checkout/compare/v6.0.1...v6.0.2</a></p>\n<h2>v6.0.1</h2>\n<h2>What's Changed</h2>\n<ul>\n<li>Update all references from v5 and v4 to v6 by <a\nhref=\"https://github.com/ericsciple\"><code>@​ericsciple</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2314\">actions/checkout#2314</a></li>\n<li>Add worktree support for persist-credentials includeIf by <a\nhref=\"https://github.com/ericsciple\"><code>@​ericsciple</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2327\">actions/checkout#2327</a></li>\n<li>Clarify v6 README by <a\nhref=\"https://github.com/ericsciple\"><code>@​ericsciple</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2328\">actions/checkout#2328</a></li>\n</ul>\n<p><strong>Full Changelog</strong>: <a\nhref=\"https://github.com/actions/checkout/compare/v6...v6.0.1\">https://github.com/actions/checkout/compare/v6...v6.0.1</a></p>\n</blockquote>\n</details>\n<details>\n<summary>Changelog</summary>\n<p><em>Sourced from <a\nhref=\"https://github.com/actions/checkout/blob/main/CHANGELOG.md\">actions/checkout's\nchangelog</a>.</em></p>\n<blockquote>\n<h1>Changelog</h1>\n<h2>v7.0.0</h2>\n<ul>\n<li>Block checking out fork PR for pull_request_target and workflow_run\nby <a href=\"https://github.com/aiqiaoy\"><code>@​aiqiaoy</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2454\">actions/checkout#2454</a></li>\n<li>Bump actions/publish-immutable-action from 0.0.3 to 0.0.4 in the\nminor-actions-dependencies group across 1 directory by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2458\">actions/checkout#2458</a></li>\n<li>Bump flatted from 3.3.1 to 3.4.2 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2460\">actions/checkout#2460</a></li>\n<li>Bump js-yaml from 4.1.0 to 4.2.0 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2461\">actions/checkout#2461</a></li>\n<li>Bump <code>@​actions/core</code> and\n<code>@​actions/tool-cache</code> and Remove uuid by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2459\">actions/checkout#2459</a></li>\n<li>upgrade module to esm and update dependencies by <a\nhref=\"https://github.com/aiqiaoy\"><code>@​aiqiaoy</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2463\">actions/checkout#2463</a></li>\n<li>Bump the minor-npm-dependencies group across 1 directory with 3\nupdates by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2462\">actions/checkout#2462</a></li>\n</ul>\n<h2>v6.0.3</h2>\n<ul>\n<li>Fix checkout init for SHA-256 repositories by <a\nhref=\"https://github.com/yaananth\"><code>@​yaananth</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2439\">actions/checkout#2439</a></li>\n<li>fix: expand merge commit SHA regex and add SHA-256 test cases by <a\nhref=\"https://github.com/yaananth\"><code>@​yaananth</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2414\">actions/checkout#2414</a></li>\n</ul>\n<h2>v6.0.2</h2>\n<ul>\n<li>Fix tag handling: preserve annotations and explicit fetch-tags by <a\nhref=\"https://github.com/ericsciple\"><code>@​ericsciple</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2356\">actions/checkout#2356</a></li>\n</ul>\n<h2>v6.0.1</h2>\n<ul>\n<li>Add worktree support for persist-credentials includeIf by <a\nhref=\"https://github.com/ericsciple\"><code>@​ericsciple</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2327\">actions/checkout#2327</a></li>\n</ul>\n<h2>v6.0.0</h2>\n<ul>\n<li>Persist creds to a separate file by <a\nhref=\"https://github.com/ericsciple\"><code>@​ericsciple</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2286\">actions/checkout#2286</a></li>\n<li>Update README to include Node.js 24 support details and requirements\nby <a href=\"https://github.com/salmanmkc\"><code>@​salmanmkc</code></a>\nin <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2248\">actions/checkout#2248</a></li>\n</ul>\n<h2>v5.0.1</h2>\n<ul>\n<li>Port v6 cleanup to v5 by <a\nhref=\"https://github.com/ericsciple\"><code>@​ericsciple</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2301\">actions/checkout#2301</a></li>\n</ul>\n<h2>v5.0.0</h2>\n<ul>\n<li>Update actions checkout to use node 24 by <a\nhref=\"https://github.com/salmanmkc\"><code>@​salmanmkc</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2226\">actions/checkout#2226</a></li>\n</ul>\n<h2>v4.3.1</h2>\n<ul>\n<li>Port v6 cleanup to v4 by <a\nhref=\"https://github.com/ericsciple\"><code>@​ericsciple</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2305\">actions/checkout#2305</a></li>\n</ul>\n<h2>v4.3.0</h2>\n<ul>\n<li>docs: update README.md by <a\nhref=\"https://github.com/motss\"><code>@​motss</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/1971\">actions/checkout#1971</a></li>\n<li>Add internal repos for checking out multiple repositories by <a\nhref=\"https://github.com/mouismail\"><code>@​mouismail</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/1977\">actions/checkout#1977</a></li>\n<li>Documentation update - add recommended permissions to Readme by <a\nhref=\"https://github.com/benwells\"><code>@​benwells</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2043\">actions/checkout#2043</a></li>\n<li>Adjust positioning of user email note and permissions heading by <a\nhref=\"https://github.com/joshmgross\"><code>@​joshmgross</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2044\">actions/checkout#2044</a></li>\n<li>Update README.md by <a\nhref=\"https://github.com/nebuk89\"><code>@​nebuk89</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2194\">actions/checkout#2194</a></li>\n<li>Update CODEOWNERS for actions by <a\nhref=\"https://github.com/TingluoHuang\"><code>@​TingluoHuang</code></a>\nin <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2224\">actions/checkout#2224</a></li>\n<li>Update package dependencies by <a\nhref=\"https://github.com/salmanmkc\"><code>@​salmanmkc</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/2236\">actions/checkout#2236</a></li>\n</ul>\n<h2>v4.2.2</h2>\n<ul>\n<li><code>url-helper.ts</code> now leverages well-known environment\nvariables by <a href=\"https://github.com/jww3\"><code>@​jww3</code></a>\nin <a\nhref=\"https://redirect.github.com/actions/checkout/pull/1941\">actions/checkout#1941</a></li>\n<li>Expand unit test coverage for <code>isGhes</code> by <a\nhref=\"https://github.com/jww3\"><code>@​jww3</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/1946\">actions/checkout#1946</a></li>\n</ul>\n<h2>v4.2.1</h2>\n<ul>\n<li>Check out other refs/* by commit if provided, fall back to ref by <a\nhref=\"https://github.com/orhantoy\"><code>@​orhantoy</code></a> in <a\nhref=\"https://redirect.github.com/actions/checkout/pull/1924\">actions/checkout#1924</a></li>\n</ul>\n<!-- raw HTML omitted -->\n</blockquote>\n<p>... (truncated)</p>\n</details>\n<details>\n<summary>Commits</summary>\n<ul>\n<li><a\nhref=\"https://github.com/actions/checkout/commit/9c091bb21b7c1c1d1991bb908d89e4e9dddfe3e0\"><code>9c091bb</code></a>\nupdate error wording (<a\nhref=\"https://redirect.github.com/actions/checkout/issues/2467\">#2467</a>)</li>\n<li><a\nhref=\"https://github.com/actions/checkout/commit/1044a6dea927916f2c38ba5aeffbc0a847b1221a\"><code>1044a6d</code></a>\ngetting ready for checkout v7 release (<a\nhref=\"https://redirect.github.com/actions/checkout/issues/2464\">#2464</a>)</li>\n<li><a\nhref=\"https://github.com/actions/checkout/commit/f0282184c7ce73ab54c7e4ab5a617122602e575f\"><code>f028218</code></a>\nBump the minor-npm-dependencies group across 1 directory with 3 updates\n(<a\nhref=\"https://redirect.github.com/actions/checkout/issues/2462\">#2462</a>)</li>\n<li><a\nhref=\"https://github.com/actions/checkout/commit/d914b262ffc244530a203ab40decab34c3abf34d\"><code>d914b26</code></a>\nupgrade module to esm and update dependencies (<a\nhref=\"https://redirect.github.com/actions/checkout/issues/2463\">#2463</a>)</li>\n<li><a\nhref=\"https://github.com/actions/checkout/commit/537c7ef99cef6e5ddb5e7ff5d16d14510503801d\"><code>537c7ef</code></a>\nBump <code>@​actions/core</code> and <code>@​actions/tool-cache</code>\nand Remove uuid (<a\nhref=\"https://redirect.github.com/actions/checkout/issues/2459\">#2459</a>)</li>\n<li><a\nhref=\"https://github.com/actions/checkout/commit/130a169078a413d3a5246a393625e8e742f387f6\"><code>130a169</code></a>\nBump js-yaml from 4.1.0 to 4.2.0 (<a\nhref=\"https://redirect.github.com/actions/checkout/issues/2461\">#2461</a>)</li>\n<li><a\nhref=\"https://github.com/actions/checkout/commit/7d09575332117a40b46e5e020664df234cd416f3\"><code>7d09575</code></a>\nBump flatted from 3.3.1 to 3.4.2 (<a\nhref=\"https://redirect.github.com/actions/checkout/issues/2460\">#2460</a>)</li>\n<li><a\nhref=\"https://github.com/actions/checkout/commit/0f9f3aa320cb53abeb534aeb54048075d9697a0e\"><code>0f9f3aa</code></a>\nBump actions/publish-immutable-action (<a\nhref=\"https://redirect.github.com/actions/checkout/issues/2458\">#2458</a>)</li>\n<li><a\nhref=\"https://github.com/actions/checkout/commit/f9e715a95fcd1f9253f77dd28f11e88d2d6460c7\"><code>f9e715a</code></a>\nblock checking out fork pr for pull_request_target and workflow_run (<a\nhref=\"https://redirect.github.com/actions/checkout/issues/2454\">#2454</a>)</li>\n<li>See full diff in <a\nhref=\"https://github.com/actions/checkout/compare/v6...v7\">compare\nview</a></li>\n</ul>\n</details>\n<br />\n\nUpdates `softprops/action-gh-release` from 3.0.0 to 3.0.1\n<details>\n<summary>Release notes</summary>\n<p><em>Sourced from <a\nhref=\"https://github.com/softprops/action-gh-release/releases\">softprops/action-gh-release's\nreleases</a>.</em></p>\n<blockquote>\n<h2>v3.0.1</h2>\n<h2>3.0.1</h2>\n<ul>\n<li>maintenance release with updated dependencies</li>\n</ul>\n</blockquote>\n</details>\n<details>\n<summary>Changelog</summary>\n<p><em>Sourced from <a\nhref=\"https://github.com/softprops/action-gh-release/blob/master/CHANGELOG.md\">softprops/action-gh-release's\nchangelog</a>.</em></p>\n<blockquote>\n<h2>3.0.1</h2>\n<ul>\n<li>maintenance release with updated dependencies</li>\n</ul>\n<h2>3.0.0</h2>\n<p><code>3.0.0</code> is a major release that moves the action runtime\nfrom Node 20 to Node 24.\nUse <code>v3</code> on GitHub-hosted runners and self-hosted fleets that\nalready support the\nNode 24 Actions runtime. If you still need the last Node 20-compatible\nline, stay on\n<code>v2.6.2</code>.</p>\n<h2>What's Changed</h2>\n<h3>Other Changes 🔄</h3>\n<ul>\n<li>Move the action runtime and bundle target to Node 24</li>\n<li>Update <code>@types/node</code> to the Node 24 line and allow future\nDependabot updates</li>\n<li>Keep the floating major tag on <code>v3</code>; <code>v2</code>\nremains pinned to the latest <code>2.x</code> release</li>\n</ul>\n<h2>2.6.2</h2>\n<h2>What's Changed</h2>\n<h3>Other Changes 🔄</h3>\n<ul>\n<li>chore(deps): bump picomatch from 4.0.3 to 4.0.4 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/softprops/action-gh-release/pull/775\">softprops/action-gh-release#775</a></li>\n<li>chore(deps): bump brace-expansion from 5.0.4 to 5.0.5 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/softprops/action-gh-release/pull/777\">softprops/action-gh-release#777</a></li>\n<li>chore(deps): bump vite from 8.0.0 to 8.0.5 by <a\nhref=\"https://github.com/dependabot\"><code>@​dependabot</code></a>[bot]\nin <a\nhref=\"https://redirect.github.com/softprops/action-gh-release/pull/781\">softprops/action-gh-release#781</a></li>\n</ul>\n<h2>2.6.1</h2>\n<p><code>2.6.1</code> is a patch release focused on restoring linked\ndiscussion thread creation when\n<code>discussion_category_name</code> is set. It fixes\n<code>[#764](https://github.com/softprops/action-gh-release/issues/764)</code>,\nwhere the draft-first publish flow\nstopped carrying the discussion category through the final publish\nstep.</p>\n<p>If you still hit an issue after upgrading, please open a report with\nthe bug template and include a minimal repro or sanitized workflow\nsnippet where possible.</p>\n<h2>What's Changed</h2>\n<h3>Bug fixes 🐛</h3>\n<ul>\n<li>fix: preserve discussion category on publish by <a\nhref=\"https://github.com/chenrui333\"><code>@​chenrui333</code></a> in <a\nhref=\"https://redirect.github.com/softprops/action-gh-release/pull/765\">softprops/action-gh-release#765</a></li>\n</ul>\n<h2>2.6.0</h2>\n<p><code>2.6.0</code> is a minor release centered on\n<code>previous_tag</code> support for\n<code>generate_release_notes</code>,\nwhich lets workflows pin GitHub's comparison base explicitly instead of\nrelying on the default range.\nIt also includes the recent concurrent asset upload recovery fix, a\n<code>working_directory</code> docs sync,\na checked-bundle freshness guard for maintainers, and clearer\nimmutable-prerelease guidance where\nGitHub platform behavior imposes constraints on how prerelease asset\nuploads can be published.</p>\n<!-- raw HTML omitted -->\n</blockquote>\n<p>... (truncated)</p>\n</details>\n<details>\n<summary>Commits</summary>\n<ul>\n<li><a\nhref=\"https://github.com/softprops/action-gh-release/commit/718ea10b132b3b2eba29c1007bb80653f286566b\"><code>718ea10</code></a>\nrelease 3.0.1</li>\n<li><a\nhref=\"https://github.com/softprops/action-gh-release/commit/f1a938b9d84ca9b770d0d8dfeb3e7285fe261e63\"><code>f1a938b</code></a>\nchore(deps): bump esbuild from 0.28.0 to 0.28.1 (<a\nhref=\"https://redirect.github.com/softprops/action-gh-release/issues/802\">#802</a>)</li>\n<li><a\nhref=\"https://github.com/softprops/action-gh-release/commit/0066ead0de7252b4876b36b5357fc3974619d36a\"><code>0066ead</code></a>\nchore(deps): bump vite from 8.0.14 to 8.0.16 (<a\nhref=\"https://redirect.github.com/softprops/action-gh-release/issues/806\">#806</a>)</li>\n<li><a\nhref=\"https://github.com/softprops/action-gh-release/commit/dc643cac6252aaa00c9b0b6c940d489cd7bf6b23\"><code>dc643ca</code></a>\nchore(deps): bump the npm group with 3 updates (<a\nhref=\"https://redirect.github.com/softprops/action-gh-release/issues/805\">#805</a>)</li>\n<li><a\nhref=\"https://github.com/softprops/action-gh-release/commit/85ee99b6b20742a3823a8a289ee5e6ceab44e8aa\"><code>85ee99b</code></a>\nchore(deps): bump actions/checkout in the github-actions group (<a\nhref=\"https://redirect.github.com/softprops/action-gh-release/issues/804\">#804</a>)</li>\n<li><a\nhref=\"https://github.com/softprops/action-gh-release/commit/9ed3cf9a6863b31f005d951c8d19de20628cf4eb\"><code>9ed3cf9</code></a>\nchore(deps): bump the npm group with 2 updates (<a\nhref=\"https://redirect.github.com/softprops/action-gh-release/issues/800\">#800</a>)</li>\n<li><a\nhref=\"https://github.com/softprops/action-gh-release/commit/3efcac8951299998593f871640ea8059d6818655\"><code>3efcac8</code></a>\nchore(deps): bump the npm group with 3 updates (<a\nhref=\"https://redirect.github.com/softprops/action-gh-release/issues/798\">#798</a>)</li>\n<li><a\nhref=\"https://github.com/softprops/action-gh-release/commit/05d6b9164aa74958de40b0179d6a773112fcdc7f\"><code>05d6b91</code></a>\nchore(deps): bump brace-expansion from 5.0.5 to 5.0.6 (<a\nhref=\"https://redirect.github.com/softprops/action-gh-release/issues/797\">#797</a>)</li>\n<li><a\nhref=\"https://github.com/softprops/action-gh-release/commit/403a5240f3837fa857f642062e05aad6bb3391ca\"><code>403a524</code></a>\nchore(deps): bump <code>@​types/node</code> from 24.12.2 to 24.12.3 in\nthe npm group (<a\nhref=\"https://redirect.github.com/softprops/action-gh-release/issues/796\">#796</a>)</li>\n<li><a\nhref=\"https://github.com/softprops/action-gh-release/commit/437e073e786973c6b6af97d9e445c41ae43b1d29\"><code>437e073</code></a>\nchore(deps): bump the npm group with 4 updates (<a\nhref=\"https://redirect.github.com/softprops/action-gh-release/issues/792\">#792</a>)</li>\n<li>Additional commits viewable in <a\nhref=\"https://github.com/softprops/action-gh-release/compare/b4309332981a82ec1c5618f44dd2e27cc8bfbfda...718ea10b132b3b2eba29c1007bb80653f286566b\">compare\nview</a></li>\n</ul>\n</details>\n<br />\n\n\nDependabot will resolve any conflicts with this PR as long as you don't\nalter it yourself. You can also trigger a rebase manually by commenting\n`@dependabot rebase`.\n\n[//]: # (dependabot-automerge-start)\n[//]: # (dependabot-automerge-end)\n\n---\n\n<details>\n<summary>Dependabot commands and options</summary>\n<br />\n\nYou can trigger Dependabot actions by commenting on this PR:\n- `@dependabot rebase` will rebase this PR\n- `@dependabot recreate` will recreate this PR, overwriting any edits\nthat have been made to it\n- `@dependabot show <dependency name> ignore conditions` will show all\nof the ignore conditions of the specified dependency\n- `@dependabot ignore <dependency name> major version` will close this\ngroup update PR and stop Dependabot creating any more for the specific\ndependency's major version (unless you unignore this specific\ndependency's major version or upgrade to it yourself)\n- `@dependabot ignore <dependency name> minor version` will close this\ngroup update PR and stop Dependabot creating any more for the specific\ndependency's minor version (unless you unignore this specific\ndependency's minor version or upgrade to it yourself)\n- `@dependabot ignore <dependency name>` will close this group update PR\nand stop Dependabot creating any more for the specific dependency\n(unless you unignore this specific dependency or upgrade to it yourself)\n- `@dependabot unignore <dependency name>` will remove all of the ignore\nconditions of the specified dependency\n- `@dependabot unignore <dependency name> <ignore condition>` will\nremove the ignore condition of the specified dependency and ignore\nconditions\n\n\n</details>",
          "timestamp": "2026-06-22T15:20:08-04:00",
          "tree_id": "073377fa8288dd57ed28389d82e86757092bb3a3",
          "url": "https://github.com/Chris-Wolfgang/DbContextBuilder/commit/ef4505a47cfbb316beb8784be10409801caf0363"
        },
        "date": 1782156090048,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_NoSeed(SeedCount: 1)",
            "value": 93796.55794270833,
            "unit": "ns",
            "range": "± 452.1540363912172"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_SeedWith(SeedCount: 1)",
            "value": 117847.10807291667,
            "unit": "ns",
            "range": "± 354.62177234008203"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_SeedWithRandom(SeedCount: 1)",
            "value": 246766.85872395834,
            "unit": "ns",
            "range": "± 16680.03815420537"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_NoSeed(SeedCount: 10)",
            "value": 97020.9825032552,
            "unit": "ns",
            "range": "± 1855.8486307044773"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_SeedWith(SeedCount: 10)",
            "value": 165595.94108072916,
            "unit": "ns",
            "range": "± 14822.753018231917"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_SeedWithRandom(SeedCount: 10)",
            "value": 487520.1878255208,
            "unit": "ns",
            "range": "± 34370.11135444873"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_NoSeed(SeedCount: 100)",
            "value": 95713.52278645833,
            "unit": "ns",
            "range": "± 2501.5345202664125"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_SeedWith(SeedCount: 100)",
            "value": 479757.6510416667,
            "unit": "ns",
            "range": "± 5933.255732515048"
          },
          {
            "name": "Wolfgang.DbContextBuilderCore.Benchmarks.BuildAsyncBenchmarks.InMemory_SeedWithRandom(SeedCount: 100)",
            "value": 3124693.2395833335,
            "unit": "ns",
            "range": "± 926921.2396771321"
          }
        ]
      }
    ]
  }
}