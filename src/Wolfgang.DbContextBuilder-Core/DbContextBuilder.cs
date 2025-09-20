using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Wolfgang.DbContextBuilderCore.Tests.Unit;

namespace Wolfgang.DbContextBuilderCore;




/// <summary>
/// Uses the Builder pattern to create instances of DbContext types seeded with specified data.
/// </summary>
public class DbContextBuilder<T> where T : DbContext
{
	internal enum DbProvider
	{
		InMemory,
		Sqlite
	}
    
	private DbProvider _dbProvider = DbProvider.InMemory;
    private readonly List<object> _seedData = new();
    private IServiceProvider? _serviceProvider;
    private DbContextOptionsBuilder<T>? _dbContextOptionsBuilder;


    internal IGenerateRandomEntities RandomEntityGenerator { get; private set; } = new AutoFixtureRandomEntityGenerator();



    /// <summary>
    /// Instructs the builder to use SQLite as the database provider.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    public DbContextBuilder<T> UseSqlite() // TODO Move to extension method
	{
		_dbProvider = DbProvider.Sqlite;

        _serviceProvider =  new ServiceCollection()
            .AddEntityFrameworkSqlite()
            .AddSingleton<IModelCacheKeyFactory, SqliteModelCacheKeyFactory>()
            .AddSingleton<IModelCustomizer, SqliteModelCustomizer>()
            .BuildServiceProvider();

        return this;
	}



    /// <summary>
    /// Instructs the builder to use SQLite as the database provider.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    public DbContextBuilder<T> UseSqlite(SqliteOverrides overrides) // TODO Move to extension method
    {
        ArgumentNullException.ThrowIfNull(overrides);

        _dbProvider = DbProvider.Sqlite;
        return this;
    }



    /// <summary>
    /// Instructs the builder to use InMemory as the database provider.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    public DbContextBuilder<T> UseInMemory()
	{
		_dbProvider = DbProvider.InMemory;
		return this;
	}



    /// <summary>
    /// Tell DbContextBuilder to use AutoFixture to create random entities.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    public DbContextBuilder<T> UseAutoFixture()
    {
        RandomEntityGenerator = new AutoFixtureRandomEntityGenerator();

        return this;
    }



    /// <summary>
    /// Allows the user to specify their own implementation of IGenerateRandomEntities
    /// for generating random entities.
    /// </summary>
    /// <param name="generator">The generator to use</param>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    public DbContextBuilder<T> UseCustomRandomEntityGenerator(IGenerateRandomEntities generator)
    {
        ArgumentNullException.ThrowIfNull(generator);
        RandomEntityGenerator = generator;
        return this;
    }



    /// <summary>
    /// Provides a method to override how the database is created since different databases
    /// engines support different features
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    /// <remarks>
    /// This method allows you to intercept the creation of the database and alter or
    /// customize it. This is useful, for example, if your production database is SQL Server
    /// or Oracle and is using features that are not supported by the InMemory or Sqlite
    /// databases. Using this method and passing in your customizations, allows you to
    /// alter or opt out altogether certain columns.
    ///
    /// Note: This does mean that the features that you change will not match your
    /// production system and so testing those features is pointless, however you can still
    /// test the rest of your code.
    ///
    /// One such option is say your production database has a column that is a computed
    /// type using functions that is are not supported under InMemory or Sqlite database.
    /// You could use this method to alter the table to just store an integer rather than
    /// a calculation. As long as you seed the database correctly you can still use the
    /// table in your tests.
    /// </remarks>
    public DbContextBuilder<T> UseServiceProvider(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;

        return this;
    }



    /// <summary>
    /// Specifies a specific instance of UseDbContextOptionsBuilder to use when creating the DbContext.
    /// </summary>
    /// <param name="dbContextOptionsBuilder"></param>
    /// <returns></returns>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    /// <exception cref="ArgumentNullException">callback is null</exception>
    public DbContextBuilder<T> UseDbContextOptionsBuilder(DbContextOptionsBuilder<T> dbContextOptionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(dbContextOptionsBuilder);

        _dbContextOptionsBuilder = dbContextOptionsBuilder;

        return this;
    }



    /// <summary>
    /// Populates the specified DbSet with the provided entities.
    /// </summary>
    /// <param name="entities">The entities to populate the database with</param>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    /// <exception cref="ArgumentNullException">entities is null</exception>
    public DbContextBuilder<T> SeedWith<TEntity>(IEnumerable<TEntity> entities) 
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        _seedData.AddRange(entities);

