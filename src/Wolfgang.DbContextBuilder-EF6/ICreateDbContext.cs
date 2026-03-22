using System;
using System.Data.Entity;

namespace Wolfgang.DbContextBuilderEF6;

/// <summary>
/// Provides an API to create instances of <see cref="DbContext"/> for testing.
/// </summary>
public interface ICreateDbContext : IDisposable
{
    /// <summary>
    /// Creates a new instance of the specified <typeparamref name="TDbContext"/> type.
    /// </summary>
    /// <typeparam name="TDbContext">The type of DbContext to create.</typeparam>
    /// <returns>A new instance of <typeparamref name="TDbContext"/>.</returns>
    TDbContext CreateDbContext<TDbContext>() where TDbContext : DbContext;
}
