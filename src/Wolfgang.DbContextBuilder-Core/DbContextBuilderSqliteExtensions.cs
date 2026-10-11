using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage;
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
    /// <typeparam name="TDbContext">The <see cref="DbContext"/> type the builder creates.</typeparam>
    /// <param name="builder">The builder to configure.</param>
    /// <returns>The builder, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is null.</exception>
    /// <exception cref="ObjectDisposedException"><paramref name="builder"/> has been disposed.</exception>
    /// <remarks>
    /// Provider selection is last-write-wins — calling <c>UseSqlite</c> after a previous
    /// <c>UseInMemory</c>, <c>UseSqlite</c>, or <c>UseSqliteForMsSqlServer</c> call
    /// overrides the earlier choice.
    /// Choose one provider per builder.
    /// </remarks>
    /// <exception cref="ObjectDisposedException"><paramref name="builder"/> has been disposed.</exception>
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
    /// <typeparam name="TDbContext">The <see cref="DbContext"/> type the builder creates.</typeparam>
    /// <param name="builder">The builder to configure.</param>
    /// <returns>The builder, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is null.</exception>
    /// <exception cref="ObjectDisposedException"><paramref name="builder"/> has been disposed.</exception>
    /// <remarks>
    /// Provider selection is last-write-wins — calling <c>UseSqliteForMsSqlServer</c> after a
    /// previous <c>UseInMemory</c>, <c>UseSqlite</c>, or <c>UseSqliteForMsSqlServer</c> call
    /// overrides the earlier choice.
    /// </remarks>
    /// <exception cref="ObjectDisposedException"><paramref name="builder"/> has been disposed.</exception>
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
    /// <returns>The builder, for chaining.</returns>
    private static DbContextBuilder<TDbContext> UseSqlite<TDbContext>
    (
        this DbContextBuilder<TDbContext> builder,
        Type modelCustomizerType
    ) where TDbContext : DbContext
    {
        // Before anything is registered or created: a disposed builder must not gain SQLite
        // services or an open in-memory connection that nothing can release (#563).
        builder.ThrowIfDisposed();

        // Avoid registering EF services multiple times. AddEntityFrameworkSqlite registers
        // DatabaseProvider<SqliteOptionsExtension> as an IDatabaseProvider, so that descriptor
        // marks the SQLite services as already present. Match it with typeof rather than a
        // FullName string (which silently breaks if EF renames or moves the type).
        //
        // SqliteOptionsExtension lives in EF Core's internal namespace
        // (Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal) — the typeof
        // check on it is deliberate for the reason above, so EF1001 is expected here.
#pragma warning disable EF1001 // Internal EF Core API usage
        if (!builder.ServiceCollection.Any
            (
                sd => sd.ServiceType == typeof(IDatabaseProvider) &&
                      sd.ImplementationType == typeof(DatabaseProvider<SqliteOptionsExtension>)
            ))
#pragma warning restore EF1001
        {
            builder.ServiceCollection.AddEntityFrameworkSqlite();
        }

        // Remove any existing IModelCustomizer registrations to avoid duplicates/competing implementations.
        // This runs after AddEntityFrameworkSqlite so it also removes the default ModelCustomizer
        // that EF registers, leaving modelCustomizerType as the only one.
        var modelCustomizerDescriptors = builder.ServiceCollection
            .Where(sd => sd.ServiceType == typeof(IModelCustomizer))
            .ToList();
        foreach (var descriptor in modelCustomizerDescriptors)
        {
            builder.ServiceCollection.Remove(descriptor);
        }

        builder.ServiceCollection.AddSingleton(typeof(IModelCustomizer), modelCustomizerType);
        builder.SetCreateDbContext(new SqliteDbContextCreator());

        return builder;
    }
}
