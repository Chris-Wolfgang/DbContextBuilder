using System;
using System.Collections.Generic;

namespace AdventureWorks.Models;

/// <summary>
/// Lookup table containing the languages in which some AdventureWorks data is stored.
/// </summary>
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
