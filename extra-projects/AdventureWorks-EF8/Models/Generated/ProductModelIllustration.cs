using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Cross-reference table mapping product models and illustrations.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record ProductModelIllustration{
    /// <summary>
    /// Primary key. Foreign key to ProductModel.ProductModelID.
    /// </summary>
    public int ProductModelId { get; set; }

    /// <summary>
    /// Primary key. Foreign key to Illustration.IllustrationID.
    /// </summary>
    public int IllustrationId { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public virtual Illustration Illustration { get; set; }

    public virtual ProductModel ProductModel { get; set; }
}
