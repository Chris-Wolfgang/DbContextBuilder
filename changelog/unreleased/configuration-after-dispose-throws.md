type: fix

Every configuration method on a disposed `DbContextBuilder<T>` (`UseInMemory`, `UseSqlite`, `SeedWith`, `SeedWithRandom` and the other `Use*` methods) now throws `ObjectDisposedException`, as `BuildAsync` already did, instead of accepting state, or opening a SQLite connection that is never released.
