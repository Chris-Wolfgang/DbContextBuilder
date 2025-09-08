using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Wolfgang.DbContextBuilder;

/// <summary>
/// Builder for creating DbContext instances with seeded data for testing purposes.
/// </summary>
/// <typeparam name="T">The DbContext type to build</typeparam>
public class DbContextBuilder<T> where T : DbContext
{
    private readonly List<object> _seedData = new();
    private DbConnection? _connection;
    private string? _databaseName;

    /// <summary>
    /// Seed the DbContext with specific data.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to seed</typeparam>
    /// <param name="entity">The entity instance to seed</param>
    /// <returns>The builder instance for method chaining</returns>
    public DbContextBuilder<T> SeedWith<TEntity>(TEntity entity) where TEntity : class
    {
        this._seedData.Add(entity ?? throw new ArgumentNullException(nameof(entity)));
        return this;
    }

    /// <summary>
    /// Seed the DbContext with random data.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to seed</typeparam>
    /// <param name="count">Number of random entities to create</param>
    /// <returns>The builder instance for method chaining</returns>
    public DbContextBuilder<T> SeedWithRandom<TEntity>(int count) where TEntity : class, new()
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than 0");
        
        var random = new Random();
        for (int i = 0; i < count; i++)
        {
            var entity = new TEntity();
            
            // Try to set Id property if it exists
            var idProperty = typeof(TEntity).GetProperty("Id");
            if (idProperty != null && idProperty.CanWrite)
            {
                if (idProperty.PropertyType == typeof(int))
                {
                    idProperty.SetValue(entity, random.Next(1000, 999999));
                }
                else if (idProperty.PropertyType == typeof(string))
                {
                    idProperty.SetValue(entity, Guid.NewGuid().ToString());
                }
            }
            
            this._seedData.Add(entity);
        }
        return this;
    }

    /// <summary>
    /// Use a custom database connection.
    /// </summary>
    /// <param name="connection">The database connection to use</param>
    /// <returns>The builder instance for method chaining</returns>
    public DbContextBuilder<T> UseConnection(DbConnection connection)
    {
        this._connection = connection ?? throw new ArgumentNullException(nameof(connection));
        return this;
    }

    /// <summary>
    /// Use a custom database name for SQLite in-memory database.
    /// </summary>
    /// <param name="databaseName">The database name</param>
    /// <returns>The builder instance for method chaining</returns>
    public DbContextBuilder<T> UseDatabaseName(string databaseName)
    {
        this._databaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
        return this;
    }

    /// <summary>
    /// Build the DbContext instance with seeded data.
    /// </summary>
    /// <returns>The configured DbContext instance</returns>
    public T Build()
    {
        // Create the DbContext instance using reflection
        var contextType = typeof(T);
        var constructor = contextType.GetConstructor(new[] { typeof(DbContextOptions<T>) });
        
        if (constructor == null)
        {
            throw new InvalidOperationException($"DbContext type {contextType.Name} must have a constructor that accepts DbContextOptions<{contextType.Name}>");
        }

        // Configure the DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<T>();
        
        if (this._connection != null)
        {
            optionsBuilder.UseSqlite(this._connection);
        }
        else
        {
            // Use Entity Framework InMemory provider for testing
            optionsBuilder.UseInMemoryDatabase(this._databaseName ?? Guid.NewGuid().ToString());
        }

        var options = optionsBuilder.Options;
        var context = (T)constructor.Invoke(new object[] { options });

        // Ensure database is created
        context.Database.EnsureCreated();

        // Seed data
        foreach (var entity in this._seedData)
        {
            context.Add(entity);
        }

        context.SaveChanges();

        return context;
    }
}
