using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Tests for <see cref="DbContextActivator{TDbContext}"/> — specifically the two paths
/// inside <c>BuildFactory</c>: the compiled-delegate happy path (covered indirectly by
/// every test that uses <c>UseInMemory()</c> or <c>UseSqlite()</c>), and the fallback to
/// <c>Activator.CreateInstance</c> when the target type has no ctor that accepts a
/// <see cref="DbContextOptions{TContext}"/>. Note that <c>GetConstructor</c>'s default binder
/// accepts assignable parameter types, so a ctor taking the non-generic
/// <see cref="DbContextOptions"/> is found and takes the compiled path too.
/// </summary>
public class DbContextActivatorTests
{

    /// <summary>
    /// The happy path: a DbContext with a <c>DbContextOptions&lt;TContext&gt;</c> ctor
    /// goes through the compiled Expression delegate and is instantiated.
    /// </summary>
    [Fact]
    public void Create_when_TDbContext_has_a_generic_options_ctor_returns_a_new_instance()
    {
        var options = new DbContextOptionsBuilder<ContextWithGenericOptionsCtor>()
            .UseInMemoryDatabase($"happy-{Guid.NewGuid()}")
            .Options;

        using var context = DbContextActivator<ContextWithGenericOptionsCtor>.Create(options);

        Assert.NotNull(context);
        Assert.IsType<ContextWithGenericOptionsCtor>(context);
    }



    /// <summary>
    /// A DbContext whose only ctor takes the non-generic <see cref="DbContextOptions"/> is still
    /// constructed through the compiled delegate: <c>GetConstructor</c>'s default binder accepts
    /// the assignable parameter type, so the fallback is NOT taken here. (This test was once
    /// named for the fallback, which it never reached; coverage showed the fallback line at 0 hits.)
    /// </summary>
    [Fact]
    public void Create_when_TDbContext_only_has_a_non_generic_options_ctor_uses_the_compiled_ctor()
    {
        var options = new DbContextOptionsBuilder<ContextWithNonGenericOptionsCtor>()
            .UseInMemoryDatabase($"fallback-{Guid.NewGuid()}")
            .Options;

        using var context = DbContextActivator<ContextWithNonGenericOptionsCtor>.Create(options);

        Assert.NotNull(context);
        Assert.IsType<ContextWithNonGenericOptionsCtor>(context);
    }



    /// <summary>
    /// Fallback path: with no ctor that accepts the options, <c>BuildFactory</c> falls back to
    /// <see cref="Activator.CreateInstance(Type, object[])"/>, which throws the familiar
    /// <see cref="MissingMethodException"/> promised by the source comment, not a cryptic
    /// expression-tree error.
    /// </summary>
    [Fact]
    public void Create_when_TDbContext_has_no_options_ctor_falls_back_to_Activator_and_throws_MissingMethodException()
    {
        var options = new DbContextOptionsBuilder<ContextWithoutOptionsCtor>()
            .UseInMemoryDatabase($"fallback-{Guid.NewGuid()}")
            .Options;

        Assert.Throws<MissingMethodException>(() => DbContextActivator<ContextWithoutOptionsCtor>.Create(options));
    }



    [ExcludeFromCodeCoverage(Justification = "Test-only DbContext used solely to exercise the activator's compiled-delegate path.")]
    private sealed class ContextWithGenericOptionsCtor : DbContext
    {
        public ContextWithGenericOptionsCtor(DbContextOptions<ContextWithGenericOptionsCtor> options)
            : base(options)
        {
        }
    }



    [ExcludeFromCodeCoverage(Justification = "Test-only DbContext used solely to show a non-generic options ctor takes the compiled path.")]
    private sealed class ContextWithNonGenericOptionsCtor : DbContext
    {
        public ContextWithNonGenericOptionsCtor(DbContextOptions options)
            : base(options)
        {
        }
    }



    [ExcludeFromCodeCoverage(Justification = "Test-only DbContext with no options ctor, used solely to drive the activator's Activator.CreateInstance fallback.")]
    private sealed class ContextWithoutOptionsCtor : DbContext
    {
    }
}
