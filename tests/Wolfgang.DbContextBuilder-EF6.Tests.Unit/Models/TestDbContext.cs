using System.Data.Common;
using System.Data.Entity;

namespace Wolfgang.DbContextBuilderEF6.Tests.Unit.Models;

public class TestDbContext : DbContext
{
    public TestDbContext(DbConnection connection, bool contextOwnsConnection)
        : base(connection, contextOwnsConnection)
    {
    }



    public virtual DbSet<Product> Products { get; set; } = null!;


    public virtual DbSet<Category> Categories { get; set; } = null!;
}
