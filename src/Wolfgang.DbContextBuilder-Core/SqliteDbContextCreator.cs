using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore;

internal class SqliteDbContextCreator : ICreateDbContext
{

    private readonly SqliteConnection _connection;

    public SqliteDbContextCreator()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }


    public Task<TDbContext> CreateDbContext<TDbContext>(DbContextOptionsBuilder<TDbContext> optionsBuilder) where TDbContext : DbContext
    {

        optionsBuilder.UseSqlite(_connection);

        var options = optionsBuilder.Options;

        return Task.FromResult((TDbContext)Activator.CreateInstance(typeof(TDbContext), options)!);
    }
}