using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Tests for the <see cref="DbContextBuilderBogusExtensions.UseBogus{T}"/> extension.
/// </summary>
public class UseBogusExtensionsTests
{
    /// <summary>
    /// Verifies that calling UseBogus on a null builder throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void UseBogus_when_builder_is_null_throws_ArgumentNullException()
    {
        DbContextBuilder<SampleDbContext> builder = null!;

        var ex = Assert.Throws<ArgumentNullException>(() => builder.UseBogus());
        Assert.Equal("builder", ex.ParamName);
    }



    /// <summary>
    /// Verifies that UseBogus returns the same builder instance for chaining.
    /// </summary>
    [Fact]
    public void UseBogus_returns_the_same_builder_for_chaining()
    {
        using var builder = new DbContextBuilder<SampleDbContext>();

        Assert.Same(builder, builder.UseBogus());
    }



    /// <summary>
    /// Verifies that after UseBogus, SeedWithRandom produces the requested number of rows
    /// (i.e. the Bogus provider is wired in as the random-entity creator).
    /// </summary>
    [Fact]
    public async Task UseBogus_enables_SeedWithRandom()
    {
        await using var context = await new DbContextBuilder<SampleDbContext>()
            .UseInMemory()
            .UseBogus()
            .SeedWithRandom<SampleEntity>(3)
            .BuildAsync();

        Assert.Equal(3, context.Samples.Count());
    }



    [ExcludeFromCodeCoverage(Justification = "Test model")]
    public class SampleEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }



    [ExcludeFromCodeCoverage(Justification = "Test model")]
    public class SampleDbContext(DbContextOptions<SampleDbContext> options) : DbContext(options)
    {
        public DbSet<SampleEntity> Samples => Set<SampleEntity>();
    }
}
