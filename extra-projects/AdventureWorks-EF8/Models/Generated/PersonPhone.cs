using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Telephone number and type of a person.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record PersonPhone{
    /// <summary>
    /// Business entity identification number. Foreign key to Person.BusinessEntityID.
    /// </summary>
    public int BusinessEntityId { get; set; }

    /// <summary>
    /// Telephone number identification number.
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Kind of phone number. Foreign key to PhoneNumberType.PhoneNumberTypeID.
    /// </summary>
    public int PhoneNumberTypeId { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public virtual Person BusinessEntity { get; set; }

    public virtual PhoneNumberType PhoneNumberType { get; set; }
}
