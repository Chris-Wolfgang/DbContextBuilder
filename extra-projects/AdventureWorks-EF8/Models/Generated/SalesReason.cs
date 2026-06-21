using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Lookup table of customer purchase reasons.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record SalesReason{
    /// <summary>
    /// Primary key for SalesReason records.
    /// </summary>
    public int SalesReasonId { get; set; }

    /// <summary>
    /// Sales reason description.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Category the sales reason belongs to.
    /// </summary>
    public string ReasonType { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public virtual ICollection<SalesOrderHeaderSalesReason> SalesOrderHeaderSalesReasons { get; set; } = new List<SalesOrderHeaderSalesReason>();
}
