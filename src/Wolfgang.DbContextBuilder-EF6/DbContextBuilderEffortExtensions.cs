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
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    public static DbContextBuilder<T> UseEffort<T>(this DbContextBuilder<T> builder) where T : DbContext
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.CreateDbContext = new EffortDbContextCreator();

        return builder;
    }
}
