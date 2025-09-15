using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
    private string? _dumpTableNamesCommandText;
    private Action<IReadOnlyCollection<string>>? _dumpTableNamesCallback;


    internal IGenerateRandomEntities RandomEntityGenerator { get; private set; } = new AutoFixtureRandomEntityGenerator();



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
    /// When specified, tells the context builder to dump the names of all tables in the database
    /// </summary>
    /// <param name="commandText">The command to use to get the tables. Varies by database provider</param>
    /// <param name="callback">A function to receive the results after the tables names are retrieved</param>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    /// <exception cref="ArgumentException">commandText is either null or whitespace</exception>
    /// <exception cref="ArgumentNullException">callback is null</exception>
    [UsedImplicitly]
    // TODO Move this to a configuration object that gets passed into the UseInMemory and UseSqlite methods
    public DbContextBuilder<T> DumpTablesNames(string commandText, Action<IReadOnlyCollection<string>> callback)
    {
        // TODO write test methods
        ArgumentNullException.ThrowIfNull(callback);

        if (string.IsNullOrWhiteSpace(commandText))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(commandText));
        }

        _dumpTableNamesCommandText = commandText;
        _dumpTableNamesCallback = callback;

        return this;
    }



    private static async Task<IReadOnlyCollection<string>> LogTableNamesAsync(DbConnection connection, string commandText)
    {
        var command = connection.CreateCommand();
        command.CommandText = commandText;
        await using var reader = await command.ExecuteReaderAsync();

        if (reader.FieldCount != 2)
        {
            throw new InvalidOperationException
            (
                "Select statement must return exactly two columns. The first must be the schema name and the second the table name"
            );
        }

        var tables = new List<string>();
        while (await reader.ReadAsync())
        {
            var value = reader.GetValue(0);
            var schemaName = value == DBNull.Value ? null : value.ToString();

            value = reader.GetValue(1);
            var tableName = value == DBNull.Value ? null : value.ToString();

            var fullName = string.IsNullOrWhiteSpace(schemaName)
                ? $"[{tableName}]"
                : $"[{schemaName}].[{tableName}]";

            tables.Add(fullName);
        }
        return tables;
    }




    /// <summary>
    /// Creates a new instance of T seeded with specified data."/>.
    /// </summary>
    /// <returns>instance of {T}</returns>
    /// <exception cref="NotSupportedException">The specified database provider is not supported</exception>
    public async Task<T> BuildAsync()
    {
        // ReSharper disable once TooWideLocalVariableScope
        DbConnection? connection; // variable must remain in outer scope

        DbContextOptionsBuilder<T>? optionBuilder;

        switch (_dbProvider)
        {
            case DbProvider.InMemory:
                optionBuilder = new DbContextOptionsBuilder<T>().UseInMemoryDatabase(Guid.NewGuid().ToString());
                break;
            case DbProvider.Sqlite:
                connection = new SqliteConnection("DataSource=:memory:");
                await connection.OpenAsync();
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
            .LogTo(Console.WriteLine)
            //.LogTo( s => Debug.WriteLine(s)) // TODO Figure out logging
            //.LogTo(logs.Add)
            .LogTo(Console.WriteLine, LogLevel.Information)
            .ConfigureWarnings(builder => builder.Throw())
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        var context = (T)Activator.CreateInstance(typeof(T), options)!;

        try
        {
            // Create a context to initialize and seed the database
            await context.Database.EnsureCreatedAsync();
        }
        catch (InvalidOperationException e)
        {
            // TODO Improve exception
            var ex = new InvalidOperationException("Failed to create database. See InnerExceptions and Logs in the Data property for additional information", e);
            //ex.Data.Add("Logs", logs);
            throw ex;
        }

        // TODO Need tests
        if (_dumpTableNamesCommandText is not null && _dumpTableNamesCallback is not null)
        {
            var tableNames = await LogTableNamesAsync(context.Database.GetDbConnection(), _dumpTableNamesCommandText);
            _dumpTableNamesCallback(tableNames);
        }

        if (_seedData.Count > 0)
        {
            context.AddRange(_seedData.AsEnumerable());
            await context.SaveChangesAsync();
        }

        // TODO Need tests
        if (_dumpTableNamesCommandText is not null && _dumpTableNamesCallback is not null)
        {
            var tableNames = await LogTableNamesAsync(context.Database.GetDbConnection(), _dumpTableNamesCommandText);
            _dumpTableNamesCallback(tableNames);
        }


        // Create a new clean context instance to return
        return (T)Activator.CreateInstance(typeof(T), options)!;
    }




}