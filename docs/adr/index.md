# Architecture Decision Records

This folder records the **non-obvious design decisions** behind
`Wolfgang.DbContextBuilder.*`, so the reasoning survives the PR that introduced
it.

Each record is a short, immutable document: it captures the context, the
decision, and the consequences at a point in time. When a decision is later
reversed, add a **new** ADR that supersedes the old one rather than editing
history.

Format: [MADR](https://adr.github.io/madr/) (lightweight). Start a new record by
copying [`TEMPLATE.md`](TEMPLATE.md) and giving it the next number. New ADRs
land alongside the PR that introduces the corresponding decision — the ADR is
part of the review.

| ADR | Title | Status |
|---|---|---|
| [0001](0001-ef-version-wrappers-link-shared-source.md) | EF-version wrapper packages link the same source files as -Core | Accepted |
| [0002](0002-many-to-many-join-table-heuristic-with-override-hook.md) | Many-to-many join table detection is a documented heuristic with a public override hook | Accepted |
| [0003](0003-core-package-deprecated-not-unpublished.md) | Wolfgang.DbContextBuilder-Core stopped shipping and is deprecated, never unpublished | Accepted |
| [0004](0004-ef6-keeps-its-own-icreaterandomentities.md) | The classic -EF6 package keeps its own ICreateRandomEntities | Proposed |
