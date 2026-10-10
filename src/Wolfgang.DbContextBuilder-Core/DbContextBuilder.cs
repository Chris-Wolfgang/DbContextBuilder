using System.Globalization;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Wolfgang.DbContextBuilderCore;

/// <summary>
/// Uses the Builder pattern to create instances of DbContext types seeded with specified data.
/// </summary>
/// <typeparam name="T">The concrete <see cref="DbContext"/> type to construct.</typeparam>
/// <remarks>
/// <para>
/// The first <see cref="BuildAsync"/> call creates and seeds the database; later calls return
/// another context over the same, already-seeded database. Add all seed data before the first
/// <see cref="BuildAsync"/> call. Selecting a different provider afterwards starts a new, empty
/// database that the next <see cref="BuildAsync"/> call seeds again.
/// </para>
/// <para>
/// When using the SQLite provider, the builder holds an open SQLite in-memory connection and
/// the EF Core service provider every context shares. Dispose the builder only after all
/// <see cref="DbContext"/> instances returned by <see cref="BuildAsync"/> are no longer in use,
/// as disposing the builder closes the shared connection and destroys the in-memory database.
/// </para>
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
    // The creator whose database BuildAsync has already created and seeded. Later builds on the
    // same creator neither re-create nor re-seed it (#559); selecting a new creator resets it.
    private ICreateDbContext? _seededCreator;



    internal ServiceCollection ServiceCollection { get; } = [];



    internal ICreateDbContext? CreateDbContext { get; set; }



    // Built from ServiceCollection on the first BuildAsync and shared by every context after it,
    // so EF's singletons (model, caches) are built once per builder and disposed with it (#559).
    internal ServiceProvider? InternalServiceProvider { get; private set; }



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
    /// <returns>The builder, for chaining.</returns>
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    /// <remarks>
    /// Provider selection is last-write-wins — calling <see cref="UseInMemory"/> after a
    /// previous call to either <see cref="UseInMemory"/> or a SQLite extension overrides
    /// the earlier choice, including the EF Core services and model customizer the SQLite
    /// extension registered. Choose one provider per builder.
    /// </remarks>
    public DbContextBuilder<T> UseInMemory()
    {
        ThrowIfDisposed();

        // The only registrations in ServiceCollection are the SQLite provider's services and its
        // IModelCustomizer. Left in place, BuildAsync would build an internal service provider
        // holding only SQLite services for an InMemory context, and EF rejects it (#558).
        ServiceCollection.Clear();
        SetCreateDbContext(new InMemoryDbContextCreator());
        return this;
    }



    // Replaces the active DbContext creator, disposing the previous one if it owns resources
    // (e.g. the SQLite creator holds an open in-memory connection). Provider selection is
    // last-write-wins, so without this, re-selecting a provider on one builder would leak the
    // abandoned creator's connection.
    internal void SetCreateDbContext(ICreateDbContext creator)
    {
        ThrowIfDisposed();

        if (!ReferenceEquals(CreateDbContext, creator))
        {
            (CreateDbContext as IDisposable)?.Dispose();

            // A new provider means a new, empty database and (for SQLite) a changed
            // ServiceCollection: drop the services built for the old one and seed again.
            InternalServiceProvider?.Dispose();
            InternalServiceProvider = null;
            _seededCreator = null;
        }

        CreateDbContext = creator;
    }



    // Every configuration entry point calls this first (#563): after Dispose a builder must not
    // accept new state, and a SQLite creator created then could never be released, because a
    // second Dispose returns early.
    internal void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DbContextBuilder<T>));
        }
    }



    // The seeding methods call this: seed data added after the database was seeded would never
    // reach it, because later BuildAsync calls do not seed again (#559).
    private void ThrowIfSeeded()
    {
        if (_seededCreator is not null)
        {
            throw new InvalidOperationException
            (
                "Seed data must be added before the first BuildAsync() call: the database has already been " +
                "created and seeded. Add all seed data first, or use a new DbContextBuilder."
            );
        }
    }



    /// <summary>
    /// Allows the user to specify their own implementation of ICreateRandomEntities
    /// for creating random entities.
    /// </summary>
    /// <param name="creator">The creator to use</param>
    /// <returns>The builder, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="creator"/> is null.</exception>
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    public DbContextBuilder<T> UseCustomRandomEntityCreator(ICreateRandomEntities creator)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(creator);
        RandomEntityCreator = creator;
        return this;
    }



    /// <summary>
    /// Specifies a specific <see cref="DbContextOptionsBuilder{TContext}"/> instance to use when creating the DbContext.
    /// </summary>
    /// <param name="dbContextOptionsBuilder">The options builder to use when creating the DbContext.</param>
    /// <returns>The builder, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dbContextOptionsBuilder"/> is null.</exception>
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    public DbContextBuilder<T> UseDbContextOptionsBuilder(DbContextOptionsBuilder<T> dbContextOptionsBuilder)
    {
        ThrowIfDisposed();

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
    /// <returns>The builder, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="profile"/> is null.</exception>
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    public DbContextBuilder<T> UseSeedProfile(ISeedProfile<T> profile)
    {
        ThrowIfDisposed();

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
    /// <returns>The builder, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="writeLine"/> is null.</exception>
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    public DbContextBuilder<T> UseDiagnosticOutput(Action<string> writeLine)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(writeLine);

        _diagnosticOutput = writeLine;

        return this;
    }



    /// <summary>
    /// Populates the specified DbSet with the provided entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities to seed.</typeparam>
    /// <param name="entities">The entities to populate the database with</param>
    /// <returns>The builder, for chaining.</returns>
    /// <exception cref="ArgumentNullException">entities is null</exception>
    /// <exception cref="ArgumentException">entities contains a null item</exception>
    /// <exception cref="ArgumentException">entities contains a string</exception>
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    /// <exception cref="InvalidOperationException">A previous <see cref="BuildAsync"/> call has already created and seeded the database.</exception>
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
        ThrowIfDisposed();
        ThrowIfSeeded();

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
    /// <typeparam name="TEntity">The type of the entities to seed.</typeparam>
    /// <param name="entities">The entities to populate the database with</param>
    /// <returns>The builder, for chaining.</returns>
    /// <exception cref="ArgumentNullException">entities is null</exception>
    /// <exception cref="ArgumentException">entities contains a null item</exception>
    /// <exception cref="ArgumentException">entities contains a string</exception>
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    /// <exception cref="InvalidOperationException">A previous <see cref="BuildAsync"/> call has already created and seeded the database.</exception>
    public DbContextBuilder<T> SeedWith<TEntity>(params TEntity[] entities)
        where TEntity : class
    {
        ThrowIfDisposed();
        ThrowIfSeeded();

        ArgumentNullException.ThrowIfNull(entities);
        AddSeedItems(entities, nameof(entities));
        return this;
    }



    /// <summary>
    /// Populates the specified DbSet with a single entity. Equivalent to calling the
    /// <c>params</c>-array overload with one element, but avoids the per-call allocation
    /// of a one-element array — useful in tests that seed many single rows.
    /// </summary>
    /// <remarks>
    /// A <see cref="List{T}"/> or array argument binds to this overload, not to
    /// <see cref="SeedWith{TEntity}(IEnumerable{TEntity})"/>, because <typeparamref name="TEntity"/>
    /// is inferred as the collection type. When <paramref name="entity"/> is a sequence of
    /// objects, each of its items is seeded instead of the sequence itself.
    /// </remarks>
    /// <typeparam name="TEntity">The type of the entity to seed.</typeparam>
    /// <param name="entity">The entity, or sequence of entities, to populate the database with.</param>
    /// <returns>The builder, for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="entity"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="entity"/> is a <see cref="string"/> instance (matches the
    /// <c>params</c> overload's rejection regardless of how <typeparamref name="TEntity"/> was inferred), or
    /// <paramref name="entity"/> is a sequence that contains a null or a <see cref="string"/> item.</exception>
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    /// <exception cref="InvalidOperationException">A previous <see cref="BuildAsync"/> call has already created and seeded the database.</exception>
    public DbContextBuilder<T> SeedWith<TEntity>(TEntity entity)
        where TEntity : class
    {
        ThrowIfDisposed();
        ThrowIfSeeded();

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
                // Same per-item arms as AddSeedItems: a null item used to be stored and fail
                // later, inside EF, with an unhelpful ArgumentNullException (#560).
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
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    /// <exception cref="InvalidOperationException">No random-entity provider is configured (call <c>UseAutoFixture()</c>, <c>UseBogus()</c> or <see cref="UseCustomRandomEntityCreator"/> first), or a previous <see cref="BuildAsync"/> call has already created and seeded the database.</exception>
    /// <remarks>
    /// Foreign keys on the generated entities are reconciled against the model when the
    /// context is built: a required FK is wired to a seeded principal of its type (so seed the
    /// principals too), and an optional FK with no seeded principal is set to <c>null</c>. The
    /// FK values on a randomly-seeded entity are therefore not the raw random values produced
    /// by the creator. Entities added via <c>SeedWith</c> are never reconciled.
    /// A single-property integer primary key is kept unique across all seeded entities of the
    /// type: a random key that collides with another (random or <c>SeedWith</c>) key is replaced
    /// with the lowest unused value. A key that a <c>SeedWith</c> entity sets is never changed. A
    /// key left unset (at the property's sentinel, 0 unless configured otherwise) on a key EF
    /// generates is assigned the lowest unused value instead, by either method, as EF would have
    /// generated one for it.
    /// </remarks>
    /// <param name="count">The number of items to create</param>
    /// <typeparam name="TEntity">The type of entity to create</typeparam>
    /// <returns>The builder, for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">count is less than 1</exception>
    public DbContextBuilder<T> SeedWithRandom<TEntity>(int count) where TEntity : class
    {
        ThrowIfDisposed();
        ThrowIfSeeded();

        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than 0");
        }

        return AddRandomlySeeded
        (
            GetRandomEntityCreator()
                .CreateRandomEntities<TEntity>(count)
        );
    }



    /// <summary>
    /// Populates the specified DbSet with random entities of type TEntity.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    /// <exception cref="InvalidOperationException">No random-entity provider is configured (call <c>UseAutoFixture()</c>, <c>UseBogus()</c> or <see cref="UseCustomRandomEntityCreator"/> first), or a previous <see cref="BuildAsync"/> call has already created and seeded the database.</exception>
    /// <remarks>
    /// Foreign keys on the generated entities are reconciled against the model when the
    /// context is built: a required FK is wired to a seeded principal of its type (so seed the
    /// principals too), and an optional FK with no seeded principal is set to <c>null</c>. The
    /// FK values on a randomly-seeded entity are therefore not the raw random values produced
    /// by the creator. Entities added via <c>SeedWith</c> are never reconciled.
    /// A single-property integer primary key is kept unique across all seeded entities of the
    /// type: a random key that collides with another (random or <c>SeedWith</c>) key is replaced
    /// with the lowest unused value. A key that a <c>SeedWith</c> entity sets is never changed. A
    /// key left unset (at the property's sentinel, 0 unless configured otherwise) on a key EF
    /// generates is assigned the lowest unused value instead, by either method, as EF would have
    /// generated one for it.
    /// </remarks>
    /// <param name="count">The number of items to create</param>
    /// <param name="func">A function that takes a TEntity and returns an updated TEntity</param>
    /// <typeparam name="TEntity">The type of entity to create</typeparam>
    /// <returns>The builder, for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">count is less than 1</exception>
    /// <exception cref="ArgumentNullException"><paramref name="func"/> is null.</exception>
    public DbContextBuilder<T> SeedWithRandom<TEntity>(int count, Func<TEntity, TEntity> func) where TEntity : class
    {
        ThrowIfDisposed();
        ThrowIfSeeded();

        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than 0");
        }

        ArgumentNullException.ThrowIfNull(func);

        return AddRandomlySeeded
        (
            GetRandomEntityCreator()
                .CreateRandomEntities<TEntity>(count)
                .Select(func)
        );
    }



    /// <summary>
    /// Populates the specified DbSet with random entities of type TEntity.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    /// <exception cref="InvalidOperationException">No random-entity provider is configured (call <c>UseAutoFixture()</c>, <c>UseBogus()</c> or <see cref="UseCustomRandomEntityCreator"/> first), or a previous <see cref="BuildAsync"/> call has already created and seeded the database.</exception>
    /// <remarks>
    /// Foreign keys on the generated entities are reconciled against the model when the
    /// context is built: a required FK is wired to a seeded principal of its type (so seed the
    /// principals too), and an optional FK with no seeded principal is set to <c>null</c>. The
    /// FK values on a randomly-seeded entity are therefore not the raw random values produced
    /// by the creator. Entities added via <c>SeedWith</c> are never reconciled.
    /// A single-property integer primary key is kept unique across all seeded entities of the
    /// type: a random key that collides with another (random or <c>SeedWith</c>) key is replaced
    /// with the lowest unused value. A key that a <c>SeedWith</c> entity sets is never changed. A
    /// key left unset (at the property's sentinel, 0 unless configured otherwise) on a key EF
    /// generates is assigned the lowest unused value instead, by either method, as EF would have
    /// generated one for it.
    /// </remarks>
    /// <param name="count">The number of items to create</param>
    /// <param name="func">A function that takes a TEntity and the index number of the entity and returns an updated TEntity</param>
    /// <typeparam name="TEntity">The type of entity to create</typeparam>
    /// <returns>The builder, for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">count is less than 1</exception>
    /// <exception cref="ArgumentNullException"><paramref name="func"/> is null.</exception>
    public DbContextBuilder<T> SeedWithRandom<TEntity>(int count, Func<TEntity, int, TEntity> func) where TEntity : class
    {
        ThrowIfDisposed();
        ThrowIfSeeded();

        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than 0");
        }

        ArgumentNullException.ThrowIfNull(func);

        return AddRandomlySeeded
        (
            GetRandomEntityCreator()
                .CreateRandomEntities<TEntity>(count)
                .Select(func)
        );
    }



    // Shared by the three SeedWithRandom overloads (#573). Materialises once — a creator may return
    // a lazy sequence, and the func overloads add a lazy Select — and records the entities as
    // randomly seeded so their keys and foreign keys are reconciled at build time.
    private DbContextBuilder<T> AddRandomlySeeded<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : class
    {
        var materialized = entities as IReadOnlyList<TEntity> ?? entities.ToList();
        _seedData.AddRange(materialized);
        foreach (var entity in materialized)
        {
            _randomlySeeded.Add(entity);
        }

        return this;
    }



    private static readonly HashSet<Type> IntegralKeyTypes = [typeof(int), typeof(long), typeof(short), typeof(byte)];



    /// <summary>
    /// Makes single-property integer primary keys unique across the seeded entities sharing each
    /// key, changing only randomly-seeded entities. Every type in an inheritance hierarchy shares
    /// the root's key, so a base and a derived entity are made unique against each other. A random creator fills keys like any other integer
    /// (Bogus draws from 1..100,000), so two entities can share a key and EF refuses to track them
    /// (#515). A random key that is still free is kept; one that collides with a key already taken,
    /// by a <c>SeedWith</c> entity or an earlier random one, gets the lowest unused value. On a key
    /// EF generates, the property's sentinel (0 unless configured with <c>HasSentinel</c> on EF
    /// Core 8+) means "not set": EF would generate a value for it, and that value could collide with
    /// a key assigned here (#530). So every entity in the group left at the sentinel, random or
    /// <c>SeedWith</c>, gets the lowest unused value too, as EF would have given it. The sentinel is
    /// never assigned, since EF would read it as "not set" again.
    /// Runs before <see cref="ReconcileRandomForeignKeys"/>, so foreign keys copy the final keys.
    /// </summary>
    /// <param name="context">A context whose model is used to find each type's primary key.</param>
    private void EnsureUniqueRandomPrimaryKeys(DbContext context)
    {
        if (_randomlySeeded.Count == 0)
        {
            return;
        }

        // FindPrimaryKey on a derived type returns the root's key, so grouping by its property
        // puts a whole hierarchy in one group: EF tracks identity per root, not per CLR type.
        var keyedGroups = _seedData
            .Select(entity => (Entity: entity, Key: FindIntegralPrimaryKey(context, entity.GetType())))
            .Where(item => item.Key is not null)
            .GroupBy(item => item.Key!, item => item.Entity);

        foreach (var entities in keyedGroups)
        {
            var keyProperty = entities.Key.PropertyInfo!;
            // On a key EF never generates there is no "not set" value: every value is a real key.
            long? sentinel = entities.Key.ValueGenerated == ValueGenerated.Never ? null : ReadSentinel(entities.Key);
            bool IsUnset(object entity) => ReadKey(keyProperty, entity) == sentinel;

            // Keys given via SeedWith are fixed; random keys yield to them.
            var used = entities
                .Where(entity => !_randomlySeeded.Contains(entity) && !IsUnset(entity))
                .Select(entity => ReadKey(keyProperty, entity))
                .ToHashSet();

            // Never hand out the sentinel: EF would treat that key as unset and generate over it.
            if (sentinel is { } reserved)
            {
                used.Add(reserved);
            }

            long candidate = 1;
            foreach (var entity in entities)
            {
                // An unset key is always assigned. A set SeedWith key is fixed (it is already in
                // `used`); a random entity whose key is still free claims it, and only a colliding
                // one is renumbered.
                if (!IsUnset(entity) && (!_randomlySeeded.Contains(entity) || used.Add(ReadKey(keyProperty, entity))))
                {
                    continue;
                }

                while (!used.Add(candidate))
                {
                    candidate++;
                }

                keyProperty.SetValue(entity, Convert.ChangeType(candidate, keyProperty.PropertyType, CultureInfo.InvariantCulture));
            }
        }
    }



    private static IProperty? FindIntegralPrimaryKey(DbContext context, Type clrType)
    {
        var keyProperties = context.Model.FindEntityType(clrType)?.FindPrimaryKey()?.Properties;
        var property = keyProperties is { Count: 1 } ? keyProperties[0] : null;
        return property?.PropertyInfo is not null && IntegralKeyTypes.Contains(property.PropertyInfo.PropertyType) ? property : null;
    }



    // EF Core 8 added per-property sentinels (HasSentinel). Before that the CLR default of the
    // key's type was the only "not set" value (0 for the integer keys handled here).
    private static long ReadSentinel(IProperty key) =>
