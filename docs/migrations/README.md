# Migration guides

When a **major** version ships (a release with breaking changes — `0.x → 1.0`
with breaks, or `1.x → 2.0`), it gets a migration guide here so consumers have a
written upgrade path.

No major version with breaking changes has shipped yet, so this folder currently
holds only the template. When the first breaking release lands, copy
[`TEMPLATE-major-version-migration.md`](TEMPLATE-major-version-migration.md) to
`v1-to-v2.md` (or the appropriate name) and fill it in.

Breaking changes are also surfaced in [`../../CHANGELOG.md`](../../CHANGELOG.md);
the migration guide is the *narrative* companion — before/after code, rename
tables, and the recommended upgrade order.
