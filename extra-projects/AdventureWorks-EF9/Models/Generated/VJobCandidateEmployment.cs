using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record VJobCandidateEmployment{
    public int JobCandidateId { get; set; }

    public DateTime? EmpStartDate { get; set; }

    public DateTime? EmpEndDate { get; set; }

    public string EmpOrgName { get; set; }

    public string EmpJobTitle { get; set; }

    public string EmpResponsibility { get; set; }

    public string EmpFunctionCategory { get; set; }

    public string EmpIndustryCategory { get; set; }

    public string EmpLocCountryRegion { get; set; }

    public string EmpLocState { get; set; }

    public string EmpLocCity { get; set; }
}
