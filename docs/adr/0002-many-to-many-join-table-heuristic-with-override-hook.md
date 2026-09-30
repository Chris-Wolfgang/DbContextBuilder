# 0002 — Many-to-many join table detection is a documented heuristic with a public override hook, not exhaustive metadata inspection

- **Status:** Accepted
- **Date:** 2026-09-28
- **Deciders:** Chris Wolfgang

## Context and problem statement

`SqliteModelCustomizer` renames tables for the SQLite test provider, including
implicit many-to-many join tables EF Core generates for skip-navigation
relationships. EF Core's model metadata does not expose "this entity type is a
join table" as a first-class flag — there is no API that answers the question
with certainty for every possible model shape (composite keys, join entities
with extra columns, self-referencing many-to-many, three-or-more-FK junction
tables that aren't really M:N joins). Detecting join tables purely from
`IMutableEntityType` metadata means choosing an inference rule, and every
inference rule has a model shape it gets wrong (#152, #123).

## Considered options

- **Try to make the heuristic exhaustively correct** by enumerating more
  model-shape edge cases (composite key detection, checking for a specific EF
  Core-generated naming pattern, inspecting the CLR type for
  `[Keyless]`/anonymous-entity markers). Each additional case narrows the
  false-positive/false-negative surface but can never close it entirely —
  EF Core's model builder supports enough flexibility that a new edge case can
  always be constructed to defeat the next heuristic refinement.
- **Require the consumer to explicitly declare every join table** (e.g. via a
  fluent configuration call naming each one). Fully correct by construction,
  but pushes boilerplate onto every consumer for the common case, defeating
  the point of an automatic customizer.
- **Keep a simple, documented heuristic (exactly two foreign keys and no
  navigations) as the default, and expose it as an overridable public
  delegate** (`OverrideManyToManyTableHandling`) so a consumer whose model
  defeats the heuristic can supply their own detection logic without forking
  the library.

## Decision

**The default many-to-many detection heuristic stays simple and documented
(two foreign keys, no navigations → rename to `{Left}_{Right}`), and
`OverrideManyToManyTableHandling` is a public, settable
`Action<IMutableEntityType>` property so any consumer can replace the default
behavior entirely for their own model.** The XML doc on the property states the
heuristic's exact rule and points to the override as the escape hatch, rather
than promising the heuristic handles every model shape.

### Rationale

- A heuristic that is simple enough to state in one sentence is also simple
  enough for a consumer to reason about when it's wrong for their model —
  "exactly two FKs, no navigations" is easy to check against your own entity
  types; a heuristic with a dozen special cases is not.
- The override hook means the library never has to chase every real-world
  model shape to stay useful — a consumer with a self-referencing join, a
  three-FK junction table, or a join entity with extra columns just supplies
  their own `Action<IMutableEntityType>` instead of waiting for (or forking to
  get) a library update.
- This mirrors the same pattern already used for `OverrideDefaultValueHandling`,
  `OverrideComputedValueHandling`, and `OverrideTableRenaming` — the
  customizer's whole design is "sensible default + override point," not
  "the library tries to solve every case internally."

## Consequences

- **Positive:** the default heuristic's tests (self-referencing joins, 3-FK
  entities, entities with navigations — added in #260) only need to prove the
  *documented* rule behaves as documented, not that it's correct for every
  conceivable model; correctness for an unusual model is the consumer's
  responsibility once they override it.
- **Negative:** a consumer who doesn't read the XML doc and hits a model shape
  the default heuristic gets wrong sees an incorrectly-named table rather than
  an error — there's no detection of "the heuristic probably guessed wrong
  here," just silent best-effort renaming.
- **Follow-ups:** none scheduled. If a specific wrong-guess pattern turns out
  to be common in practice, the default heuristic can be refined without
  breaking the override hook — the property signature doesn't change either
  way.
