using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Type of phone number of a person.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record PhoneNumberType{
    /// <summary>
    /// Primary key for telephone number type records.
    /// </summary>
    public int PhoneNumberTypeId { get; set; }

    /// <summary>
    /// Name of the telephone number type
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public virtual ICollection<PersonPhone> PersonPhones { get; set; } = new List<PersonPhone>();
}
