# 0003 — Wolfgang.DbContextBuilder-Core stopped shipping and is deprecated, never unpublished

- **Status:** Accepted
- **Date:** 2026-09-28
- **Deciders:** Chris Wolfgang

## Context and problem statement

The package family originally shipped a `net10.0`-only flagship
(`Wolfgang.DbContextBuilder-Core`) alongside five EF-version-pinned wrappers
(`-Core-EF{6,7,8,9,10}`) that link the exact same source (see
[ADR-0001](0001-ef-version-wrappers-link-shared-source.md)). The flagship's
`net10.0`-only, EF-version-unpinned shape duplicated what `-Core-EF10` already
provides, while giving consumers a package whose EF Core version compatibility
wasn't explicit in its name the way every `-Core-EF{N}` sibling's is (#362).

## Considered options

- **Keep shipping `-Core` alongside the EF-pinned siblings indefinitely.**
  Preserves the existing install command for anyone already depending on it,
  but perpetuates a package whose purpose (an EF-version-unpinned entry point)
  is redundant with `-Core-EF10` now that every consumer needs an EF-version
  choice anyway.
- **Unpublish (`nuget delete`) `-Core` from nuget.org.** Removes the redundant
  package outright, but breaks any build that still resolves a floating or
  pinned version of it — unpublishing pulls the package out from under
  restores, not just new installs, and NuGet.org itself discourages true
  deletion for exactly that reason.
- **Stop packing `-Core` going forward (`IsPackable=false`) and mark the last
  published version deprecated on nuget.org**, without unpublishing it.
  Existing consumers' restores keep working against the versions already on
  the feed; new consumers see a deprecation notice steering them to the
  EF-pinned sibling that matches their EF Core version.

## Decision

**`Wolfgang.DbContextBuilder-Core` stopped packing as of #362
(`IsPackable=false` in its `.csproj`, merged in #432) and its last published
version is marked deprecated on nuget.org — it is never unpublished.** The nine
remaining packages (`-Core-EF{6,7,8,9,10}`, `-EF6`, `.Abstractions`,
`.AutoFixture`, `.Bogus`) are the supported surface going forward.

### Rationale

- Deprecation without unpublishing is the standard, restore-safe way to retire
  a NuGet package — it's discoverable (a warning in the IDE and on the package
  page) without being a breaking change for anyone already pinned to a
  specific version.
- Every remaining package name already states its EF Core version compatibility
  explicitly (`-Core-EF8`, etc.); `-Core`'s name did not, which was a source of
  ambiguity this decision removes going forward without removing history.

## Consequences

- **Positive:** new consumers get an unambiguous, EF-version-explicit package
  name from the start; the deprecation notice on nuget.org actively points
  them at the right sibling instead of leaving them to discover the ambiguity
  themselves.
- **Negative:** a consumer who was floating on `-Core`'s latest version (rather
  than pinning) stops receiving updates silently — they only find out via the
  deprecation notice, not a build break. `SECURITY.md`'s "Release path &
  compromise scope" appendix (#315) explicitly excludes `-Core` from the
  package-coordinates list for this reason: it is not part of the actively
  shipped set.
- **Follow-ups:** the nuget.org deprecation flag itself is a manual step in the
  package-management UI, not something CI can set — tracked as the one
  remaining action item on #362.
