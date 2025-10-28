#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Where to send a person email.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record EmailAddress
{
	/// <summary>
	/// Primary key. Person associated with this email address.  Foreign key to Person.BusinessEntityID
	/// </summary>
	public int BusinessEntityId { get; set; }

	/// <summary>
	/// Primary key. ID of this email address.
	/// </summary>
	public int EmailAddressId { get; set; }

	/// <summary>
	/// E-mail address for the person.
	/// </summary>
	public string? EmailAddress1 { get; set; }

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
