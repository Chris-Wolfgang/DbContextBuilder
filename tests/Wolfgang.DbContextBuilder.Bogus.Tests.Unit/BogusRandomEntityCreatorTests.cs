using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Tests for <see cref="BogusRandomEntityCreator"/>.
/// </summary>
public class BogusRandomEntityCreatorTests
{
    // Every property on `Sample` is populated by `BogusRandomEntityCreator` via
    // reflection — the class exists to give the creator a shape with one property
    // per type-rule to exercise. R# and Sonar can only see the declaration and
    // report them as unused / unassigned; the fixture pattern is deliberate.
    // Suppressions kept on the fixture class only, not the surrounding tests.
    // ReSharper disable UnusedMember.Local
    // ReSharper disable UnusedAutoPropertyAccessor.Local
#pragma warning disable S3459 // Unassigned auto-property — assigned via reflection by Bogus
    private sealed class Sample
    {
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public byte ByteValue { get; set; }

        public short ShortValue { get; set; }

        public int Id { get; set; }

        public long LongValue { get; set; }

        public float FloatValue { get; set; }

        public double DoubleValue { get; set; }

        public decimal Price { get; set; }

        public Guid Reference { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTimeOffset UpdatedOn { get; set; }

        public DateOnly Birthday { get; set; }

        public TimeOnly OpensAt { get; set; }

        public bool? NullableBool { get; set; }

        public byte? NullableByte { get; set; }

        public short? NullableShort { get; set; }

        public int? NullableInt { get; set; }

        public long? NullableLong { get; set; }

        public float? NullableFloat { get; set; }

        public double? NullableDouble { get; set; }

        public decimal? NullableDecimal { get; set; }

        public Guid? NullableGuid { get; set; }

        public DateTime? NullableDateTime { get; set; }

        public DateTimeOffset? NullableDateTimeOffset { get; set; }

        public DateOnly? NullableDateOnly { get; set; }

        public TimeOnly? NullableTimeOnly { get; set; }

        public Shade Shade { get; set; }

        public Shade? NullableShade { get; set; }

        // Not settable from outside, so the creator must leave it alone.
        public Shade FixedShade { get; private set; } = Shade.Unset;
    }



    // An entity with a public enum-typed indexer next to a plain enum property. Bogus can only set
    // a plain property, so the creator must leave the indexer alone (#571 review).
    private sealed class IndexedSample
    {
        private readonly Dictionary<int, Shade> _byIndex = [];



        public Shade Shade { get; set; }



        public Shade this[int index]
        {
            get => _byIndex.TryGetValue(index, out var shade) ? shade : Shade.Unset;
            set => _byIndex[index] = value;
        }
    }



    // Unset is 0 and never generated, so an unpopulated property is visible.
    private enum Shade
    {
        Unset = 0,
        Light = 1,
        Dark = 2,
    }
#pragma warning restore S3459
    // ReSharper restore UnusedAutoPropertyAccessor.Local
    // ReSharper restore UnusedMember.Local



    /// <summary>
    /// Verifies that the requested number of entities is generated.
    /// </summary>
    [Fact]
    public void CreateRandomEntities_generates_the_requested_count()
    {
        var sut = new BogusRandomEntityCreator();

        var items = sut.CreateRandomEntities<Sample>(10).ToList();

        Assert.Equal(10, items.Count);
    }



    /// <summary>
    /// Verifies that a count of exactly 1, the smallest valid count, is accepted (kills the
    /// `count &lt; 1` → `count &lt;= 1` boundary mutant).
    /// </summary>
    [Fact]
    public void CreateRandomEntities_when_count_is_one_returns_one_entity()
    {
        var sut = new BogusRandomEntityCreator();

        var items = sut.CreateRandomEntities<Sample>(1).ToList();

        Assert.Single(items);
    }



    /// <summary>
    /// Verifies that every type rule fills its property. Rules whose range includes the default
    /// (bool, byte, float, double, decimal) are checked across a batch, where an all-default
    /// result is vanishingly unlikely; the others must be non-default on every entity.
    /// </summary>
    [Fact]
    public void CreateRandomEntities_populates_every_rule_typed_property()
    {
        var sut = new BogusRandomEntityCreator();

        var items = sut.CreateRandomEntities<Sample>(64).ToList();

        Assert.All(items, item => Assert.False(string.IsNullOrEmpty(item.Name)));
        Assert.All(items, item => Assert.True(item.ShortValue >= 1));
        Assert.All(items, item => Assert.InRange(item.Id, 1, 100_000));
        Assert.All(items, item => Assert.True(item.LongValue >= 1));
        Assert.All(items, item => Assert.NotEqual(Guid.Empty, item.Reference));
        Assert.All(items, item => Assert.NotEqual(default, item.CreatedOn));
        Assert.All(items, item => Assert.NotEqual(default, item.UpdatedOn));
        Assert.Contains(items, item => item.IsActive);
        Assert.Contains(items, item => !item.IsActive);
        Assert.Contains(items, item => item.ByteValue != 0);
        Assert.Contains(items, item => item.FloatValue != 0f);
        Assert.Contains(items, item => item.DoubleValue != 0d);
        Assert.Contains(items, item => item.Price != 0m);
        Assert.All(items, item => Assert.NotEqual(default, item.Birthday));
        Assert.Contains(items, item => item.OpensAt != default);
    }



