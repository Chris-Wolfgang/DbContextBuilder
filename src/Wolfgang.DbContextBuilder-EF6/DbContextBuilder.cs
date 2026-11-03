using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Wolfgang.DbContextBuilderEF6;

/// <summary>
/// Uses the Builder pattern to create instances of DbContext types seeded with specified data.
/// </summary>
/// <remarks>
/// <para>
/// The target <typeparamref name="T"/> must have a constructor that accepts
/// (<see cref="System.Data.Common.DbConnection"/>, <see cref="bool"/>)
/// for use with in-memory database providers such as Effort.
/// </para>
/// <para>
/// The first <see cref="Build"/> or <see cref="BuildAsync"/> call creates and seeds the database;
/// later calls return another context over the same, already-seeded database. Add all seed data
/// before the first build: the seeding methods throw <see cref="InvalidOperationException"/> after
/// it. Selecting a provider again (<c>UseEffort()</c>, the same or a different one) starts a new,
/// empty database that the next build seeds again.
/// </para>
/// </remarks>
public class DbContextBuilder<T> where T : DbContext
{
    private readonly List<object> _seedData = new List<object>();

    // The creator whose database Build/BuildAsync has already created and seeded. Later builds on
    // the same creator do not seed it again (#734, the EF Core builder's #559); a new creator
    // (UseEffort) is a new, empty database, so it is seeded again.
    private ICreateDbContext? _seededCreator;



    internal ICreateDbContext? CreateDbContext { get; set; }



    internal ICreateRandomEntities RandomEntityCreator { get; private set; } = new AutoFixtureRandomEntityCreator();



    /// <summary>
    /// Allows the user to specify their own implementation of ICreateRandomEntities
    /// for creating random entities.
    /// </summary>
    /// <param name="creator">The creator to use</param>
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
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
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    /// <exception cref="ArgumentNullException">entities is null</exception>
    /// <exception cref="ArgumentException">entities contains a null item</exception>
    /// <exception cref="ArgumentException">entities contains a string</exception>
    /// <exception cref="InvalidOperationException">A previous <see cref="Build"/> or <see cref="BuildAsync"/> call has already created and seeded the database.</exception>
    public DbContextBuilder<T> SeedWith<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : class
    {
        ThrowIfSeeded();

        if (entities == null)
        {
            throw new ArgumentNullException(nameof(entities));
        }

        if (typeof(TEntity) == typeof(string))
        {
            throw new ArgumentException("The type of TEntity cannot be string", nameof(entities));
        }

        // Iterate directly rather than materializing to an array; the per-item arms are shared
        // with the params overload (#740).
        AddSeedItems(entities, nameof(entities));
        return this;
    }



    /// <summary>
    /// Populates the specified DbSet with the provided entities.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    /// <param name="entities">The entities to populate the database with</param>
    /// <exception cref="ArgumentNullException">entities is null</exception>
    /// <exception cref="ArgumentException">entities contains a null item</exception>
    /// <exception cref="ArgumentException">entities contains a string</exception>
    /// <exception cref="InvalidOperationException">A previous <see cref="Build"/> or <see cref="BuildAsync"/> call has already created and seeded the database.</exception>
    public DbContextBuilder<T> SeedWith<TEntity>(params TEntity[] entities)
        where TEntity : class
    {
        ThrowIfSeeded();

        if (entities == null)
        {
            throw new ArgumentNullException(nameof(entities));
        }

        AddSeedItems(entities, nameof(entities));
        return this;
    }



    /// <summary>
    /// Populates the specified DbSet with a single entity. Equivalent to calling the
    /// <c>params</c>-array overload with one element, but avoids the per-call allocation
    /// of a one-element array — useful in tests that seed many single rows.
    /// </summary>
    /// <param name="entity">The entity to populate the database with.</param>
    /// <returns>The builder, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entity"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="entity"/> is a <see cref="string"/> instance (matches the
    /// <c>params</c> overload's rejection regardless of how <typeparamref name="TEntity"/> was inferred), or
    /// <paramref name="entity"/> is a sequence that contains a null or a <see cref="string"/> item.</exception>
    /// <exception cref="InvalidOperationException">A previous <see cref="Build"/> or <see cref="BuildAsync"/> call has already created and seeded the database.</exception>
    public DbContextBuilder<T> SeedWith<TEntity>(TEntity entity)
        where TEntity : class
    {
        ThrowIfSeeded();

        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        // Reject by runtime type, not just TEntity, so `SeedWith<object>("...")` is still
        // caught (matches the params overload's `case string:` arm).
        if (entity is string)
        {
            throw new ArgumentException("One of the entities passed in is of type string", nameof(entity));
        }

        if (entity is IEnumerable<object> sequence)
        {
            AddSequenceItems(sequence, nameof(entity));
        }
        else
        {
            _seedData.Add(entity);
        }

        return this;
    }



