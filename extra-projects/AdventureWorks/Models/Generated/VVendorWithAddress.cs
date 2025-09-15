#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record VVendorWithAddress
{
	public int BusinessEntityId { get; set; }

	public string Name { get; set; } = null!;

	public string AddressType { get; set; } = null!;

	public string AddressLine1 { get; set; } = null!;

	public string? AddressLine2 { get; set; }

	public string City { get; set; } = null!;

	public string StateProvinceName { get; set; } = null!;

	public string PostalCode { get; set; } = null!;

	public string CountryRegionName { get; set; } = null!;
}
