#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record VJobCandidateEducation
{
	public int JobCandidateId { get; set; }

	public string? EduLevel { get; set; }

	public DateTime? EduStartDate { get; set; }

	public DateTime? EduEndDate { get; set; }

	public string? EduDegree { get; set; }

	public string? EduMajor { get; set; }

	public string? EduMinor { get; set; }

	public string? EduGpa { get; set; }

	public string? EduGpascale { get; set; }

	public string? EduSchool { get; set; }

	public string? EduLocCountryRegion { get; set; }

	public string? EduLocState { get; set; }

	public string? EduLocCity { get; set; }
}
