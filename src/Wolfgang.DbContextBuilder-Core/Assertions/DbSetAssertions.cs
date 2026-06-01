using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore.Assertions;

/// <summary>
/// Fluent, chainable assertions over a single <see cref="DbSet{TEntity}"/> for use in
/// DbContextBuilder-backed tests. Each method runs the corresponding query asynchronously
/// against the underlying <see cref="IQueryable{TEntity}"/>, throws
/// <see cref="DbContextAssertionException"/> with an EF-aware message on failure, and
/// returns <c>this</c> so the next assertion can be chained.
/// </summary>
/// <typeparam name="TEntity">The entity type tracked by the wrapped DbSet.</typeparam>
/// <remarks>
/// Obtain an instance via <c>DbContextAssertionsExtensions.Should</c> on a
/// <see cref="DbSet{TEntity}"/> or <see cref="IQueryable{TEntity}"/>. The assertion
/// methods are async because they translate to EF queries; <c>await</c> the final chain.
/// </remarks>
public sealed class DbSetAssertions<TEntity>
    where TEntity : class
{
    private readonly IQueryable<TEntity> _query;

    internal DbSetAssertions(IQueryable<TEntity> query)
    {
        _query = query;
    }



    /// <summary>
    /// Asserts that the wrapped <see cref="DbSet{TEntity}"/> contains exactly <paramref name="expected"/>
    /// entities.
    /// </summary>
    /// <param name="expected">The exact entity count expected.</param>
    /// <returns>This assertions instance for chaining.</returns>
    /// <exception cref="DbContextAssertionException">
    /// The actual count differs from <paramref name="expected"/>.
    /// </exception>
    public async Task<DbSetAssertions<TEntity>> HaveCount(int expected)
    {
        var actual = await _query.CountAsync().ConfigureAwait(false);
        if (actual != expected)
        {
            throw new DbContextAssertionException(
                $"Expected DbSet<{typeof(TEntity).Name}> to have {expected} entities, but found {actual}.");
        }

        return this;
    }



    /// <summary>
    /// Asserts that the wrapped <see cref="DbSet{TEntity}"/> contains no entities.
    /// </summary>
    /// <returns>This assertions instance for chaining.</returns>
    /// <exception cref="DbContextAssertionException">
    /// The DbSet contains at least one entity.
    /// </exception>
    public async Task<DbSetAssertions<TEntity>> BeEmpty()
    {
        var actual = await _query.CountAsync().ConfigureAwait(false);
        if (actual != 0)
        {
            throw new DbContextAssertionException(
                $"Expected DbSet<{typeof(TEntity).Name}> to be empty, but found {actual} entities.");
        }

        return this;
    }



    /// <summary>
    /// Asserts that the wrapped <see cref="DbSet{TEntity}"/> contains at least one entity.
    /// </summary>
    /// <returns>This assertions instance for chaining.</returns>
    /// <exception cref="DbContextAssertionException">
    /// The DbSet is empty.
    /// </exception>
    public async Task<DbSetAssertions<TEntity>> NotBeEmpty()
    {
        var actual = await _query.AnyAsync().ConfigureAwait(false);
        if (!actual)
        {
            throw new DbContextAssertionException(
                $"Expected DbSet<{typeof(TEntity).Name}> to contain at least one entity, but it was empty.");
        }

        return this;
    }



    /// <summary>
    /// Asserts that at least one entity in the wrapped <see cref="DbSet{TEntity}"/> satisfies
    /// <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">Expression evaluated server-side via EF.</param>
    /// <returns>This assertions instance for chaining.</returns>
    /// <exception cref="DbContextAssertionException">
    /// No entity matches <paramref name="predicate"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
    public async Task<DbSetAssertions<TEntity>> Contain(Expression<Func<TEntity, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var matched = await _query.AnyAsync(predicate).ConfigureAwait(false);
        if (!matched)
        {
            var total = await _query.CountAsync().ConfigureAwait(false);
            throw new DbContextAssertionException(
                $"Expected DbSet<{typeof(TEntity).Name}> to contain an entity matching ({predicate}), " +
                $"but no matching entity was found among {total} entities.");
        }

        return this;
    }



    /// <summary>
    /// Asserts that no entity in the wrapped <see cref="DbSet{TEntity}"/> satisfies
    /// <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">Expression evaluated server-side via EF.</param>
    /// <returns>This assertions instance for chaining.</returns>
    /// <exception cref="DbContextAssertionException">
    /// At least one entity matches <paramref name="predicate"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
    public async Task<DbSetAssertions<TEntity>> NotContain(Expression<Func<TEntity, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var matched = await _query.CountAsync(predicate).ConfigureAwait(false);
        if (matched != 0)
        {
            throw new DbContextAssertionException(
                $"Expected DbSet<{typeof(TEntity).Name}> to contain NO entity matching ({predicate}), " +
                $"but {matched} matching entities were found.");
        }

        return this;
    }



    /// <summary>
    /// Asserts that every entity in the wrapped <see cref="DbSet{TEntity}"/> satisfies
    /// <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">Expression evaluated server-side via EF.</param>
    /// <returns>This assertions instance for chaining.</returns>
    /// <exception cref="DbContextAssertionException">
    /// At least one entity fails <paramref name="predicate"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
    public async Task<DbSetAssertions<TEntity>> AllSatisfy(Expression<Func<TEntity, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var total = await _query.CountAsync().ConfigureAwait(false);
        if (total == 0)
        {
            // Vacuously true: an empty set "all satisfies" any predicate.
            return this;
        }

        var matching = await _query.CountAsync(predicate).ConfigureAwait(false);
        if (matching != total)
        {
            var failing = total - matching;
            throw new DbContextAssertionException(
                $"Expected all {total} entities in DbSet<{typeof(TEntity).Name}> to satisfy ({predicate}), " +
                $"but {failing} of {total} failed.");
        }

        return this;
    }
}
