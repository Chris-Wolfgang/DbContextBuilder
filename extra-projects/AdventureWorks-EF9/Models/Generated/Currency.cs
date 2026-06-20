using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Lookup table containing standard ISO currencies.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record Currency{
    /// <summary>
    /// The ISO code for the Currency.
    /// </summary>
    public string CurrencyCode { get; set; }

    /// <summary>
    /// Currency name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public virtual ICollection<CountryRegionCurrency> CountryRegionCurrencies { get; set; } = new List<CountryRegionCurrency>();

    public virtual ICollection<CurrencyRate> CurrencyRateFromCurrencyCodeNavigations { get; set; } = new List<CurrencyRate>();

    public virtual ICollection<CurrencyRate> CurrencyRateToCurrencyCodeNavigations { get; set; } = new List<CurrencyRate>();
}
