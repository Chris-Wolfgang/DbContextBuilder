#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Telephone number and type of a person.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial class PersonPhone
{
	/// <summary>
	/// Business entity identification number. Foreign key to Person.BusinessEntityID.
	/// </summary>
	public int BusinessEntityId { get; set; }

	/// <summary>
	/// Telephone number identification number.
	/// </summary>
	public string PhoneNumber { get; set; } = null!;

	/// <summary>
	/// Kind of phone number. Foreign key to PhoneNumberType.PhoneNumberTypeID.
	/// </summary>
	public int PhoneNumberTypeId { get; set; }

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual Person BusinessEntity { get; set; } = null!;

	public virtual PhoneNumberType PhoneNumberType { get; set; } = null!;
}
