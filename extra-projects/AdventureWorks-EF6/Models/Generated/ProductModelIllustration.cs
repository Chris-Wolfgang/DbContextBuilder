#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Cross-reference table mapping product models and illustrations.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record ProductModelIllustration
{
	/// <summary>
	/// Primary key. Foreign key to ProductModel.ProductModelID.
	/// </summary>
	public int ProductModelId { get; set; }

	/// <summary>
	/// Primary key. Foreign key to Illustration.IllustrationID.
	/// </summary>
	public int IllustrationId { get; set; }

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual Illustration Illustration { get; set; } = null!;

	public virtual ProductModel ProductModel { get; set; } = null!;
}
