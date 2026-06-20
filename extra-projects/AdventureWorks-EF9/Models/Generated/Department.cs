using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Lookup table containing the departments within the Adventure Works Cycles company.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record Department{
    /// <summary>
    /// Primary key for Department records.
    /// </summary>
    public short DepartmentId { get; set; }

    /// <summary>
    /// Name of the department.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Name of the group to which the department belongs.
    /// </summary>
    public string GroupName { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public virtual ICollection<EmployeeDepartmentHistory> EmployeeDepartmentHistories { get; set; } = new List<EmployeeDepartmentHistory>();
}
