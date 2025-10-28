#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Cross-reference table mapping product descriptions and the language the description is written in.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record ProductModelProductDescriptionCulture
{
	/// <summary>
	/// Primary key. Foreign key to ProductModel.ProductModelID.
	/// </summary>
	public int ProductModelId { get; set; }

	/// <summary>
	/// Primary key. Foreign key to ProductDescription.ProductDescriptionID.
	/// </summary>
	public int ProductDescriptionId { get; set; }

	/// <summary>
	/// Culture identification number. Foreign key to Culture.CultureID.
	/// </summary>
	public string CultureId { get; set; } = null!;

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual Culture Culture { get; set; } = null!;

	public virtual ProductDescription ProductDescription { get; set; } = null!;

	public virtual ProductModel ProductModel { get; set; } = null!;
}
