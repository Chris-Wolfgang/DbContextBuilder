using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore;

/// <summary>
/// A reusable, named bundle of seed data that can be applied to any
/// <see cref="DbContextBuilder{T}"/> in a single call via
/// <see cref="DbContextBuilder{T}.UseSeedProfile(ISeedProfile{T})"/>.
/// </summary>
/// <typeparam name="T">The <see cref="DbContext"/> type the profile seeds.</typeparam>
/// <remarks>
/// Seed profiles encapsulate a complete set of seed entities so the same setup can be
/// shared across many test classes instead of being re-built in each one. Implement
/// <see cref="Apply"/> to call <c>SeedWith</c> one or more times on the supplied builder.
/// </remarks>
/// <example>
/// <code>
/// public sealed class StandardCatalogue : ISeedProfile&lt;ShopDbContext&gt;
/// {
///     public void Apply(DbContextBuilder&lt;ShopDbContext&gt; builder) =>
///         builder.SeedWith(
///             new Product { Name = "Widget" },
///             new Product { Name = "Gadget" });
/// }
///
/// await using var context = await new DbContextBuilder&lt;ShopDbContext&gt;()
///     .UseInMemory()
///     .UseSeedProfile(new StandardCatalogue())
///     .BuildAsync();
/// </code>
/// </example>
public interface ISeedProfile<T> where T : DbContext
{
    /// <summary>
    /// Applies this profile's seed data to <paramref name="builder"/>, typically by
    /// calling <c>SeedWith</c> one or more times.
    /// </summary>
    /// <param name="builder">The builder to seed.</param>
    void Apply(DbContextBuilder<T> builder);
}