    // Shared validation + add loop for the IEnumerable and params overloads, so their
    // null/string/sequence arms cannot drift (#740, as in the EF Core builder).
    private void AddSeedItems<TEntity>(IEnumerable<TEntity> source, string paramName)
        where TEntity : class
    {
        foreach (var entity in source)
        {
            switch (entity)
            {
                case null:
                    throw new ArgumentException("One of the entities is null", paramName);
                case string:
                    throw new ArgumentException("One of the entities passed in is of type string", paramName);
                case IEnumerable<object> e:
                    // A nested sequence gets the same per-item check as the single-entity
                    // overload (#735).
                    AddSequenceItems(e, paramName);
                    break;
                default:
                    _seedData.Add(entity);
                    break;
            }
        }
    }



    // Adds each item of a sequence passed as one seed argument. Buffers first so a failing
    // sequence leaves `_seedData` untouched. IEnumerable<T> is covariant for reference types, so
    // a List<string> arrives here as IEnumerable<object> and is caught item by item (#560, #735).
    private void AddSequenceItems(IEnumerable<object> sequence, string paramName)
    {
        var buffer = new List<object>();
        foreach (var item in sequence)
        {
            if (item is null)
            {
                throw new ArgumentException("One of the entities is null", paramName);
            }

            if (item is string)
            {
                throw new ArgumentException("One of the entities passed in is of type string", paramName);
            }

            buffer.Add(item);
        }

        _seedData.AddRange(buffer);
    }



    /// <summary>
    /// Populates the specified DbSet with random entities of type TEntity.
    /// </summary>
    /// <param name="count">The number of items to create</param>
    /// <typeparam name="TEntity">The type of entity to create</typeparam>
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">count is less than 1</exception>
    /// <exception cref="InvalidOperationException">A previous <see cref="Build"/> or <see cref="BuildAsync"/> call has already created and seeded the database.</exception>
    public DbContextBuilder<T> SeedWithRandom<TEntity>(int count) where TEntity : class
    {
        ThrowIfSeeded();

        ThrowIfCountInvalid(count);

        return AddRandom
        (
            RandomEntityCreator
                .CreateRandomEntities<TEntity>(count)
        );
    }



    /// <summary>
    /// Populates the specified DbSet with random entities of type TEntity.
    /// </summary>
    /// <param name="count">The number of items to create</param>
    /// <param name="func">A function that takes a TEntity and returns an updated TEntity</param>
    /// <typeparam name="TEntity">The type of entity to create</typeparam>
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">count is less than 1</exception>
    /// <exception cref="InvalidOperationException">A previous <see cref="Build"/> or <see cref="BuildAsync"/> call has already created and seeded the database.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="func"/> is <c>null</c>.</exception>
    public DbContextBuilder<T> SeedWithRandom<TEntity>(int count, Func<TEntity, TEntity> func) where TEntity : class
    {
        ThrowIfSeeded();

        ThrowIfCountInvalid(count);

        if (func == null)
        {
            throw new ArgumentNullException(nameof(func));
        }

        return AddRandom
        (
            RandomEntityCreator
                .CreateRandomEntities<TEntity>(count)
                .Select(func)
        );
    }



    /// <summary>
    /// Populates the specified DbSet with random entities of type TEntity.
    /// </summary>
    /// <param name="count">The number of items to create</param>
    /// <param name="func">A function that takes a TEntity and the index number of the entity and returns an updated TEntity</param>
    /// <typeparam name="TEntity">The type of entity to create</typeparam>
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">count is less than 1</exception>
    /// <exception cref="InvalidOperationException">A previous <see cref="Build"/> or <see cref="BuildAsync"/> call has already created and seeded the database.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="func"/> is <c>null</c>.</exception>
    public DbContextBuilder<T> SeedWithRandom<TEntity>(int count, Func<TEntity, int, TEntity> func) where TEntity : class
    {
        ThrowIfSeeded();

        ThrowIfCountInvalid(count);

        if (func == null)
        {
            throw new ArgumentNullException(nameof(func));
        }

        return AddRandom
        (
            RandomEntityCreator
                .CreateRandomEntities<TEntity>(count)
                .Select(func)
        );
    }



