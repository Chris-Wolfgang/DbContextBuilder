using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Lookup table containing the languages in which some AdventureWorks data is stored.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record Culture{
    /// <summary>
    /// Primary key for Culture records.
    /// </summary>
    public string CultureId { get; set; }

    /// <summary>
    /// Culture description.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public virtual ICollection<ProductModelProductDescriptionCulture> ProductModelProductDescriptionCultures { get; set; } = new List<ProductModelProductDescriptionCulture>();
}
