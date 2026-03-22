using System;
using System.Data.Common;
using System.Data.Entity;
using Effort;

namespace Wolfgang.DbContextBuilderEF6;



/// <summary>
/// Creates <see cref="DbContext"/> instances backed by an Effort in-memory database.
/// </summary>
internal class EffortDbContextCreator : ICreateDbContext
{
    private readonly DbConnection _connection;
    private bool _disposed;



    /// <summary>
    /// Initializes a new instance of the <see cref="EffortDbContextCreator"/> class
    /// with a new transient in-memory database connection.
    /// </summary>
    public EffortDbContextCreator()
    {
        _connection = DbConnectionFactory.CreateTransient();
    }



    /// <summary>
    /// Creates a new instance of the specified <typeparamref name="TDbContext"/> type
    /// using the shared in-memory connection.
    /// </summary>
    /// <typeparam name="TDbContext">The type of DbContext to create.</typeparam>
    /// <returns>A new instance of <typeparamref name="TDbContext"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// The specified <typeparamref name="TDbContext"/> type does not have a constructor
    /// that accepts (<see cref="DbConnection"/>, <see cref="bool"/>).
    /// </exception>
    public TDbContext CreateDbContext<TDbContext>() where TDbContext : DbContext
    {
        return (TDbContext)Activator.CreateInstance
        (
            typeof(TDbContext),
            _connection,
            false
        )!;
    }



    /// <summary>
    /// Releases the in-memory database connection.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _connection.Dispose();
            _disposed = true;
        }
    }
}
