using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore;

/// <summary>
/// Caches a compiled constructor delegate per <typeparamref name="TDbContext"/> so that
/// each <c>BuildAsync</c> call does not pay the reflection cost of
/// <see cref="Activator.CreateInstance(Type, object[])"/>. Hot in test suites that build
/// many contexts.
/// </summary>
/// <typeparam name="TDbContext">The concrete <see cref="DbContext"/> type to construct.</typeparam>
/// <remarks>
/// The first call for a given <typeparamref name="TDbContext"/> resolves the ctor and
/// compiles a delegate that calls it; subsequent calls invoke the delegate directly.
/// The cache field is static-per-closed-generic, so there is no concurrency primitive
/// needed — the runtime initializes it at most once per type.
/// </remarks>
internal static class DbContextActivator<TDbContext>
    where TDbContext : DbContext
{
    private static readonly Func<DbContextOptions<TDbContext>, TDbContext> _factory = BuildFactory();



    /// <summary>
    /// Instantiates a <typeparamref name="TDbContext"/> using the cached compiled ctor.
    /// </summary>
    /// <param name="options">The configured <see cref="DbContextOptions{TContext}"/>.</param>
    /// <returns>A new <typeparamref name="TDbContext"/> instance.</returns>
    public static TDbContext Create(DbContextOptions<TDbContext> options) => _factory(options);



    private static Func<DbContextOptions<TDbContext>, TDbContext> BuildFactory()
    {
        var type = typeof(TDbContext);
        var ctor = type.GetConstructor
        (
            BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: new[] { typeof(DbContextOptions<TDbContext>) },
            modifiers: null
        );

        if (ctor is null)
        {
            // Fall back to Activator.CreateInstance so the call still works (and
            // throws a familiar MissingMethodException) for types that take
            // DbContextOptions (non-generic) or other shapes. Same allocation cost
            // as before, but only on the first call for this TDbContext.
            return options => (TDbContext)Activator.CreateInstance(type, options)!;
        }

        var optionsParam = Expression.Parameter(typeof(DbContextOptions<TDbContext>), "options");
        var newExpr = Expression.New(ctor, optionsParam);
        return Expression.Lambda<Func<DbContextOptions<TDbContext>, TDbContext>>(newExpr, optionsParam).Compile();
    }
}
