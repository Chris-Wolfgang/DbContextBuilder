#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial class VJobCandidate
{
	public int JobCandidateId { get; set; }

	public int? BusinessEntityId { get; set; }

	public string? NamePrefix { get; set; }

	public string? NameFirst { get; set; }

	public string? NameMiddle { get; set; }

	public string? NameLast { get; set; }

	public string? NameSuffix { get; set; }

	public string? Skills { get; set; }

	public string? AddrType { get; set; }

	public string? AddrLocCountryRegion { get; set; }

	public string? AddrLocState { get; set; }

	public string? AddrLocCity { get; set; }

	public string? AddrPostalCode { get; set; }

	public string? Email { get; set; }

	public string? WebSite { get; set; }

	public DateTime ModifiedDate { get; set; }
}
