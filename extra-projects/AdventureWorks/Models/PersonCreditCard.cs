#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Cross-reference table mapping people to their credit card information in the CreditCard table. 
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial class PersonCreditCard
{
	/// <summary>
	/// Business entity identification number. Foreign key to Person.BusinessEntityID.
	/// </summary>
	public int BusinessEntityId { get; set; }

	/// <summary>
	/// Credit card identification number. Foreign key to CreditCard.CreditCardID.
	/// </summary>
	public int CreditCardId { get; set; }

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual Person BusinessEntity { get; set; } = null!;

	public virtual CreditCard CreditCard { get; set; } = null!;
}
