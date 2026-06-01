using System;
using System.Data.Entity;

namespace Wolfgang.DbContextBuilderEF6;

/// <summary>
/// Provides an API to create instances of <see cref="DbContext"/> for testing. Implementations
/// own any test-only infrastructure they allocate (in-memory connections, AutoFixture
/// instances, etc.) and must release it from <see cref="IDisposable.Dispose"/>.
/// </summary>
/// <remarks>
/// Implementations are expected to be disposed by the caller. The contexts they return are
/// owned by the caller and disposed independently of the creator itself.
/// </remarks>
public interface ICreateDbContext : IDisposable
{
    /// <summary>
    /// Creates a new instance of the specified <typeparamref name="TDbContext"/> type.
    /// </summary>
    /// <typeparam name="TDbContext">The type of DbContext to create.</typeparam>
    /// <returns>A new instance of <typeparamref name="TDbContext"/>.</returns>
    TDbContext CreateDbContext<TDbContext>() where TDbContext : DbContext;
}
