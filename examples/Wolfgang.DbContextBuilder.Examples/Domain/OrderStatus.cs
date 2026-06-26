namespace Wolfgang.DbContextBuilder.Examples.Domain;

/// <summary>Lifecycle status of an <see cref="Order"/>.</summary>
public enum OrderStatus
{
    Pending,
    Shipped,
    Delivered,
    Cancelled,
}
