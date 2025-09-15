#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record VAdditionalContactInfo
{
	public int BusinessEntityId { get; set; }

	public string FirstName { get; set; } = null!;

	public string? MiddleName { get; set; }

	public string LastName { get; set; } = null!;

	public string? TelephoneNumber { get; set; }

	public string? TelephoneSpecialInstructions { get; set; }

	public string? Street { get; set; }

	public string? City { get; set; }

	public string? StateProvince { get; set; }

	public string? PostalCode { get; set; }

	public string? CountryRegion { get; set; }

	public string? HomeAddressSpecialInstructions { get; set; }

	public string? EmailAddress { get; set; }

	public string? EmailSpecialInstructions { get; set; }

	public string? EmailTelephoneNumber { get; set; }

	public Guid Rowguid { get; set; }

	public DateTime ModifiedDate { get; set; }
}
