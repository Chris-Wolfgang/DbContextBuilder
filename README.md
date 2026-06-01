# DbContextBuilder

Uses the `Builder` pattern to create Entity Framework Core and classic Entity Framework `DbContext` instances using an in-memory database for testing purposes. With DbContextBuilder, you can easily set up a `DbContext` with predefined data, making it ideal for unit tests and integration tests without having to rely on an actual database whose data can be changed or deleted over time.

[![NuGet](https://img.shields.io/nuget/v/Wolfgang.DbContextBuilder-Core.svg?logo=nuget&label=NuGet)](https://www.nuget.org/packages/Wolfgang.DbContextBuilder-Core)
[![NuGet downloads](https://img.shields.io/nuget/dt/Wolfgang.DbContextBuilder-Core.svg?logo=nuget&label=downloads)](https://www.nuget.org/packages/Wolfgang.DbContextBuilder-Core)
[![PR build](https://img.shields.io/github/actions/workflow/status/Chris-Wolfgang/DbContextBuilder/pr.yaml?event=pull_request_target&label=PR%20build&logo=github)](https://github.com/Chris-Wolfgang/DbContextBuilder/actions/workflows/pr.yaml)
[![Release](https://img.shields.io/github/actions/workflow/status/Chris-Wolfgang/DbContextBuilder/release.yaml?label=release&logo=github)](https://github.com/Chris-Wolfgang/DbContextBuilder/actions/workflows/release.yaml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Multi--Targeted-purple.svg)](https://dotnet.microsoft.com/)
[![GitHub](https://img.shields.io/badge/GitHub-Repository-181717?logo=github)](https://github.com/Chris-Wolfgang/DbContextBuilder)

---

## 📦 Installation

DbContextBuilder ships as a family of packages — install the one that matches your EF flavor:

| Package | EF flavor | Notes |
|---|---|---|
| [`Wolfgang.DbContextBuilder-Core`](https://www.nuget.org/packages/Wolfgang.DbContextBuilder-Core) | EF Core 6 | Currently pins EF Core 6 (`[6.0.36, 7.0.0)`). EF Core 7-10 consumers should install the matching `-Core-EF7/8/9/10` package below. |
| [`Wolfgang.DbContextBuilder-Core-EF6`](https://www.nuget.org/packages/Wolfgang.DbContextBuilder-Core-EF6) | EF Core 6 | Pins EF Core 6 for projects on the LTS 6.x line. |
| [`Wolfgang.DbContextBuilder-Core-EF7`](https://www.nuget.org/packages/Wolfgang.DbContextBuilder-Core-EF7) | EF Core 7 | Pins EF Core 7. |
| [`Wolfgang.DbContextBuilder-Core-EF8`](https://www.nuget.org/packages/Wolfgang.DbContextBuilder-Core-EF8) | EF Core 8 | Pins EF Core 8 (LTS). |
| [`Wolfgang.DbContextBuilder-Core-EF9`](https://www.nuget.org/packages/Wolfgang.DbContextBuilder-Core-EF9) | EF Core 9 | Pins EF Core 9. |
| [`Wolfgang.DbContextBuilder-Core-EF10`](https://www.nuget.org/packages/Wolfgang.DbContextBuilder-Core-EF10) | EF Core 10 | Pins EF Core 10. |
| [`Wolfgang.DbContextBuilder-EF6`](https://www.nuget.org/packages/Wolfgang.DbContextBuilder-EF6) | Classic EF 6 (non-Core) | For applications still on `System.Data.Entity` / EF 6.x. |

```bash
# Pick whichever matches your project's EF version
dotnet add package Wolfgang.DbContextBuilder-Core-EF8
```

### Target frameworks

Each package targets the .NET runtimes its EF version requires:

| Package | Target Frameworks |
|---|---|
| `Wolfgang.DbContextBuilder-Core` / `-Core-EF6` | `net6.0` |
| `Wolfgang.DbContextBuilder-Core-EF7` | `net7.0` |
| `Wolfgang.DbContextBuilder-Core-EF8` | `net8.0` |
| `Wolfgang.DbContextBuilder-Core-EF9` | `net9.0` |
| `Wolfgang.DbContextBuilder-Core-EF10` | `net10.0` |
| `Wolfgang.DbContextBuilder-EF6` (classic) | `net462; net47; net471; net472; net48; net481` |

---

## 📄 License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

---

## 📚 Documentation

- **GitHub Repository:** [https://github.com/Chris-Wolfgang/DbContextBuilder](https://github.com/Chris-Wolfgang/DbContextBuilder)
- **API Documentation:** https://Chris-Wolfgang.github.io/DbContextBuilder/
- **CHANGELOG:** [CHANGELOG.md](CHANGELOG.md)
- **Contributing Guide:** [CONTRIBUTING.md](CONTRIBUTING.md)
- **DocFX Version Picker Troubleshooting:** [docs/DOCFX-VERSION-PICKER.md](docs/DOCFX-VERSION-PICKER.md)

---

## ✨ Features

- **In-memory `DbContext` for tests.** By default, DbContextBuilder uses the EF Core InMemory provider. Pass your own `DbContextOptionsBuilder` to switch to SQLite in-memory or any other provider.

- **Seed with your own data** using `.SeedWith<T>(...)` — accepts an `IEnumerable<T>` or a `params T[]`.

- **Seed with random data** using `.SeedWithRandom<T>(count)` to simulate real-world databases where additional rows beyond your test fixtures exist. Plug in a custom `ICreateRandomEntities` to control how the random rows are generated.

- **Composable.** Chain `SeedWith`, `SeedWithRandom`, and provider options in any order, then call `.BuildAsync()` to materialize the `DbContext`.

---

## 🚀 Usage

```csharp
// Create a DbContext with seeded random data and your test data
var context = await new DbContextBuilder<YourDbContext>()
    // Seed with 10 random entities
    .SeedWithRandom<YourEntity>(10)

    // Seed with specific data
    .SeedWith
    (
        new YourEntity
        {
            Id = 1,
            Name = "Test Entity"
        }
    )

    // Seed with 5 more random entities
    .SeedWithRandom<YourEntity>(5)

    // Build the DbContext instance
    .BuildAsync();

// Use the context in your tests
var sut = new YourService(context);
```

---

## 📖 API surface

The Core package exposes a small, focused surface. The full reference is on the [docs site](https://Chris-Wolfgang.github.io/DbContextBuilder/api/Wolfgang.DbContextBuilderCore.html); the most-used entry points are:

| Type / Method | What it does |
|---|---|
| `DbContextBuilder<T>` | The builder itself. Construct one per test, chain configuration, then call `BuildAsync()`. |
| `.UseInMemory()` | EF Core InMemory provider. Fastest, ignores relational constraints. |
| `.UseSqlite()` | SQLite in-memory provider. Enforces relational constraints. |
| `.UseSqliteForMsSqlServer()` | SQLite in-memory with a SQL-Server compatibility layer that flattens schema-qualified table names and rewrites SQL-Server-specific default-value SQL. |
| `.UseAutoFixture()` | Plug AutoFixture in as the random-entity generator (used by `SeedWithRandom`). |
| `.UseDbContextOptionsBuilder(opts)` | Bring your own `DbContextOptionsBuilder<T>` to override the provider entirely. |
| `.UseCustomRandomEntityCreator(creator)` | Plug in any `ICreateRandomEntities` implementation. |
| `.SeedWith<TEntity>(...)` | Seed specific rows. Accepts `IEnumerable<T>` or `params T[]`. |
| `.SeedWithRandom<TEntity>(count, [func])` | Seed N random rows. Optional `func` mutates each generated entity. |
| `.BuildAsync()` | Materialize the `DbContext`. The builder owns the underlying connection; dispose the context with `await using`. |
| `SqliteModelCustomizer` | Customization hooks for the SQLite-for-SQL-Server mode: `OverrideTableRenaming`, `OverrideDefaultValueHandling`, `OverrideComputedValueHandling`, `OverrideManyToManyTableHandling`, `DefaultValueMap`. |
| `ICreateDbContext` / `ICreateRandomEntities` | Extension points for plugging in your own provider or random-entity generator. |
