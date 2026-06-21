using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Work shift lookup table.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record Shift{
    /// <summary>
    /// Primary key for Shift records.
    /// </summary>
    public byte ShiftId { get; set; }

    /// <summary>
    /// Shift description.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Shift start time.
    /// </summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// Shift end time.
    /// </summary>
    public TimeOnly EndTime { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public virtual ICollection<EmployeeDepartmentHistory> EmployeeDepartmentHistories { get; set; } = new List<EmployeeDepartmentHistory>();
}
