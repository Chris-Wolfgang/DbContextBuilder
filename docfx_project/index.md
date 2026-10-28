---
_layout: landing
---

# DbContextBuilder


Uses the `Builder pattern` to create Entity Framework Core and classic Entity Framework `DbContext` instances
using an in-memory database for testing purposes. With DbContextBuilder, you can easily set up
a DbContext with predefined data, making it ideal for unit tests and integration tests without
having to rely on an actual database whose data can be changed or deleted over time.

## Where to next

- **[Documentation](docs/introduction.md)** — concepts and getting-started walkthrough
- **[Getting started](docs/getting-started.md)** — quickest path to a working test
- **[API Reference](api/Wolfgang.DbContextBuilderCore.yml)** — full member listing for the EF Core
  packages (`-Core-EF6` through `-Core-EF10` share one API, documented from `-Core-EF10`),
  `Wolfgang.DbContextBuilder.Abstractions`, `.AutoFixture` and `.Bogus`

> The classic `Wolfgang.DbContextBuilder-EF6` package (namespace `Wolfgang.DbContextBuilderEF6`) is
> not in the API reference: the docs are generated for `net10.0` on a Linux runner, and that package
> targets .NET Framework 4.6.2–4.8.1 only. Its public surface is small (`DbContextBuilder<T>`,
> `UseEffort`, `UseAutoFixture`, `ICreateDbContext`, `ICreateRandomEntities`); the
> [README](https://github.com/Chris-Wolfgang/DbContextBuilder#readme) and the XML docs shipped in the
> package cover it.
- **[GitHub repository](https://github.com/Chris-Wolfgang/DbContextBuilder)** — source, issues, releases
