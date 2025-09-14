#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// One way hashed authentication information
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial class Password
{
	public int BusinessEntityId { get; set; }

	/// <summary>
	/// Password for the e-mail account.
	/// </summary>
	public string PasswordHash { get; set; } = null!;

	/// <summary>
	/// Random value concatenated with the password string before the password is hashed.
	/// </summary>
	public string PasswordSalt { get; set; } = null!;

	/// <summary>
	/// ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.
	/// </summary>
	public Guid Rowguid { get; set; }

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual Person BusinessEntity { get; set; } = null!;
}
