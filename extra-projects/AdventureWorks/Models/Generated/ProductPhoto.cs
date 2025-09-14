#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

/// <summary>
/// Product images.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial class ProductPhoto
{
	/// <summary>
	/// Primary key for ProductPhoto records.
	/// </summary>
	public int ProductPhotoId { get; set; }

	/// <summary>
	/// Small image of the product.
	/// </summary>
	public byte[]? ThumbNailPhoto { get; set; }

	/// <summary>
	/// Small image file name.
	/// </summary>
	public string? ThumbnailPhotoFileName { get; set; }

	/// <summary>
	/// Large image of the product.
	/// </summary>
	public byte[]? LargePhoto { get; set; }

	/// <summary>
	/// Large image file name.
	/// </summary>
	public string? LargePhotoFileName { get; set; }

	/// <summary>
	/// Date and time the record was last updated.
	/// </summary>
	public DateTime ModifiedDate { get; set; }

	public virtual ICollection<ProductProductPhoto> ProductProductPhotos { get; set; } = new List<ProductProductPhoto>();
}
