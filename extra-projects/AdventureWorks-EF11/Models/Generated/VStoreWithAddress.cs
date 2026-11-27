using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record VStoreWithAddress{
    public int BusinessEntityId { get; set; }

    public string Name { get; set; }

    public string AddressType { get; set; }

    public string AddressLine1 { get; set; }

    public string AddressLine2 { get; set; }

    public string City { get; set; }

    public string StateProvinceName { get; set; }

    public string PostalCode { get; set; }

    public string CountryRegionName { get; set; }
}
