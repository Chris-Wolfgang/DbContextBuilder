namespace Wolfgang.DbContextBuilder.Examples.Domain;

/// <summary>A shop customer who can place orders.</summary>
public class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    /// <summary>Orders this customer has placed.</summary>
    public List<Order> Orders { get; } = [];
}
