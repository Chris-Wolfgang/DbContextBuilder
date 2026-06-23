using Wolfgang.DbContextBuilder.Examples.Domain;
using Wolfgang.DbContextBuilder.Examples.Services;
using Wolfgang.DbContextBuilderCore;
using Xunit;

namespace Wolfgang.DbContextBuilder.Examples.Examples;

/// <summary>
/// Example 5 — Shaping random data.
///
/// <c>SeedWithRandom</c> has overloads that hand you each generated entity (and its
/// index) so you can force the parts your test cares about while leaving everything
/// else random. Here: 30 random products, but the first five are forced out of stock
/// so the test has a known number of low-stock rows to assert on.
/// </summary>
public class Example05_ShapingRandomData
{
    [Fact]
    public async Task Generated_products_can_be_shaped_per_item()
    {
        await using var context = await new DbContextBuilder<ShopDbContext>()
            .UseInMemory()
            .SeedWithRandom<Product>(count: 30, (product, index) =>
            {
                // First five are out of stock; the rest are comfortably stocked.
                product.UnitsInStock = index < 5 ? 0 : 100;
                return product;
            })
            .BuildAsync();

        Assert.Equal(30, context.Products.Count());

        var service = new OrderService(context);
        var outOfStock = await service.GetLowStockProductsAsync(threshold: 1);

        // Exactly the five we forced to zero stock.
        Assert.Equal(5, outOfStock.Count);
    }
}
