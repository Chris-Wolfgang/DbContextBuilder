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
        var options = optionsBuilder.Options;
        return Task.FromResult((TDbContext)Activator.CreateInstance(typeof(TDbContext), options)!);
    }
}
