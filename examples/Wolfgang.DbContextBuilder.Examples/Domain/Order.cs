namespace Wolfgang.DbContextBuilder.Examples.Domain;

/// <summary>An order placed by a <see cref="Customer"/>.</summary>
public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public DateTime PlacedOn { get; set; }

    public OrderStatus Status { get; set; }

    /// <summary>The line items on this order.</summary>
    public List<OrderLine> Lines { get; } = [];
}
