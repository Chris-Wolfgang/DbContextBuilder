using Wolfgang.DbContextBuilder.Examples.Domain;
using Wolfgang.DbContextBuilder.Examples.Services;
using Wolfgang.DbContextBuilderCore;
using Xunit;

namespace Wolfgang.DbContextBuilder.Examples.Examples;

/// <summary>
/// Example 1 — Building a DbContext.
///
/// The simplest case: get a ready-to-use, fully isolated <see cref="ShopDbContext"/>
/// in a single call. No connection string, no shared test database, no teardown —
/// the store lives only as long as the context.
/// </summary>
public class Example01_BuildingAContext
{
    [Fact]
    public async Task BuildAsync_returns_a_ready_to_use_empty_context()
    {
        // Arrange & Act — one call gives you a fresh, empty in-memory ShopDbContext.
        // 'await using' disposes the context and tears down its backing store.
        await using var context = await new DbContextBuilder<ShopDbContext>()
            .UseInMemory()
            .BuildAsync();

        var service = new OrderService(context);

        // Assert — nothing was seeded, so the service sees an empty store.
        Assert.Empty(context.Products);
        Assert.Equal(0m, await service.GetLifetimeValueAsync(customerId: 1));
    }
}
