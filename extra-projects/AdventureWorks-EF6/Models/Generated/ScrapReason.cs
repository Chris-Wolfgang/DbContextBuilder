#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Manufacturing failure reasons lookup table.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record ScrapReason
{
	/// <summary>
	/// Primary key for ScrapReason records.
	/// </summary>
	public short ScrapReasonId { get; set; }

	/// <summary>
	/// Failure description.
	/// </summary>
	public string Name { get; set; } = null!;

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}
