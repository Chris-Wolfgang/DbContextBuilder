using Microsoft.Coyote;
using Microsoft.Coyote.SystematicTesting;
using Microsoft.EntityFrameworkCore;
using Wolfgang.DbContextBuilderCore;

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
        var rows = await context.Entities.ToListAsync().ConfigureAwait(false);

        VerifyIsolated(tag, rows.Select(r => r.Tag).ToList());
    }


    // Extracted so the failure path (which has no other way to execute during a normal,
    // passing test run - it only fires when isolation is genuinely broken) can be pinned by a
    // direct, deterministic unit test instead of relying on Coyote to ever schedule a real
    // cross-contamination.
    internal static void VerifyIsolated(string tag, IReadOnlyList<string> seenTags)
    {
        if (seenTags.Count != 1 || !string.Equals(seenTags[0], tag, StringComparison.Ordinal))
        {
            var seen = string.Join(", ", seenTags);
            throw new InvalidOperationException(
                $"Isolation violated: builder tagged '{tag}' expected exactly one row of its own "
                + $"data, but saw {seenTags.Count} row(s) [{seen}] - another concurrent builder's "
                + "seed data leaked across DbContextBuilder<T> instances.");
        }
    }


    [Fact]
    public void VerifyIsolated_throws_when_another_builders_data_leaked_in()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => VerifyIsolated("first", ["first", "second"]));

        Assert.Contains("first", exception.Message, StringComparison.Ordinal);
    }


    // EF Core's InMemory materializer assigns ConcurrencyEntity.Id via a direct field write
    // rather than the compiler-generated setter (a known optimization), and the isolation test
    // above only ever reads Tag back - so Id's own get/set accessors need a direct test to be
    // exercised at all.
    [Fact]
    public void ConcurrencyEntity_Id_round_trips()
    {
        var entity = new ConcurrencyEntity { Id = 42, Tag = "whatever" };

        Assert.Equal(42, entity.Id);
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
