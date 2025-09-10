#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace AdventureWorks.Models;

public partial class VProductAndDescription
{
	public int ProductId { get; set; }

	public string Name { get; set; } = null!;

	public string ProductModel { get; set; } = null!;

	public string CultureId { get; set; } = null!;

	public string Description { get; set; } = null!;
}
