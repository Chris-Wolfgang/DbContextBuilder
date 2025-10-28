#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Lookup table containing the languages in which some AdventureWorks data is stored.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record Culture
{
	/// <summary>
	/// Primary key for Culture records.
	/// </summary>
	public string CultureId { get; set; } = null!;

	/// <summary>
	/// Culture description.
	/// </summary>
	public string Name { get; set; } = null!;

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual ICollection<ProductModelProductDescriptionCulture> ProductModelProductDescriptionCultures { get; set; } = new List<ProductModelProductDescriptionCulture>();
}
