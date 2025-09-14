#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial class VStateProvinceCountryRegion
{
	public int StateProvinceId { get; set; }

	public string StateProvinceCode { get; set; } = null!;

	public bool IsOnlyStateProvinceFlag { get; set; }

	public string StateProvinceName { get; set; } = null!;

	public int TerritoryId { get; set; }

	public string CountryRegionCode { get; set; } = null!;

	public string CountryRegionName { get; set; } = null!;
}
