using System;
using System.Collections.Generic;

namespace AdventureWorks.Models;

/// <summary>
/// Lookup table containing the ISO standard codes for countries and regions.
/// </summary>
public partial record CountryRegion{
    /// <summary>
    /// ISO standard code for countries and regions.
    /// </summary>
    public string CountryRegionCode { get; set; }

    /// <summary>
    /// Country or region name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public virtual ICollection<CountryRegionCurrency> CountryRegionCurrencies { get; set; } = new List<CountryRegionCurrency>();

    public virtual ICollection<SalesTerritory> SalesTerritories { get; set; } = new List<SalesTerritory>();

    public virtual ICollection<StateProvince> StateProvinces { get; set; } = new List<StateProvince>();
}
