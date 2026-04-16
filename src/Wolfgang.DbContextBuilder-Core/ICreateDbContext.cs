using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore;

/// <summary>
/// Defines the contract for creating instances of <see cref="DbContext"/>.
/// </summary>
public interface ICreateDbContext
{
    /// <summary>
    /// Creates a new instance of the specified <typeparamref name="TDbContext"/> type.
    /// </summary>
    /// <param name="optionsBuilder">The options builder used to configure the DbContext.</param>
    /// <typeparam name="TDbContext">The type of DbContext to create.</typeparam>
    /// <returns>A task that resolves to a new instance of <typeparamref name="TDbContext"/>.</returns>
    Task<TDbContext> CreateDbContextAsync<TDbContext>(DbContextOptionsBuilder<TDbContext> optionsBuilder) where TDbContext : DbContext;
}
