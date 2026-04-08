using System.Data.Common;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;

namespace Wolfgang.DbContextBuilderEF6.Tests.Unit.Models;

[ExcludeFromCodeCoverage]
public class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }



    public TestDbContext(DbConnection connection, bool contextOwnsConnection)
        : base(connection, contextOwnsConnection)
    {
    }



    public virtual DbSet<Product> Products { get; set; } = null!;


    public virtual DbSet<Category> Categories { get; set; } = null!;
}
