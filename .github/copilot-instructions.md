# DbContextBuilder — Copilot instructions

Read these first; fall back to searching the repository only when they are incomplete or wrong.
`README.md` (usage and API table) and `CONTRIBUTING.md` (analyzers and build) are the longer
references.

## What the library is

A builder for Entity Framework Core and classic Entity Framework 6 `DbContext` instances backed by an
in-memory database, for tests: choose a provider, seed specific and/or random rows, then
`await BuildAsync()`.

**Shipped packages (9)**

| Package | Targets | Notes |
|---|---|---|
| `Wolfgang.DbContextBuilder-Core-EF6` … `-Core-EF10` | net6.0 … net10.0 (one each) | Each pins one EF Core major. |
| `Wolfgang.DbContextBuilder-EF6` | net462–net481 | Classic EF 6 on Effort; has its own `ICreateRandomEntities` (ADR-0004). |
| `Wolfgang.DbContextBuilder.Abstractions` | netstandard2.0; net10.0 | Shared `ICreateRandomEntities`. |
| `Wolfgang.DbContextBuilder.AutoFixture` / `.Bogus` | net6.0–net10.0 | Random-data providers: `UseAutoFixture()` / `UseBogus()`. |

`Wolfgang.DbContextBuilder-Core` is retired (`IsPackable=false`, ADR-0003) but still the **canonical
source**: the `-Core-EFx` projects link its `.cs` files (ADR-0001). Edit code in
`src/Wolfgang.DbContextBuilder-Core/`, never in a wrapper.

## Public surface (EF Core builder)

`DbContextBuilder<T>`: `UseInMemory()`, `UseSqlite()`, `UseSqliteForMsSqlServer()`,
`UseDbContextOptionsBuilder(...)`, `UseCustomRandomEntityCreator(...)`, `UseSeedProfile(...)`,
`UseDiagnosticOutput(...)`, `SeedWith(...)` (single entity, `params T[]`, `IEnumerable<T>`),
`SeedWithRandom<T>(count[, func])`, `BuildAsync()`, `Dispose()`. There is no synchronous `Build()`
on the EF Core builder (the classic EF 6 builder has `Build()` and `BuildAsync()`).

- `SeedWithRandom` throws `InvalidOperationException` unless a random-data provider is configured.
- The first `BuildAsync()` creates and seeds the database; later calls return new contexts over it.
- The builder owns the provider resources (the SQLite connection, the EF service provider): dispose
  it after the last context.
- Fluent assertions: `using Wolfgang.DbContextBuilderCore.Assertions;` → `dbSet.Should().HaveCount(n)`
  etc.

```csharp
using var builder = new DbContextBuilder<ShopDbContext>();
await using var context = await builder
    .UseSqlite()
    .UseAutoFixture()
    .SeedWith(new Customer { Id = 1, Name = "Alice" })
    .SeedWithRandom<Order>(20)
    .BuildAsync();
```

## Layout

```
Wolfgang.DbContextBuilder.slnx        solution (add every new project to it)
src/                                  the packages above (+ the retired -Core source project)
tests/                                Core.Tests.Unit (shared sources, linked into Core-EF7..EF10
                                      wrappers), EF6, AutoFixture, Bogus, Concurrency (Coyote),
                                      Fuzz (FsCheck), DocExamples (compiles doc samples)
benchmarks/                           BenchmarkDotNet suite (charted on gh-pages /dev/bench/)
samples/                              shadow-testing workload (shadow.yaml)
examples/                             consumer-style examples against the published packages
extra-projects/                       scaffolded AdventureWorks models used by the tests
docfx_project/                        docs site; docs/adr/ holds the decision records
changelog/unreleased/                 changelog fragments (see below)
scripts/                              build-pr.ps1, changelog.ps1, tfm-parity.ps1, ...
```

A new test source file in `tests/Wolfgang.DbContextBuilder-Core.Tests.Unit/` must also be linked
(`<Compile Include=... Link=...>`) into each `tests/*-Core-EF7..EF10.Tests.Unit` wrapper.

## Build, test, gates

```bash
dotnet build Wolfgang.DbContextBuilder.slnx -c Release   # warnings are errors in Release
dotnet test  Wolfgang.DbContextBuilder.slnx -c Release
pwsh ./scripts/build-pr.ps1                               # pr.yaml's Windows stage locally
```

- **Coverage:** per assembly, **≥ 95 % line** for each `src/` package and **100 %** for each test
  assembly. A member no test executes is dead code: test it or delete it;
  `[ExcludeFromCodeCoverage]` is a last resort.
- **Mutation testing:** Stryker floors in `mutation-floors.json` (repository `break` in
  `stryker-config.json`); PRs are measured on the mutants they bring into scope.
- **Analyzers:** the .NET analyzers, SonarAnalyzer, Meziantou, Roslynator, AsyncFixer,
  VS Threading and BannedApiAnalyzers on every project, plus PublicApiAnalyzers on `src/`.
  `BannedSymbols.txt` bans synchronous I/O and blocking waits in `src/` — use the async APIs.
- **DevSkim and gitleaks** must report nothing.

## Rules a PR must follow

- **Changelog fragment:** any change under `src/` adds `changelog/unreleased/<name>.md` (first line
  `type: breaking|feature|fix|docs|internal`), or carries the `no-changelog` label.
- **Protected files:** a PR either touches only protected files or none. Protected:
  `.editorconfig`, `*.editorconfig`, `Directory.Build.props/targets`, `BannedSymbols.txt`,
  `coverlet.runsettings`, `.config/dotnet-tools.json`, `.gitleaks.toml`, `*.globalconfig`,
  `*.ruleset`, `*.DotSettings`, `.github/workflows/*`, `.github/license-audit/*.json`,
  `.github/requirements/*`, and `scripts/{changelog,tfm-parity,build-pr,third-party-notices}.ps1`.
- **Public API:** new members go in `PublicAPI.Unshipped.txt`. A Core change updates Core and every
  `-Core-EFx` copy.
- **Style:** Allman braces, file-scoped namespaces, three blank lines between members, multi-line
  argument lists with the parentheses on their own lines, tests named
  `MethodUnderTest_when_condition_expected_result`, xunit 2.9.3.
