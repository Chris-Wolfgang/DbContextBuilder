using System;
using System.Threading.Tasks;
using AdventureWorks.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Runs all the tests using the default values for DbContextBuilder
/// </summary>
public class TestsWithDefaults : DbContextBuilderTestsBase
{

    /// <summary>
    /// Creates a new instance of DbContextBuilder for AdventureWorksDbContext using default settings.
    /// </summary>
    /// <returns>A new DbContextBuilder for AdventureWorksDbContext.</returns>
    /// <remarks>
    /// The builder no longer defaults to a random-entity provider, so the shared
    /// <c>SeedWithRandom</c> tests opt in to AutoFixture here. The database is left at its
    /// default (InMemory).
    /// </remarks>
    protected override DbContextBuilder<AdventureWorksDbContext> CreateDbContextBuilder() =>
        new DbContextBuilder<AdventureWorksDbContext>().UseAutoFixture();


    /// <summary>
    /// Verifies that SeedWithRandom throws when no random-entity provider has been configured
    /// (the builder no longer defaults to AutoFixture).
    /// </summary>
    [Fact]
    public void SeedWithRandom_when_no_random_provider_configured_throws_InvalidOperationException()
    {
        // Arrange — a builder with no UseAutoFixture/UseBogus/UseCustomRandomEntityCreator call
        using var sut = new DbContextBuilder<AdventureWorksDbContext>();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => sut.SeedWithRandom<Address>(1));
        Assert.Contains("UseAutoFixture", ex.Message, StringComparison.Ordinal);
    }



    /// <summary>
    /// Verifies that the default database is Microsoft's InMemory database
    /// </summary>
    [Fact]
    public async Task Default_database_is_InMemory()
    {
        // Arrange
        using var sut = new DbContextBuilder<AdventureWorksDbContext>();

        await using var context = await sut.BuildAsync();

        // Act & Assert
        Assert.True(context.Database.IsInMemory());
    }



    /// <summary>
    /// Verifies that BuildAsync throws ObjectDisposedException after Dispose.
    /// </summary>
    [Fact]
    public async Task BuildAsync_when_disposed_throws_ObjectDisposedException()
    {
        // Arrange
        using var sut = new DbContextBuilder<AdventureWorksDbContext>();
        sut.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>
        (
            async () => await sut.BuildAsync()
        );
    }



    /// <summary>
    /// Verifies that disposing a SQLite-backed builder disposes its creator, which closes the
    /// in-memory connection, and that a second Dispose is a no-op.
    /// </summary>
    [Fact]
    public void Dispose_when_using_Sqlite_disposes_creator()
    {
        // Arrange
        using var sut = new DbContextBuilder<AdventureWorksDbContext>().UseSqlite();
        var creator = Assert.IsType<SqliteDbContextCreator>(sut.CreateDbContext);

        // Act
        sut.Dispose();
        sut.Dispose();

        // Assert
        Assert.True(creator.IsDisposed);
    }
}