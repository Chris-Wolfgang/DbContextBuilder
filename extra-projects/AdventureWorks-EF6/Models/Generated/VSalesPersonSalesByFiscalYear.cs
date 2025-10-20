#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record VSalesPersonSalesByFiscalYear
{
	public int? SalesPersonId { get; set; }

	public string? FullName { get; set; }

	public string JobTitle { get; set; } = null!;

	public string SalesTerritory { get; set; } = null!;

	public decimal? _2002 { get; set; }

	public decimal? _2003 { get; set; }

	public decimal? _2004 { get; set; }
}
