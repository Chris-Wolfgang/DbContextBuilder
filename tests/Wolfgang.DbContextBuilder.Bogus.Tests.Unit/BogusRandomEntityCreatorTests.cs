namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Tests for <see cref="BogusRandomEntityCreator"/>.
/// </summary>
public class BogusRandomEntityCreatorTests
{
    // One property of every type BogusRandomEntityCreator has a rule for, so a single
    // generation exercises every value generator.
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
    /// Verifies that common scalar properties are populated with fake values.
    /// </summary>
    [Fact]
    public void CreateRandomEntities_populates_scalar_properties_with_fake_values()
    {
        var sut = new BogusRandomEntityCreator();

        var item = sut.CreateRandomEntities<Sample>(1).Single();

        Assert.False(string.IsNullOrEmpty(item.Name));
        Assert.NotEqual(default, item.CreatedOn);
        Assert.NotEqual(Guid.Empty, item.Reference);
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
    }
}
