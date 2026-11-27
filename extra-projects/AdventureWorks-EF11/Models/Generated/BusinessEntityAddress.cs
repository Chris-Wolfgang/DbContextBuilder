using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Cross-reference table mapping customers, vendors, and employees to their addresses.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record BusinessEntityAddress{
    /// <summary>
    /// Primary key. Foreign key to BusinessEntity.BusinessEntityID.
    /// </summary>
    public int BusinessEntityId { get; set; }

    /// <summary>
    /// Primary key. Foreign key to Address.AddressID.
    /// </summary>
    public int AddressId { get; set; }

    /// <summary>
    /// Primary key. Foreign key to AddressType.AddressTypeID.
    /// </summary>
    public int AddressTypeId { get; set; }

    /// <summary>
    /// ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.
    /// </summary>
    public Guid Rowguid { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public virtual Address Address { get; set; }

    public virtual AddressType AddressType { get; set; }

    public virtual BusinessEntity BusinessEntity { get; set; }
}
