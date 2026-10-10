# Introduction

DbContextBuilder is a fluent builder for Entity Framework `DbContext` instances intended for use in tests. It wraps the boilerplate of configuring an in-memory provider, seeding data, and building a context into a single chainable API so test setup looks like the data the test cares about — not the plumbing.

## Why a builder?

Tests that need a `DbContext` usually need one with:

- A specific provider (InMemory, SQLite in-memory, etc.) so the test does not touch a real database
- Specific rows of specific entities so the assertions are predictable
- Sometimes additional random rows so the SUT sees realistic data volumes
- An options builder configured exactly the way the test scenario expects

Wiring those up manually for every test costs a lot of repeated lines. `DbContextBuilder<T>` collapses them into:

```csharp
using var builder = new DbContextBuilder<MyDbContext>();
await using var context = await builder
    .UseInMemory()
    .UseAutoFixture()                 // a random-data provider is required for SeedWithRandom
    .SeedWith(new User { Id = 1, Name = "Alice" })
    .SeedWithRandom<Order>(20)
    .BuildAsync();
```

The same builder works against the InMemory provider, SQLite in-memory, and a SQL-Server-compatibility SQLite mode that flattens schema-qualified table names so a typical production `DbContext` can be exercised without code changes.

## Package family

DbContextBuilder ships one package per supported EF version so you can install only what you need:

- `Wolfgang.DbContextBuilder-Core-EF6` through `-Core-EF10` — pinned to a single EF Core major
- `Wolfgang.DbContextBuilder-EF6` — for projects still on classic Entity Framework 6 (`System.Data.Entity`)

Pick the one that matches your project's EF flavor. With the EF Core packages, `SeedWithRandom`
also needs a random-data provider package next to it. The classic `Wolfgang.DbContextBuilder-EF6`
package keeps its built-in AutoFixture and needs neither:

- `Wolfgang.DbContextBuilder.AutoFixture` — adds `.UseAutoFixture()`
- `Wolfgang.DbContextBuilder.Bogus` — adds `.UseBogus()` (realistic fake values)

Both build on `Wolfgang.DbContextBuilder.Abstractions` (the shared `ICreateRandomEntities`), which
comes in as a dependency. See the [README](https://github.com/Chris-Wolfgang/DbContextBuilder) for the full installation matrix.

## Where to go next

- [Getting started](getting-started.md) — install, build a first `DbContext`, run a first test
- [API](../api/Wolfgang.DbContextBuilderCore.yml) — generated reference for every public type
