using System;
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
    }



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
        Assert.StartsWith("Count must be greater than 0", ex.Message, StringComparison.Ordinal);
    }
}
