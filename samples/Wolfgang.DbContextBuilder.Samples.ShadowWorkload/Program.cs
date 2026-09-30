using System.Globalization;
using System.Text.Json;
using Wolfgang.DbContextBuilder.Samples.ShadowWorkload;
using Wolfgang.DbContextBuilderCore;

// Realistic mixed-usage workload for shadow-testing (#292) -- distinct from
// benchmarks/ (curated BDN micro-benchmarks graphed on every push to main): this
// replays END-TO-END builder scenarios (varying seed sizes, both providers,
// concurrent builders) and reports mean latency + mean allocated bytes per
// scenario, comparable across two runs of this same program (see the csproj's
// UseBaselinePackage/BaselineVersion toggle and shadow.yaml).
//
// Scoped to -Core-EF10's own surface (UseInMemory/UseSqlite/SeedWith/BuildAsync) --
// no random-entity-creator scenario, since that would require baselining a SECOND
// package (.Bogus or .AutoFixture) alongside -Core-EF10, doubling the
// UseBaselinePackage/BaselineVersion toggle complexity for one more scenario.

var outputPath = args.Length > 0 ? args[0] : "shadow-results.json";
var iterations = args.Length > 1 && int.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : 100;
const int WarmupIterations = 5;
const int ConcurrentBuilders = 20;

var results = new Dictionary<string, ScenarioResult>(StringComparer.Ordinal);

results["NoSeed"] = await MeasureAsync("NoSeed", iterations, NoSeedAsync).ConfigureAwait(false);
results["SeedWith10"] = await MeasureAsync("SeedWith10", iterations, () => SeedWithAsync(10)).ConfigureAwait(false);
results["SeedWith100"] = await MeasureAsync("SeedWith100", iterations, () => SeedWithAsync(100)).ConfigureAwait(false);
results["SqliteSeedWith10"] = await MeasureAsync("SqliteSeedWith10", iterations, () => SqliteSeedWithAsync(10)).ConfigureAwait(false);
results["ConcurrentBuilds20"] = await MeasureConcurrentAsync("ConcurrentBuilds20", iterations, ConcurrentBuilders, NoSeedAsync).ConfigureAwait(false);

var report = new ShadowReport
(
    DateTime.UtcNow,
    typeof(DbContextBuilder<>).Assembly.GetName().Version?.ToString() ?? "unknown",
    results
);

var json = JsonSerializer.Serialize(report, ShadowJsonContext.Default.ShadowReport);
await File.WriteAllTextAsync(outputPath, json).ConfigureAwait(false);
Console.WriteLine($"Wrote {outputPath} ({results.Count} scenarios, {iterations} iterations each, library {report.LibraryVersion}).");

return 0;



static async Task<ScenarioResult> MeasureAsync(string name, int iterationCount, Func<Task> operation)
{
    for (var i = 0; i < WarmupIterations; i++)
    {
        await operation().ConfigureAwait(false);
    }

    var elapsedMs = new double[iterationCount];
    var allocatedBytes = new long[iterationCount];

    for (var i = 0; i < iterationCount; i++)
    {
        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var startedAt = DateTime.UtcNow;

        await operation().ConfigureAwait(false);

        elapsedMs[i] = (DateTime.UtcNow - startedAt).TotalMilliseconds;
        allocatedBytes[i] = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
    }

    Console.WriteLine($"{name}: {iterationCount} iterations done.");
    return new ScenarioResult(iterationCount, elapsedMs.Average(), allocatedBytes.Average());
}



// GC.GetAllocatedBytesForCurrentThread() is per-thread; once work fans out
// across Task.WhenAll (with no ConfigureAwait(false) guarantee of staying on
// one thread), attributing allocations to "the operation" stops being
// meaningful. Concurrent scenarios report wall-clock latency only.
static async Task<ScenarioResult> MeasureConcurrentAsync
(
    string name,
    int iterationCount,
    int concurrentCallers,
    Func<Task> operation
)
{
    for (var i = 0; i < WarmupIterations; i++)
    {
        await operation().ConfigureAwait(false);
    }

    var elapsedMs = new double[iterationCount];

    for (var i = 0; i < iterationCount; i++)
    {
        var startedAt = DateTime.UtcNow;

        var tasks = new Task[concurrentCallers];
        for (var c = 0; c < concurrentCallers; c++)
        {
            tasks[c] = operation();
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        elapsedMs[i] = (DateTime.UtcNow - startedAt).TotalMilliseconds;
    }

    Console.WriteLine($"{name}: {iterationCount} iterations of {concurrentCallers} concurrent builders done.");
    return new ScenarioResult(iterationCount, elapsedMs.Average(), AllocatedBytesMean: null);
}



static async Task NoSeedAsync()
{
    using var builder = new DbContextBuilder<ShopDbContext>();
    builder.UseInMemory();
    await using var context = await builder.BuildAsync().ConfigureAwait(false);
}



static async Task SeedWithAsync(int count)
{
    var products = new Product[count];
    for (var i = 0; i < count; i++)
    {
        products[i] = new Product { Name = $"Widget {i.ToString(CultureInfo.InvariantCulture)}", Price = 9.99m + i };
    }

    using var builder = new DbContextBuilder<ShopDbContext>();
    builder.UseInMemory().SeedWith(products);
    await using var context = await builder.BuildAsync().ConfigureAwait(false);
}



static async Task SqliteSeedWithAsync(int count)
{
    var products = new Product[count];
    for (var i = 0; i < count; i++)
    {
        products[i] = new Product { Name = $"Widget {i.ToString(CultureInfo.InvariantCulture)}", Price = 9.99m + i };
    }

    using var builder = new DbContextBuilder<ShopDbContext>();
    builder.UseSqlite().SeedWith(products);
    await using var context = await builder.BuildAsync().ConfigureAwait(false);
}
