using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Cross-reference table mapping stores, vendors, and employees to people
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record BusinessEntityContact{
    /// <summary>
    /// Primary key. Foreign key to BusinessEntity.BusinessEntityID.
    /// </summary>
    public int BusinessEntityId { get; set; }

    /// <summary>
    /// Primary key. Foreign key to Person.BusinessEntityID.
    /// </summary>
    public int PersonId { get; set; }

    /// <summary>
    /// Primary key.  Foreign key to ContactType.ContactTypeID.
    /// </summary>
    public int ContactTypeId { get; set; }

    /// <summary>
    /// ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.
    /// </summary>
    public Guid Rowguid { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public virtual BusinessEntity BusinessEntity { get; set; }

    public virtual ContactType ContactType { get; set; }

    public virtual Person Person { get; set; }
}
