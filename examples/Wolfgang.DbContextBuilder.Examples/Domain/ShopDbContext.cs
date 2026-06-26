using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilder.Examples.Domain;

/// <summary>
/// A typical application <see cref="DbContext"/>. Note the
/// <c>DbContextOptions&lt;ShopDbContext&gt;</c> constructor — that is the only
/// requirement <c>DbContextBuilder&lt;T&gt;</c> places on your context.
/// </summary>
public class ShopDbContext(DbContextOptions<ShopDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
}
