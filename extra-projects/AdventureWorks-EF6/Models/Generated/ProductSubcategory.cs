#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Product subcategories. See ProductCategory table.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record ProductSubcategory
{
	/// <summary>
	/// Primary key for ProductSubcategory records.
	/// </summary>
	public int ProductSubcategoryId { get; set; }

	/// <summary>
	/// Product category identification number. Foreign key to ProductCategory.ProductCategoryID.
	/// </summary>
	public int ProductCategoryId { get; set; }

	/// <summary>
	/// Subcategory description.
	/// </summary>
	public string Name { get; set; } = null!;

	/// <summary>
	/// ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.
	/// </summary>
	public Guid Rowguid { get; set; }

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual ProductCategory ProductCategory { get; set; } = null!;

	public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
