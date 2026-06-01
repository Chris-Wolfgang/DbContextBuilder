using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore;

/// <summary>
/// Extension methods that wire AutoFixture-backed random entity creation
/// into <see cref="DbContextBuilder{T}"/>.
/// </summary>
public static class DbContextBuilderAutoFixtureExtensions
{
    /// <summary>
    /// Tell DbContextBuilder to use AutoFixture to create random entities.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    public static DbContextBuilder<T> UseAutoFixture<T>(this DbContextBuilder<T> builder) where T : DbContext
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.UseCustomRandomEntityCreator(new AutoFixtureRandomEntityCreator());

        return builder;
    }

}
