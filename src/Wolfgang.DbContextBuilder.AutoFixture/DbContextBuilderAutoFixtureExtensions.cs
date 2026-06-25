using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore;

/// <summary>
/// Extension methods that wire AutoFixture-backed random entity creation
/// into <see cref="DbContextBuilder{T}"/>.
/// </summary>
public static class DbContextBuilderAutoFixtureExtensions
{
    /// <summary>
    /// Configures the builder to generate <c>SeedWithRandom</c> entities with AutoFixture.
    /// Equivalent to <c>UseCustomRandomEntityCreator(new AutoFixtureRandomEntityCreator())</c>.
    /// </summary>
    /// <param name="builder">The builder to configure.</param>
    /// <typeparam name="T">The <see cref="DbContext"/> type being built.</typeparam>
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is null.</exception>
    public static DbContextBuilder<T> UseAutoFixture<T>(this DbContextBuilder<T> builder) where T : DbContext
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.UseCustomRandomEntityCreator(new AutoFixtureRandomEntityCreator());

        return builder;
    }
}
