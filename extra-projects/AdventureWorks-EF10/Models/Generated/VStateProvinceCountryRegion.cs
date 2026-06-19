using System;
using System.Collections.Generic;

namespace AdventureWorks.Models;

public partial record VStateProvinceCountryRegion{
    public int StateProvinceId { get; set; }

    public string StateProvinceCode { get; set; }

    public bool IsOnlyStateProvinceFlag { get; set; }

    public string StateProvinceName { get; set; }

    public int TerritoryId { get; set; }

    public string CountryRegionCode { get; set; }

    public string CountryRegionName { get; set; }
}
