type: fix

`BuildAsync()` can be called more than once on one builder: the first call creates and seeds the database, later calls return another context over it instead of re-seeding (which failed on a duplicate key), and the EF Core service provider is built once and disposed with the builder. Seeding methods called after the database was seeded now throw `InvalidOperationException`.
