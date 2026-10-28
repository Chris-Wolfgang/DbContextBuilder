type: fix

The classic EF6 `DbContextBuilder<T>` seeds the database only on the first `Build()`/`BuildAsync()` call: later builds on the same builder return another context over the already-seeded database instead of inserting the seed data again, and the `SeedWith*` methods throw `InvalidOperationException` after the first build (selecting the provider again with `UseEffort()` starts a new, empty database), matching the EF Core builder.
