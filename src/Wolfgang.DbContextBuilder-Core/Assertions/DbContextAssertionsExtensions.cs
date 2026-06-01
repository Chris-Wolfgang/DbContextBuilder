using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore.Assertions;

/// <summary>
/// Entry-point extensions that produce a <see cref="DbSetAssertions{TEntity}"/> from a
/// <see cref="DbSet{TEntity}"/> or <see cref="IQueryable{TEntity}"/>. Intended to be
/// imported via <c>using Wolfgang.DbContextBuilderCore.Assertions;</c> in test files so
/// that <c>context.Set&lt;Product&gt;().Should()</c> resolves.
/// </summary>
public static class DbContextAssertionsExtensions
{
    /// <summary>
    /// Begins a fluent assertion chain against the specified <see cref="DbSet{TEntity}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity type tracked by the DbSet.</typeparam>
    /// <param name="set">The DbSet to assert against.</param>
    /// <returns>A <see cref="DbSetAssertions{TEntity}"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="set"/> is null.</exception>
    public static DbSetAssertions<TEntity> Should<TEntity>(this DbSet<TEntity> set)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(set);
        return new DbSetAssertions<TEntity>(set);
    }



    /// <summary>
    /// Begins a fluent assertion chain against an arbitrary <see cref="IQueryable{TEntity}"/>.
    /// Useful for asserting against a filtered/projected query rather than a whole DbSet.
    /// </summary>
    /// <typeparam name="TEntity">The element type of the queryable.</typeparam>
    /// <param name="query">The queryable to assert against.</param>
    /// <returns>A <see cref="DbSetAssertions{TEntity}"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> is null.</exception>
    public static DbSetAssertions<TEntity> Should<TEntity>(this IQueryable<TEntity> query)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);
        return new DbSetAssertions<TEntity>(query);
    }
}
