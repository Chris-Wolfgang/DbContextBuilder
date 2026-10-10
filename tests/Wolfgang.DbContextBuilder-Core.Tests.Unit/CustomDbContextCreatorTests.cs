using AdventureWorks.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Wolfgang.DbContextBuilderCore.Tests.Unit.Models;
using Xunit;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Tests for <see cref="DbContextBuilder{T}.UseCustomDbContextCreator"/> (#564).
/// </summary>
public class CustomDbContextCreatorTests
{
    /// <summary>
    /// Verifies that the builder creates and seeds its contexts through the custom creator.
    /// </summary>
    [Fact]
    public async Task UseCustomDbContextCreator_builds_and_seeds_through_the_creator()
    {
        var creator = new RecordingDbContextCreator();
        using var sut = new DbContextBuilder<BasicContext>().UseCustomDbContextCreator(creator);

        await using var context = await sut.SeedWith(NewLog("custom")).BuildAsync();

        // One temporary seed context, one returned context.
        Assert.Equal(2, creator.Created);
        Assert.Equal("custom", Assert.Single(context.DatabaseLogs).Event);
    }



    /// <summary>
    /// Verifies that a null creator is rejected.
    /// </summary>
    [Fact]
    public void UseCustomDbContextCreator_when_creator_is_null_throws_ArgumentNullException()
    {
        using var sut = new DbContextBuilder<BasicContext>();

        var ex = Assert.Throws<ArgumentNullException>(() => sut.UseCustomDbContextCreator(null!));

        Assert.Equal("creator", ex.ParamName);
    }



    /// <summary>
    /// Verifies that a custom creator replacing a SQLite flavor drops the SQLite services, so the
    /// build uses the creator's provider (as UseInMemory does, #558).
    /// </summary>
    [Fact]
    public async Task UseCustomDbContextCreator_after_UseSqlite_drops_the_SQLite_services()
    {
        using var sut = new DbContextBuilder<BasicContext>().UseSqlite();

        sut.UseCustomDbContextCreator(new RecordingDbContextCreator());
        await using var context = await sut.BuildAsync();

        Assert.Empty(sut.ServiceCollection);
        Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", context.Database.ProviderName);
    }



    /// <summary>
    /// Verifies that the builder owns the creator: it is disposed when replaced and when the
    /// builder is disposed.
    /// </summary>
    [Fact]
    public void UseCustomDbContextCreator_takes_ownership_of_a_disposable_creator()
    {
        var replaced = new RecordingDbContextCreator();
        var current = new RecordingDbContextCreator();
        var sut = new DbContextBuilder<BasicContext>().UseCustomDbContextCreator(replaced);

        sut.UseCustomDbContextCreator(current);
        Assert.True(replaced.Disposed);
        Assert.False(current.Disposed);

        sut.Dispose();
        Assert.True(current.Disposed);
    }



    /// <summary>
    /// Verifies that a disposed builder rejects a custom creator.
    /// </summary>
    [Fact]
    public void UseCustomDbContextCreator_after_Dispose_throws_ObjectDisposedException()
    {
        var sut = new DbContextBuilder<BasicContext>();
        sut.Dispose();

        Assert.Throws<ObjectDisposedException>(() => sut.UseCustomDbContextCreator(new RecordingDbContextCreator()));
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



/// <summary>A consumer-style creator: InMemory underneath, counting creations and disposal.</summary>
internal sealed class RecordingDbContextCreator : ICreateDbContext, IDisposable
{
    private readonly string _databaseName = Guid.NewGuid().ToString();



    public int Created { get; private set; }



    public bool Disposed { get; private set; }



    public Task<TDbContext> CreateDbContextAsync<TDbContext>(DbContextOptionsBuilder<TDbContext> optionsBuilder)
        where TDbContext : DbContext
    {
        Created++;
        optionsBuilder.UseInMemoryDatabase(_databaseName);
        return Task.FromResult((TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options)!);
    }



    public void Dispose() => Disposed = true;
}
