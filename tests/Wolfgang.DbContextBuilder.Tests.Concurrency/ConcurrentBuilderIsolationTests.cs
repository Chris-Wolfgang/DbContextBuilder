using Microsoft.Coyote;
using Microsoft.Coyote.SystematicTesting;
using Microsoft.EntityFrameworkCore;
using Wolfgang.DbContextBuilderCore;
using Xunit;

namespace Wolfgang.DbContextBuilder.Tests.Concurrency;

/// <summary>
/// Systematic exploration of concurrent <see cref="DbContextBuilder{T}"/> usage (#302) — the
/// realistic scenario is a parallel test run where many test classes build their own
/// InMemory-backed context at the same time. <see cref="InMemoryDbContextCreator"/> isolates
/// each builder with a fresh <see cref="Guid"/>-named database, and this suite proves that
/// isolation holds under adversarial interleaving of the async build/seed path, not just under
/// the ordinary uncontrolled scheduling a plain concurrent unit test would exercise.
/// </summary>
public class ConcurrentBuilderIsolationTests
{
    // This project is auto-discovered by pr.yaml's `find ./tests -name *.csproj` test loop,
    // so it runs on every PR with NO `coyote rewrite` step - a real, if uncontrolled, execution
    // of the concurrent scenario. The dedicated coyote.yaml workflow rewrites both assemblies
    // first (for genuine systematic interleaving exploration) and overrides
    // COYOTE_ITERATIONS to a much larger budget. Default here is deliberately modest - same
    // convention as Tests.Fuzz's FUZZ_CASES - so the auto-discovered run stays cheap and
    // reliable rather than repeating an uncontrolled scenario thousands of times for no extra
    // coverage.
    private static readonly uint Iterations =
        uint.TryParse(Environment.GetEnvironmentVariable("COYOTE_ITERATIONS"), out var n) ? n : 5;



    [Fact]
    public void ConcurrentBuilders_do_not_leak_data_across_instances()
    {
        // WithPotentialDeadlocksReportedAsBugs(false): matches the convention this suite is
        // modeled on (AuditTrail's ConcurrentSchemaInstallTests) - the EF Core InMemory
        // provider does real work Coyote cannot fully control, and the periodic
        // deadlock-detection heuristic can mistake ordinary latency for a hang. A real bug
        // still fails the test via the exception thrown below.
        var config = Configuration.Create()
            .WithTestingIterations(Iterations)
            .WithPotentialDeadlocksReportedAsBugs(false);
        var engine = TestingEngine.Create(config, RunConcurrentBuildsAsync);
        engine.Run();

        Assert.True(engine.TestReport.NumOfFoundBugs == 0, engine.TestReport.GetText(config));
    }



    private static async Task RunConcurrentBuildsAsync()
    {
        var first = BuildSeedAndVerifyAsync("first");
        var second = BuildSeedAndVerifyAsync("second");
        await Task.WhenAll(first, second).ConfigureAwait(false);
    }



    private static async Task BuildSeedAndVerifyAsync(string tag)
    {
        using var builder = new DbContextBuilder<ConcurrencyTestContext>();
        builder
            .UseInMemory()
            .SeedWith(new ConcurrencyEntity { Tag = tag });

        await using var context = await builder.BuildAsync().ConfigureAwait(false);
        var rows = await context.Set<ConcurrencyEntity>().ToListAsync().ConfigureAwait(false);

        if (rows.Count != 1 || !string.Equals(rows[0].Tag, tag, StringComparison.Ordinal))
        {
            var seen = string.Join(", ", rows.Select(r => r.Tag));
            throw new InvalidOperationException(
                $"Isolation violated: builder tagged '{tag}' expected exactly one row of its own "
                + $"data, but saw {rows.Count} row(s) [{seen}] - another concurrent builder's seed "
                + "data leaked across DbContextBuilder<T> instances.");
        }
    }
}



/// <summary>Minimal DbContext + entity used only to exercise concurrent builder isolation.</summary>
public sealed class ConcurrencyTestContext : DbContext
{
    public ConcurrencyTestContext(DbContextOptions<ConcurrencyTestContext> options) : base(options)
    {
    }


    public DbSet<ConcurrencyEntity> Entities => Set<ConcurrencyEntity>();
}


public sealed class ConcurrencyEntity
{
    public int Id { get; set; }
    public string Tag { get; set; } = string.Empty;
}
