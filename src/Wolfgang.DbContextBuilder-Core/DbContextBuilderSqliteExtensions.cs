using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Wolfgang.DbContextBuilderCore;

/// <summary>
/// Extension methods that configure <see cref="DbContextBuilder{T}"/> to
/// use a SQLite in-memory database (plain SQLite, or SQLite with the
/// SQL-Server-compatibility customizations).
/// </summary>
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
    where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(builder);
        return UseSqlite(builder, typeof(SqliteModelCustomizer));
    }



    /// <summary>
    /// Configures the builder to use SQLite as the database provider with SQL Server-specific adjustments,
    /// such as default value mappings, to better mimic SQL Server behavior for testing or compatibility.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    public static DbContextBuilder<TDbContext> UseSqliteForMsSqlServer<TDbContext>
    (
        this DbContextBuilder<TDbContext> builder
    )
    where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(builder);
        return UseSqlite(builder, typeof(SqliteForMsSqlServerModelCustomizer));
    }



    /// <summary>
    /// Instructs the builder to use SQLite as the database provider.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    private static DbContextBuilder<TDbContext> UseSqlite<TDbContext>
    (
        this DbContextBuilder<TDbContext> builder,
        Type modelCustomizerType
    ) where TDbContext : DbContext
    {

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
