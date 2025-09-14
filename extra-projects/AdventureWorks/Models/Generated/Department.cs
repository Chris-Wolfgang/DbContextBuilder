#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Lookup table containing the departments within the Adventure Works Cycles company.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial class Department
{
	/// <summary>
	/// Primary key for Department records.
	/// </summary>
	public short DepartmentId { get; set; }

	/// <summary>
	/// Name of the department.
	/// </summary>
	public string Name { get; set; } = null!;

	/// <summary>
	/// Name of the group to which the department belongs.
	/// </summary>
	public string GroupName { get; set; } = null!;

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual ICollection<EmployeeDepartmentHistory> EmployeeDepartmentHistories { get; set; } = new List<EmployeeDepartmentHistory>();
}
