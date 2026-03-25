#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Type of phone number of a person.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record PhoneNumberType
{
	/// <summary>
	/// Primary key for telephone number type records.
	/// </summary>
	public int PhoneNumberTypeId { get; set; }

	/// <summary>
	/// Name of the telephone number type
	/// </summary>
	public string Name { get; set; } = null!;

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual ICollection<PersonPhone> PersonPhones { get; set; } = new List<PersonPhone>();
}
