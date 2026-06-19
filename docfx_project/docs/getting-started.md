# Getting Started

## 1. Install the package that matches your EF version

```bash
# Pick whichever matches your project's EF version
dotnet add package Wolfgang.DbContextBuilder-Core-EF8
```

For the full version-to-package mapping, see the [Installation table in the README](https://github.com/Chris-Wolfgang/DbContextBuilder#-installation).

## 2. Build your first DbContext

```csharp
using Wolfgang.DbContextBuilderCore;

await using var context = await new DbContextBuilder<MyDbContext>()
    .UseInMemory()
    .SeedWith(new User { Id = 1, Name = "Alice" })
    .BuildAsync();

var sut = new UserService(context);
var user = await sut.GetByIdAsync(1);

Assert.Equal("Alice", user.Name);
```

`BuildAsync()` returns a real `MyDbContext` instance whose state matches the seeds you provided. The builder owns the underlying connection; `await using` disposes both.

## 3. Pick a provider

The builder ships three providers out of the box:

- `.UseInMemory()` — EF Core's InMemory provider. Fastest, but ignores relational constraints (foreign keys, indexes, default values). Good for unit tests of code that doesn't depend on SQL semantics.
- `.UseSqlite()` — SQLite in-memory. Enforces relational constraints. Each builder uses a unique in-memory database; contexts built from the same builder share state.
- `.UseSqliteForMsSqlServer()` — SQLite in-memory with a compatibility layer that flattens `[Schema].[Table]` to `Schema_Table` and rewrites SQL-Server-specific default-value SQL (`getdate()`, `newid()`, etc.) into SQLite equivalents. Lets you exercise a production SQL-Server `DbContext` without modification.

## 4. Seed with data

Two ways to seed:

```csharp
// Specific data — for predictable assertions
.SeedWith(new User { Id = 1, Name = "Alice" })
.SeedWith(new[] { user1, user2, user3 })

// Random data — for realistic row counts. Uses AutoFixture under the hood;
// plug in a custom ICreateRandomEntities to control generation.
.SeedWithRandom<Order>(count: 20)
```

`SeedWith` and `SeedWithRandom` are chainable in any order. The builder accumulates all seeds and inserts them when `BuildAsync()` runs.

## 5. Customize the model (SQLite for SQL Server only)

`SqliteModelCustomizer` exposes hooks to override the schema-renaming heuristic, the default-value SQL translation, the computed-column handling, and the many-to-many join-table renaming. See the [API reference](../api/Wolfgang.DbContextBuilderCore.SqliteModelCustomizer.html) for the exact signatures.

## Where to go next

- [API reference](../api/Wolfgang.DbContextBuilderCore.html) — every public type and method
- [GitHub repository](https://github.com/Chris-Wolfgang/DbContextBuilder) — source, issues, releases
