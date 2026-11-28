using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Changes in the list price of a product over time.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record ProductListPriceHistory{
    /// <summary>
    /// Product identification number. Foreign key to Product.ProductID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// List price start date.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// List price end date
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Product list price.
    /// </summary>
    public decimal ListPrice { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public virtual Product Product { get; set; }
}
