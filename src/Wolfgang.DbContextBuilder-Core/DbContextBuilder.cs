using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

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
    private readonly List<object> _seedData = [];
    private readonly ServiceCollection _serviceCollection = [];
    private DbContextOptionsBuilder<T>? _dbContextOptionsBuilder;


    internal IGenerateRandomEntities RandomEntityGenerator { get; private set; } = new AutoFixtureRandomEntityGenerator();



    /// <summary>
    /// Instructs the builder to use SQLite as the database provider.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    public DbContextBuilder<T> UseSqlite() // TODO Move to extension method
	{
		_dbProvider = DbProvider.Sqlite;

        // TODO Check is items exist in the list and don't add duplicates
        // TODO Check if Sql Server provider is already registered and if so, remove it
        _serviceCollection
            .AddEntityFrameworkSqlite()
            .AddSingleton<IModelCustomizer, SqliteModelCustomizer>();

        return this;
	}



    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    public DbContextBuilder<T> UseSqliteForMsSqlServer()  // TODO Move to extension method
    {
        _dbProvider = DbProvider.Sqlite;

        // TODO Check is items exist in the list and don't add duplicates
        // TODO Check if Sql Server provider is already registered and if so, remove it
        _serviceCollection
            .AddEntityFrameworkSqlite()
            .AddSingleton<IModelCustomizer, SqliteForMsSqlServerModelCustomizer>();

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

        if (_serviceCollection.Count > 0)
        {
            var provider = _serviceCollection.BuildServiceProvider();
            optionBuilder.UseInternalServiceProvider(provider);
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
            // TODO Is this message correct and complete? The last line may be incomplete
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