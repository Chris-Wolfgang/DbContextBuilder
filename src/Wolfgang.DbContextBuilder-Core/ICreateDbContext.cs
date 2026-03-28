using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore;
public interface ICreateDbContext
{
    Task<TDbContext> CreateDbContextAsync<TDbContext>(DbContextOptionsBuilder<TDbContext> optionsBuilder) where TDbContext : DbContext;
}