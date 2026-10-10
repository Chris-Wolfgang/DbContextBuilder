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
/// Tests for calling <see cref="DbContextBuilder{T}.BuildAsync"/> more than once on one builder (#559):
/// the database is created and seeded once, and the EF Core service provider is built once and
/// disposed with the builder.
/// </summary>
public class BuildAsyncReentrancyTests
{
    /// <summary>
    /// A second BuildAsync returns another context over the already-seeded database instead of
    /// seeding it again (which failed on a duplicate key on both providers).
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BuildAsync_when_called_twice_seeds_the_database_once(bool sqlite)
    {
        using var builder = new DbContextBuilder<BasicContext>();
        if (sqlite) { builder.UseSqlite(); } else { builder.UseInMemory(); }
        builder.SeedWith(NewLog("only"));

        await using var first = await builder.BuildAsync();
        await using var second = await builder.BuildAsync();

        Assert.NotSame(first, second);
        Assert.Equal("only", Assert.Single(await second.DatabaseLogs.ToListAsync()).Event);
        Assert.Single(await first.DatabaseLogs.ToListAsync());
    }



    /// <summary>
    /// The second build writes its own diagnostic line instead of re-reporting the seeded rows.
    /// </summary>
    [Fact]
    public async Task BuildAsync_when_called_twice_reports_that_the_database_is_already_seeded()
    {
        var lines = new List<string>();
        using var builder = new DbContextBuilder<BasicContext>()
            .UseInMemory()
            .UseDiagnosticOutput(lines.Add)
            .SeedWith(NewLog("a"));

        await using var first = await builder.BuildAsync();
        await using var second = await builder.BuildAsync();

        Assert.Single(lines, line => line.Contains("seeded 1 entity row(s)", StringComparison.Ordinal));
        Assert.Single(lines, line => line.Contains("database already seeded, no rows added", StringComparison.Ordinal));
    }



    /// <summary>
    /// Every context from one builder shares one internal service provider, so EF's singletons
    /// are built once instead of once per BuildAsync call.
    /// </summary>
    [Fact]
    public async Task BuildAsync_when_called_twice_reuses_the_internal_service_provider()
    {
        using var builder = new DbContextBuilder<BasicContext>().UseSqlite();

        await using var first = await builder.BuildAsync();
        var provider = builder.InternalServiceProvider;
        await using var second = await builder.BuildAsync();

        Assert.NotNull(provider);
        Assert.Same(provider, builder.InternalServiceProvider);
    }



    /// <summary>
    /// Disposing the builder disposes the internal service provider it built.
    /// </summary>
    [Fact]
    public async Task Dispose_disposes_the_internal_service_provider()
    {
        var builder = new DbContextBuilder<BasicContext>().UseSqlite();
        await using (await builder.BuildAsync())
        {
        }
        var provider = builder.InternalServiceProvider!;

        builder.Dispose();

        Assert.Throws<ObjectDisposedException>(() => provider.GetService(typeof(IModelCustomizer)));
    }



    /// <summary>
    /// Selecting another provider after a build disposes the service provider built for the old
    /// one, and the next build creates and seeds the new provider's empty database.
    /// </summary>
    [Fact]
    public async Task UseInMemory_after_a_Sqlite_build_disposes_the_old_service_provider_and_seeds_the_new_database()
    {
        using var builder = new DbContextBuilder<BasicContext>().UseSqlite().SeedWith(NewLog("a"));
        await using (await builder.BuildAsync())
        {
        }
        var sqliteProvider = builder.InternalServiceProvider!;

        builder.UseInMemory();
        await using var context = await builder.BuildAsync();

        Assert.Throws<ObjectDisposedException>(() => sqliteProvider.GetService(typeof(IModelCustomizer)));
        Assert.Null(builder.InternalServiceProvider);
        Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", context.Database.ProviderName);
        Assert.Single(await context.DatabaseLogs.ToListAsync());
    }



    /// <summary>
    /// Seed data added after the database was seeded would never reach it, so every seeding
    /// method rejects it.
    /// </summary>
    [Theory]
    [MemberData(nameof(SeedCalls))]
    public async Task Seeding_after_BuildAsync_throws_InvalidOperationException(string call)
    {
        using var builder = new DbContextBuilder<BasicContext>().UseInMemory();
        await using var context = await builder.BuildAsync();

        var ex = Assert.Throws<InvalidOperationException>(() => SeedCallsByName[call](builder));

        Assert.StartsWith("Seed data must be added before the first BuildAsync() call", ex.Message, StringComparison.Ordinal);
    }



    /// <summary>
    /// After a provider switch the new, empty database has not been seeded, so seeding is
    /// accepted again and the next build saves every seed row.
    /// </summary>
    [Fact]
    public async Task Seeding_after_a_provider_switch_is_accepted_and_seeds_the_new_database()
    {
        // Explicit keys: the first build already assigned "a" its generated key, and SQLite would
        // generate the same value for "b" when the instances are inserted into the new database.
        using var builder = new DbContextBuilder<BasicContext>().UseInMemory().SeedWith(NewLog("a") with { DatabaseLogId = 1 });
        await using (await builder.BuildAsync())
        {
        }

        builder.UseSqlite().SeedWith(NewLog("b") with { DatabaseLogId = 2 });
        await using var context = await builder.BuildAsync();

        Assert.Equal
        (
            new[] { "a", "b" },
            (await context.DatabaseLogs.ToListAsync()).Select(log => log.Event).OrderBy(e => e, StringComparer.Ordinal)
        );
    }



    // BasicContext is internal, so the theory takes the call's name and looks the action up here.
    private static readonly Dictionary<string, Action<DbContextBuilder<BasicContext>>> SeedCallsByName = new()
    {
        ["SeedWith(IEnumerable)"] = b => b.SeedWith(new List<DatabaseLog> { NewLog("a") }.AsEnumerable()),
        ["SeedWith(params)"] = b => b.SeedWith(NewLog("a"), NewLog("b")),
        ["SeedWith(entity)"] = b => b.SeedWith(NewLog("a")),
        ["SeedWithRandom(count)"] = b => b.SeedWithRandom<DatabaseLog>(1),
        ["SeedWithRandom(count, func)"] = b => b.SeedWithRandom<DatabaseLog>(1, e => e),
        ["SeedWithRandom(count, func with index)"] = b => b.SeedWithRandom<DatabaseLog>(1, (e, _) => e),
    };



    /// <summary>
    /// The names of the seeding methods the after-BuildAsync theory exercises.
    /// </summary>
    public static TheoryData<string> SeedCalls()
    {
        var data = new TheoryData<string>();
        foreach (var name in SeedCallsByName.Keys)
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
