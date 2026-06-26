using Wolfgang.DbContextBuilder.Examples.Domain;
using Wolfgang.DbContextBuilder.Examples.Services;
using Wolfgang.DbContextBuilderCore;
using Xunit;

namespace Wolfgang.DbContextBuilder.Examples.Examples;

/// <summary>
/// Example 3 — Seeding random data.
///
/// When the <em>shape</em> of the data matters but the exact rows do not, let the
/// builder generate them. Here we fill a catalogue with 50 random products and test
/// an inventory query without hand-writing 50 rows. (Random generation uses
/// AutoFixture under the hood — no extra setup required.)
/// </summary>
public class Example03_SeedingRandomData
{
    [Fact]
    public async Task Low_stock_query_runs_over_a_randomly_seeded_catalogue()
    {
        await using var context = await new DbContextBuilder<ShopDbContext>()
            .UseInMemory()
            .SeedWithRandom<Product>(count: 50)
            .BuildAsync();

        // 50 random products were inserted.
        Assert.Equal(50, context.Products.Count());

        // The query works against the generated data. We do not know the exact
        // random stock levels, so we assert the *contract* rather than exact rows:
        // everything the query returns genuinely sits below the threshold.
        var service = new OrderService(context);
        var lowStock = await service.GetLowStockProductsAsync(threshold: 10);

        Assert.All(lowStock, product => Assert.True(product.UnitsInStock < 10));
    }
}
