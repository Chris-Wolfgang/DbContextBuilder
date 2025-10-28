using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Wolfgang.DbContextBuilderCore;
public static class DbContextBuilderSqliteExtensions
{

    /// <summary>
    /// Instructs the builder to use SQLite as the database provider.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    public static DbContextBuilder<TDbContext> UseSqlite<TDbContext>
    (
        this DbContextBuilder<TDbContext> builder
    )
    where TDbContext : DbContext => UseSqlite(builder, typeof(SqliteModelCustomizer));



    /// <summary>
    /// Configures the builder to use SQLite as the database provider with SQL Server-specific adjustments,
    /// such as default value mappings, to better mimic SQL Server behavior for testing or compatibility.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    public static DbContextBuilder<TDbContext> UseSqliteForMsSqlServer<TDbContext>
    (
        this DbContextBuilder<TDbContext> builder
    )
    where TDbContext : DbContext => UseSqlite(builder, typeof(SqliteForMsSqlServerModelCustomizer));



    /// <summary>
    /// Instructs the builder to use SQLite as the database provider.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    private static DbContextBuilder<TDbContext> UseSqlite<TDbContext>
    (
        this DbContextBuilder<TDbContext> builder,
        //IModelCustomizer modelCustomizer
        Type modelCustomizerType
    ) where TDbContext : DbContext
    {

        // TODO Check is items exist in the list and don't add duplicates
        // TODO Check if Sql Server provider is already registered and if so, remove it

        // Remove any existing IModelCustomizer registrations to avoid duplicates/competing implementations
        var modelCustomizerDescriptors = builder.ServiceCollection
            .Where(sd => sd.ServiceType == typeof(IModelCustomizer))
            .ToList();
        foreach (var descriptor in modelCustomizerDescriptors)
        {
            builder.ServiceCollection.Remove(descriptor);
        }

        // Avoid registering EF services multiple times
        if (!builder.ServiceCollection.Any(sd =>
                sd.ServiceType.FullName != null &&
                sd.ServiceType.FullName.Contains("Microsoft.EntityFrameworkCore.Sqlite.SqliteOptionsExtension")))
        {
            builder.ServiceCollection.AddEntityFrameworkSqlite();
        }

        builder.ServiceCollection.AddSingleton(typeof(IModelCustomizer), modelCustomizerType);
        builder.CreateDbContext = new SqliteDbContextCreator();

        return builder;
    }
}
