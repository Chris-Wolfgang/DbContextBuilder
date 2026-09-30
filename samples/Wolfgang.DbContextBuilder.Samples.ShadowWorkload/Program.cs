using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wolfgang.DbContextBuilder.Samples.ShadowWorkload;
using Wolfgang.DbContextBuilderCore;
using Wolfgang.DbContextBuilderCore.Assertions;

// Realistic mixed-usage workload for shadow-testing (#292) -- distinct from
// benchmarks/ (curated BDN micro-benchmarks graphed on every push to main): this
// replays END-TO-END builder scenarios (varying seed sizes, both providers,
// concurrent builders) and reports mean latency + mean allocated bytes per
// scenario, comparable across two runs of this same program (see the csproj's
// UseBaselinePackage/BaselineVersion toggle and shadow.yaml).
//
// Scoped to -Core-EF10's own public surface, every builder entry point included:
// UseInMemory, UseSqlite, UseSqliteForMsSqlServer, UseDbContextOptionsBuilder,
// SeedWith, SeedWithRandom (through UseCustomRandomEntityCreator with the sample's own
// deterministic creator, so no second package has to be baselined), UseSeedProfile,
// UseDiagnosticOutput, BuildAsync, and the Should() assertions. Every scenario must
// compile against the baseline package too (0.8.1 has all of them).

var outputPath = args.Length > 0 ? args[0] : "shadow-results.json";
var iterations = args.Length > 1 && int.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : 100;
const int WarmupIterations = 5;
const int ConcurrentBuilders = 20;

var results = new Dictionary<string, ScenarioResult>(StringComparer.Ordinal);

results["NoSeed"] = await MeasureAsync("NoSeed", iterations, NoSeedAsync).ConfigureAwait(false);
results["SeedWith10"] = await MeasureAsync("SeedWith10", iterations, () => SeedWithAsync(10)).ConfigureAwait(false);
results["SeedWith100"] = await MeasureAsync("SeedWith100", iterations, () => SeedWithAsync(100)).ConfigureAwait(false);
results["SqliteSeedWith10"] = await MeasureAsync("SqliteSeedWith10", iterations, () => SqliteSeedWithAsync(10)).ConfigureAwait(false);
results["SqliteForMsSqlServerSeedWith10"] = await MeasureAsync("SqliteForMsSqlServerSeedWith10", iterations, () => SqliteForMsSqlServerSeedWithAsync(10)).ConfigureAwait(false);
results["OptionsBuilderSeedWith10"] = await MeasureAsync("OptionsBuilderSeedWith10", iterations, () => OptionsBuilderSeedWithAsync(10)).ConfigureAwait(false);
results["SeedWithRandom100"] = await MeasureAsync("SeedWithRandom100", iterations, () => SeedWithRandomAsync(100)).ConfigureAwait(false);
results["SeedProfile10"] = await MeasureAsync("SeedProfile10", iterations, () => SeedProfileAsync(10)).ConfigureAwait(false);
results["DiagnosticOutputSeedWith10"] = await MeasureAsync("DiagnosticOutputSeedWith10", iterations, () => DiagnosticOutputSeedWithAsync(10)).ConfigureAwait(false);
results["AssertAfterSeedWith10"] = await MeasureAsync("AssertAfterSeedWith10", iterations, () => AssertAfterSeedWithAsync(10)).ConfigureAwait(false);
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



static Product[] CreateProducts(int count)
{
    var products = new Product[count];
    for (var i = 0; i < count; i++)
    {
        products[i] = new Product { Name = $"Widget {i.ToString(CultureInfo.InvariantCulture)}", Price = 9.99m + i };
    }

    return products;
}



static async Task SqliteForMsSqlServerSeedWithAsync(int count)
{
    using var builder = new DbContextBuilder<ShopDbContext>();
    builder.UseSqliteForMsSqlServer().SeedWith(CreateProducts(count));
    await using var context = await builder.BuildAsync().ConfigureAwait(false);
}



static async Task OptionsBuilderSeedWithAsync(int count)
{
    var options = new DbContextOptionsBuilder<ShopDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
    using var builder = new DbContextBuilder<ShopDbContext>();
    builder.UseDbContextOptionsBuilder(options).SeedWith(CreateProducts(count));
    await using var context = await builder.BuildAsync().ConfigureAwait(false);
}



static async Task SeedWithRandomAsync(int count)
{
    using var builder = new DbContextBuilder<ShopDbContext>();
    builder.UseInMemory().UseCustomRandomEntityCreator(new SampleRandomEntityCreator()).SeedWithRandom<Product>(count);
    await using var context = await builder.BuildAsync().ConfigureAwait(false);
}



static async Task SeedProfileAsync(int count)
{
    using var builder = new DbContextBuilder<ShopDbContext>();
    builder.UseInMemory().UseSeedProfile(new ShopSeedProfile(count));
    await using var context = await builder.BuildAsync().ConfigureAwait(false);
}



static async Task DiagnosticOutputSeedWithAsync(int count)
{
    using var builder = new DbContextBuilder<ShopDbContext>();
    builder.UseInMemory().UseDiagnosticOutput(static _ => { }).SeedWith(CreateProducts(count));
    await using var context = await builder.BuildAsync().ConfigureAwait(false);
}



static async Task AssertAfterSeedWithAsync(int count)
{
    using var builder = new DbContextBuilder<ShopDbContext>();
    builder.UseInMemory().SeedWith(CreateProducts(count));
    await using var context = await builder.BuildAsync().ConfigureAwait(false);
    await context.Products.Should().HaveCount(count).ConfigureAwait(false);
    await context.Products.Should().AllSatisfy(p => p.Price > 0m).ConfigureAwait(false);
}
