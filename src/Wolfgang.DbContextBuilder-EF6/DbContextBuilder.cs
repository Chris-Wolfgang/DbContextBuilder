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
/// The target <typeparamref name="T"/> must have a constructor that accepts
/// (<see cref="System.Data.Common.DbConnection"/>, <see cref="bool"/>)
/// for use with in-memory database providers such as Effort.
/// The builder owns its context creator (for Effort, the in-memory connection that holds the
/// database). Dispose the builder after the last context it built is no longer in use.
/// </remarks>
public class DbContextBuilder<T> : IDisposable where T : DbContext
{
    private readonly List<object> _seedData = new List<object>();
    private bool _disposed;



    internal ICreateDbContext? CreateDbContext { get; set; }



    // Replaces the active context creator, disposing the previous one: re-selecting a provider
    // (UseEffort twice, say) used to drop the old creator and its open connection (#562).
    internal void SetCreateDbContext(ICreateDbContext creator)
    {
        ThrowIfDisposed();

        if (!ReferenceEquals(CreateDbContext, creator))
        {
            CreateDbContext?.Dispose();
        }

        CreateDbContext = creator;
    }



    // Every configuration entry point and Build/BuildAsync call this first: a disposed builder
    // must not accept new state or create a connection nothing would release.
    internal void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DbContextBuilder<T>));
        }
    }



    internal ICreateRandomEntities RandomEntityCreator { get; private set; } = new AutoFixtureRandomEntityCreator();



    /// <summary>
    /// Allows the user to specify their own implementation of ICreateRandomEntities
    /// for creating random entities.
    /// </summary>
    /// <param name="creator">The creator to use</param>
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="creator"/> is <c>null</c>.</exception>
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    public DbContextBuilder<T> UseCustomRandomEntityCreator(ICreateRandomEntities creator)
    {
        ThrowIfDisposed();

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
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    public DbContextBuilder<T> SeedWith<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : class
    {
        ThrowIfDisposed();

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
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    /// <param name="entities">The entities to populate the database with</param>
    /// <exception cref="ArgumentNullException">entities is null</exception>
    /// <exception cref="ArgumentException">entities contains a null item</exception>
    /// <exception cref="ArgumentException">entities contains a string</exception>
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    public DbContextBuilder<T> SeedWith<TEntity>(params TEntity[] entities)
        where TEntity : class
    {
        ThrowIfDisposed();

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
    /// Populates the specified DbSet with a single entity. Equivalent to calling the
    /// <c>params</c>-array overload with one element, but avoids the per-call allocation
    /// of a one-element array — useful in tests that seed many single rows.
    /// </summary>
    /// <param name="entity">The entity to populate the database with.</param>
    /// <returns>The builder, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entity"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="entity"/> is a <see cref="string"/> instance (matches the
    /// <c>params</c> overload's rejection regardless of how <typeparamref name="TEntity"/> was inferred).</exception>
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    public DbContextBuilder<T> SeedWith<TEntity>(TEntity entity)
        where TEntity : class
    {
        ThrowIfDisposed();

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
            // Buffer first so the failing call leaves `_seedData` untouched (atomic w.r.t.
            // seed state). IEnumerable<T> is covariant in T for reference types, so
            // List<string> casts to IEnumerable<object> at runtime and would slip through
            // without the per-item check below.
            var buffer = new List<object>();
            foreach (var item in sequence)
            {
                // Same per-item arms as the params overload: a null item used to be stored and
                // fail later, inside EF, with an unhelpful ArgumentNullException (#560).
                if (item is null)
                {
                    throw new ArgumentException("One of the entities is null", nameof(entity));
                }

                if (item is string)
                {
                    throw new ArgumentException("One of the entities passed in is of type string", nameof(entity));
                }

                buffer.Add(item);
            }

            _seedData.AddRange(buffer);
        }
        else
        {
            _seedData.Add(entity);
        }

        return this;
    }



    /// <summary>
    /// Populates the specified DbSet with random entities of type TEntity.
    /// </summary>
    /// <param name="count">The number of items to create</param>
    /// <typeparam name="TEntity">The type of entity to create</typeparam>
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">count is less than 1</exception>
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    public DbContextBuilder<T> SeedWithRandom<TEntity>(int count) where TEntity : class
    {
        ThrowIfDisposed();

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
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">count is less than 1</exception>
    /// <exception cref="ArgumentNullException"><paramref name="func"/> is <c>null</c>.</exception>
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    public DbContextBuilder<T> SeedWithRandom<TEntity>(int count, Func<TEntity, TEntity> func) where TEntity : class
    {
        ThrowIfDisposed();

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
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">count is less than 1</exception>
    /// <exception cref="ArgumentNullException"><paramref name="func"/> is <c>null</c>.</exception>
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    public DbContextBuilder<T> SeedWithRandom<TEntity>(int count, Func<TEntity, int, TEntity> func) where TEntity : class
    {
        ThrowIfDisposed();

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
    /// Creates a new instance of <typeparamref name="T"/> seeded with the configured data.
    /// </summary>
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
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    public T Build()
    {
        ThrowIfDisposed();

        var contextCreator = ResolveContextCreator();

        // Create a temporary context to initialize the database (via Effort's shared
        // connection) and persist seed data. Dispose it before returning the caller's
        // context — otherwise it leaks for the lifetime of the builder. The seed data
        // remains because EffortDbContextCreator holds the underlying connection open
        // independent of any one context.
        using (var seedContext = contextCreator.CreateDbContext<T>())
        {
            if (PrepareSeedContext(seedContext))
            {
                seedContext.SaveChanges();
            }
        }

        return contextCreator.CreateDbContext<T>();
    }



    /// <summary>
    /// Creates a new instance of <typeparamref name="T"/> seeded with the configured data
    /// asynchronously.
    /// </summary>
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
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    public async Task<T> BuildAsync()
    {
        ThrowIfDisposed();

        var contextCreator = ResolveContextCreator();

        // Same temporary seed context as Build; only the save is asynchronous.
        using (var seedContext = contextCreator.CreateDbContext<T>())
        {
            if (PrepareSeedContext(seedContext))
            {
                await seedContext.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        return contextCreator.CreateDbContext<T>();
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



    /// <summary>
    /// Disposes the context creator the builder owns, releasing its resources (for Effort, the
    /// in-memory connection and with it the database).
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }



    /// <summary>
    /// Releases the context creator. A derived class that adds resources overrides this and calls
    /// the base implementation.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> when called from <see cref="Dispose()"/>;
    /// <see langword="false"/> from a finalizer, when only unmanaged resources may be released.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            CreateDbContext?.Dispose();
        }

        _disposed = true;
    }
}
