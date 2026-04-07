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



    /// <summary>
    /// Verifies that Dispose can be called without error on a default builder.
    /// </summary>
    [Fact]
    public void Dispose_when_called_does_not_throw()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        // Act & Assert
        sut.Dispose();
    }



    /// <summary>
    /// Verifies that Dispose is idempotent (safe to call multiple times).
    /// </summary>
    [Fact]
    public void Dispose_when_called_multiple_times_does_not_throw()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        // Act & Assert
        sut.Dispose();
        sut.Dispose();
    }



    /// <summary>
    /// Verifies that BuildAsync throws ObjectDisposedException after Dispose.
    /// </summary>
    [Fact]
    public async Task BuildAsync_when_disposed_throws_ObjectDisposedException()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();
        sut.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>
        (
            async () => await sut.BuildAsync()
        );
    }



    /// <summary>
    /// Verifies that Dispose is safe when using the Sqlite provider.
    /// </summary>
    [Fact]
    public void Dispose_when_using_Sqlite_disposes_creator()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>().UseSqlite();

        // Act
        sut.Dispose();

        // Assert — calling Dispose twice should be safe (idempotent)
        sut.Dispose();
    }

}