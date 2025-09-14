#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Contains online customer orders until the order is submitted or cancelled.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial class ShoppingCartItem
{
	/// <summary>
	/// Primary key for ShoppingCartItem records.
	/// </summary>
	public int ShoppingCartItemId { get; set; }

	/// <summary>
	/// Shopping cart identification number.
	/// </summary>
	public string ShoppingCartId { get; set; } = null!;

	/// <summary>
	/// Product quantity ordered.
	/// </summary>
	public int Quantity { get; set; }

	/// <summary>
	/// Product ordered. Foreign key to Product.ProductID.
	/// </summary>
	public int ProductId { get; set; }

	/// <summary>
	/// Date the time the record was created.
	/// </summary>
	public DateTime DateCreated { get; set; }

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual Product Product { get; set; } = null!;
}
