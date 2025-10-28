using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Wolfgang.DbContextBuilderCore;




/// <summary>
/// Uses the Builder pattern to create instances of DbContext types seeded with specified data.
/// </summary>
public class DbContextBuilder<T> where T : DbContext
{
    private readonly List<object> _seedData = [];
    private DbContextOptionsBuilder<T>? _dbContextOptionsBuilder;


    internal ServiceCollection ServiceCollection { get; } = [];
    internal ICreateDbContext? CreateDbContext { get; set; }


    internal ICreateRandomEntities RandomEntityCreator { get; private set; } = new AutoFixtureRandomEntityCreator();



    /// <summary>
    /// Instructs the builder to use InMemory as the database provider.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    public DbContextBuilder<T> UseInMemory()
	{
        CreateDbContext = new InMemoryDbContextCreator();
        return this;
	}


    
    /// <summary>
    /// Allows the user to specify their own implementation of ICreateRandomEntities
    /// for creating random entities.
    /// </summary>
    /// <param name="creator">The creator to use</param>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    public DbContextBuilder<T> UseCustomRandomEntityCreator(ICreateRandomEntities creator)
    {
        ArgumentNullException.ThrowIfNull(creator);
        RandomEntityCreator = creator;
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
    /// <exception cref="ArgumentException">entities contains a null item</exception>
    /// <exception cref="ArgumentException">entities contains a string</exception>
    public DbContextBuilder<T> SeedWith<TEntity>(IEnumerable<TEntity> entities) 
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        if (typeof(TEntity) == typeof(string))
        {
            throw new ArgumentException("The type of TEntity cannot be string", nameof(entities));
        }

        var enumerable = entities as TEntity[] ?? entities.ToArray();
        return SeedWith(enumerable);
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

        var entities = RandomEntityCreator
            .CreateRandomEntities<TEntity>(count);

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

        var entities = RandomEntityCreator
            .CreateRandomEntities<TEntity>(count)
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

        var entities = RandomEntityCreator
            .CreateRandomEntities<TEntity>(count)
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
        var optionBuilder = _dbContextOptionsBuilder ?? new DbContextOptionsBuilder<T>();
        if (ServiceCollection.Count > 0)
        {
            var provider = ServiceCollection.BuildServiceProvider();
            optionBuilder.UseInternalServiceProvider(provider);
        }

        var contextCreator = CreateDbContext ??= new InMemoryDbContextCreator();

        var context = await contextCreator.CreateDbContext(optionBuilder);

        try
        {
            // Create a context to initialize and seed the database
            await context.Database.EnsureCreatedAsync();
        }
        catch (InvalidOperationException e)
        {
            // TODO Is this message correct and complete? The last line may be incomplete
            const string msg = "Failed to create database. See InnerException for details. " +
                               "You can get additional information by creating a new instance of " +
                               "DbContextOptionsBuilder<T> and passing it into UseDbContextOptionsBuilder.";
            throw new InvalidOperationException(msg, e);
        }

        if (_seedData.Count > 0)
        {
            context.AddRange(_seedData.AsEnumerable());
            await context.SaveChangesAsync();
        }

        return await contextCreator.CreateDbContext(optionBuilder);
    }
}