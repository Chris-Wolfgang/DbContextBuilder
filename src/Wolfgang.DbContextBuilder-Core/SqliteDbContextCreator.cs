using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore;

/// <summary>
/// Creates <see cref="DbContext"/> instances using the SQLite in-memory database provider.
/// Holds an open <see cref="SqliteConnection"/> for the lifetime of the creator to keep
/// the in-memory database alive.
/// </summary>
internal sealed class SqliteDbContextCreator : ICreateDbContext, IDisposable
{

    private readonly SqliteConnection _connection;



    /// <summary>
    /// Initializes a new instance and opens an in-memory SQLite connection.
    /// </summary>
    public SqliteDbContextCreator()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }



    /// <inheritdoc />
    public Task<TDbContext> CreateDbContextAsync<TDbContext>(DbContextOptionsBuilder<TDbContext> optionsBuilder) where TDbContext : DbContext
    {

        optionsBuilder.UseSqlite(_connection);

        var options = optionsBuilder.Options;

        return Task.FromResult((TDbContext)Activator.CreateInstance(typeof(TDbContext), options)!);
    }



    /// <summary>
    /// Closes and disposes the underlying SQLite connection, destroying the in-memory database.
    /// </summary>
    public void Dispose()
    {
        _connection.Dispose();
    }
}
