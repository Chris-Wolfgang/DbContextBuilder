using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Current version number of the AdventureWorks 2016 sample database. 
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record AwbuildVersion{
    /// <summary>
    /// Primary key for AWBuildVersion records.
    /// </summary>
    public byte SystemInformationId { get; set; }

    /// <summary>
    /// Version number of the database in 9.yy.mm.dd.00 format.
    /// </summary>
    public string DatabaseVersion { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime VersionDate { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    public DateTime ModifiedDate { get; set; }
}
