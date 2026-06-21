using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Cross-reference table mapping sales orders to sales reason codes.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record SalesOrderHeaderSalesReason{
    /// <summary>
    /// Primary key. Foreign key to SalesOrderHeader.SalesOrderID.
    /// </summary>
    public int SalesOrderId { get; set; }

    /// <summary>
    /// Primary key. Foreign key to SalesReason.SalesReasonID.
    /// </summary>
    public int SalesReasonId { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public virtual SalesOrderHeader SalesOrder { get; set; }

    public virtual SalesReason SalesReason { get; set; }
}
