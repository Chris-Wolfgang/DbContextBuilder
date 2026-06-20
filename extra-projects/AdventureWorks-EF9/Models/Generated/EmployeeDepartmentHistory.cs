using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Employee department transfers.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record EmployeeDepartmentHistory{
    /// <summary>
    /// Employee identification number. Foreign key to Employee.BusinessEntityID.
    /// </summary>
    public int BusinessEntityId { get; set; }

    /// <summary>
    /// Department in which the employee worked including currently. Foreign key to Department.DepartmentID.
    /// </summary>
    public short DepartmentId { get; set; }

    /// <summary>
    /// Identifies which 8-hour shift the employee works. Foreign key to Shift.Shift.ID.
    /// </summary>
    public byte ShiftId { get; set; }

    /// <summary>
    /// Date the employee started work in the department.
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Date the employee left the department. NULL = Current department.
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public virtual Employee BusinessEntity { get; set; }

    public virtual Department Department { get; set; }

    public virtual Shift Shift { get; set; }
}
