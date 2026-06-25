using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore;

/// <summary>
/// Extension methods that wire Bogus-backed random entity creation
/// into <see cref="DbContextBuilder{T}"/>.
/// </summary>
public static class DbContextBuilderBogusExtensions
{
    /// <summary>
    /// Configures the builder to generate <c>SeedWithRandom</c> entities with Bogus.
    /// Equivalent to <c>UseCustomRandomEntityCreator(new BogusRandomEntityCreator())</c>.
    /// </summary>
    /// <param name="builder">The builder to configure.</param>
    /// <typeparam name="T">The <see cref="DbContext"/> type being built.</typeparam>
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is null.</exception>
    public static DbContextBuilder<T> UseBogus<T>(this DbContextBuilder<T> builder) where T : DbContext
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.UseCustomRandomEntityCreator(new BogusRandomEntityCreator());

        return builder;
    }
}
