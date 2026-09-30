using System.Globalization;
using Wolfgang.DbContextBuilderCore;

namespace Wolfgang.DbContextBuilder.Samples.ShadowWorkload;

/// <summary>
/// Deterministic <see cref="ICreateRandomEntities"/> for the <c>SeedWithRandom</c> scenario.
/// It lives in the sample so the workload exercises <c>UseCustomRandomEntityCreator</c> and
/// <c>SeedWithRandom</c> without baselining a second package (.Bogus or .AutoFixture). Values
/// are fixed, not random, so baseline and current runs measure identical work.
/// </summary>
public sealed class SampleRandomEntityCreator : ICreateRandomEntities
{
    public IEnumerable<TEntity> CreateRandomEntities<TEntity>(int count) where TEntity : class
    {
        if (typeof(TEntity) != typeof(Product))
        {
            throw new NotSupportedException($"The shadow workload only seeds {nameof(Product)}.");
        }

        var products = new List<TEntity>(count);
        for (var i = 0; i < count; i++)
        {
            products.Add((TEntity)(object)new Product { Name = $"Random {i.ToString(CultureInfo.InvariantCulture)}", Price = 1m + i });
        }

        return products;
    }
}



/// <summary>
/// A reusable seed profile for the <c>UseSeedProfile</c> scenario. It builds fresh
/// <see cref="Product"/> instances on every <see cref="Apply"/>: reusing instances would
/// carry the keys EF assigned on an earlier build into the next one, so later iterations
/// would do different work from the first.
/// </summary>
public sealed class ShopSeedProfile : ISeedProfile<ShopDbContext>
{
    private readonly int _count;



    public ShopSeedProfile(int count) => _count = count;



    public void Apply(DbContextBuilder<ShopDbContext> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var products = new Product[_count];
        for (var i = 0; i < _count; i++)
        {
            products[i] = new Product { Name = $"Profile {i.ToString(CultureInfo.InvariantCulture)}", Price = 5m + i };
        }

        builder.SeedWith(products);
    }
}
