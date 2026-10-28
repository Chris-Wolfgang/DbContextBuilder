type: docs

XML documentation fixes: the duplicate `ObjectDisposedException` tags on `UseSqlite`/`UseSqliteForMsSqlServer` are gone, the three `SeedWithRandom` overloads share one remarks block, the EF6 and provider packages describe their fluent return value as "The builder, for chaining." like the EF Core builder, and the builder remarks say that selecting a provider again (the same or a different one) starts a new, empty database.
