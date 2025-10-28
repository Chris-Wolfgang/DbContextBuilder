#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record VStoreWithContact
{
	public int BusinessEntityId { get; set; }

	public string Name { get; set; } = null!;

	public string ContactType { get; set; } = null!;

	public string? Title { get; set; }

	public string FirstName { get; set; } = null!;

	public string? MiddleName { get; set; }

	public string LastName { get; set; } = null!;

	public string? Suffix { get; set; }

	public string? PhoneNumber { get; set; }

	public string? PhoneNumberType { get; set; }

	public string? EmailAddress { get; set; }

	public int EmailPromotion { get; set; }
}
