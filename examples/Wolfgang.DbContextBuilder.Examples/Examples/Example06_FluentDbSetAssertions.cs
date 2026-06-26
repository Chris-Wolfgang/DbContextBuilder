using Wolfgang.DbContextBuilder.Examples.Domain;
using Wolfgang.DbContextBuilderCore;
using Wolfgang.DbContextBuilderCore.Assertions;
using Xunit;

namespace Wolfgang.DbContextBuilder.Examples.Examples;

/// <summary>
/// Example 6 — Fluent DbSet assertions.
///
/// The library ships a small assertion surface so you can express expectations
/// about a <c>DbSet</c> directly. Each assertion is awaitable and chainable, and
/// failures throw with an entity-typed message. Import
/// <c>Wolfgang.DbContextBuilderCore.Assertions</c> to get <c>.Should()</c>.
/// </summary>
public class Example06_FluentDbSetAssertions
{
    [Fact]
    public async Task Assert_directly_against_the_DbSet()
    {
        var products = new[]
        {
            new Product { Name = "Widget", UnitPrice = 9.99m, UnitsInStock = 100 },
            new Product { Name = "Gadget", UnitPrice = 19.99m, UnitsInStock = 0 },
            new Product { Name = "Gizmo", UnitPrice = 4.99m, UnitsInStock = 7 },
        };

        await using var context = await new DbContextBuilder<ShopDbContext>()
            .UseInMemory()
            .SeedWith(products)
            .BuildAsync();

        await context.Products.Should().NotBeEmpty();
        await context.Products.Should().HaveCount(3);
        await context.Products.Should().Contain(product => product.Name == "Widget");
        await context.Products.Should().AllSatisfy(product => product.UnitPrice > 0m);
    }
}
