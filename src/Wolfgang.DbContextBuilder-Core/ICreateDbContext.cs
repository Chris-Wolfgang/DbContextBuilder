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



    /// <summary>
    /// Creates a new instance of the specified <typeparamref name="TDbContext"/> type, observing
    /// <paramref name="cancellationToken"/>.
    /// </summary>
    /// <param name="optionsBuilder">The options builder used to configure the DbContext.</param>
    /// <param name="cancellationToken">Cancels the creation.</param>
    /// <typeparam name="TDbContext">The type of DbContext to create.</typeparam>
    /// <returns>A task that resolves to a new instance of <typeparamref name="TDbContext"/>.</returns>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was canceled.</exception>
    /// <remarks>
    /// The builder calls this overload. Its default implementation checks the token and then calls
    /// <see cref="CreateDbContextAsync{TDbContext}(DbContextOptionsBuilder{TDbContext})"/>, so
    /// existing implementations keep working unchanged; override it to pass the token on to
    /// asynchronous work of your own (opening a connection, say).
    /// </remarks>
    Task<TDbContext> CreateDbContextAsync<TDbContext>(DbContextOptionsBuilder<TDbContext> optionsBuilder, CancellationToken cancellationToken)
        where TDbContext : DbContext
    {
        cancellationToken.ThrowIfCancellationRequested();
        return CreateDbContextAsync(optionsBuilder);
    }
}
