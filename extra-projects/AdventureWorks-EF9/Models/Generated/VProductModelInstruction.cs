using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record VProductModelInstruction{
    public int ProductModelId { get; set; }

    public string Name { get; set; }

    public string Instructions { get; set; }

    public int? LocationId { get; set; }

    public decimal? SetupHours { get; set; }

    public decimal? MachineHours { get; set; }

    public decimal? LaborHours { get; set; }

    public int? LotSize { get; set; }

    public string Step { get; set; }

    public Guid Rowguid { get; set; }

    public DateTime ModifiedDate { get; set; }
}
