namespace Wolfgang.DbContextBuilder.Examples.Domain;

/// <summary>A product in the shop catalogue.</summary>
public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int UnitsInStock { get; set; }
}
