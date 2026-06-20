using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record VProductAndDescription{
    public int ProductId { get; set; }

    public string Name { get; set; }

    public string ProductModel { get; set; }

    public string CultureId { get; set; }

    public string Description { get; set; }
}
