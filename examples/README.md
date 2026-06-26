# Examples

Real-world, runnable examples of using **DbContextBuilder** in integration tests.

Every example is an xUnit test in
[`Wolfgang.DbContextBuilder.Examples`](Wolfgang.DbContextBuilder.Examples). They
share a small but realistic shop domain (`Customer`, `Product`, `Order`,
`OrderLine`, `ShopDbContext`) and an `OrderService` that plays the role of the
"code under test". Each example builds a seeded `ShopDbContext`, hands it to the
service, and asserts on the result — exactly what you would do in your own suite.

The project references the published NuGet package
(`Wolfgang.DbContextBuilder-Core-EF8`), so it mirrors what a consumer's project
looks like. Swap the package for the one matching your EF Core version
(`-Core-EF6` … `-Core-EF10`).

## Run them

```bash
dotnet test examples/Wolfgang.DbContextBuilder.Examples
```

## The examples

| # | File | Shows |
|---|------|-------|
| 1 | [Building a context](Wolfgang.DbContextBuilder.Examples/Examples/Example01_BuildingAContext.cs) | Get a fresh, isolated in-memory `DbContext` in one call. |
| 2 | [Seeding specific test data](Wolfgang.DbContextBuilder.Examples/Examples/Example02_SeedingSpecificData.cs) | Seed a known object graph and assert exact values. |
| 3 | [Seeding random data](Wolfgang.DbContextBuilder.Examples/Examples/Example03_SeedingRandomData.cs) | Fill a table with `SeedWithRandom<T>` for realistic volume. |
| 4 | [Relational behaviour with SQLite](Wolfgang.DbContextBuilder.Examples/Examples/Example04_RelationalConstraintsWithSqlite.cs) | Use `UseSqlite()` when you need real SQL semantics. |
| 5 | [Shaping random data](Wolfgang.DbContextBuilder.Examples/Examples/Example05_ShapingRandomData.cs) | Force the fields a test cares about with the per-item `SeedWithRandom` overload. |
| 6 | [Fluent DbSet assertions](Wolfgang.DbContextBuilder.Examples/Examples/Example06_FluentDbSetAssertions.cs) | Assert on a `DbSet` directly with `.Should().HaveCount(...)` etc. |

## The one-liner

The whole library boils down to this:

```csharp
await using var context = await new DbContextBuilder<ShopDbContext>()
    .UseInMemory()                       // or .UseSqlite() / .UseSqliteForMsSqlServer()
    .SeedWith(new Customer { Name = "Alice" })
    .SeedWithRandom<Product>(50)
    .BuildAsync();
```
