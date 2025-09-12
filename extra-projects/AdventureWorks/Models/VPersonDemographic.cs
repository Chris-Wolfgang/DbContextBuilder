#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial class VPersonDemographic
{
	public int BusinessEntityId { get; set; }

	public decimal? TotalPurchaseYtd { get; set; }

	public DateTime? DateFirstPurchase { get; set; }

	public DateTime? BirthDate { get; set; }

	public string? MaritalStatus { get; set; }

	public string? YearlyIncome { get; set; }

	public string? Gender { get; set; }

	public int? TotalChildren { get; set; }

	public int? NumberChildrenAtHome { get; set; }

	public string? Education { get; set; }

	public string? Occupation { get; set; }

	public bool? HomeOwnerFlag { get; set; }

	public int? NumberCarsOwned { get; set; }
}
