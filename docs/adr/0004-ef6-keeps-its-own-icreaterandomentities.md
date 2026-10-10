# 0004 — The classic -EF6 package keeps its own ICreateRandomEntities

- **Status:** Proposed
- **Date:** 2026-10-10
- **Deciders:** Chris Wolfgang

## Context and problem statement

`Wolfgang.DbContextBuilder.Abstractions` exists so the EF Core packages and the
random-data add-ons (`.AutoFixture`, `.Bogus`) share one
`Wolfgang.DbContextBuilderCore.ICreateRandomEntities`. The classic Entity
Framework package, `Wolfgang.DbContextBuilder-EF6`, does not reference
Abstractions. It declares a second interface,
`Wolfgang.DbContextBuilderEF6.ICreateRandomEntities`, with the same single member
(`IEnumerable<TEntity> CreateRandomEntities<TEntity>(int count)`) and the same
contract. Its builder's `UseCustomRandomEntityCreator` and its built-in
AutoFixture creator use that EF6 interface. Until this record, nothing said
whether the duplication was deliberate (#615).

Abstractions targets `netstandard2.0`, which `-EF6`'s `net462`–`net481` targets
can consume, so unification is technically possible.

## Considered options

- **Keep the EF6 interface.** No change for EF6 consumers. Keep the two
  interfaces' contracts and XML docs in lock-step by hand.
- **Unify on the Abstractions interface at the next EF6 minor version.**
  Reference Abstractions from `-EF6` and add a `UseCustomRandomEntityCreator`
  overload that takes the shared interface. Staying binary-safe needs two more
  things:
  - **Keep the EF6 interface unchanged, member included**, marked `[Obsolete]`.
    An interface cannot be forwarded to one with a different full name. Turning
    it into an interface derived from the shared one, with the member moved to
    the base, would break every existing *explicit* implementation, which names
    the EF6 interface's own member.
  - **Keep the existing `UseCustomRandomEntityCreator(Wolfgang.DbContextBuilderEF6.ICreateRandomEntities)`
    overload**, also `[Obsolete]`. Compiled callers bind to that exact signature.
    It wraps its argument in a small adapter that implements the shared
    interface by delegating to the EF6 one, and then goes down the new path.
- **Unify in a patch release.** Rejected outright: changing the type
  `UseCustomRandomEntityCreator` accepts is a source and binary break for every
  EF6 consumer with a custom creator.

## Decision

Proposed: **keep the EF6 interface** for now. The duplication is intentional
and recorded here. Revisit unification only together with other EF6 surface
changes, at an EF6 minor version, if a consumer needs one creator type to serve
both the EF Core and the EF6 builders.

### Rationale

- Classic EF6 consumers and EF Core consumers rarely share test projects, so a
  single creator type serving both buys little.
- Any unification changes the parameter type of a public EF6 method, so it
  cannot be done without a break or a compatibility shim. Keeping the
  interface costs one duplicated, rarely-changing file.

## Consequences

- **Positive:** no break for EF6 consumers; `-EF6` keeps its small dependency
  set.
- **Negative:** two interfaces with the same contract. A change to one must be
  mirrored in the other, including its XML documentation.
- **Follow-ups:** if unification is chosen, supersede this record with a new ADR
  in the EF6 minor release that does it.
