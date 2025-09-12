#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial class VProductAndDescription
{
	public int ProductId { get; set; }

	public string Name { get; set; } = null!;

	public string ProductModel { get; set; } = null!;

	public string CultureId { get; set; } = null!;

	public string Description { get; set; } = null!;
}
