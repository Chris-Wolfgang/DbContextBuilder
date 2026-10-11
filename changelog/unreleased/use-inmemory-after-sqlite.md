type: fix

`UseInMemory()` called after `UseSqlite()` or `UseSqliteForMsSqlServer()` now builds a working InMemory context instead of failing, because the SQLite services the earlier call registered are dropped.
