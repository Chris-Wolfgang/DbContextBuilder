using System;
using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Contains online customer orders until the order is submitted or cancelled.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "This is a test model and not part of the production code")]
public partial record ShoppingCartItem{
    /// <summary>
    /// Primary key for ShoppingCartItem records.
    /// </summary>
    public int ShoppingCartItemId { get; set; }

    /// <summary>
    /// Shopping cart identification number.
    /// </summary>
    public string ShoppingCartId { get; set; }

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

    public virtual Product Product { get; set; }
}
