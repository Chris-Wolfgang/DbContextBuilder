#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial class VJobCandidateEmployment
{
	public int JobCandidateId { get; set; }

	public DateTime? EmpStartDate { get; set; }

	public DateTime? EmpEndDate { get; set; }

	public string? EmpOrgName { get; set; }

	public string? EmpJobTitle { get; set; }

	public string? EmpResponsibility { get; set; }

	public string? EmpFunctionCategory { get; set; }

	public string? EmpIndustryCategory { get; set; }

	public string? EmpLocCountryRegion { get; set; }

	public string? EmpLocState { get; set; }

	public string? EmpLocCity { get; set; }
}
