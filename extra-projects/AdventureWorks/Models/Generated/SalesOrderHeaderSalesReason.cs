#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Cross-reference table mapping sales orders to sales reason codes.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record SalesOrderHeaderSalesReason
{
	/// <summary>
	/// Primary key. Foreign key to SalesOrderHeader.SalesOrderID.
	/// </summary>
	public int SalesOrderId { get; set; }

	/// <summary>
	/// Primary key. Foreign key to SalesReason.SalesReasonID.
	/// </summary>
	public int SalesReasonId { get; set; }

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual SalesOrderHeader SalesOrder { get; set; } = null!;

	public virtual SalesReason SalesReason { get; set; } = null!;
}
