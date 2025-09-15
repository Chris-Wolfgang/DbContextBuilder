#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial record VProductModelCatalogDescription
{
	public int ProductModelId { get; set; }

	public string Name { get; set; } = null!;

	public string? Summary { get; set; }

	public string? Manufacturer { get; set; }

	public string? Copyright { get; set; }

	public string? ProductUrl { get; set; }

	public string? WarrantyPeriod { get; set; }

	public string? WarrantyDescription { get; set; }

	public string? NoOfYears { get; set; }

	public string? MaintenanceDescription { get; set; }

	public string? Wheel { get; set; }

	public string? Saddle { get; set; }

	public string? Pedal { get; set; }

	public string? BikeFrame { get; set; }

	public string? Crankset { get; set; }

	public string? PictureAngle { get; set; }

	public string? PictureSize { get; set; }

	public string? ProductPhotoId { get; set; }

	public string? Material { get; set; }

	public string? Color { get; set; }

	public string? ProductLine { get; set; }

	public string? Style { get; set; }

	public string? RiderExperience { get; set; }

	public Guid Rowguid { get; set; }

	public DateTime ModifiedDate { get; set; }
}
