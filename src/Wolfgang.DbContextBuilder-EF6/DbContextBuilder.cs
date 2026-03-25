using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Wolfgang.DbContextBuilderEF6;




/// <summary>
/// Uses the Builder pattern to create instances of DbContext types seeded with specified data.
/// </summary>
/// <remarks>
/// The target <typeparamref name="T"/> must have a constructor that accepts
/// (<see cref="System.Data.Common.DbConnection"/>, <see cref="bool"/>)
/// for use with in-memory database providers such as Effort.
/// </remarks>
public class DbContextBuilder<T> where T : DbContext
{
    private readonly List<object> _seedData = new List<object>();


    internal ICreateDbContext? CreateDbContext { get; set; }


    internal ICreateRandomEntities RandomEntityCreator { get; private set; } = new AutoFixtureRandomEntityCreator();



    /// <summary>
    /// Allows the user to specify their own implementation of ICreateRandomEntities
    /// for creating random entities.
    /// </summary>
    /// <param name="creator">The creator to use</param>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    /// <exception cref="ArgumentNullException"><paramref name="creator"/> is <c>null</c>.</exception>
    public DbContextBuilder<T> UseCustomRandomEntityCreator(ICreateRandomEntities creator)
    {
        if (creator == null)
        {
            throw new ArgumentNullException(nameof(creator));
        }

        RandomEntityCreator = creator;
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
        if (entities == null)
        {
            throw new ArgumentNullException(nameof(entities));
        }

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
        if (entities == null)
        {
            throw new ArgumentNullException(nameof(entities));
        }

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
    /// <exception cref="ArgumentNullException"><paramref name="func"/> is <c>null</c>.</exception>
    public DbContextBuilder<T> SeedWithRandom<TEntity>(int count, Func<TEntity, TEntity> func) where TEntity : class
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than 0");
        }

        if (func == null)
        {
            throw new ArgumentNullException(nameof(func));
        }

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
    /// <exception cref="ArgumentNullException"><paramref name="func"/> is <c>null</c>.</exception>
    public DbContextBuilder<T> SeedWithRandom<TEntity>(int count, Func<TEntity, int, TEntity> func) where TEntity : class
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than 0");
        }

        if (func == null)
        {
            throw new ArgumentNullException(nameof(func));
        }

        var entities = RandomEntityCreator
            .CreateRandomEntities<TEntity>(count)
            .Select(func);

        _seedData.AddRange(entities);

        return this;
    }



    /// <summary>
    /// Creates a new instance of T seeded with specified data.
    /// </summary>
    /// <returns>instance of {T}</returns>
    /// <exception cref="InvalidOperationException">No database provider has been configured.</exception>
    public T Build()
    {
        var contextCreator = CreateDbContext ?? new EffortDbContextCreator();
        CreateDbContext = contextCreator;

        var context = contextCreator.CreateDbContext<T>();
        InitializeDatabase(context);

        if (_seedData.Count > 0)
        {
            foreach (var entity in _seedData)
            {
                context.Set(entity.GetType()).Add(entity);
            }
            context.SaveChanges();
        }

        return contextCreator.CreateDbContext<T>();
    }



    /// <summary>
    /// Creates a new instance of T seeded with specified data asynchronously.
    /// </summary>
    /// <returns>instance of {T}</returns>
    /// <exception cref="InvalidOperationException">No database provider has been configured.</exception>
    public async Task<T> BuildAsync()
    {
        var contextCreator = CreateDbContext ?? new EffortDbContextCreator();
        CreateDbContext = contextCreator;

        var context = contextCreator.CreateDbContext<T>();
        InitializeDatabase(context);

        if (_seedData.Count > 0)
        {
            foreach (var entity in _seedData)
            {
                context.Set(entity.GetType()).Add(entity);
            }
            await context.SaveChangesAsync();
        }

        return contextCreator.CreateDbContext<T>();
    }



    [ExcludeFromCodeCoverage]
    private static void InitializeDatabase(T context)
    {
        try
        {
            context.Database.CreateIfNotExists();
        }
        catch (InvalidOperationException e)
        {
            const string msg = "Failed to create database. See InnerException for details. " +
                               "Ensure your DbContext has a constructor that accepts (DbConnection, bool).";
            throw new InvalidOperationException(msg, e);
        }
    }
}
