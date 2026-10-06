using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Wolfgang.DbContextBuilderCore.Tests.Unit.Models;
using Xunit;

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



    /// <summary>
    /// Verifies that <see cref="SqliteDbContextCreator.IsDisposed"/> tracks disposal state and
    /// that Dispose is idempotent.
    /// </summary>
    [Fact]
    public void IsDisposed_is_false_until_disposed_then_true()
    {
        // Arrange
        var sut = new SqliteDbContextCreator();
        Assert.False(sut.IsDisposed);

        // Act
        sut.Dispose();

        // Assert
        Assert.True(sut.IsDisposed);

        // Idempotent — a second Dispose stays disposed without throwing.
        sut.Dispose();
        Assert.True(sut.IsDisposed);
    }



    /// <summary>
    /// Verifies that re-selecting a provider on a builder disposes the previous SQLite creator
    /// (which holds an open in-memory connection) rather than leaking it.
    /// </summary>
    [Fact]
    public void Reselecting_a_provider_disposes_the_previous_Sqlite_creator()
    {
        // Arrange — first provider is SQLite, which owns an open connection
        using var builder = new DbContextBuilder<BasicContext>().UseSqlite();
        var firstCreator = Assert.IsType<SqliteDbContextCreator>(builder.CreateDbContext);

        // Act — last-write-wins provider selection abandons the SQLite creator
        builder.UseInMemory();

        // Assert — the abandoned creator was disposed, not leaked
        Assert.True(firstCreator.IsDisposed);
    }



    /// <summary>
    /// Verifies that selecting SQLite replaces EF's default model customizer rather than
    /// leaving it registered alongside the SQLite one.
    /// </summary>
    [Fact]
    public void UseSqlite_registers_only_the_Sqlite_model_customizer()
    {
        // Arrange & Act
        using var builder = new DbContextBuilder<BasicContext>().UseSqlite();

        // Assert
        var modelCustomizer = Assert.Single
        (
            builder.ServiceCollection,
            sd => sd.ServiceType == typeof(IModelCustomizer)
        );
        Assert.Equal(typeof(SqliteModelCustomizer), modelCustomizer.ImplementationType);
    }



    /// <summary>
    /// Verifies that selecting SQLite a second time swaps the model customizer without adding
    /// service registrations, so the builder ends up with exactly one model customizer.
    /// </summary>
    [Fact]
    public void UseSqlite_when_Sqlite_already_selected_swaps_model_customizer_without_adding_services()
    {
        // Arrange — the first selection registers EF's SQLite services
        using var builder = new DbContextBuilder<BasicContext>().UseSqliteForMsSqlServer();
        var serviceCountAfterFirstSelection = builder.ServiceCollection.Count;

        // Act
        builder.UseSqlite();

        // Assert — only the swapped customizer remains, and nothing was added
        var modelCustomizer = Assert.Single
        (
            builder.ServiceCollection,
            sd => sd.ServiceType == typeof(IModelCustomizer)
        );
        Assert.Equal(typeof(SqliteModelCustomizer), modelCustomizer.ImplementationType);
        Assert.Equal
        (
            serviceCountAfterFirstSelection,
            builder.ServiceCollection.Count
        );
    }
}
