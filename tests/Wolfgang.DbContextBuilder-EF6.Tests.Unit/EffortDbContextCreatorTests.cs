using System;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using Wolfgang.DbContextBuilderEF6.Tests.Unit.Models;
using Xunit;

namespace Wolfgang.DbContextBuilderEF6.Tests.Unit;

public class EffortDbContextCreatorTests
{

    /// <summary>
    /// Verifies that CreateDbContext returns a valid context instance.
    /// </summary>
    [Fact]
    public void CreateDbContext_returns_valid_context()
    {
        // Arrange
        using var sut = new EffortDbContextCreator();

        // Act
        var context = sut.CreateDbContext<TestDbContext>();

        // Assert
        Assert.NotNull(context);
        Assert.IsType<TestDbContext>(context);
    }



    /// <summary>
    /// Verifies that CreateDbContext can create multiple contexts sharing the same connection.
    /// </summary>
    [Fact]
    public void CreateDbContext_returns_distinct_instances_sharing_same_database()
    {
        // Arrange
        using var sut = new EffortDbContextCreator();

        // Act
        var context1 = sut.CreateDbContext<TestDbContext>();
        var context2 = sut.CreateDbContext<TestDbContext>();

        // Assert
        Assert.NotSame(context1, context2);
    }



    /// <summary>
    /// Verifies that Dispose can be called without error.
    /// </summary>
    [Fact]
    public void Dispose_can_be_called()
    {
        // Arrange
        var sut = new EffortDbContextCreator();

        // Act & Assert — should not throw
        sut.Dispose();
    }



    /// <summary>
    /// Verifies that Dispose can be called multiple times without error.
    /// </summary>
    [Fact]
    public void Dispose_can_be_called_multiple_times()
    {
        // Arrange
        var sut = new EffortDbContextCreator();

        // Act & Assert — should not throw
        sut.Dispose();
        sut.Dispose();
    }



    /// <summary>
    /// Verifies that CreateDbContext throws when the DbContext type
    /// does not have a (DbConnection, bool) constructor.
    /// </summary>
    [Fact]
    public void CreateDbContext_when_context_has_no_matching_constructor_throws()
    {
        // Arrange
        using var sut = new EffortDbContextCreator();

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => sut.CreateDbContext<NoConnectionConstructorContext>());
    }


    /// <summary>
    /// Contexts are created with contextOwnsConnection: false, so disposing one leaves the shared
    /// connection (and the in-memory database other contexts use) alone.
    /// </summary>
    [Fact]
    public void Disposing_a_context_does_not_dispose_the_shared_connection()
    {
        using var sut = new EffortDbContextCreator();
        var disposals = 0;
        using (var context = sut.CreateDbContext<TestDbContext>())
        {
            context.Database.Connection.Disposed += (_, _) => disposals++;
        }

        Assert.Equal(0, disposals);
    }



    /// <summary>
    /// Dispose disposes the shared connection exactly once, however often it is called.
    /// </summary>
    [Fact]
    public void Dispose_disposes_the_shared_connection_once()
    {
        var sut = new EffortDbContextCreator();
        var disposals = 0;
        using (var context = sut.CreateDbContext<TestDbContext>())
        {
            context.Database.Connection.Disposed += (_, _) => disposals++;
        }

        sut.Dispose();
        sut.Dispose();

        Assert.Equal(1, disposals);
    }

}



/// <summary>
/// A DbContext without a (DbConnection, bool) constructor for testing error paths.
/// </summary>
[ExcludeFromCodeCoverage]
internal class NoConnectionConstructorContext : DbContext
{
    public NoConnectionConstructorContext() : base("name=NonExistent")
    {
    }
}
