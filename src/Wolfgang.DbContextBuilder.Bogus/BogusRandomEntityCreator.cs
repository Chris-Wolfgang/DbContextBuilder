using System;
using System.Collections.Generic;
using Bogus;

namespace Wolfgang.DbContextBuilderCore;

/// <summary>
/// An <see cref="ICreateRandomEntities"/> implementation backed by
/// <see href="https://github.com/bchavez/Bogus">Bogus</see>. It auto-populates the common
/// scalar property types of an entity with realistic-looking fake values, leaving navigation
/// and other reference-type properties unset (so seeding does not pull in object graphs).
/// </summary>
/// <remarks>
/// Enable it with <c>UseBogus()</c> (or, equivalently,
/// <c>UseCustomRandomEntityCreator(new BogusRandomEntityCreator())</c>):
/// <example>
/// <code>
/// await using var context = await new DbContextBuilder&lt;ShopDbContext&gt;()
///     .UseInMemory()
///     .UseBogus()
///     .SeedWithRandom&lt;Product&gt;(50)
///     .BuildAsync();
/// </code>
/// </example>
/// Entity types must expose a public parameterless constructor (Bogus instantiates them).
/// </remarks>
public class BogusRandomEntityCreator : ICreateRandomEntities
{
    /// <inheritdoc />
    public IEnumerable<TEntity> CreateRandomEntities<TEntity>(int count) where TEntity : class
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than 0");
        }

        var faker = FakerCache<TEntity>.Instance;

        // Faker<T> keeps per-instance generation state, so one cached instance is not safe to
        // drive from two threads at once; seeding calls are short, so a lock is enough.
        lock (faker)
        {
            return faker.Generate(count);
        }
    }



    /// <summary>
    /// Holds one configured <see cref="Faker{T}"/> per entity type, so the twelve type rules are
    /// registered once per type instead of on every <see cref="CreateRandomEntities{TEntity}(int)"/> call (#572).
    /// </summary>
    /// <typeparam name="TEntity">The entity type the cached faker generates.</typeparam>
    private static class FakerCache<TEntity> where TEntity : class
    {
        // Bogus is non-strict by default: properties without a rule are left at their
        // default value, so navigation/reference-type properties stay unset.
        internal static readonly Faker<TEntity> Instance = new Faker<TEntity>()
            .RuleForType(typeof(string), f => f.Lorem.Word())
            .RuleForType(typeof(bool), f => f.Random.Bool())
            .RuleForType(typeof(byte), f => f.Random.Byte())
            .RuleForType(typeof(short), f => f.Random.Short(1))
            .RuleForType(typeof(int), f => f.Random.Int(1, 100_000))
            .RuleForType(typeof(long), f => f.Random.Long(1))
            .RuleForType(typeof(float), f => f.Random.Float())
            .RuleForType(typeof(double), f => f.Random.Double())
            .RuleForType(typeof(decimal), f => f.Random.Decimal(0, 100_000))
            .RuleForType(typeof(Guid), f => f.Random.Guid())
            .RuleForType(typeof(DateTime), f => f.Date.Past())
            .RuleForType(typeof(DateTimeOffset), f => f.Date.PastOffset());
    }
}
