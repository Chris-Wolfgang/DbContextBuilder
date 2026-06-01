using System;
using System.Data.Entity;

namespace Wolfgang.DbContextBuilderEF6;

/// <summary>
/// Provides an API to create instances of <see cref="DbContext"/> for testing. Implementations
/// own any test-only infrastructure they allocate (in-memory connections, AutoFixture
/// instances, etc.) and must release it from <see cref="IDisposable.Dispose"/>.
/// </summary>
/// <remarks>
/// In the current builder topology the implementation is constructed and owned by
/// <see cref="DbContextBuilder{T}"/> itself — consumers do not typically hold or dispose
/// the creator directly. The <see cref="IDisposable"/> contract is on the interface so
/// that custom implementations registered via a future public extension point can still
/// release resources deterministically when the builder is disposed. <see cref="DbContext"/>
/// instances returned by <see cref="CreateDbContext{TDbContext}"/> are owned by the caller
/// and disposed independently of the creator.
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
