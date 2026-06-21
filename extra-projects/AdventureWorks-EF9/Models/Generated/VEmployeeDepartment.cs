using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record VEmployeeDepartment{
    public int BusinessEntityId { get; set; }

    public string Title { get; set; }

    public string FirstName { get; set; }

    public string MiddleName { get; set; }

    public string LastName { get; set; }

    public string Suffix { get; set; }

    public string JobTitle { get; set; }

    public string Department { get; set; }

    public string GroupName { get; set; }

    public DateOnly StartDate { get; set; }
}
