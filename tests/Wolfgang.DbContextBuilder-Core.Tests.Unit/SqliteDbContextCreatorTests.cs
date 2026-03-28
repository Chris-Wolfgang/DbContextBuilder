using Microsoft.EntityFrameworkCore;
using Wolfgang.DbContextBuilderCore.Tests.Unit.Models;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Tests for <see cref="SqliteDbContextCreator"/> to ensure coverage of Dispose and CreateDbContextAsync.
/// </summary>
public class SqliteDbContextCreatorTests
{


    /// <summary>
    /// Verifies that CreateDbContextAsync returns a context configured with SQLite.
    /// </summary>
    [Fact]
    public async Task CreateDbContextAsync_returns_configured_context()
    {
        // Arrange
        using var sut = new SqliteDbContextCreator();
        var optionsBuilder = new DbContextOptionsBuilder<BasicContext>();

        // Act
        var context = await sut.CreateDbContextAsync(optionsBuilder);

        // Assert
        Assert.NotNull(context);
        Assert.True(context.Database.IsSqlite());
        context.Dispose();
    }



    /// <summary>
    /// Verifies that Dispose can be called without error.
    /// </summary>
    [Fact]
    public void Dispose_can_be_called()
    {
        // Arrange
        var sut = new SqliteDbContextCreator();

        // Act & Assert — no exception
        sut.Dispose();
    }



    /// <summary>
    /// Verifies that Dispose can be called multiple times without error.
    /// </summary>
    [Fact]
    public void Dispose_can_be_called_multiple_times()
    {
        // Arrange
        var sut = new SqliteDbContextCreator();

        // Act & Assert — no exception
        sut.Dispose();
        sut.Dispose();
    }
}
