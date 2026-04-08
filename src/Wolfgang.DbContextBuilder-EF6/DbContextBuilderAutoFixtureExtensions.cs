using System;
using System.Data.Entity;

namespace Wolfgang.DbContextBuilderEF6;

/// <summary>
/// Extension methods for configuring <see cref="DbContextBuilder{T}"/> to use AutoFixture.
/// </summary>
public static class DbContextBuilderAutoFixtureExtensions
{
    /// <summary>
    /// Tell DbContextBuilder to use AutoFixture to create random entities.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <c>null</c>.</exception>
    public static DbContextBuilder<T> UseAutoFixture<T>(this DbContextBuilder<T> builder) where T : DbContext
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.UseCustomRandomEntityCreator(new AutoFixtureRandomEntityCreator());

        return builder;
    }

}
