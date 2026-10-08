using System;
using System.Linq;
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
    /// The creator's connection is an in-memory database, and disposing the creator closes it.
    /// </summary>
    [Fact]
    public async Task Dispose_closes_the_in_memory_connection_the_contexts_use()
    {
        var sut = new SqliteDbContextCreator();
        using var context = await sut.CreateDbContextAsync(new DbContextOptionsBuilder<BasicContext>());
        var connection = context.Database.GetDbConnection();

        Assert.Equal("DataSource=:memory:", connection.ConnectionString);
        Assert.Equal(System.Data.ConnectionState.Open, connection.State);

        sut.Dispose();

        Assert.Equal(System.Data.ConnectionState.Closed, connection.State);
    }



    /// <summary>
    /// <c>UseSqlite</c> rejects a null builder with <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void UseSqlite_when_builder_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => ((DbContextBuilder<BasicContext>)null!).UseSqlite());

        Assert.Equal("builder", ex.ParamName);
    }



    /// <summary>
    /// <c>UseSqliteForMsSqlServer</c> rejects a null builder with <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void UseSqliteForMsSqlServer_when_builder_is_null_throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => ((DbContextBuilder<BasicContext>)null!).UseSqliteForMsSqlServer());

        Assert.Equal("builder", ex.ParamName);
    }



    /// <summary>
    /// Re-selecting a SQLite flavor removes the earlier flavor's model customizer registration, so
    /// the customizer EF resolves is the one selected last.
    /// </summary>
    [Fact]
    public void Reselecting_a_Sqlite_flavor_removes_the_previous_model_customizer()
    {
        using var builder = new DbContextBuilder<BasicContext>().UseSqliteForMsSqlServer();

        builder.UseSqlite();

        var customizers = builder.ServiceCollection
            .Where(sd => sd.ServiceType == typeof(IModelCustomizer))
            .Select(sd => sd.ImplementationType)
            .ToList();
        Assert.DoesNotContain(typeof(SqliteForMsSqlServerModelCustomizer), customizers);
        Assert.Equal(typeof(SqliteModelCustomizer), customizers.Last());
    }
}
