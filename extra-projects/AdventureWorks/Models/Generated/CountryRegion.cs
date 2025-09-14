#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Lookup table containing the ISO standard codes for countries and regions.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial class CountryRegion
{
	/// <summary>
	/// ISO standard code for countries and regions.
	/// </summary>
	public string CountryRegionCode { get; set; } = null!;

	/// <summary>
	/// Country or region name.
	/// </summary>
	public string Name { get; set; } = null!;

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual ICollection<CountryRegionCurrency> CountryRegionCurrencies { get; set; } = new List<CountryRegionCurrency>();

	public virtual ICollection<SalesTerritory> SalesTerritories { get; set; } = new List<SalesTerritory>();

	public virtual ICollection<StateProvince> StateProvinces { get; set; } = new List<StateProvince>();
}
