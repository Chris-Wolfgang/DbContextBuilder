type: feature

`BuildAsync(CancellationToken)`, `ICreateDbContext.CreateDbContextAsync(optionsBuilder, CancellationToken)` (with a default implementation, so existing creators keep working) and a `CancellationToken` overload of every `DbSetAssertions` method; the token reaches database creation, the seed save, the context creator and the assertion queries.
