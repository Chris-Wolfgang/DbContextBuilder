using System.Text.Json.Serialization;

namespace Wolfgang.DbContextBuilder.Samples.ShadowWorkload;

public sealed record ScenarioResult(int Iterations, double MeanMs, double? AllocatedBytesMean);

public sealed record ShadowReport(DateTime GeneratedAtUtc, string LibraryVersion, IReadOnlyDictionary<string, ScenarioResult> Operations);

[JsonSerializable(typeof(ShadowReport))]
internal sealed partial class ShadowJsonContext : JsonSerializerContext
{
}
