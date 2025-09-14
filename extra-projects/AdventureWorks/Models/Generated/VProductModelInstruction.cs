#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Diagnostics.CodeAnalysis;

namespace AdventureWorks.Models;

[ExcludeFromCodeCoverage(Justification = "These are test models created by scaffolding the database and should not be tested")]
public partial class VProductModelInstruction
{
	public int ProductModelId { get; set; }

	public string Name { get; set; } = null!;

	public string? Instructions { get; set; }

	public int? LocationId { get; set; }

	public decimal? SetupHours { get; set; }

	public decimal? MachineHours { get; set; }

	public decimal? LaborHours { get; set; }

	public int? LotSize { get; set; }

	public string? Step { get; set; }

	public Guid Rowguid { get; set; }

	public DateTime ModifiedDate { get; set; }
}
