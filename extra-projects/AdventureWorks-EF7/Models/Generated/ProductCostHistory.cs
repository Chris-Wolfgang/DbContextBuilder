#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Changes in the cost of a product over time.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record ProductCostHistory
{
	/// <summary>
	/// Product identification number. Foreign key to Product.ProductID
	/// </summary>
	public int ProductId { get; set; }

	/// <summary>
	/// Product cost start date.
	/// </summary>
	public DateTime StartDate { get; set; }

	/// <summary>
	/// Product cost end date.
	/// </summary>
	public DateTime? EndDate { get; set; }

	/// <summary>
	/// Standard cost of the product.
	/// </summary>
	public decimal StandardCost { get; set; }

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual Product Product { get; set; } = null!;
}
