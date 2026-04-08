using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore;

internal sealed class SqliteDbContextCreator : ICreateDbContext, IDisposable
{

    private readonly SqliteConnection _connection;

    public SqliteDbContextCreator()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }


    public Task<TDbContext> CreateDbContextAsync<TDbContext>(DbContextOptionsBuilder<TDbContext> optionsBuilder) where TDbContext : DbContext
    {

        optionsBuilder.UseSqlite(_connection);

        var options = optionsBuilder.Options;

        return Task.FromResult((TDbContext)Activator.CreateInstance(typeof(TDbContext), options)!);
    }


    public void Dispose()
    {
        _connection.Dispose();
    }
}