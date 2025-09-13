using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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


    internal IGenerateRandomEntities RandomEntityGenerator { get; private set; } = new AutoFixtureRandomEntityGenerator();



    /// <summary>
    /// Creates a new instance of T seeded with specified data."/>.
    /// </summary>
    /// <returns>instance of {T}</returns>
    [Obsolete($"Use {nameof(BuildAsync)}")]
    public T Build()
	{
        var options = _dbProvider switch
        {
            DbProvider.InMemory => new DbContextOptionsBuilder<T>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options,
            DbProvider.Sqlite => new DbContextOptionsBuilder<T>()
                .UseSqlite("DataSource=:memory:")
                .Options,
            _ => throw new NotSupportedException($"Provider {this._dbProvider} is not supported.")
        };

        // Create a context to initialize and seed the database
        var context = (T)Activator.CreateInstance(typeof(T), options)!;
        context.Database.EnsureCreated();


        if (_seedData.Count > 0)
        {
            context.AddRange(_seedData.AsEnumerable());
            context.SaveChanges();
        }

        // Create a new clean context instance to return
        return (T)Activator.CreateInstance(typeof(T), options)!;
    }



    /// <summary>
    /// Creates a new instance of T seeded with specified data."/>.
    /// </summary>
    /// <returns>instance of {T}</returns>
    /// <exception cref="NotSupportedException">The specified database provider is not supported</exception>
    public async Task<T> BuildAsync()
    {
        DbConnection? connection;
        DbContextOptionsBuilder<T>? optionBuilder;

        switch (_dbProvider)
        {
            case DbProvider.InMemory:
                optionBuilder = new DbContextOptionsBuilder<T>().UseInMemoryDatabase(Guid.NewGuid().ToString());
                break;
            case DbProvider.Sqlite:
                connection = new SqliteConnection("DataSource=:memory:");
                optionBuilder = new DbContextOptionsBuilder<T>().UseSqlite(connection);
                break;
            default:
                throw new NotSupportedException($"Provider {this._dbProvider} is not supported.");
        }

        if (_serviceProvider != null)
        {
            optionBuilder.UseInternalServiceProvider(_serviceProvider);
        }

        var logs = new List<string>();

        // TODO add UseVerboseOutput option to log SQL to console
        var options = optionBuilder
            //.LogTo(Console.WriteLine)
            .LogTo( s => Debug.WriteLine(s)) // TODO Figure out logging
            .ConfigureWarnings(builder => builder.Throw())
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        await using var context = (T)Activator.CreateInstance(typeof(T), options)!;

        await context.Database.OpenConnectionAsync();
        try
        {
            // Create a context to initialize and seed the database
            await context.Database.EnsureCreatedAsync();
        }
        catch (Exception e)
        {
            var ex = new Exception("Failed to create database. See InnerExceptions and Logs in the Data property for additional information", e);
            ex.Data.Add("Logs", logs);
            throw ex;
        }

        await LogTableNamesAsync(context);
        
        if (_seedData.Count > 0)
        {
            context.AddRange(_seedData.AsEnumerable());
            await context.SaveChangesAsync();
        }


        // Create a new clean context instance to return
        var context2 =  (T)Activator.CreateInstance(typeof(T), options)!;
        //await context2.Database.OpenConnectionAsync();

        await LogTableNamesAsync(context2);

        return context2;
    }



    private static async Task LogTableNamesAsync(T context)
    {
        var command = new SqliteCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";
            command.Connection = context.Database.GetDbConnection() as SqliteConnection;

            await using var reader = await command.ExecuteReaderAsync();
            //var tables = new Collection<string>();
            while (reader.Read())
            {
                var tableName = reader.GetString(0);
                //tables.Add(tableName);
                Console.WriteLine(tableName);
            }

            await reader.CloseAsync();
    }



    /// <summary>
    /// Instructs the builder to use SQLite as the database provider.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    public DbContextBuilder<T> UseSqlite()
	{
		this._dbProvider = DbProvider.Sqlite;
		return this;
	}

    

    /// <summary>
    /// Instructs the builder to use InMemory as the database provider.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
	public DbContextBuilder<T> UseInMemory()
	{
		this._dbProvider = DbProvider.InMemory;
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
    /// Tell DbContextBuilder to use AutoFixture to create random entities.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    public DbContextBuilder<T> UseAutoFixture()
    {
        RandomEntityGenerator = new AutoFixtureRandomEntityGenerator();

        return this;
    }



    /// <summary>
    /// Provides a method to override how the database is created since different databases
    /// support different features
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    /// <remarks>
    /// This method allows you to intercept the creation of the database and alter or
    /// customize it. This is useful, for example, if your production database in SQL Server
    /// or Oracle and is using features that are not supported by the InMemory or Sqlite
    /// databases. Using this method and passing in your customizations allows you to
    /// alter or opt out altogether certain columns.
    ///
    /// Note: This does mean that the features that you change will not match your
    /// production system and so testing to features is pointless you can still
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
}