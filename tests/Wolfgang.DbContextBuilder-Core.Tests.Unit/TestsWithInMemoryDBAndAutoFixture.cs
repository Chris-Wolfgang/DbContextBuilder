using AdventureWorks.Models;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Runs all the tests using the default values for DbContextBuilder
/// </summary>
public class TestsWithInMemoryDbAndAutoFixture(ITestOutputHelper testOutputHelper) : DbContextBuilderTestsBase(testOutputHelper)
{

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override DbContextBuilder<AdventureWorksDbContext> CreateDbContextBuilder()
    {
        return new DbContextBuilder<AdventureWorksDbContext>()
            .UseInMemory()
            .UseAutoFixture();
    }



    /// <summary>
    /// Verifies that UseInMemory returns a DbContext{T} for chaining additional calls
    /// </summary>
    [Fact]
    public void UseInMemory_returns_DbContextBuilder()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        // Act & Assert
        Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(sut.UseInMemory());
    }



    /// <summary>
    /// Verifies that UseAutoFixture returns a DbContext{T} for chaining additional calls
    /// </summary>
    [Fact]
    public void UseAutoFixture_returns_DbContextBuilder()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        // Act & Assert
        Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(sut.UseAutoFixture());
    }



    /// <summary>
    /// Verifies that the RandomEntityGenerator used is an instance of AutoFixtureRandomEntityGenerator
    /// </summary>
    [Fact]
    public void RandomEntityGenerator_is_AutoFixture()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        // Act & Assert
        Assert.IsType<AutoFixtureRandomEntityGenerator>(sut.RandomEntityGenerator);

    }



    /// <summary>
    /// Verifies that the database used Microsoft's InMemory database
    /// </summary>
    [Fact]
    public async Task Database_is_InMemory()
    {
        // Arrange
        var sut = await new DbContextBuilder<AdventureWorksDbContext>().BuildAsync();

        // Act & Assert
        Assert.True(sut.Database.IsInMemory());
    }



}