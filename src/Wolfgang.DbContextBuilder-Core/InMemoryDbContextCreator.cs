using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore;

/// <summary>
/// Creates <see cref="DbContext"/> instances using the EF Core in-memory database provider.
/// </summary>
internal class InMemoryDbContextCreator : ICreateDbContext
{

    private readonly string _databaseName = Guid.NewGuid().ToString();



    /// <inheritdoc />
    public Task<TDbContext> CreateDbContextAsync<TDbContext>(DbContextOptionsBuilder<TDbContext> optionsBuilder) where TDbContext : DbContext
    {
        optionsBuilder.UseInMemoryDatabase(_databaseName);
        return Task.FromResult(DbContextActivator<TDbContext>.Create(optionsBuilder.Options));
    }
}
