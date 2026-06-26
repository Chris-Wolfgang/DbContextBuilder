using Wolfgang.DbContextBuilder.Examples.Domain;
using Wolfgang.DbContextBuilder.Examples.Services;
using Wolfgang.DbContextBuilderCore;
using Xunit;

namespace Wolfgang.DbContextBuilder.Examples.Examples;

/// <summary>
/// Example 4 — Real relational behaviour with SQLite.
///
/// The InMemory provider is fast but ignores SQL semantics (joins, GROUP BY,
/// constraints, default values). When your code relies on real relational
/// behaviour, switch the provider to SQLite with a single call — you get an
/// isolated in-memory SQLite database per builder, and the rest of the test is
/// identical.
/// </summary>
public class Example04_RelationalConstraintsWithSqlite
{
    [Fact]
    public async Task Aggregates_run_as_real_sql_against_sqlite()
    {
        var product = new Product { Name = "Widget", UnitPrice = 5m, UnitsInStock = 50 };

        var customer = new Customer { Name = "Bob" };
        customer.Orders.Add(new Order
        {
            PlacedOn = DateTime.UtcNow,
            Status = OrderStatus.Delivered,
            Lines = { new OrderLine { Product = product, Quantity = 4, UnitPrice = 5m } },
        });

        // The only change from Example 2 is UseSqlite() instead of UseInMemory().
        await using var context = await new DbContextBuilder<ShopDbContext>()
            .UseSqlite()
            .SeedWith(customer)
            .BuildAsync();

        var service = new OrderService(context);

        // These run as real SQL (JOIN + COUNT/SUM) against SQLite.
        Assert.Equal(1, await service.GetOrderCountAsync(customer.Id));
        Assert.Equal(4, await service.GetTotalUnitsOrderedAsync(customer.Id));

        // Caveat worth knowing: SQLite has no native decimal type, so EF Core
        // cannot translate a decimal SUM. A query like GetLifetimeValueAsync
        // (which sums Quantity * UnitPrice) works on the InMemory provider but
        // throws on SQLite — pick the provider that matches what your code needs.
    }
}
