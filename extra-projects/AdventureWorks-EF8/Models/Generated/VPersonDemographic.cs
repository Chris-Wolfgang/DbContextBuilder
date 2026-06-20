using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record VPersonDemographic{
    public int BusinessEntityId { get; set; }

    public decimal? TotalPurchaseYtd { get; set; }

    public DateTime? DateFirstPurchase { get; set; }

    public DateTime? BirthDate { get; set; }

    public string MaritalStatus { get; set; }

    public string YearlyIncome { get; set; }

    public string Gender { get; set; }

    public int? TotalChildren { get; set; }

    public int? NumberChildrenAtHome { get; set; }

    public string Education { get; set; }

    public string Occupation { get; set; }

    public bool? HomeOwnerFlag { get; set; }

    public int? NumberCarsOwned { get; set; }
}