        return this;
    }



    /// <summary>
    /// Populates the specified DbSet with the provided entities.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    /// <param name="entities">The entities to populate the database with</param>
    /// <exception cref="ArgumentNullException">entities is null</exception>
    /// <exception cref="ArgumentException">entities contains a null item</exception>
    /// <exception cref="ArgumentException">entities contains a string</exception>
    public DbContextBuilder<T> SeedWith<TEntity>(params TEntity[] entities) 
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));



        foreach (var entity in entities)
        {
            switch (entity)
            {
                case null:
                    throw new ArgumentException("One of the entities is null", nameof(entities));
                case string:
                    throw new ArgumentException("One of the entities passed in is of type string", nameof(entities));
                case IEnumerable<object> e:
                    _seedData.AddRange(e);
                    break;
                default:
                    _seedData.Add(entity);
                    break;
            }
        }
        return this;
    }



    /// <summary>
    /// Populates the specified DbSet with random entities of type TEntity.
    /// </summary>
    /// <param name="count">The number of items to create</param>
    /// <typeparam name="TEntity">The type of entity to create</typeparam>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    /// <exception cref="ArgumentOutOfRangeException">count is less than 1</exception>
    public DbContextBuilder<T> SeedWithRandom<TEntity>(int count) where TEntity : class
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than 0");
        }

        var entities = RandomEntityGenerator
            .GenerateRandomEntities<TEntity>(count);

        _seedData.AddRange(entities);

        return this;
    }



    /// <summary>
    /// Populates the specified DbSet with random entities of type TEntity.
    /// </summary>
    /// <param name="count">The number of items to create</param>
    /// <param name="func">A function that takes a TEntity and returns an updated TEntity</param>
    /// <typeparam name="TEntity">The type of entity to create</typeparam>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    /// <exception cref="ArgumentOutOfRangeException">count is less than 1</exception>
    public DbContextBuilder<T> SeedWithRandom<TEntity>(int count, Func<TEntity, TEntity> func) where TEntity : class
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than 0");
        }

        ArgumentNullException.ThrowIfNull(func, nameof(func));



        var entities = RandomEntityGenerator
            .GenerateRandomEntities<TEntity>(count)
            .Select(func);
            
        _seedData.AddRange(entities);

        return this;
    }



    /// <summary>
    /// Populates the specified DbSet with random entities of type TEntity.
    /// </summary>
    /// <param name="count">The number of items to create</param>
    /// <param name="func">A function that takes a TEntity and the index number of the entity and returns an updated TEntity</param>
    /// <typeparam name="TEntity">The type of entity to create</typeparam>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    /// <exception cref="ArgumentOutOfRangeException">count is less than 1</exception>
    public DbContextBuilder<T> SeedWithRandom<TEntity>(int count, Func<TEntity, int, TEntity> func) where TEntity : class
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than 0");
        }

        ArgumentNullException.ThrowIfNull(func, nameof(func));

        var entities = RandomEntityGenerator
            .GenerateRandomEntities<TEntity>(count)
            .Select(func);

        _seedData.AddRange(entities);

        return this;
    }



    /// <summary>
    /// Creates a new instance of T seeded with specified data."/>.
    /// </summary>
    /// <returns>instance of {T}</returns>
    /// <exception cref="NotSupportedException">The specified database provider is not supported</exception>
    public async Task<T> BuildAsync()
    {
        // ReSharper disable once TooWideLocalVariableScope
        DbConnection? connection; // variable must remain in outer scope so it can be reused later

        var optionBuilder = _dbContextOptionsBuilder ?? new DbContextOptionsBuilder<T>();

        switch (_dbProvider)
        {
            case DbProvider.InMemory:
                optionBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
                break;
            case DbProvider.Sqlite:
                connection = new SqliteConnection("DataSource=:memory:");
                await connection.OpenAsync();
                optionBuilder.UseSqlite(connection);
                break;
            default:
                throw new NotSupportedException($"Provider {_dbProvider} is not supported.");
        }

        if (_serviceProvider != null)
        {
            optionBuilder.UseInternalServiceProvider(_serviceProvider);
        }

        var options = optionBuilder.Options;

        var context = (T)Activator.CreateInstance(typeof(T), options)!;

        try
        {
            // Create a context to initialize and seed the database
            await context.Database.EnsureCreatedAsync();
        }
        catch (InvalidOperationException e)
        {
            const string msg = "Failed to create database. See InnerExceptions for details. " +
                               "You can get addition information by creating a new instance of " +
                               "DbContextOptionsBuilder<T> and passing into UseDbContextOptionsBuilder";
            throw new InvalidOperationException(msg, e);
        }

        if (_seedData.Count > 0)
        {
            context.AddRange(_seedData.AsEnumerable());
            await context.SaveChangesAsync();
        }

        // Create a new clean context instance to return
        return (T)Activator.CreateInstance(typeof(T), options)!;
    }
}






