using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Wolfgang.DbContextBuilderEF6.Tests.Unit.Models;

// Test-only EF entity POCO — properties are populated by EF Core via reflection.
// R# cannot see external reflection consumers so it reports the getter as unused.
// ReSharper disable UnusedAutoPropertyAccessor.Global
[ExcludeFromCodeCoverage]
[Table("Category")]
public class Category
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
