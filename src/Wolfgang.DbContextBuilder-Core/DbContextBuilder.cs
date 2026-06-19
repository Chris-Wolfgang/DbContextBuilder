using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Wolfgang.DbContextBuilderCore;

/// <summary>
/// Uses the Builder pattern to create instances of DbContext types seeded with specified data.
/// </summary>
/// <typeparam name="T">The concrete <see cref="DbContext"/> type to construct.</typeparam>
/// <remarks>
/// When using the SQLite provider, the builder holds an open SQLite in-memory connection.
/// Dispose the builder only after all <see cref="DbContext"/> instances returned by
/// <see cref="BuildAsync"/> are no longer in use, as disposing the builder closes the
/// shared connection and destroys the in-memory database.
/// </remarks>
public class DbContextBuilder<T> : IDisposable where T : DbContext
{
    private bool _disposed;
    private readonly List<object> _seedData = [];
    private DbContextOptionsBuilder<T>? _dbContextOptionsBuilder;



    internal ServiceCollection ServiceCollection { get; } = [];



    internal ICreateDbContext? CreateDbContext { get; set; }



    internal ICreateRandomEntities RandomEntityCreator { get; private set; } = new AutoFixtureRandomEntityCreator();



    /// <summary>
    /// Instructs the builder to use InMemory as the database provider.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
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
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    public DbContextBuilder<T> UseCustomRandomEntityCreator(ICreateRandomEntities creator)
    {
        ArgumentNullException.ThrowIfNull(creator);
        RandomEntityCreator = creator;
        return this;
    }



    /// <summary>
    /// Specifies a specific <see cref="DbContextOptionsBuilder{TContext}"/> instance to use when creating the DbContext.
    /// </summary>
    /// <param name="dbContextOptionsBuilder">The options builder to use when creating the DbContext.</param>
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="dbContextOptionsBuilder"/> is null.</exception>
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
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    /// <exception cref="ArgumentNullException">entities is null</exception>
    /// <exception cref="ArgumentException">entities contains a null item</exception>
    /// <exception cref="ArgumentException">entities contains a string</exception>
    public DbContextBuilder<T> SeedWith<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entities);

        if (typeof(TEntity) == typeof(string))
        {
            throw new ArgumentException("The type of TEntity cannot be string", nameof(entities));
        }

        var entityArray = entities as TEntity[] ?? entities.ToArray();
        return SeedWith(entityArray);
    }



    /// <summary>
    /// Populates the specified DbSet with the provided entities.
    /// </summary>
    /// <param name="entities">The entities to populate the database with</param>
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    /// <exception cref="ArgumentNullException">entities is null</exception>
    /// <exception cref="ArgumentException">entities contains a null item</exception>
    /// <exception cref="ArgumentException">entities contains a string</exception>
    public DbContextBuilder<T> SeedWith<TEntity>(params TEntity[] entities)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entities);

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
    public DbContextBuilder<T> SeedWith<TEntity>(TEntity entity)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        // Reject by runtime type, not just TEntity, so `SeedWith<object>("...")` is still
        // caught (matches the params overload's `case string:` arm).
        if (entity is string)
        {
            throw new ArgumentException("One of the entities passed in is of type string", nameof(entity));
        }

        if (entity is IEnumerable<object> sequence)
        {
            // Walk the sequence so a List<string> (or any IEnumerable wrapping strings) is
            // rejected element-by-element, matching the per-item check the params overload
            // performs. Buffer first so the failing call leaves `_seedData` untouched
            // (atomic w.r.t. seed state).
            var buffer = new List<object>();
            foreach (var item in sequence)
            {
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
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">count is less than 1</exception>
    public DbContextBuilder<T> SeedWithRandom<TEntity>(int count, Func<TEntity, TEntity> func) where TEntity : class
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than 0");
        }

        ArgumentNullException.ThrowIfNull(func);

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
    public DbContextBuilder<T> SeedWithRandom<TEntity>(int count, Func<TEntity, int, TEntity> func) where TEntity : class
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than 0");
        }

        ArgumentNullException.ThrowIfNull(func);

        var entities = RandomEntityCreator
            .CreateRandomEntities<TEntity>(count)
            .Select(func);

        _seedData.AddRange(entities);

        return this;
    }



    /// <summary>
    /// Creates a new instance of <typeparamref name="T"/> seeded with the specified data.
    /// </summary>
    /// <returns>A new instance of <typeparamref name="T"/>.</returns>
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    public async Task<T> BuildAsync()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DbContextBuilder<T>));
        }

        var optionBuilder = _dbContextOptionsBuilder ?? new DbContextOptionsBuilder<T>();
        if (ServiceCollection.Count > 0)
        {
            var provider = ServiceCollection.BuildServiceProvider();
            optionBuilder.UseInternalServiceProvider(provider);
        }

        var contextCreator = CreateDbContext ??= new InMemoryDbContextCreator();

        // Create a temporary context to initialize and seed the database, then dispose it
        var seedContext = await contextCreator.CreateDbContextAsync(optionBuilder).ConfigureAwait(false);
        await using (seedContext.ConfigureAwait(false))
        {
            try
            {
                await seedContext.Database.EnsureCreatedAsync().ConfigureAwait(false);
            }
            catch (InvalidOperationException e)
            {
                const string msg =
                    "DbContextBuilder failed to create the in-memory database for the " +
                    "configured DbContext. See InnerException for the EF Core failure details. " +
                    "Common causes: no database provider has been configured (InMemory is used " +
                    "by default, so this usually indicates a custom ICreateDbContext returned a " +
                    "context with no provider); the configured provider cannot model one of the " +
                    "DbContext's entities; or a required EF service has not been registered. " +
                    "If you need to capture EF Core diagnostic logs to investigate, build a " +
                    "DbContextOptionsBuilder<T> with .LogTo(...) or .EnableSensitiveDataLogging() " +
                    "yourself and pass it to UseDbContextOptionsBuilder(...) before calling " +
                    "BuildAsync().";
                throw new InvalidOperationException(msg, e);
            }

            if (_seedData.Count > 0)
            {
                seedContext.AddRange(_seedData.AsEnumerable());
                await seedContext.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        return await contextCreator.CreateDbContextAsync(optionBuilder).ConfigureAwait(false);
    }



    /// <summary>
    /// Disposes the underlying database context creator, releasing any held resources
    /// (e.g., the SQLite in-memory connection).
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }



    /// <summary>
    /// Releases unmanaged and optionally managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing && CreateDbContext is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _disposed = true;
    }
}