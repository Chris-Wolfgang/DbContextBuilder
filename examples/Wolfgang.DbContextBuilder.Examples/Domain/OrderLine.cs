namespace Wolfgang.DbContextBuilder.Examples.Domain;

/// <summary>A single line item on an <see cref="Order"/>.</summary>
public class OrderLine
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public Order? Order { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public int Quantity { get; set; }

    /// <summary>Unit price captured at the time the order was placed.</summary>
    public decimal UnitPrice { get; set; }
}
