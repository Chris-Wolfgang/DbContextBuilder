#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Bicycle assembly diagrams.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial class Illustration
{
	/// <summary>
	/// Primary key for Illustration records.
	/// </summary>
	public int IllustrationId { get; set; }

	/// <summary>
	/// Illustrations used in manufacturing instructions. Stored as XML.
	/// </summary>
	public string? Diagram { get; set; }

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual ICollection<ProductModelIllustration> ProductModelIllustrations { get; set; } = new List<ProductModelIllustration>();
}
