using BenchmarkDotNet.Attributes;

namespace Wolfgang.DbContextBuilderCore.Benchmarks;

/// <summary>
/// Baseline microbenchmarks for <see cref="DbContextBuilder{T}.BuildAsync"/>
/// across the most common usage shapes. Scenarios:
///
/// <list type="bullet">
///   <item><c>InMemory_NoSeed</c> — empty options, no data seeded.</item>
///   <item><c>InMemory_SeedWith_N</c> — N pre-built entities seeded via
///   <see cref="DbContextBuilder{T}.SeedWith{TEntity}(TEntity[])"/>.</item>
///   <item><c>InMemory_SeedWithRandom_N</c> — N AutoFixture-generated
///   entities seeded via
///   <see cref="DbContextBuilder{T}.SeedWithRandom{TEntity}(int)"/>.</item>
/// </list>
///
/// MemoryDiagnoser is on so allocation regressions surface in the gh-pages
/// benchmark chart immediately.
///
/// Each benchmark disposes the constructed <see cref="BenchmarkContext"/>
/// before returning so successive iterations don't accumulate in-memory
/// databases or tracked entities that would distort the allocation
/// measurements.
/// </summary>
[MemoryDiagnoser]
public class BuildAsyncBenchmarks
{
    /// <summary>
    /// Row counts for the seed scenarios. Kept small (1, 10, 100) so the
    /// chart shows the per-row component of build cost without dominating
    /// runtime.
    /// </summary>
    [Params(1, 10, 100)]
    public int SeedCount { get; set; }

    private BenchmarkEntity[] _seedRows = [];

    /// <summary>
    /// Pre-builds the <see cref="BenchmarkEntity"/> array once per
    /// SeedCount value so the <c>SeedWith</c> benchmark measures only the
    /// builder cost, not entity construction.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _seedRows = new BenchmarkEntity[SeedCount];
        for (var i = 0; i < SeedCount; i++)
        {
            _seedRows[i] = new BenchmarkEntity { Id = i + 1, Name = $"row-{i + 1}" };
        }
    }



    /// <summary>
    /// Baseline: builder with default options (InMemory provider), no seed.
    /// Measures the irreducible cost of <see cref="DbContextBuilder{T}.BuildAsync"/>.
    /// </summary>
    [Benchmark(Baseline = true)]
    public async Task InMemory_NoSeed()
    {
        using var builder = new DbContextBuilder<BenchmarkContext>();
        await using var context = await builder.BuildAsync().ConfigureAwait(false);
    }



    /// <summary>
    /// Builder with <c>SeedCount</c> pre-built entities seeded via
    /// <see cref="DbContextBuilder{T}.SeedWith{TEntity}(TEntity[])"/>.
    /// </summary>
    [Benchmark]
    public async Task InMemory_SeedWith()
    {
        using var builder = new DbContextBuilder<BenchmarkContext>()
            .SeedWith(_seedRows);
        await using var context = await builder.BuildAsync().ConfigureAwait(false);
    }



    /// <summary>
    /// Builder with <c>SeedCount</c> AutoFixture-generated entities seeded
    /// via <see cref="DbContextBuilder{T}.SeedWithRandom{TEntity}(int)"/>.
    /// Captures the AutoFixture cost on top of the baseline.
    /// </summary>
    [Benchmark]
    public async Task InMemory_SeedWithRandom()
    {
        using var builder = new DbContextBuilder<BenchmarkContext>()
            .UseAutoFixture()
            .SeedWithRandom<BenchmarkEntity>(SeedCount);
        await using var context = await builder.BuildAsync().ConfigureAwait(false);
    }
}
