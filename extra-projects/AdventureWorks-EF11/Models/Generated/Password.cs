using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// One way hashed authentication information
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record Password{
    public int BusinessEntityId { get; set; }

    /// <summary>
    /// Password for the e-mail account.
    /// </summary>
    public string PasswordHash { get; set; }

    /// <summary>
    /// Random value concatenated with the password string before the password is hashed.
    /// </summary>
    public string PasswordSalt { get; set; }

    /// <summary>
    /// ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.
    /// </summary>
    public Guid Rowguid { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public virtual Person BusinessEntity { get; set; }
}
