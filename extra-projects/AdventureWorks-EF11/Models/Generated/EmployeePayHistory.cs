using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Employee pay history.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record EmployeePayHistory{
    /// <summary>
    /// Employee identification number. Foreign key to Employee.BusinessEntityID.
    /// </summary>
    public int BusinessEntityId { get; set; }

    /// <summary>
    /// Date the change in pay is effective
    /// </summary>
    public DateTime RateChangeDate { get; set; }

    /// <summary>
    /// Salary hourly rate.
    /// </summary>
    public decimal Rate { get; set; }

    /// <summary>
    /// 1 = Salary received monthly, 2 = Salary received biweekly
    /// </summary>
    public byte PayFrequency { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public virtual Employee BusinessEntity { get; set; }
}
