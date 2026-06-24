# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- New **`Wolfgang.DbContextBuilder.Abstractions`** package containing `ICreateRandomEntities`,
  so add-on packages (e.g. an upcoming Bogus random-data provider) can target the family
  without taking a dependency on a specific EF Core version.

- New `ISeedProfile<T>` and `DbContextBuilder<T>.UseSeedProfile(ISeedProfile<T>)` —
  reusable, named bundles of seed data that can be applied to any builder in one call.
  Multiple profiles accumulate, so common setups can be shared across many test classes
  instead of being re-built in each. (#104)
- New `DbContextBuilder<T>.UseDiagnosticOutput(Action<string>)` — routes EF Core logs
  (including generated SQL) produced while building/seeding, plus a one-line seed summary,
  to a caller-supplied sink. Framework-agnostic: pass `testOutputHelper.WriteLine` in xUnit
  so the seeded context is visible in the log when a test fails. (#117)

### Changed

### Fixed

### Removed

### Deprecated

### Security

## [0.7.0] - 2026-06-20

### Added

- New `Wolfgang.DbContextBuilderCore.Assertions` namespace providing a small fluent,
  chainable assertion surface for verifying the state of a `DbSet<T>` in tests:
  `HaveCount`, `BeEmpty`, `NotBeEmpty`, `Contain`, `NotContain`, `AllSatisfy`. Obtain
  the assertion chain via `dbSet.Should()` or `queryable.Should()`. Failures throw
  `DbContextAssertionException` with an entity-typed message. (#106)
- New `DbContextBuilder<T>.SeedWith<TEntity>(TEntity entity)` singleton overload that
  avoids the one-element array allocation of the existing `params` overload — useful
  in tests that seed many single rows.
- New `DocFX` content pages (`docs/introduction.md`, `docs/getting-started.md`) and
  a Target Frameworks + API surface section in `README.md`.

### Changed

- `SqliteModelCustomizer` default delegate fields are now ctor-initialized rather than
  lazy-initialized in the property getter, eliminating a race that could allocate
  duplicate delegates when EF's model customization runs concurrently.
- `SqliteModelCustomizer.OverrideTableRenaming` default lambda no longer allocates
  a `$"{prefix}_"` interpolation per entity type — uses prefix-substring + separator
  char comparison instead.
- `DbContextBuilderSqliteExtensions`'s SQLite-already-registered check uses
  `typeof(SqliteOptionsExtension)` instead of matching the type's `FullName` string.
- New internal `DbContextActivator<TDbContext>` caches a compiled Expression-based
  factory delegate per closed generic, so each `BuildAsync` no longer pays the
  `Activator.CreateInstance` reflection cost. Used by both `InMemoryDbContextCreator`
  and `SqliteDbContextCreator`.
- XML doc sweep across Core + EF6: fixed stale `<exception cref>` claims, typos,
  truncated comments, malformed `<see cref></see>` tags, and literal `{T}` in
  `<returns>` (now `<typeparamref name="T"/>`).
- `DbContextBuilder<T>.SeedWith`, `UseInMemory`, `UseSqlite`, and
  `SqliteModelCustomizer.DefaultValueMap` now document their contracts
  (last-write-wins provider selection, insertion-order semantics, inheritance
  support, DI-singleton mutation hazard).

### Fixed

- EF6 (classic) `DbContextBuilder<T>.Build()` and `BuildAsync()` no longer leak the
  temporary seed-time `DbContext`. Both wrap it in `using` before returning the
  caller's context.
- `DbContextBuilder<T>.SeedWith<TEntity>(TEntity entity)` (singleton overload added
  this release) now rejects string elements per-item when passed a `List<string>` or
  other `IEnumerable<object>`-castable sequence, matching the params overload's
  `case string` behaviour.

### Removed

- Stale `<AssemblyVersion>1.0.0</AssemblyVersion>` from all 7 test csprojs (tests
  aren't shipped; let the SDK derive it).
- Dead test constant `TestConstants.MaxTestRuntimeMs` (zero references) and its
  associated stale `// TODO` comment.
- Commented-out `//IModelCustomizer modelCustomizer` parameter from the private
  `UseSqlite` extension.

### Deprecated

### Security

- Pinned `SQLitePCLRaw.lib.e_sqlite3` to `3.50.3` across the SQLite-enabled
  packages, above the `<= 2.1.11` range affected by GHSA-2m69-gcr7-jv3q
  (a vulnerability in the bundled SQLite native library). Consumers of the
  SQLite providers now restore the patched native library.

## [0.6.2] - 2026-05-29

Canonical maintenance round + binding-stability fix. No public API or
runtime behavior change vs v0.6.1 across any of the seven packages.

### Added

- **D8** — `verify-docs-build` job in `release.yaml` runs DocFX during
  the release pipeline before the NuGet push, so a docs build failure
  now blocks the package from shipping.
- **A1** — `PublicApiAnalyzers` scaffolding (analyzers activate when
  `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt` are present
  alongside a csproj).
- **CI3** — canonical NuGet package metadata: `Authors`, `Copyright`,
  `RepositoryType`, SourceLink, snupkg symbol packages, deterministic
  CI build flag, and `EmbedUntrackedSources` hoisted to
  `Directory.Build.props`.
- **T3** — Stryker mutation-testing workflow (`stryker.yaml`).
- **T1** — coverage report published to docs site.
- **S1** — CodeQL `security-extended` query pack.
- **D6** — versions.json preservation guard on the docs deploy.
- **D7** — docs build cache hygiene.
- `docs/DOCFX-VERSION-PICKER.md` documenting the version picker.

### Changed

- **C1** — fleet-wide template-drift sync: workflow files (`pr.yaml`,
  `release.yaml`, `docfx.yaml`, `codeql.yaml`,
  `build-all-versions.yaml`, `stryker.yaml`), `.editorconfig`,
  `BannedSymbols.txt`, `Directory.Build.props`, and per-context
  `tests/Directory.Build.props` consolidated to the canonical baseline.
- **Nullable** — `<Nullable>enable</Nullable>` consolidated into
  `Directory.Build.props` (was per-csproj); per-project opt-out via
  override still supported.
- **CI2** — Dependabot `github-actions` ecosystem added.
- **D3** — repo scripts hardened (`Setup-Labels.ps1`,
  `Fix-BranchRuleset.ps1`).

### Fixed

- **C4** — restored explicit `<AssemblyVersion>1.0.0.0</AssemblyVersion>`
  and added a prerelease-safe `<FileVersion>` (regex-strip property
  function) to every src csproj. The original C4 fanout had dropped
  these on the rationale that the hardcoded values were "stale"
  relative to released package versions — but that staleness was the
  correct binding-stability behaviour for libraries that ship a
  `net462` TFM. Without an explicit pin, SDK-derived `AssemblyVersion`
  would change on every minor/patch release, breaking .NET Framework
  consumers without a binding redirect. (See DateTime-Extensions v1.3.1
  for the post-mortem on what happens when this regression reaches a
  release.)

## [0.6.1] - 2026-05-09

### Changed
- Dependency-range bumps to consume the latest EF Core 9.x / 10.x patches.

No public API change vs `0.6.0`.

## [0.6.0] - 2026-05-03

### Changed
- Incremental EF Core targeting tweaks (per-package version-range
  alignment to the EF major being pinned).

No public API change vs `0.5.0`.

## [0.5.0] - 2026-05-03

### Changed
- Incremental EF Core targeting tweaks.

No public API change vs `0.4.0`.

## [0.4.0] - 2026-05-02

### Changed
- Dependency-range updates across all seven `Wolfgang.DbContextBuilder-*`
  packages.

No public API change vs `0.3.3`.

## [0.3.3]

### Changed
- EF Core package dependency version ranges updated

## [0.3.2]

### Changed
- EF Core package dependency version ranges updated

## [0.3.1]

### Changed
- EF Core package dependency version ranges updated

## [0.3.0]

### Added
- EF Core 10 support (`Wolfgang.DbContextBuilder-Core-EF10`)

## [0.2.0]

### Added
- Sqlite in-memory database support via `UseSqlite()` extension method
- `SqliteModelCustomizer` for Sqlite-specific model customization
- `SqliteForMsSqlServerModelCustomizer` for SQL Server model compatibility with Sqlite

## [0.1.0]

### Added
- Initial release
- Builder pattern for creating `DbContext` instances with in-memory databases
- `SeedWith<T>` for seeding specific test data
- `SeedWithRandom<T>` for seeding random test data via AutoFixture
- Support for EF Core 6, 7, 8, and 9
- Support for Entity Framework 6 (classic) on .NET Framework 4.6.2--4.8.1
- `Wolfgang.DbContextBuilder-EF6` package for Entity Framework 6

[Unreleased]: https://github.com/Chris-Wolfgang/DbContextBuilder/compare/v0.7.0...HEAD
[0.7.0]: https://github.com/Chris-Wolfgang/DbContextBuilder/compare/v0.6.2...v0.7.0
[0.6.2]: https://github.com/Chris-Wolfgang/DbContextBuilder/compare/v0.6.1...v0.6.2
[0.6.1]: https://github.com/Chris-Wolfgang/DbContextBuilder/compare/v0.6.0...v0.6.1
[0.6.0]: https://github.com/Chris-Wolfgang/DbContextBuilder/compare/v0.5.0...v0.6.0
[0.5.0]: https://github.com/Chris-Wolfgang/DbContextBuilder/compare/v0.4.0...v0.5.0
[0.4.0]: https://github.com/Chris-Wolfgang/DbContextBuilder/compare/v0.3.3...v0.4.0
[0.3.3]: https://github.com/Chris-Wolfgang/DbContextBuilder/compare/v0.3.2...v0.3.3
[0.3.2]: https://github.com/Chris-Wolfgang/DbContextBuilder/compare/v0.3.1...v0.3.2
[0.3.1]: https://github.com/Chris-Wolfgang/DbContextBuilder/compare/v0.3.0...v0.3.1
[0.3.0]: https://github.com/Chris-Wolfgang/DbContextBuilder/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/Chris-Wolfgang/DbContextBuilder/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/Chris-Wolfgang/DbContextBuilder/releases/tag/v0.1.0
