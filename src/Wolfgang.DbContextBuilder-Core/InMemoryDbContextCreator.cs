using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore;

internal class InMemoryDbContextCreator : ICreateDbContext
{

    private readonly string _databaseName = Guid.NewGuid().ToString();

    public Task<TDbContext> CreateDbContext<TDbContext>(DbContextOptionsBuilder<TDbContext> optionsBuilder) where TDbContext : DbContext
    {
        optionsBuilder.UseInMemoryDatabase(_databaseName);
        var options = optionsBuilder.Options;
        return Task.FromResult((TDbContext)Activator.CreateInstance(typeof(TDbContext), options)!);
    }
}