using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Wolfgang.DbContextBuilderEF6.Tests.Unit.Models;

// Test-only EF entity POCO — properties are populated by EF Core via reflection.
// R# cannot see external reflection consumers so it reports the getter as unused.
// ReSharper disable UnusedAutoPropertyAccessor.Global
[ExcludeFromCodeCoverage]
[Table("Product")]
public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public DateTime CreatedDate { get; set; }
}
