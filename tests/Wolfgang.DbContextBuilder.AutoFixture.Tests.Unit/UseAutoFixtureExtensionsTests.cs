namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Tests for the <see cref="DbContextBuilderAutoFixtureExtensions.UseAutoFixture{T}"/> extension.
/// </summary>
public class UseAutoFixtureExtensionsTests
{
    /// <summary>
    /// Verifies that calling UseAutoFixture on a null builder throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void UseAutoFixture_when_builder_is_null_throws_ArgumentNullException()
    {
        DbContextBuilder<SampleDbContext> builder = null!;

        var ex = Assert.Throws<ArgumentNullException>(() => builder.UseAutoFixture());
        Assert.Equal("builder", ex.ParamName);
    }



    /// <summary>
    /// Verifies that UseAutoFixture returns the same builder instance for chaining.
    /// </summary>
    [Fact]
    public void UseAutoFixture_returns_the_same_builder_for_chaining()
    {
        using var builder = new DbContextBuilder<SampleDbContext>();

        Assert.Same(builder, builder.UseAutoFixture());
    }



    /// <summary>
    /// Verifies that after UseAutoFixture, SeedWithRandom produces the requested number of rows
    /// (i.e. the AutoFixture provider is wired in as the random-entity creator).
    /// </summary>
    [Fact]
    public async Task UseAutoFixture_enables_SeedWithRandom()
    {
        await using var context = await new DbContextBuilder<SampleDbContext>()
            .UseInMemory()
            .UseAutoFixture()
            .SeedWithRandom<SampleEntity>(3)
            .BuildAsync();

        Assert.Equal(3, context.Samples.Count());
    }
}