    /// <summary>
    /// Creates a new instance of <typeparamref name="T"/> seeded with the configured data.
    /// </summary>
    /// <remarks>
    /// The first call creates the database and saves the seed data. Later calls return a new
    /// context over the same database without seeding it again.
    /// </remarks>
    /// <returns>A new instance of <typeparamref name="T"/>.</returns>
    /// <exception cref="MissingMethodException">
    /// <typeparamref name="T"/> does not have a constructor that accepts
    /// (<see cref="System.Data.Common.DbConnection"/>, <see cref="bool"/>) — propagated from
    /// <see cref="Activator.CreateInstance(Type, object[])"/> inside
    /// <see cref="EffortDbContextCreator.CreateDbContext{TDbContext}"/>, which the builder
    /// calls before <c>InitializeDatabase</c> can wrap it.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The underlying <see cref="System.Data.Entity.Database"/> could not create itself
    /// (a different failure mode than the missing-ctor case above). Wrapped by
    /// <c>InitializeDatabase</c> with a more actionable message; the original exception is
    /// in <see cref="Exception.InnerException"/>.
    /// </exception>
    public T Build()
    {
        var contextCreator = ResolveContextCreator();

        // Create a temporary context to initialize the database (via Effort's shared
        // connection) and persist seed data. Dispose it before returning the caller's
        // context — otherwise it leaks for the lifetime of the builder. The seed data
        // remains because EffortDbContextCreator holds the underlying connection open
        // independent of any one context.
        if (!ReferenceEquals(_seededCreator, contextCreator))
        {
            using (var seedContext = contextCreator.CreateDbContext<T>())
            {
                if (PrepareSeedContext(seedContext))
                {
                    seedContext.SaveChanges();
                }
            }

            _seededCreator = contextCreator;
        }

        return contextCreator.CreateDbContext<T>();
    }



    /// <summary>
    /// Creates a new instance of <typeparamref name="T"/> seeded with the configured data
    /// asynchronously.
    /// </summary>
    /// <remarks>
    /// The first call creates the database and saves the seed data. Later calls return a new
    /// context over the same database without seeding it again.
    /// </remarks>
    /// <returns>A new instance of <typeparamref name="T"/>.</returns>
    /// <exception cref="MissingMethodException">
    /// <typeparamref name="T"/> does not have a constructor that accepts
    /// (<see cref="System.Data.Common.DbConnection"/>, <see cref="bool"/>) — propagated from
    /// <see cref="Activator.CreateInstance(Type, object[])"/> inside
    /// <see cref="EffortDbContextCreator.CreateDbContext{TDbContext}"/>, which the builder
    /// calls before <c>InitializeDatabase</c> can wrap it.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The underlying <see cref="System.Data.Entity.Database"/> could not create itself
    /// (a different failure mode than the missing-ctor case above). Wrapped by
    /// <c>InitializeDatabase</c> with a more actionable message; the original exception is
    /// in <see cref="Exception.InnerException"/>.
    /// </exception>
    public async Task<T> BuildAsync()
    {
        var contextCreator = ResolveContextCreator();

        // Same temporary seed context as Build; only the save is asynchronous.
        if (!ReferenceEquals(_seededCreator, contextCreator))
        {
            using (var seedContext = contextCreator.CreateDbContext<T>())
            {
                if (PrepareSeedContext(seedContext))
                {
                    await seedContext.SaveChangesAsync().ConfigureAwait(false);
                }
            }

            _seededCreator = contextCreator;
        }

        return contextCreator.CreateDbContext<T>();
    }



    // The seeding methods call this: seed data added after the database was seeded would never
    // reach it, because later builds on the same creator do not seed again (#734).
    private void ThrowIfSeeded()
    {
        if (_seededCreator is not null && ReferenceEquals(_seededCreator, CreateDbContext))
        {
            throw new InvalidOperationException
            (
                "Seed data must be added before the first Build() or BuildAsync() call: the database has already " +
                "been created and seeded. Add all seed data first, or use a new DbContextBuilder."
            );
        }
    }



    // The count guard the three SeedWithRandom overloads share (#740).
    private static void ThrowIfCountInvalid(int count)
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than 0");
        }
    }



    // Adds the generated entities for the three SeedWithRandom overloads (#740).
    private DbContextBuilder<T> AddRandom<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : class
    {
        _seedData.AddRange(entities);
        return this;
    }



    // Shared by Build and BuildAsync (#573): the configured creator, or Effort by default,
    // remembered so every later context shares its connection.
    private ICreateDbContext ResolveContextCreator()
    {
        var contextCreator = CreateDbContext ?? new EffortDbContextCreator();
        CreateDbContext = contextCreator;
        return contextCreator;
    }



    // Shared by Build and BuildAsync (#573): creates the database and adds the seed data to
    // seedContext. Returns whether there is anything for the caller to save.
    private bool PrepareSeedContext(T seedContext)
    {
        InitializeDatabase(seedContext);

        if (_seedData.Count == 0)
        {
            return false;
        }

        foreach (var entity in _seedData)
        {
            seedContext.Set(entity.GetType()).Add(entity);
        }

        return true;
    }



    private static void InitializeDatabase(T context)
    {
        try
        {
            context.Database.CreateIfNotExists();
        }
        catch (InvalidOperationException e)
        {
            // A missing (DbConnection, bool) constructor cannot reach this catch: it throws
            // MissingMethodException while the context is created, before this runs (#565).
            const string msg = "Failed to create database. See InnerException for details. " +
                               "Common causes: the model cannot be mapped to the database provider, " +
                               "or the context's connection is not usable (for example, the context " +
                               "returned by the context creator has already been disposed).";
            throw new InvalidOperationException(msg, e);
        }
    }
}
