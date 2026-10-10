# Migrating from vX to vY

> One-paragraph summary: what changed at a high level and roughly how much work
> the upgrade is for a typical consumer.

## At a glance

| | |
|---|---|
| **From** | vX.y.z |
| **To** | vY.0.0 |
| **Estimated effort** | Trivial / Moderate / Significant |
| **Database migration required?** | Yes / No |
| **Runtime behaviour change?** | Yes / No |

## Breaking-change inventory

| # | Change | Kind | Action required |
|---|---|---|---|
| 1 | `OldMethod(...)` removed | API removal | Replace with `NewMethod(...)` — see below |
| 2 | `DbContextBuilder<T>.SomeOption` default changed | Behaviour | Set the option explicitly if you relied on the old default |
| 3 | A new required constructor argument | Signature change | Update call sites — see below |

## Before / after

### 1. `OldMethod` → `NewMethod`

```csharp
// Before (vX)
var context = await new DbContextBuilder<AppContext>()
    .OldMethod(...)
    .BuildAsync();

// After (vY)
var context = await new DbContextBuilder<AppContext>()
    .NewMethod(...)
    .BuildAsync();
```

## Database changes

Describe any schema or provider-behavior changes (e.g. SQLite model customization,
default-value handling) and exactly how to apply them — EF migration or hand-run
DDL. Include rollback notes.

## Recommended upgrade order

1. Bump the package version(s) — the nine packages ship in lockstep, so move every
   `Wolfgang.DbContextBuilder*` reference you have to the same version.
2. Fix compile errors using the before/after table above.
3. Apply any database migration.
4. Run your test suite.

## Deprecations (not yet removed)

List anything marked `[Obsolete]` in this release so consumers can migrate ahead
of the *next* major.
