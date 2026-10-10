type: feature

New `DbContextBuilder<T>.UseCustomDbContextCreator(ICreateDbContext)` installs your own `ICreateDbContext` (for a provider the builder does not ship); the builder takes ownership of it and disposes it when it is replaced or the builder is disposed.
