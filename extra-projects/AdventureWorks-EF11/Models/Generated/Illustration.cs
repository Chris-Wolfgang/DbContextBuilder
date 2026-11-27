using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Bicycle assembly diagrams.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record Illustration{
    /// <summary>
    /// Primary key for Illustration records.
    /// </summary>
    public int IllustrationId { get; set; }

    /// <summary>
    /// Illustrations used in manufacturing instructions. Stored as XML.
    /// </summary>
    public string Diagram { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public virtual ICollection<ProductModelIllustration> ProductModelIllustrations { get; set; } = new List<ProductModelIllustration>();
}
