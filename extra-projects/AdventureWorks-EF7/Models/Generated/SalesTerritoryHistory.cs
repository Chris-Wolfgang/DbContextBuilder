#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Sales representative transfers to other sales territories.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record SalesTerritoryHistory
{
    /// <summary>
	/// Primary key. The sales rep.  Foreign key to SalesPerson.BusinessEntityID.
	/// </summary>
	public int BusinessEntityId { get; set; }

	/// <summary>
	/// Primary key. Territory identification number. Foreign key to SalesTerritory.SalesTerritoryID.
	/// </summary>
	public int TerritoryId { get; set; }

	/// <summary>
	/// Primary key. Date the sales representive started work in the territory.
	/// </summary>
	public DateTime StartDate { get; set; }

	/// <summary>
	/// Date the sales representative left work in the territory.
	/// </summary>
	public DateTime? EndDate { get; set; }

	/// <summary>
	/// ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.
	/// </summary>
	public Guid Rowguid { get; set; }

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual SalesPerson BusinessEntity { get; set; } = null!;

	public virtual SalesTerritory Territory { get; set; } = null!;
}
