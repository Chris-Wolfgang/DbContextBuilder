#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Product model classification.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record ProductModel
{
	/// <summary>
	/// Primary key for ProductModel records.
	/// </summary>
	public int ProductModelId { get; set; }

	/// <summary>
	/// Product model description.
	/// </summary>
	public string Name { get; set; } = null!;

	/// <summary>
	/// Detailed product catalog information in xml format.
	/// </summary>
	public string? CatalogDescription { get; set; }

	/// <summary>
	/// Manufacturing instructions in xml format.
	/// </summary>
	public string? Instructions { get; set; }

	/// <summary>
	/// ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.
	/// </summary>
	public Guid Rowguid { get; set; }

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual ICollection<ProductModelIllustration> ProductModelIllustrations { get; set; } = new List<ProductModelIllustration>();

	public virtual ICollection<ProductModelProductDescriptionCulture> ProductModelProductDescriptionCultures { get; set; } = new List<ProductModelProductDescriptionCulture>();

	public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