    /// <summary>
    /// Verifies that the nullable form of every rule-typed value type is populated (#571):
    /// RuleForType matches the exact type, so int? needs its own rule beside int.
    /// </summary>
    [Fact]
    public void CreateRandomEntities_populates_nullable_value_type_properties()
    {
        var sut = new BogusRandomEntityCreator();

        var items = sut.CreateRandomEntities<Sample>(64).ToList();

        Assert.All(items, item => Assert.NotNull(item.NullableBool));
        Assert.All(items, item => Assert.NotNull(item.NullableByte));
        Assert.All(items, item => Assert.True(item.NullableShort >= 1));
        Assert.All(items, item => Assert.InRange(item.NullableInt!.Value, 1, 100_000));
        Assert.All(items, item => Assert.True(item.NullableLong >= 1));
        Assert.All(items, item => Assert.NotNull(item.NullableFloat));
        Assert.All(items, item => Assert.NotNull(item.NullableDouble));
        Assert.All(items, item => Assert.NotNull(item.NullableDecimal));
        Assert.All(items, item => Assert.NotEqual(Guid.Empty, item.NullableGuid!.Value));
        Assert.All(items, item => Assert.NotEqual(default, item.NullableDateTime!.Value));
        Assert.All(items, item => Assert.NotEqual(default, item.NullableDateTimeOffset!.Value));
        Assert.All(items, item => Assert.NotEqual(default, item.NullableDateOnly!.Value));
        Assert.All(items, item => Assert.NotNull(item.NullableTimeOnly));
    }



    /// <summary>
    /// Verifies that settable enum and nullable-enum properties get a defined value, and that an
    /// enum property without a public setter is left alone (#571).
    /// </summary>
    [Fact]
    public void CreateRandomEntities_populates_settable_enum_properties_only()
    {
        var sut = new BogusRandomEntityCreator();

        var items = sut.CreateRandomEntities<Sample>(64).ToList();

        Assert.Contains(items, item => item.Shade != Shade.Unset);
        Assert.All(items, item => Assert.True(Enum.IsDefined(item.Shade)));
        Assert.All(items, item => Assert.NotNull(item.NullableShade));
        Assert.All(items, item => Assert.Equal(Shade.Unset, item.FixedShade));
    }



    /// <summary>
    /// Verifies that a public enum-typed indexer is left alone: Bogus cannot set an indexer, so
    /// adding a rule for it would make generation throw (#571 review).
    /// </summary>
    [Fact]
    public void CreateRandomEntities_when_entity_has_an_enum_indexer_leaves_it_alone()
    {
        var sut = new BogusRandomEntityCreator();

        var items = sut.CreateRandomEntities<IndexedSample>(8).ToList();

        Assert.Contains(items, item => item.Shade != Shade.Unset);
        Assert.All(items, item => Assert.Equal(Shade.Unset, item[0]));
        items[0][0] = Shade.Dark;
        Assert.Equal(Shade.Dark, items[0][0]);
    }



    /// <summary>
    /// Verifies that two creators with the same seed generate the same entities, call for call.
    /// </summary>
    [Fact]
    public void CreateRandomEntities_with_the_same_seed_is_reproducible()
    {
        var first = new BogusRandomEntityCreator(seed: 42);
        var second = new BogusRandomEntityCreator(seed: 42);

        Assert.Equal(Fingerprint(first.CreateRandomEntities<Sample>(5)), Fingerprint(second.CreateRandomEntities<Sample>(5)));
        Assert.Equal(Fingerprint(first.CreateRandomEntities<Sample>(5)), Fingerprint(second.CreateRandomEntities<Sample>(5)));
    }



    /// <summary>
    /// Verifies that successive calls on one seeded creator, and creators with different seeds,
    /// generate different entities.
    /// </summary>
    [Fact]
    public void CreateRandomEntities_with_a_seed_varies_between_calls_and_seeds()
    {
        var sut = new BogusRandomEntityCreator(seed: 42);
        var other = new BogusRandomEntityCreator(seed: 43);

        var firstCall = Fingerprint(sut.CreateRandomEntities<Sample>(5));
        var secondCall = Fingerprint(sut.CreateRandomEntities<Sample>(5));
        var otherSeed = Fingerprint(other.CreateRandomEntities<Sample>(5));

        Assert.NotEqual(firstCall, secondCall);
        Assert.NotEqual(firstCall, otherSeed);
    }



    private static string Fingerprint(IEnumerable<Sample> items) =>
        string.Join("|", items.Select(item => $"{item.Name}/{item.Id}/{item.Reference}/{item.Shade}"));



    /// <summary>
    /// Verifies that generated entities vary (random generation, not constant values).
    /// </summary>
    [Fact]
    public void CreateRandomEntities_produces_varied_values_across_entities()
    {
        var sut = new BogusRandomEntityCreator();

        var items = sut.CreateRandomEntities<Sample>(20).ToList();

        Assert.Equal(20, items.Select(item => item.Reference).Distinct().Count());
    }



    /// <summary>
    /// Verifies that a count below one throws ArgumentOutOfRangeException.
    /// </summary>
    [Fact]
    public void CreateRandomEntities_when_count_is_less_than_one_throws_ArgumentOutOfRangeException()
    {
        var sut = new BogusRandomEntityCreator();

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => sut.CreateRandomEntities<Sample>(0));
        Assert.Equal("count", ex.ParamName);
        Assert.Equal(0, ex.ActualValue);
        // The same message as AutoFixtureRandomEntityCreator (#571).
        Assert.StartsWith("Value cannot be less than 1", ex.Message, StringComparison.Ordinal);
    }
}