// TODO add property to config OverrideDefaultSqliteModelCacheKeyFactory = T/F
// TODO Modify UseSqlite to return a SqliteDbContextBuilder with additional properties including this one

internal class SqliteModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime) => new SqliteModelCacheKey(context, designTime);

    private sealed class SqliteModelCacheKey(DbContext context, bool designTime)
        : ModelCacheKey(context, designTime)
    {
    }
}



internal sealed class SqliteModelCustomizer(ModelCustomizerDependencies dependencies)
    : ModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        if (!context.Database.IsSqlite())
        {
            return;
        }

        // TODO Create property SchemaHandling
        //      Options: LeaveAlone, Strip, PrefixToTableName
        // Rename all tables and fix relationships
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var originalTableName = entityType.GetTableName() ?? "";
            var originalSchemaName = entityType.GetSchema() ?? "";

            { // TODO Override schema handling
                var schemaPrefix = string.IsNullOrEmpty(originalSchemaName) ? "dbo" : originalSchemaName;

                // Avoid recursive renaming
                if (!originalTableName.StartsWith($"{schemaPrefix}_", StringComparison.InvariantCultureIgnoreCase))
                {
                    var newTableName = $"{schemaPrefix}_{originalTableName}";
                    entityType.SetTableName(newTableName);
                }

                entityType.SetSchema(null); // Always strip schema for SQLite
            }

            { // TODO Override computed column handling

                //// Fix computed columns and default values
                //var computedColumns = new Dictionary<(string Schema, string Table, string Column), string?>
                //{
                //    { ("Person", "Person", "OrganizationLevel"), null }, { ("HumanResources", "Employee", "OrganizationLevel"), null }, { ("Sales", "Customer", "AccountNumber"), null }, { ("Sales", "SalesOrderHeader", "SalesOrderNumber"), "(IFNULL('SO' || CAST(\"SalesOrderID\" AS TEXT), '*** ERROR ***'))" },
                //};

                foreach (var property in entityType.GetProperties())
                {
                    //if (computedColumns.TryGetValue((originalSchemaName, originalTableName, property.Name), out var value))
                    //{
                    //    property.SetComputedColumnSql(value);
                    //}
                    //else
                    //{
                    //    var sql = property.GetComputedColumnSql();

                    //    if (!string.IsNullOrEmpty(sql))
                    //    {
                    //        var rewrittenSql = sql.Replace("ISNULL", "IFNULL", StringComparison.OrdinalIgnoreCase)
                    //                .Replace("N'", "'", StringComparison.OrdinalIgnoreCase)
                    //                .Replace("+", "||", StringComparison.OrdinalIgnoreCase)
                    //            //.Replace("CONVERT", "CAST", StringComparison.OrdinalIgnoreCase)
                    //            //.Replace("[dbo].", "")
                    //            //.Replace("dbo.", "")
                    //            ;

                    //        property.SetComputedColumnSql(rewrittenSql);
                    //    }
                    //}
                    property.SetComputedColumnSql(null!);
                }

                { // TODO Override default value handling
                    foreach (var property in entityType.GetProperties())
                    {
                        var defaultValueSql = property.GetDefaultValueSql();

                        if (!string.IsNullOrWhiteSpace(defaultValueSql))
                        {
                            if (!string.IsNullOrEmpty(defaultValueSql) &&
                                defaultValueSql.Contains("newid", StringComparison.OrdinalIgnoreCase))
                            {
                                property.SetDefaultValueSql("lower(hex(randomblob(16)))");
                            }
                            else if (defaultValueSql.Contains("getdate()"))
                            {
                                property.SetDefaultValueSql("datetime('now')");
                            }
                        }
                        //property.SetDefaultValueSql(null!); // TODO Uncomment this and comment out above to make tests pass
                    }
                }
            }

            { // TODO Override join table handling
                // Heuristic: rename many-to-many join tables
                var foreignKeys = entityType.GetForeignKeys().ToList();
                var navigation = entityType.GetNavigations().ToList();

                if (foreignKeys.Count == 2 && navigation.Count == 0)
                {
                    var left = foreignKeys[0].PrincipalEntityType;
                    var right = foreignKeys[1].PrincipalEntityType;

                    var leftName = left.GetTableName();
                    var rightName = right.GetTableName();

                    var joinName = $"{leftName}_{rightName}";
                    entityType.SetTableName(joinName);
                }
            }
        }
    }
}