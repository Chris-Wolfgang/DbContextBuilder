using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilder.Samples.ShadowWorkload;

public sealed class Product
{
    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }
}


public sealed class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options)
    {
    }


    public DbSet<Product> Products => Set<Product>();
}
