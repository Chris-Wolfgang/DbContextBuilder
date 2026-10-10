using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bogus;

namespace Wolfgang.DbContextBuilderCore;

/// <summary>
/// An <see cref="ICreateRandomEntities"/> implementation backed by
/// <see href="https://github.com/bchavez/Bogus">Bogus</see>. It auto-populates the common
/// scalar property types of an entity with realistic-looking fake values, leaving navigation
/// and other reference-type properties unset (so seeding does not pull in object graphs).
/// </summary>
/// <remarks>
/// Populated property types: <see cref="string"/>, <see cref="bool"/>, <see cref="byte"/>,
/// <see cref="short"/>, <see cref="int"/>, <see cref="long"/>, <see cref="float"/>,
/// <see cref="double"/>, <see cref="decimal"/>, <see cref="Guid"/>, <see cref="DateTime"/>,
/// <see cref="DateTimeOffset"/>, <see cref="DateOnly"/>, <see cref="TimeOnly"/>, the nullable
/// forms of all of these value types, and any enum (or nullable enum) with a public setter.
/// </remarks>
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
    // Null for the unseeded creator. Seeded, it hands each CreateRandomEntities call its own seed,
    // so a run is reproducible while successive calls still produce different entities.
    private readonly Randomizer? _seeds;



    /// <summary>
    /// Creates an instance that uses Bogus's shared, unseeded randomizer.
    /// </summary>
    public BogusRandomEntityCreator()
    {
    }



    /// <summary>
    /// Creates an instance whose output is reproducible: two creators made with the same
    /// <paramref name="seed"/> generate the same entities, call for call (#571).
    /// </summary>
    /// <param name="seed">The seed for the random values.</param>
    public BogusRandomEntityCreator(int seed)
    {
        _seeds = new Randomizer(seed);
    }



    /// <inheritdoc />
    public IEnumerable<TEntity> CreateRandomEntities<TEntity>(int count) where TEntity : class
    {
        // The same guard, message and ActualValue as AutoFixtureRandomEntityCreator (#571).
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Value cannot be less than 1");
        }

        // Bogus is non-strict by default: properties without a rule are left at their
        // default value, so navigation/reference-type properties stay unset.
        var faker = new Faker<TEntity>()
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
            .RuleForType(typeof(DateTimeOffset), f => f.Date.PastOffset())
            .RuleForType(typeof(DateOnly), f => DateOnly.FromDateTime(f.Date.Past()))
            .RuleForType(typeof(TimeOnly), f => TimeOnly.FromDateTime(f.Date.Past()))
            // RuleForType matches the exact type, so the nullable forms need their own rules.
            .RuleForType(typeof(bool?), f => (bool?)f.Random.Bool())
            .RuleForType(typeof(byte?), f => (byte?)f.Random.Byte())
            .RuleForType(typeof(short?), f => (short?)f.Random.Short(1))
            .RuleForType(typeof(int?), f => (int?)f.Random.Int(1, 100_000))
            .RuleForType(typeof(long?), f => (long?)f.Random.Long(1))
            .RuleForType(typeof(float?), f => (float?)f.Random.Float())
            .RuleForType(typeof(double?), f => (double?)f.Random.Double())
            .RuleForType(typeof(decimal?), f => (decimal?)f.Random.Decimal(0, 100_000))
            .RuleForType(typeof(Guid?), f => (Guid?)f.Random.Guid())
            .RuleForType(typeof(DateTime?), f => (DateTime?)f.Date.Past())
            .RuleForType(typeof(DateTimeOffset?), f => (DateTimeOffset?)f.Date.PastOffset())
            .RuleForType(typeof(DateOnly?), f => (DateOnly?)DateOnly.FromDateTime(f.Date.Past()))
            .RuleForType(typeof(TimeOnly?), f => (TimeOnly?)TimeOnly.FromDateTime(f.Date.Past()));

        // Enums cannot be matched by RuleForType (each is its own type): add a rule per settable
        // enum-typed property, picking one of the enum's defined values.
        foreach (var property in typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.SetMethod?.IsPublic == true))
        {
            var enumType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            if (enumType.IsEnum)
            {
                var values = Enum.GetValues(enumType).Cast<object>().ToArray();
                faker.RuleFor(property.Name, f => f.PickRandom(values));
            }
        }

        if (_seeds is not null)
        {
            faker.UseSeed(_seeds.Int());
        }

        return faker.Generate(count);
    }
}
