using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wolfgang.DbContextBuilderEF6.Tests.Unit.Models;
using Xunit;

namespace Wolfgang.DbContextBuilderEF6.Tests.Unit;

/// <summary>
/// Tests that the classic EF6 builder owns and disposes its context creator (#562).
/// </summary>
public class DbContextBuilderDisposalTests
{
    /// <summary>
    /// Verifies that disposing the builder disposes its context creator.
    /// </summary>
    [Fact]
    public void Dispose_disposes_the_context_creator()
    {
        var creator = new TrackingDbContextCreator();
        var sut = new DbContextBuilder<TestDbContext> { CreateDbContext = creator };

        sut.Dispose();

        Assert.True(creator.Disposed);
    }



    /// <summary>
    /// Verifies that the creator a build used stays usable until the builder is disposed, and is
    /// disposed with it.
    /// </summary>
    [Fact]
    public void Dispose_after_Build_disposes_the_creator_the_build_used()
    {
        var creator = new TrackingDbContextCreator();
        var sut = new DbContextBuilder<TestDbContext> { CreateDbContext = creator }.SeedWith(new Category { Name = "seeded" });

        using (var context = sut.Build())
        {
            Assert.Single(context.Categories);
            Assert.False(creator.Disposed);
        }

        sut.Dispose();

        Assert.True(creator.Disposed);
    }



    /// <summary>
    /// Verifies that a second Dispose does not dispose the creator again.
    /// </summary>
    [Fact]
    public void Dispose_when_called_twice_disposes_the_creator_once()
    {
        var creator = new TrackingDbContextCreator();
        var sut = new DbContextBuilder<TestDbContext> { CreateDbContext = creator };

        sut.Dispose();
        sut.Dispose();

        Assert.Equal(1, creator.DisposeCount);
    }



    /// <summary>
    /// Verifies that disposing a builder that never selected a creator does not throw.
    /// </summary>
    [Fact]
    public void Dispose_when_no_creator_was_selected_does_not_throw()
    {
        var sut = new DbContextBuilder<TestDbContext>();

        var ex = Record.Exception(() => sut.Dispose());

        Assert.Null(ex);
    }



    /// <summary>
    /// Verifies that re-selecting Effort disposes the creator it replaces.
    /// </summary>
    [Fact]
    public void UseEffort_when_a_creator_is_already_set_disposes_it()
    {
        var previous = new TrackingDbContextCreator();
        using var sut = new DbContextBuilder<TestDbContext> { CreateDbContext = previous };

        sut.UseEffort();

        Assert.True(previous.Disposed);
        Assert.IsType<EffortDbContextCreator>(sut.CreateDbContext);
    }



    /// <summary>
    /// Verifies that passing the active creator again keeps it undisposed.
    /// </summary>
    [Fact]
    public void SetCreateDbContext_with_the_active_creator_keeps_it()
    {
        var creator = new TrackingDbContextCreator();
        using var sut = new DbContextBuilder<TestDbContext> { CreateDbContext = creator };

        sut.SetCreateDbContext(creator);

        Assert.False(creator.Disposed);
        Assert.Same(creator, sut.CreateDbContext);
    }



    /// <summary>
    /// Verifies that every configuration and build entry point rejects a disposed builder.
    /// </summary>
    [Theory]
    [MemberData(nameof(Calls))]
    public async Task Calls_after_Dispose_throw_ObjectDisposedException(string call)
    {
        var sut = new DbContextBuilder<TestDbContext>();
        sut.Dispose();

        var ex = await Record.ExceptionAsync(() => CallsByName[call](sut));

        var disposed = Assert.IsType<ObjectDisposedException>(ex);
        Assert.Equal(nameof(DbContextBuilder<TestDbContext>), disposed.ObjectName);
    }



    // TestDbContext is public, but the entry points are spread over methods and extensions, so the
    // theory takes a name and looks the call up here.
    private static readonly Dictionary<string, Func<DbContextBuilder<TestDbContext>, Task>> CallsByName = new()
    {
        ["UseEffort"] = b => Task.FromResult(b.UseEffort()),
        ["UseAutoFixture"] = b => Task.FromResult(b.UseAutoFixture()),
        ["UseCustomRandomEntityCreator"] = b => Task.FromResult(b.UseCustomRandomEntityCreator(new AutoFixtureRandomEntityCreator())),
        ["SeedWith(IEnumerable)"] = b => Task.FromResult(b.SeedWith((IEnumerable<Category>)new[] { new Category { Name = "a" } })),
        ["SeedWith(params)"] = b => Task.FromResult(b.SeedWith(new Category { Name = "a" }, new Category { Name = "b" })),
        ["SeedWith(entity)"] = b => Task.FromResult(b.SeedWith(new Category { Name = "a" })),
        ["SeedWithRandom(count)"] = b => Task.FromResult(b.SeedWithRandom<Category>(1)),
        // The disposed check runs before the func's null check, so null stands in for a transform.
        ["SeedWithRandom(count, func)"] = b => Task.FromResult(b.SeedWithRandom<Category>(1, (Func<Category, Category>)null!)),
        ["SeedWithRandom(count, func with index)"] = b => Task.FromResult(b.SeedWithRandom<Category>(1, (Func<Category, int, Category>)null!)),
        ["Build"] = b => Task.FromResult(b.Build()),
        ["BuildAsync"] = b => b.BuildAsync(),
    };



    /// <summary>
    /// The names of the entry points the disposed-builder theory exercises.
    /// </summary>
    public static TheoryData<string> Calls()
    {
        var data = new TheoryData<string>();
        foreach (var name in CallsByName.Keys)
        {
            data.Add(name);
        }

        return data;
    }
}



/// <summary>Wraps <see cref="EffortDbContextCreator"/> and records its own disposal.</summary>
internal sealed class TrackingDbContextCreator : ICreateDbContext
{
    private readonly EffortDbContextCreator _inner = new EffortDbContextCreator();



    public int DisposeCount { get; private set; }



    public bool Disposed => DisposeCount > 0;



    public TDbContext CreateDbContext<TDbContext>() where TDbContext : System.Data.Entity.DbContext =>
        _inner.CreateDbContext<TDbContext>();



    public void Dispose()
    {
        DisposeCount++;
        _inner.Dispose();
    }
}
