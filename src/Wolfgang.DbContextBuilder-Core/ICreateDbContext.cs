using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore;
public interface ICreateDbContext
{
    Task<TDbContext> CreateDbContext<TDbContext>(DbContextOptionsBuilder<TDbContext> optionsBuilder) where TDbContext : DbContext;
}