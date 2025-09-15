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
    /// 
    /// </summary>
    /// <returns></returns>
    protected override DbContextBuilder<AdventureWorksDbContext> CreateDbContextBuilder()
    {
        return new DbContextBuilder<AdventureWorksDbContext>();
    }



    /// <summary>
    /// The SQL command to select schema and table names from the database.
    /// </summary>
    /// <remarks>
    /// If the database does not support schemas, like Sqlite, then return and empty string form the schema
    /// </remarks>
    protected override string SelectTablesCommandText => throw new NotImplementedException();



    /// <summary>
    /// Verifies that the default RandomEntityGenerator is an instance of AutoFixtureRandomEntityGenerator
    /// </summary>
    [Fact]
    public void Default_RandomEntityGenerator_is_AutoFixture()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        // Act & Assert
        Assert.IsType<AutoFixtureRandomEntityGenerator>(sut.RandomEntityGenerator);

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