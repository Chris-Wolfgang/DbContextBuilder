using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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
    // Entities added via SeedWithRandom (reference identity). Their foreign keys are
    // reconciled against the model at build time so random FK values don't violate
    // constraints; explicitly-SeedWith'd entities are never touched.
    private readonly HashSet<object> _randomlySeeded = new(ReferenceEqualityComparer.Instance);
    private DbContextOptionsBuilder<T>? _dbContextOptionsBuilder;
    private Action<string>? _diagnosticOutput;



    internal ServiceCollection ServiceCollection { get; } = [];



    internal ICreateDbContext? CreateDbContext { get; set; }



    // No default provider: random-entity generation lives in add-on packages
    // (Wolfgang.DbContextBuilder.AutoFixture / .Bogus). SeedWithRandom throws a clear
    // exception until a provider is configured. See GetRandomEntityCreator.
    internal ICreateRandomEntities? RandomEntityCreator { get; private set; }



    // Resolves the configured random-entity provider or throws a clear, actionable error.
    // Called by every SeedWithRandom overload before generating entities.
    private ICreateRandomEntities GetRandomEntityCreator() =>
        RandomEntityCreator ?? throw new InvalidOperationException
        (
            "SeedWithRandom requires a random-entity provider, but none is configured. " +
            "Call UseAutoFixture() (add the Wolfgang.DbContextBuilder.AutoFixture package), " +
            "UseBogus() (add the Wolfgang.DbContextBuilder.Bogus package), or " +
            "UseCustomRandomEntityCreator(...) before SeedWithRandom."
        );



    /// <summary>
    /// Instructs the builder to use InMemory as the database provider.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    /// <remarks>
    /// Provider selection is last-write-wins — calling <see cref="UseInMemory"/> after a
    /// previous call to either <see cref="UseInMemory"/> or a SQLite extension overrides
    /// the earlier choice. Choose one provider per builder.
    /// </remarks>
    public DbContextBuilder<T> UseInMemory()
    {
        SetCreateDbContext(new InMemoryDbContextCreator());
        return this;
    }



    // Replaces the active DbContext creator, disposing the previous one if it owns resources
    // (e.g. the SQLite creator holds an open in-memory connection). Provider selection is
    // last-write-wins, so without this, re-selecting a provider on one builder would leak the
    // abandoned creator's connection.
    internal void SetCreateDbContext(ICreateDbContext creator)
    {
        if (!ReferenceEquals(CreateDbContext, creator) && CreateDbContext is IDisposable previous)
        {
            previous.Dispose();
        }

        CreateDbContext = creator;
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
    /// Applies a reusable <see cref="ISeedProfile{T}"/> to this builder. Profiles bundle
    /// a complete set of seed data so the same setup can be shared across many tests with
    /// a single call. Multiple profiles can be applied; their seed data accumulates.
    /// </summary>
    /// <param name="profile">The seed profile to apply.</param>
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="profile"/> is null.</exception>
    public DbContextBuilder<T> UseSeedProfile(ISeedProfile<T> profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        profile.Apply(this);

        return this;
    }



    /// <summary>
    /// Routes diagnostic output to <paramref name="writeLine"/>: EF Core logs (including the
    /// generated SQL) produced while creating and seeding the database, plus a one-line summary
    /// of how many entity rows were seeded. Pass your test framework's output sink — for example
    /// <c>UseDiagnosticOutput(testOutputHelper.WriteLine)</c> in xUnit — so the seeded context is
    /// visible in the test log when an assertion fails.
    /// </summary>
    /// <param name="writeLine">Receives each diagnostic line.</param>
    /// <returns><see cref="DbContextBuilder{T}"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="writeLine"/> is null.</exception>
    public DbContextBuilder<T> UseDiagnosticOutput(Action<string> writeLine)
    {
        ArgumentNullException.ThrowIfNull(writeLine);

        _diagnosticOutput = writeLine;

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
    /// <remarks>
    /// Insertion order across distinct entity types is not guaranteed — the builder
    /// accumulates seeds in a single list and EF's <c>SaveChangesAsync</c> orders the
    /// inserts by FK dependency, not by the order <c>SeedWith</c> calls were made. For
    /// scenarios where the order of two same-type rows matters (e.g. identity-generated
    /// keys), pass them in the desired order within a single <c>SeedWith</c> call.
    /// Inheritance mapping (TPH / TPT / TPC) is supported via <c>entity.GetType()</c>;
    /// the runtime type determines which DbSet receives the row.
    /// </remarks>
    public DbContextBuilder<T> SeedWith<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entities);

        if (typeof(TEntity) == typeof(string))
        {
            throw new ArgumentException("The type of TEntity cannot be string", nameof(entities));
        }

        // Iterate directly rather than materializing to an array (saves one allocation
        // vs delegating to the params overload via ToArray). nameof(entities) keeps
        // the public parameter name on any thrown ArgumentException.
        AddSeedItems(entities, nameof(entities));
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
    public DbContextBuilder<T> SeedWith<TEntity>(params TEntity[] entities)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(entities);
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
    /// Shared validation + add loop for both <see cref="SeedWith{TEntity}(IEnumerable{TEntity})"/>
    /// and <see cref="SeedWith{TEntity}(TEntity[])"/>. Keeps the null/string/IEnumerable
    /// arms in one place so the two overloads cannot drift. <paramref name="paramName"/>
    /// is forwarded to <see cref="ArgumentException.ParamName"/> so the public overload's
    /// argument name is preserved on failure.
    /// </summary>
    /// <exception cref="ArgumentException">An element of <paramref name="source"/> is null or a string.</exception>
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
                    _seedData.AddRange(e);
                    break;
                default:
                    _seedData.Add(entity);
                    break;
            }
        }
    }



    /// <summary>
    /// Populates the specified DbSet with random entities of type TEntity.
    /// </summary>
    /// <remarks>
    /// Foreign keys on the generated entities are reconciled against the model when the
    /// context is built: a required FK is wired to a seeded principal of its type (so seed the
    /// principals too), and an optional FK with no seeded principal is set to <c>null</c>. The
    /// FK values on a randomly-seeded entity are therefore not the raw random values produced
    /// by the creator. Entities added via <c>SeedWith</c> are never reconciled.
    /// </remarks>
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

        var entities = GetRandomEntityCreator()
            .CreateRandomEntities<TEntity>(count);

        // Materialise once (the func overloads use a lazy Select) and record the entities
        // as randomly seeded so their foreign keys are reconciled at build time.
        var materialized = entities as IReadOnlyList<TEntity> ?? entities.ToList();
        _seedData.AddRange(materialized);
        foreach (var entity in materialized)
        {
            _randomlySeeded.Add(entity);
        }

        return this;
    }



    /// <summary>
    /// Populates the specified DbSet with random entities of type TEntity.
    /// </summary>
    /// <remarks>
    /// Foreign keys on the generated entities are reconciled against the model when the
    /// context is built: a required FK is wired to a seeded principal of its type (so seed the
    /// principals too), and an optional FK with no seeded principal is set to <c>null</c>. The
    /// FK values on a randomly-seeded entity are therefore not the raw random values produced
    /// by the creator. Entities added via <c>SeedWith</c> are never reconciled.
    /// </remarks>
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

        var entities = GetRandomEntityCreator()
            .CreateRandomEntities<TEntity>(count)
            .Select(func);

        // Materialise once (the func overloads use a lazy Select) and record the entities
        // as randomly seeded so their foreign keys are reconciled at build time.
        var materialized = entities as IReadOnlyList<TEntity> ?? entities.ToList();
        _seedData.AddRange(materialized);
        foreach (var entity in materialized)
        {
            _randomlySeeded.Add(entity);
        }

        return this;
    }



    /// <summary>
    /// Populates the specified DbSet with random entities of type TEntity.
    /// </summary>
    /// <remarks>
    /// Foreign keys on the generated entities are reconciled against the model when the
    /// context is built: a required FK is wired to a seeded principal of its type (so seed the
    /// principals too), and an optional FK with no seeded principal is set to <c>null</c>. The
    /// FK values on a randomly-seeded entity are therefore not the raw random values produced
    /// by the creator. Entities added via <c>SeedWith</c> are never reconciled.
    /// </remarks>
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

        var entities = GetRandomEntityCreator()
            .CreateRandomEntities<TEntity>(count)
            .Select(func);

        // Materialise once (the func overloads use a lazy Select) and record the entities
        // as randomly seeded so their foreign keys are reconciled at build time.
        var materialized = entities as IReadOnlyList<TEntity> ?? entities.ToList();
        _seedData.AddRange(materialized);
        foreach (var entity in materialized)
        {
            _randomlySeeded.Add(entity);
        }

        return this;
    }



    /// <summary>
    /// Reconciles foreign keys on randomly-seeded entities against the model so that the
    /// random FK values produced by the random-entity creator do not violate referential
    /// constraints (which matters for providers that enforce them, e.g. SQLite). For each
    /// foreign key on a randomly-seeded dependent: if a different seeded entity of the
    /// principal type is present, the dependent's FK is set to that principal's key; otherwise
    /// an optional FK is set to <c>null</c>. Required FKs with no available principal are left
    /// untouched (best effort). Entities added via <c>SeedWith</c> are never modified.
    /// </summary>
    /// <param name="context">A context whose model is used to resolve the relationships.</param>
    private void ReconcileRandomForeignKeys(DbContext context)
    {
        if (_randomlySeeded.Count == 0)
        {
            return;
        }

        var seededByType = new Dictionary<Type, List<object>>();
        foreach (var entity in _seedData)
        {
            var type = entity.GetType();
            if (!seededByType.TryGetValue(type, out var list))
            {
                list = [];
                seededByType[type] = list;
            }

            list.Add(entity);
        }

        foreach (var dependent in _randomlySeeded)
        {
            var entityType = context.Model.FindEntityType(dependent.GetType());
            if (entityType is null)
            {
                continue;
            }

            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                ReconcileForeignKey(dependent, foreignKey, seededByType);
            }
        }
    }



    private static void ReconcileForeignKey
    (
        object dependent,
        IForeignKey foreignKey,
        IReadOnlyDictionary<Type, List<object>> seededByType
    )
    {
        var principal = FindPrincipal(dependent, foreignKey, seededByType);

        if (principal is not null)
        {
            // Point the dependent's FK properties at the chosen principal's key values.
            var fkProperties = foreignKey.Properties;
            var principalKeyProperties = foreignKey.PrincipalKey.Properties;
            for (var i = 0; i < fkProperties.Count; i++)
            {
                var keyValue = principalKeyProperties[i].PropertyInfo?.GetValue(principal);
                fkProperties[i].PropertyInfo?.SetValue(dependent, keyValue);
            }
        }
        else if (!foreignKey.IsRequired)
        {
            // Optional relationship with no principal to point at — clear the random value.
            foreach (var property in foreignKey.Properties)
            {
                property.PropertyInfo?.SetValue(dependent, value: null);
            }
        }
    }



    private static object? FindPrincipal
    (
        object dependent,
        IForeignKey foreignKey,
        IReadOnlyDictionary<Type, List<object>> seededByType
    )
    {
        if (!seededByType.TryGetValue(foreignKey.PrincipalEntityType.ClrType, out var candidates))
        {
            return null;
        }

        // Prefer a principal that is not the dependent itself (handles self-referencing FKs).
        foreach (var candidate in candidates)
        {
            if (!ReferenceEquals(candidate, dependent))
            {
                return candidate;
            }
        }

        return null;
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

        if (_diagnosticOutput is not null)
        {
            optionBuilder.LogTo(_diagnosticOutput);
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
                ReconcileRandomForeignKeys(seedContext);
                seedContext.AddRange(_seedData.AsEnumerable());
                await seedContext.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        _diagnosticOutput?.Invoke
        (
            $"DbContextBuilder<{typeof(T).Name}> built; seeded {_seedData.Count} entity row(s)."
        );

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