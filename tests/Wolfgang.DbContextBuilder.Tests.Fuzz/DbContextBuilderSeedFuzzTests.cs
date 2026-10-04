using Microsoft.EntityFrameworkCore;
using Wolfgang.DbContextBuilderCore;

namespace Wolfgang.DbContextBuilder.Tests.Fuzz;

/// <summary>
/// Fuzz properties for the core <see cref="DbContextBuilder{T}"/> seeding API: whatever is
/// seeded is exactly what the built context holds. The creator suites fuzz the
/// <see cref="ICreateRandomEntities"/> contract; these fuzz the builder that consumes it.
/// Each case builds a real InMemory context, which costs far more than a creator call, so
/// inputs are clamped to <see cref="MagnitudeBound"/>. Clamped, not skipped: an early
/// "out of scope, return true" line would never run at the default case count and would
/// break this assembly's 100% line gate.
/// </summary>
[Trait("Category", "Fuzz")]
public sealed class DbContextBuilderSeedFuzzTests
{
    private const int MagnitudeBound = 500;



    /// <summary>
    /// <c>SeedWith</c> round-trips every entity: same count, same values, nothing added or
    /// dropped. Covers the empty set too, which seeds nothing and must build an empty table.
    /// <c>EndSize</c> raises FsCheck's size ceiling from its default of 100, which never produced an
    /// array longer than 47 in 2,000 cases. With it, 10,000 cases reach 247 entities (23% of cases
    /// above 100); <see cref="MagnitudeBound"/> caps the rest.
    /// </summary>
    [FuzzProperty(EndSize = MagnitudeBound)]
    public async Task<bool> SeedWith_round_trips_every_entity(string?[] names, bool firstIsActive)
    {
        ArgumentNullException.ThrowIfNull(names);

        var seeded = names
            .Take(MagnitudeBound)
            .Select((name, i) => new FuzzEntity
            {
                Id = i + 1,
                Name = name ?? string.Empty,
                IsActive = firstIsActive ^ (i % 2 == 1),
                Price = i * 1.25m,
            })
            .ToList();

        using var builder = new DbContextBuilder<FuzzDbContext>();
        await using var context = await builder.UseInMemory().SeedWith(seeded).BuildAsync().ConfigureAwait(false);
        try
        {
            var saved = await context.Entities.AsNoTracking().OrderBy(e => e.Id).ToListAsync().ConfigureAwait(false);

            return saved.Count == seeded.Count
                && saved.Zip(seeded).All(pair =>
                    pair.First.Id == pair.Second.Id
                    && string.Equals(pair.First.Name, pair.Second.Name, StringComparison.Ordinal)
                    && pair.First.IsActive == pair.Second.IsActive
                    && pair.First.Price == pair.Second.Price);
        }
        finally
        {
            // Each builder seeds a GUID-named database in EF's process-wide InMemory root, and
            // disposing the context does not remove it. Over the weekly 100,000-case run every
            // case's rows would stay resident for the life of the process.
            await context.Database.EnsureDeletedAsync().ConfigureAwait(false);
        }
    }



    /// <summary>
    /// #515/#530: whatever integer keys a random creator hands back (colliding, negative, or 0,
    /// which EF reads as "not set"), SeedWithRandom builds, and every entity gets its own key.
    /// A 0 is always prepended, so every case covers the unset key and the count is never 0.
    /// </summary>
    [FuzzProperty(EndSize = MagnitudeBound)]
    public async Task<bool> SeedWithRandom_gives_every_entity_a_distinct_key(int[] keys)
    {
        ArgumentNullException.ThrowIfNull(keys);

        var ids = keys.Take(MagnitudeBound).Prepend(0).ToArray();

        using var builder = new DbContextBuilder<FuzzDbContext>();
        await using var context = await builder
            .UseInMemory()
            .UseCustomRandomEntityCreator(new FixedKeyCreator(ids))
            .SeedWithRandom<FuzzEntity>(ids.Length)
            .BuildAsync()
            .ConfigureAwait(false);
        try
        {
            var saved = await context.Entities.AsNoTracking().Select(e => e.Id).ToListAsync().ConfigureAwait(false);

            return saved.Count == ids.Length && saved.Distinct().Count() == ids.Length;
        }
        finally
        {
            await context.Database.EnsureDeletedAsync().ConfigureAwait(false);
        }
    }
}



/// <summary>Hands back one <see cref="FuzzEntity"/> per given key, in order.</summary>
internal sealed class FixedKeyCreator(int[] ids) : ICreateRandomEntities
{
    public IEnumerable<TEntity> CreateRandomEntities<TEntity>(int count)
        where TEntity : class
        => ids.Take(count).Select(id => (TEntity)(object)new FuzzEntity { Id = id, Name = "fuzz" }).ToList();
}



/// <summary>InMemory context for the builder fuzz properties.</summary>
public sealed class FuzzDbContext : DbContext
{
    public FuzzDbContext(DbContextOptions<FuzzDbContext> options) : base(options)
    {
    }



    public DbSet<FuzzEntity> Entities => Set<FuzzEntity>();
}
