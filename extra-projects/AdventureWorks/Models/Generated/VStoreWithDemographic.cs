#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record VStoreWithDemographic
{
	public int BusinessEntityId { get; set; }

	public string Name { get; set; } = null!;

	public decimal? AnnualSales { get; set; }

	public decimal? AnnualRevenue { get; set; }

	public string? BankName { get; set; }

	public string? BusinessType { get; set; }

	public int? YearOpened { get; set; }

	public string? Specialty { get; set; }

	public int? SquareFeet { get; set; }

	public string? Brands { get; set; }

	public string? Internet { get; set; }

	public int? NumberEmployees { get; set; }
}
