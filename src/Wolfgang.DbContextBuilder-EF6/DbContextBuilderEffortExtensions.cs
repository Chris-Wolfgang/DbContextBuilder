using System;
using System.Data.Entity;

namespace Wolfgang.DbContextBuilderEF6;

/// <summary>
/// Extension methods for configuring <see cref="DbContextBuilder{T}"/> to use Effort in-memory database.
/// </summary>
public static class DbContextBuilderEffortExtensions
{
    /// <summary>
    /// Instructs the builder to use Effort as the in-memory database provider.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <c>null</c>.</exception>
    /// <exception cref="ObjectDisposedException"><paramref name="builder"/> has been disposed.</exception>
    /// <remarks>
    /// Calling it again replaces the Effort database: the previous creator, and its in-memory
    /// connection, is disposed.
    /// </remarks>
    public static DbContextBuilder<T> UseEffort<T>(this DbContextBuilder<T> builder) where T : DbContext
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        // Before the creator opens a connection that a disposed builder could never release.
        builder.ThrowIfDisposed();
        builder.SetCreateDbContext(new EffortDbContextCreator());

        return builder;
    }
}
