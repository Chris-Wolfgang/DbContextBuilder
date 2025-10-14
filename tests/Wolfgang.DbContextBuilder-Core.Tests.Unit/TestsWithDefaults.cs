using AdventureWorks.Models;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Runs all the tests using the default values for DbContextBuilder
/// </summary>
public class TestsWithDefaults(ITestOutputHelper testOutputHelper) : DbContextBuilderTestsBase(testOutputHelper)
{

    /// <summary>
    /// Creates a new instance of DbContextBuilder for AdventureWorksDbContext using default settings.
    /// </summary>
    /// <returns>A new DbContextBuilder for AdventureWorksDbContext.</returns>
    protected override DbContextBuilder<AdventureWorksDbContext> CreateDbContextBuilder() => new();


    /// <summary>
    /// Verifies that the default RandomEntityCreator is an instance of AutoFixtureRandomEntityCreate
    /// </summary>
    [Fact]
    public void Default_RandomEntityCreate_is_AutoFixture()
    {
	    // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        // Act & Assert
        _ = Assert.IsType<AutoFixtureRandomEntityCreator>(sut.RandomEntityCreator);
    }



    /// <summary>
    /// Verifies that the default database is Microsoft's InMemory database
    /// </summary>
    [Fact]
    public async Task Default_database_is_InMemory()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        var context = await sut.BuildAsync();

        // Act & Assert
        Assert.True(context.Database.IsInMemory());
    }

}