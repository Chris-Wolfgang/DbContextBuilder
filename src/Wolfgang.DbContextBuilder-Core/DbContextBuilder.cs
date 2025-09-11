using Microsoft.EntityFrameworkCore;

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


	private readonly List<object> _seedData = new List<object>();


    /// <summary>
    /// Creates a new instance of T seeded with specified data."/>.
    /// </summary>
    /// <returns>instance of {T}</returns>
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
    public DbContextBuilder<T> SeedWith<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
    {
        if (entities == null)
        {
            throw new ArgumentNullException(nameof(entities));
        }
		_seedData.AddRange(entities);

        return this;
    }



    /// <summary>
    /// Populates the specified DbSet with the provided entities.
    /// </summary>
    /// <returns><see cref="DbContextBuilder{T}"></see></returns>
    /// <param name="entities">The entities to populate the database with</param>
    /// <exception cref="ArgumentNullException">entities is null</exception>
    public DbContextBuilder<T> SeedWith<TEntity>(params TEntity[] entities) where TEntity : class
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

}

