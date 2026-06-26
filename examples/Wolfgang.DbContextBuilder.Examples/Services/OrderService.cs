using Microsoft.EntityFrameworkCore;
using Wolfgang.DbContextBuilder.Examples.Domain;

namespace Wolfgang.DbContextBuilder.Examples.Services;

/// <summary>
/// Ordinary application code that depends on a <see cref="ShopDbContext"/>. This is
/// the "system under test" in the examples — each example builds a seeded context,
/// hands it to this service, and asserts on the result.
/// </summary>
public class OrderService(ShopDbContext db)
{
    /// <summary>Total value of every line item across all of a customer's orders.</summary>
    public async Task<decimal> GetLifetimeValueAsync(int customerId) =>
        await db.OrderLines
            .Where(line => line.Order!.CustomerId == customerId)
            .SumAsync(line => line.Quantity * line.UnitPrice);

    /// <summary>Products whose stock is below <paramref name="threshold"/>, lowest first.</summary>
    public async Task<IReadOnlyList<Product>> GetLowStockProductsAsync(int threshold) =>
        await db.Products
            .Where(product => product.UnitsInStock < threshold)
            .OrderBy(product => product.UnitsInStock)
            .ToListAsync();

    /// <summary>Number of orders a customer has placed.</summary>
    public async Task<int> GetOrderCountAsync(int customerId) =>
        await db.Orders.CountAsync(order => order.CustomerId == customerId);

    /// <summary>
    /// Total quantity of items across all of a customer's orders. Uses an integer
    /// <c>SUM</c>, which every provider (including SQLite) translates to SQL —
    /// unlike a <c>decimal</c> SUM, which SQLite cannot aggregate natively.
    /// </summary>
    public async Task<int> GetTotalUnitsOrderedAsync(int customerId) =>
        await db.OrderLines
            .Where(line => line.Order!.CustomerId == customerId)
            .SumAsync(line => line.Quantity);
}
