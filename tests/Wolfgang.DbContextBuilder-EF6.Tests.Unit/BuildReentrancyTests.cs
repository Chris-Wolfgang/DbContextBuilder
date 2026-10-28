using System;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.DbContextBuilderEF6.Tests.Unit.Models;
using Xunit;

namespace Wolfgang.DbContextBuilderEF6.Tests.Unit;

/// <summary>
/// The first Build/BuildAsync seeds the database; later builds on the same builder return another
/// context over it without seeding again, and seeding after the first build throws (#734, the EF
/// Core builder's #559).
/// </summary>
public class BuildReentrancyTests
{
    private const string AlreadySeededMessage = "Seed data must be added before the first Build() or BuildAsync() call";



    /// <summary>
    /// A second Build on one builder saves the seed data only once and leaves one row.
    /// </summary>
    [Fact]
    public void Build_when_called_twice_seeds_the_database_once()
    {
        var saves = SaveCountingContext.StartCounting();
        var sut = new DbContextBuilder<SaveCountingContext>().SeedWith(new Category { Name = "seeded" });

        using var first = sut.Build();
        using var second = sut.Build();

        Assert.Equal(1, saves.Value);
        Assert.Equal("seeded", Assert.Single(second.Categories).Name);
    }



    /// <summary>
    /// A second BuildAsync on one builder saves the seed data only once and leaves one row.
    /// </summary>
    [Fact]
    public async Task BuildAsync_when_called_twice_seeds_the_database_once()
    {
        var saves = SaveCountingContext.StartCounting();
        var sut = new DbContextBuilder<SaveCountingContext>().SeedWith(new Category { Name = "seeded" });

        using var first = await sut.BuildAsync();
        using var second = await sut.BuildAsync();

        Assert.Equal(1, saves.Value);
        Assert.Equal("seeded", Assert.Single(second.Categories).Name);
    }



    /// <summary>
    /// Build after BuildAsync (and the reverse order) also returns the already-seeded database.
    /// </summary>
    [Fact]
    public async Task Build_when_called_after_BuildAsync_does_not_seed_again()
    {
        var saves = SaveCountingContext.StartCounting();
        var sut = new DbContextBuilder<SaveCountingContext>().SeedWith(new Category { Name = "seeded" });

        using var first = await sut.BuildAsync();
        using var second = sut.Build();

        Assert.Equal(1, saves.Value);
        Assert.Single(second.Categories);
    }



    /// <summary>
    /// Every seeding method throws after the first build: the data would never reach the database.
    /// </summary>
    [Theory]
    [InlineData("SeedWith(entity)")]
    [InlineData("SeedWith(params)")]
    [InlineData("SeedWith(IEnumerable)")]
    [InlineData("SeedWithRandom(count)")]
    [InlineData("SeedWithRandom(count, func)")]
    [InlineData("SeedWithRandom(count, indexed func)")]
    public void SeedWith_and_SeedWithRandom_when_called_after_Build_throw_InvalidOperationException(string method)
    {
        var sut = new DbContextBuilder<TestDbContext>()
            .UseEffort()
            .UseAutoFixture();
        using var context = sut.Build();

        Action seed = method switch
        {
            "SeedWith(entity)" => () => sut.SeedWith(new Category()),
            "SeedWith(params)" => () => sut.SeedWith(new Category(), new Category()),
            "SeedWith(IEnumerable)" => () => sut.SeedWith(Enumerable.Repeat(new Category(), 1)),
            "SeedWithRandom(count)" => () => sut.SeedWithRandom<Category>(1),
            "SeedWithRandom(count, func)" => () => sut.SeedWithRandom<Category>(1, category => category),
            _ => () => sut.SeedWithRandom<Category>(1, (category, _) => category),
        };

        var ex = Assert.Throws<InvalidOperationException>(seed);
        Assert.StartsWith(AlreadySeededMessage, ex.Message, StringComparison.Ordinal);
    }



    /// <summary>
    /// Selecting the provider again starts a new, empty database: seeding is accepted again and the
    /// next build seeds the new database.
    /// </summary>
    [Fact]
    public void UseEffort_when_called_again_after_Build_accepts_and_seeds_the_new_database()
    {
        var sut = new DbContextBuilder<TestDbContext>()
            .UseEffort()
            .SeedWith(new Category { Name = "first" });
        using var first = sut.Build();

        sut.UseEffort().SeedWith(new Category { Name = "second" });
        using var second = sut.Build();

        Assert.Equal(new[] { "first", "second" }, second.Categories.Select(category => category.Name).ToList().OrderBy(name => name, StringComparer.Ordinal).ToArray());
        Assert.Single(first.Categories);
    }
}
