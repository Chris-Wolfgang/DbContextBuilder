using AdventureWorks.Models;
using System;
using System.Collections.Generic;
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
        await using var context = await sut.CreateDbContextAsync(optionsBuilder);

        // Assert
        Assert.NotNull(context);
        Assert.True(context.Database.IsSqlite());
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
    /// #599: passing the active creator to SetCreateDbContext again is a no-op: the creator is
    /// not disposed (its connection still holds the database) and the shared service provider
    /// is kept.
    /// </summary>
    [Fact]
    public async Task SetCreateDbContext_with_the_active_creator_keeps_it_and_its_service_provider()
    {
        using var builder = new DbContextBuilder<BasicContext>().UseSqlite();
        var creator = Assert.IsType<SqliteDbContextCreator>(builder.CreateDbContext);
        await using (await builder.BuildAsync())
        {
        }
        var provider = builder.InternalServiceProvider;

        builder.SetCreateDbContext(creator);

        Assert.False(creator.IsDisposed);
        Assert.Same(creator, builder.CreateDbContext);
        Assert.Same(provider, builder.InternalServiceProvider);
    }



    /// <summary>
    /// Re-selecting InMemory after a SQLite flavor must leave a working InMemory builder: the
    /// SQLite services and model customizer the extension registered are dropped, so BuildAsync
    /// no longer builds an internal service provider without the InMemory services (#558).
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UseInMemory_after_a_Sqlite_flavor_builds_an_InMemory_context(bool forMsSqlServer)
    {
        using var builder = new DbContextBuilder<BasicContext>();
        if (forMsSqlServer) { builder.UseSqliteForMsSqlServer(); } else { builder.UseSqlite(); }

        builder.UseInMemory().SeedWith(NewLog("reselected"));
        await using var context = await builder.BuildAsync();

        Assert.Empty(builder.ServiceCollection);
        Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", context.Database.ProviderName);
        Assert.Equal("reselected", Assert.Single(context.DatabaseLogs).Event);
    }



    /// <summary>
    /// With a caller-supplied options builder, a SQLite build followed by re-selecting InMemory
    /// builds an InMemory context: BuildAsync applies its per-build configuration (internal
    /// service provider, provider, logging) to a copy, so the caller's builder never carries the
    /// earlier SQLite configuration into the next build (#558 review).
    /// </summary>
    [Fact]
    public async Task UseInMemory_after_a_Sqlite_build_with_a_caller_options_builder_builds_an_InMemory_context()
    {
        var options = new DbContextOptionsBuilder<BasicContext>();
        using var builder = new DbContextBuilder<BasicContext>().UseDbContextOptionsBuilder(options).UseSqlite();
        await using (var sqlite = await builder.BuildAsync())
        {
            Assert.Equal("Microsoft.EntityFrameworkCore.Sqlite", sqlite.Database.ProviderName);
        }

        builder.UseInMemory().SeedWith(NewLog("reselected"));
        await using var context = await builder.BuildAsync();

        Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", context.Database.ProviderName);
        Assert.Equal("reselected", Assert.Single(context.DatabaseLogs).Event);
    }



    /// <summary>
    /// BuildAsync leaves the caller-supplied options builder exactly as the caller configured it.
    /// </summary>
    [Fact]
    public async Task BuildAsync_does_not_modify_the_caller_options_builder()
    {
        var options = new DbContextOptionsBuilder<BasicContext>();
        var before = options.Options.Extensions.Select(e => e.GetType()).ToList();
        using var builder = new DbContextBuilder<BasicContext>().UseDbContextOptionsBuilder(options).UseSqlite().UseDiagnosticOutput(_ => { });

        await using var context = await builder.BuildAsync();

        Assert.Equal(before, options.Options.Extensions.Select(e => e.GetType()).ToList());
    }



    /// <summary>
    /// After Dispose, every configuration entry point throws <see cref="ObjectDisposedException"/>
    /// instead of accepting new state (#563).
    /// </summary>
    [Theory]
    [MemberData(nameof(ConfigurationCalls))]
    public void Configuration_after_Dispose_throws_ObjectDisposedException(string call)
    {
        var builder = new DbContextBuilder<BasicContext>();
        builder.Dispose();

        var ex = Assert.Throws<ObjectDisposedException>(() => ConfigurationCallsByName[call](builder));

        Assert.Equal(nameof(DbContextBuilder<BasicContext>), ex.ObjectName);
    }



    /// <summary>
    /// UseSqlite on a disposed builder registers nothing and opens no connection that a later
    /// Dispose could never release (#563).
    /// </summary>
    [Fact]
    public void UseSqlite_after_Dispose_registers_no_services_and_creates_no_creator()
    {
        var builder = new DbContextBuilder<BasicContext>();
        builder.Dispose();

        Assert.Throws<ObjectDisposedException>(() => builder.UseSqlite());

        Assert.Empty(builder.ServiceCollection);
        Assert.Null(builder.CreateDbContext);
    }



    // BasicContext is internal, so the theory takes the call's name and looks the action up here.
    private static readonly Dictionary<string, Action<DbContextBuilder<BasicContext>>> ConfigurationCallsByName = new()
    {
        ["UseInMemory"] = b => b.UseInMemory(),
        ["UseSqlite"] = b => b.UseSqlite(),
        ["UseSqliteForMsSqlServer"] = b => b.UseSqliteForMsSqlServer(),
        ["UseCustomDbContextCreator"] = b => b.UseCustomDbContextCreator(null!), // the disposed check runs first
        ["UseCustomRandomEntityCreator"] = b => b.UseCustomRandomEntityCreator(new DeterministicRandomEntityCreator()),
        ["UseDbContextOptionsBuilder"] = b => b.UseDbContextOptionsBuilder(new DbContextOptionsBuilder<BasicContext>()),
        ["UseSeedProfile"] = b => b.UseSeedProfile(null!), // the disposed check runs before the null check
        ["UseDiagnosticOutput"] = b => b.UseDiagnosticOutput(_ => { }),
        // Internal, but it has its own disposed guard: covered directly, not only through its callers.
        ["SetCreateDbContext"] = b => b.SetCreateDbContext(new InMemoryDbContextCreator()),
        ["SeedWith(IEnumerable)"] = b => b.SeedWith(new List<DatabaseLog> { NewLog("a") }.AsEnumerable()),
        ["SeedWith(params)"] = b => b.SeedWith(NewLog("a"), NewLog("b")),
        ["SeedWith(entity)"] = b => b.SeedWith(NewLog("a")),
        ["SeedWithRandom(count)"] = b => b.SeedWithRandom<DatabaseLog>(1),
        ["SeedWithRandom(count, func)"] = b => b.SeedWithRandom<DatabaseLog>(1, e => e),
        ["SeedWithRandom(count, func with index)"] = b => b.SeedWithRandom<DatabaseLog>(1, (e, _) => e),
    };



    /// <summary>
    /// The names of the configuration entry points the disposed-builder theory exercises.
    /// </summary>
    public static TheoryData<string> ConfigurationCalls()
    {
        var data = new TheoryData<string>();
        foreach (var name in ConfigurationCallsByName.Keys)
        {
            data.Add(name);
        }
        return data;
    }



    private static DatabaseLog NewLog(string evt) => new()
    {
        PostTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        DatabaseUser = "user",
        Event = evt,
        Schema = "dbo",
        Object = "obj",
        Tsql = "select 1",
        XmlEvent = "<e/>",
    };
}
