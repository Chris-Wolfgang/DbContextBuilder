#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Employee pay history.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record EmployeePayHistory
{
	/// <summary>
	/// Employee identification number. Foreign key to Employee.BusinessEntityID.
	/// </summary>
	public int BusinessEntityId { get; set; }

	/// <summary>
	/// Date the change in pay is effective
	/// </summary>
	public DateTime RateChangeDate { get; set; }

	/// <summary>
	/// Salary hourly rate.
	/// </summary>
	public decimal Rate { get; set; }

	/// <summary>
	/// 1 = Salary received monthly, 2 = Salary received biweekly
	/// </summary>
	public byte PayFrequency { get; set; }

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual Employee BusinessEntity { get; set; } = null!;
}