#if EF_CORE_6 || EF_CORE_7
        Convert.ToInt64(Activator.CreateInstance(key.ClrType), CultureInfo.InvariantCulture);
#else
        Convert.ToInt64(key.Sentinel, CultureInfo.InvariantCulture);
#endif



    private static long ReadKey(PropertyInfo keyProperty, object entity) =>
        Convert.ToInt64(keyProperty.GetValue(entity), CultureInfo.InvariantCulture);



    /// <summary>
    /// Reconciles foreign keys on randomly-seeded entities against the model so that the
    /// random FK values produced by the random-entity creator do not violate referential
    /// constraints (which matters for providers that enforce them, e.g. SQLite). For each
    /// foreign key on a randomly-seeded dependent: if a different seeded entity of the
    /// principal type is present, the dependent's FK is set to that principal's key; otherwise
    /// an optional FK is set to <c>null</c>. Required FKs with no available principal are left
    /// untouched (best effort). Entities added via <c>SeedWith</c> are never modified.
    /// Only foreign keys exposed as CLR properties are reconciled; shadow foreign keys (tracked
    /// by EF without a CLR property) are left as-is, since there is no property to set.
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
        // Seeded entities are grouped by runtime type, so match every group whose type IS the
        // principal type, not just the exact one: an inheritance base principal (TPH/TPT) whose
        // seeded instances are of a derived type was never found (#569).
        var principalType = foreignKey.PrincipalEntityType.ClrType;

        // Prefer a principal that is not the dependent itself (handles self-referencing FKs).
        return seededByType
            .Where(group => principalType.IsAssignableFrom(group.Key))
            .SelectMany(group => group.Value)
            .FirstOrDefault(candidate => !ReferenceEquals(candidate, dependent));
    }



    /// <summary>
    /// Creates a new instance of <typeparamref name="T"/> seeded with the specified data.
    /// </summary>
    /// <remarks>
    /// The first call creates the database and saves the seed data. Later calls return a new
    /// context over the same database without seeding it again.
    /// </remarks>
    /// <returns>A new instance of <typeparamref name="T"/>.</returns>
    /// <exception cref="ObjectDisposedException">The builder has been disposed.</exception>
    /// <exception cref="InvalidOperationException">The database could not be created for the configured
    /// provider; EF Core's failure is the <see cref="Exception.InnerException"/>.</exception>
    public async Task<T> BuildAsync()
    {
        ThrowIfDisposed();

        var optionBuilder = _dbContextOptionsBuilder ?? new DbContextOptionsBuilder<T>();
        if (ServiceCollection.Count > 0)
        {
            InternalServiceProvider ??= ServiceCollection.BuildServiceProvider();
            optionBuilder.UseInternalServiceProvider(InternalServiceProvider);
        }

        if (_diagnosticOutput is not null)
        {
            optionBuilder.LogTo(_diagnosticOutput);
        }

        var contextCreator = CreateDbContext ??= new InMemoryDbContextCreator();
        if (ReferenceEquals(_seededCreator, contextCreator))
        {
            _diagnosticOutput?.Invoke
            (
                $"DbContextBuilder<{typeof(T).Name}> built; database already seeded, no rows added."
            );
        }
        else
        {
            await CreateAndSeedDatabaseAsync(contextCreator, optionBuilder).ConfigureAwait(false);
            _seededCreator = contextCreator;

            _diagnosticOutput?.Invoke
            (
                $"DbContextBuilder<{typeof(T).Name}> built; seeded {_seedData.Count} entity row(s)."
            );
        }

        return await contextCreator.CreateDbContextAsync(optionBuilder).ConfigureAwait(false);
    }



    // Creates the database through a temporary context and saves the seed data, then disposes it.
    private async Task CreateAndSeedDatabaseAsync(ICreateDbContext contextCreator, DbContextOptionsBuilder<T> optionBuilder)
    {
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
                    "To capture EF Core's log of the failure, call UseDiagnosticOutput(...) before " +
                    "BuildAsync(). To include entity values in that log, pass a " +
                    "DbContextOptionsBuilder<T> with .EnableSensitiveDataLogging() to " +
                    "UseDbContextOptionsBuilder(...).";
                throw new InvalidOperationException(msg, e);
            }

            if (_seedData.Count > 0)
            {
                EnsureUniqueRandomPrimaryKeys(seedContext);
                ReconcileRandomForeignKeys(seedContext);
                seedContext.AddRange(_seedData.AsEnumerable());
                await seedContext.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }



    /// <summary>
    /// Disposes the underlying database context creator and the EF Core service provider the
    /// builder's contexts share, releasing any held resources (e.g., the SQLite in-memory connection).
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }



    /// <summary>
    /// Releases the context creator and the EF Core service provider the builder holds. A derived
    /// class that adds resources overrides this and calls the base implementation.
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
            (CreateDbContext as IDisposable)?.Dispose();
            InternalServiceProvider?.Dispose();
        }

        _disposed = true;
    }
}