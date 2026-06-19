using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Tests for <see cref="DbContextActivator{TDbContext}"/> — specifically the two paths
/// inside <c>BuildFactory</c>: the compiled-delegate happy path (covered indirectly by
/// every test that uses <c>UseInMemory()</c> or <c>UseSqlite()</c>), and the fallback to
/// <c>Activator.CreateInstance</c> when the target type does not expose a
/// <see cref="DbContextOptions{TContext}"/> ctor.
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
    /// Fallback path: a DbContext whose only ctor takes the non-generic
    /// <see cref="DbContextOptions"/> (not <see cref="DbContextOptions{TContext}"/>)
    /// makes <c>GetConstructor</c> return <c>null</c>, so <c>BuildFactory</c> falls back
    /// to <see cref="Activator.CreateInstance(Type, object[])"/>. That fallback must
    /// still successfully construct the type.
    /// </summary>
    [Fact]
    public void Create_when_TDbContext_only_has_a_non_generic_options_ctor_falls_back_to_Activator()
    {
        var options = new DbContextOptionsBuilder<ContextWithNonGenericOptionsCtor>()
            .UseInMemoryDatabase($"fallback-{Guid.NewGuid()}")
            .Options;

        using var context = DbContextActivator<ContextWithNonGenericOptionsCtor>.Create(options);

        Assert.NotNull(context);
        Assert.IsType<ContextWithNonGenericOptionsCtor>(context);
    }



    [ExcludeFromCodeCoverage(Justification = "Test-only DbContext used solely to exercise the activator's compiled-delegate path.")]
    private sealed class ContextWithGenericOptionsCtor : DbContext
    {
        public ContextWithGenericOptionsCtor(DbContextOptions<ContextWithGenericOptionsCtor> options)
            : base(options)
        {
        }
    }



    [ExcludeFromCodeCoverage(Justification = "Test-only DbContext used solely to exercise the activator's Activator.CreateInstance fallback path.")]
    private sealed class ContextWithNonGenericOptionsCtor : DbContext
    {
        public ContextWithNonGenericOptionsCtor(DbContextOptions options)
            : base(options)
        {
        }
    }
}
