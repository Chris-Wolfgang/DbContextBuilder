type: fix

`UseSqlite()` and `UseSqliteForMsSqlServer()` now leave exactly one `IModelCustomizer` registered, instead of also keeping EF Core's default one, and selecting SQLite again no longer re-registers EF Core's SQLite services.
