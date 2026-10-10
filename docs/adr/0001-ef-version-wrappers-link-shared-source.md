# 0001 — EF-version wrapper packages link the same source files as -Core, rather than each shipping their own copy or depending on a shared library

- **Status:** Accepted
- **Date:** 2026-09-28
- **Deciders:** Chris Wolfgang

## Context and problem statement

`Wolfgang.DbContextBuilder-Core` targets `net10.0` only. Consumers on an older EF
Core major (6 through 9) still need the same `DbContextBuilder<T>` API, but the
EF Core API surface has enough cross-version incompatibility (provider APIs,
`ModelCustomizer` base members, nullable annotations) that a single assembly
can't multi-target all five majors with `#if` alone without either a maintenance
burden of conditional compilation scattered through every file, or picking one
EF version and leaving the others unserved.

## Considered options

- **A shared class-library package** (e.g. `Wolfgang.DbContextBuilder.Shared`)
  that `-Core` and each `-Core-EF{6,7,8,9,10}` package reference. Standard
  approach for code reuse, but it adds a NuGet dependency edge to every
  EF-version package, and the shared library itself would still need `#if
  EF_CORE_6` / `EF_CORE_7` / ... branches internally for the parts of the EF
  Core surface that actually differ per version — the conditional compilation
  problem doesn't go away, it just moves.
- **Independent copies per EF-version project**, hand-maintained. No shared
  dependency, but every bug fix or behavior change has to be manually
  replicated across up to 6 projects (`-Core` plus 5 EF-pinned siblings), and
  they silently drift the moment one copy is forgotten.
- **`<Compile Include>` linking**: each `-Core-EF{6,7,8,9,10}` project has no
  `.cs` files of its own for the shared API surface. Instead its `.csproj`
  links the exact same files that live under `-Core`'s directory (`<Compile
  Include="..\Wolfgang.DbContextBuilder-Core\DbContextBuilder.cs"
  Link="DbContextBuilder.cs" />` and so on), each compiled fresh against that
  project's own `TargetFramework` and EF Core `PackageReference` version range.
  A `EF_CORE_{N}` / `EF_CORE_{N}_OR_GREATER` `DefineConstants` pair lets the
  handful of genuinely version-specific lines branch with `#if` inside the
  single shared source file, rather than duplicating whole files.

## Decision

**EF-version wrapper packages (`-Core-EF6` through `-Core-EF10`) link `-Core`'s
source files directly via `<Compile Include>`, and do not ship independent
copies or depend on a shared library package.** Each sibling project:

- Sets `RootNamespace` to `Wolfgang.DbContextBuilderCore` (the same namespace
  as `-Core`) so the linked files compile unmodified.
- Defines `EF_CORE_{N}` / `EF_CORE_{N}_OR_GREATER` constants for the rare lines
  that need to branch on EF Core major version.
- Pins its own `Microsoft.EntityFrameworkCore*` `PackageReference` range (e.g.
  `[8.0.25,9.0.0)` for `-Core-EF8` when this record was written; the lower bound moves with
  EF Core patch releases) so a consumer gets a working, version-locked
  combination rather than a floating range that could resolve to an
  incompatible EF Core minor.

### Rationale

- There is exactly one copy of the actual logic to read, review, and fix — a
  bug found in `-Core`'s `DbContextBuilder.cs` is fixed in one file and every
  sibling picks it up on its next build, with no manual propagation step.
- `PublicAPI.Shipped.txt` for every `-Core-EF{N}` sibling is required to stay
  byte-identical to `-Core`'s —
  file linking makes that an automatic consequence of the approach rather than
  a rule that has to be separately enforced.
- A shared-library package would have solved code reuse but not the
  version-branching problem, while adding a dependency edge every consumer
  pays for. Linking solves both: no extra dependency, and the `#if` branches
  live inline where the version-specific behavior actually is.

## Consequences

- **Positive:** single source of truth for the shared API; new EF-version
  siblings are mechanical to add (new `.csproj`, same `<Compile Include>`
  list, new pinned EF Core version range).
- **Negative:** the `<Compile Include>` list in each sibling `.csproj` must be
  kept in sync by hand whenever a file is added to or removed from `-Core` —
  there's no tooling that enforces this today, so a forgotten entry is a
  silent gap (the missing type just doesn't exist in that sibling's build).
  `PublicAPI.Shipped.txt` sync across siblings has the same manual-`cp`
  characteristic.
- **Follow-ups:** a build-time check that every sibling's `<Compile Include>`
  list matches `-Core`'s actual file set (or a shared `.props` file that
  generates the list) would close the "forgotten entry" gap; not built yet
  because it hasn't caused an actual incident.
