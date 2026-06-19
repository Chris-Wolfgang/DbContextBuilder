using System;
using System.Collections.Generic;

namespace AdventureWorks.Models;

public partial record VEmployeeDepartmentHistory{
    public int BusinessEntityId { get; set; }

    public string Title { get; set; }

    public string FirstName { get; set; }

    public string MiddleName { get; set; }

    public string LastName { get; set; }

    public string Suffix { get; set; }

    public string Shift { get; set; }

    public string Department { get; set; }

    public string GroupName { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}
