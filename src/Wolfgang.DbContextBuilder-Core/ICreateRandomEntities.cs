namespace Wolfgang.DbContextBuilderCore;

/// <summary>
/// Provides an API to create random entities for seeding databases.
/// </summary>
public interface ICreateRandomEntities
{

    /// <summary>
    /// Creates the specified number of entities of type TEntity with random data.
    /// </summary>
    /// <param name="count">The number of entities to create</param>
    /// <typeparam name="TEntity">The type of entity to create</typeparam>
    /// <returns>An IEnumerable{TEntity}</returns>
    IEnumerable<TEntity> CreateRandomEntities<TEntity>(int count) where TEntity : class;

}