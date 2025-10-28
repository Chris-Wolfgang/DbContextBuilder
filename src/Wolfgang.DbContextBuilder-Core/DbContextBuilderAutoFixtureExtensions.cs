using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore;
public static class DbContextBuilderAutoFixtureExtensions 
{
    /// <summary>
    /// Tell DbContextBuilder to use AutoFixture to create random entities.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    public static DbContextBuilder<T> UseAutoFixture<T>(this DbContextBuilder<T> builder) where T : DbContext
    {
        builder.UseCustomRandomEntityCreator(new AutoFixtureRandomEntityCreator());

        return builder;
    }

}
