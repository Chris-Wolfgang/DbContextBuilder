#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Current version number of the AdventureWorks 2016 sample database. 
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial class AwbuildVersion
{
	/// <summary>
	/// Primary key for AWBuildVersion records.
	/// </summary>
	public byte SystemInformationId { get; set; }

	/// <summary>
	/// Version number of the database in 9.yy.mm.dd.00 format.
	/// </summary>
	public string DatabaseVersion { get; set; } = null!;

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime VersionDate { get; set; }

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }
}
