using Wolfgang.DbContextBuilder.Examples.Domain;
using Wolfgang.DbContextBuilder.Examples.Services;
using Wolfgang.DbContextBuilderCore;
using Xunit;

namespace Wolfgang.DbContextBuilder.Examples.Examples;

/// <summary>
/// Example 2 — Seeding specific test data.
///
/// The bread-and-butter pattern for deterministic integration tests: build a known
/// object graph (customer → orders → lines → products), seed it, and assert exact
/// values. Seeding the root entity inserts the whole connected graph.
/// </summary>
public class Example02_SeedingSpecificData
{
    [Fact]
    public async Task Service_computes_lifetime_value_over_seeded_orders()
    {
        // Arrange — a small catalogue and a customer with two orders.
        var widget = new Product { Name = "Widget", UnitPrice = 9.99m, UnitsInStock = 100 };
        var gadget = new Product { Name = "Gadget", UnitPrice = 19.99m, UnitsInStock = 5 };

        var customer = new Customer { Name = "Alice", Email = "alice@example.com" };

        customer.Orders.Add(new Order
        {
            PlacedOn = new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc),
            Status = OrderStatus.Delivered,
            Lines =
            {
                new OrderLine { Product = widget, Quantity = 2, UnitPrice = 9.99m },
                new OrderLine { Product = gadget, Quantity = 1, UnitPrice = 19.99m },
            },
        });

        customer.Orders.Add(new Order
        {
            PlacedOn = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            Status = OrderStatus.Shipped,
            Lines = { new OrderLine { Product = widget, Quantity = 3, UnitPrice = 9.99m } },
        });

        // Act — seed the whole graph; EF inserts the customer, both orders,
        // all three lines and both products in one pass.
        await using var context = await new DbContextBuilder<ShopDbContext>()
            .UseInMemory()
            .SeedWith(customer)
            .BuildAsync();

        var service = new OrderService(context);

        // Assert — (2 * 9.99) + (1 * 19.99) + (3 * 9.99) = 69.94
        Assert.Equal(69.94m, await service.GetLifetimeValueAsync(customer.Id));
        Assert.Equal(2, await service.GetOrderCountAsync(customer.Id));
    }
}
