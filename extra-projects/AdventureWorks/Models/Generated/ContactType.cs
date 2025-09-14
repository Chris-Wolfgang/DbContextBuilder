#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Lookup table containing the types of business entity contacts.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial class ContactType
{
	/// <summary>
	/// Primary key for ContactType records.
	/// </summary>
	public int ContactTypeId { get; set; }

	/// <summary>
	/// Contact type description.
	/// </summary>
	public string Name { get; set; } = null!;

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual ICollection<BusinessEntityContact> BusinessEntityContacts { get; set; } = new List<BusinessEntityContact>();
}
