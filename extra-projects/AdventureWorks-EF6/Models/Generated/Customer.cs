#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Current customer information. Also see the Person and Store tables.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record Customer
{
	/// <summary>
	/// Primary key.
	/// </summary>
	public int CustomerId { get; set; }

	/// <summary>
	/// Foreign key to Person.BusinessEntityID
	/// </summary>
	public int? PersonId { get; set; }

	/// <summary>
	/// Foreign key to Store.BusinessEntityID
	/// </summary>
	public int? StoreId { get; set; }

	/// <summary>
	/// ID of the territory in which the customer is located. Foreign key to SalesTerritory.SalesTerritoryID.
	/// </summary>
	public int? TerritoryId { get; set; }

	/// <summary>
	/// Unique number identifying the customer assigned by the accounting system.
	/// </summary>
	public string AccountNumber { get; set; } = null!;

	/// <summary>
	/// ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.
	/// </summary>
	public Guid Rowguid { get; set; }

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual Person? Person { get; set; }

	public virtual ICollection<SalesOrderHeader> SalesOrderHeaders { get; set; } = new List<SalesOrderHeader>();

	public virtual Store? Store { get; set; }

	public virtual SalesTerritory? Territory { get; set; }
}
