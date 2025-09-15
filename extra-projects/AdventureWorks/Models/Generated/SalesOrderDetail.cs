#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Individual products associated with a specific sales order. See SalesOrderHeader.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record SalesOrderDetail
{
	/// <summary>
	/// Primary key. Foreign key to SalesOrderHeader.SalesOrderID.
	/// </summary>
	public int SalesOrderId { get; set; }

	/// <summary>
	/// Primary key. One incremental unique number per product sold.
	/// </summary>
	public int SalesOrderDetailId { get; set; }

	/// <summary>
	/// Shipment tracking number supplied by the shipper.
	/// </summary>
	public string? CarrierTrackingNumber { get; set; }

	/// <summary>
	/// Quantity ordered per product.
	/// </summary>
	public short OrderQty { get; set; }

	/// <summary>
	/// Product sold to customer. Foreign key to Product.ProductID.
	/// </summary>
	public int ProductId { get; set; }

	/// <summary>
	/// Promotional code. Foreign key to SpecialOffer.SpecialOfferID.
	/// </summary>
	public int SpecialOfferId { get; set; }

	/// <summary>
	/// Selling price of a single product.
	/// </summary>
	public decimal UnitPrice { get; set; }

	/// <summary>
	/// Discount amount.
	/// </summary>
	public decimal UnitPriceDiscount { get; set; }

	/// <summary>
	/// Per product subtotal. Computed as UnitPrice * (1 - UnitPriceDiscount) * OrderQty.
	/// </summary>
	public decimal LineTotal { get; set; }

	/// <summary>
	/// ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.
	/// </summary>
	public Guid Rowguid { get; set; }

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual SalesOrderHeader SalesOrder { get; set; } = null!;

	public virtual SpecialOfferProduct SpecialOfferProduct { get; set; } = null!;
}
