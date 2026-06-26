using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Exercises the <c>SeedWithRandom</c> / foreign-key-reconciliation paths of
/// <see cref="DbContextBuilder{T}"/> using a deterministic in-repo
/// <see cref="ICreateRandomEntities"/> double (no AutoFixture dependency, so no Core-assembly
/// type collision). This lives in the shared Core test source and is linked into every EF-version
/// wrapper, so it runs on net8.0+ runtimes — where the coverage collector works — and therefore
/// covers <c>DbContextBuilder&lt;T&gt;</c> for the net6.0/net7.0-targeted Core-EF6/EF7 assemblies,
/// whose AutoFixture-backed integration suite only runs on the net6.0/net7.0 diagonal (where the
/// collector emits nothing).
/// </summary>
public class SeedWithRandomCoverageTests
{
    private static DbContextBuilder<CoverageContext> NewBuilder() =>
        new DbContextBuilder<CoverageContext>().UseCustomRandomEntityCreator(new DeterministicRandomEntityCreator());



    /// <summary>
    /// Verifies SeedWithRandom seeds the requested number of rows via the configured provider.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_seeds_the_requested_count()
    {
        using var sut = NewBuilder().UseInMemory();

        await using var context = await sut
            .SeedWithRandom<CoverageManufacturer>(5)
            .BuildAsync();

        Assert.Equal(5, context.Manufacturers.Count());
    }



    /// <summary>
    /// Verifies the SeedWithRandom(int, Func) overload applies the transform to each entity.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_func_overload_applies_the_transform()
    {
        using var sut = NewBuilder().UseInMemory();

        await using var context = await sut
            .SeedWithRandom<CoverageManufacturer>(3, m => { m.Name = "transformed"; return m; })
            .BuildAsync();

        Assert.Equal(3, context.Manufacturers.Count());
        Assert.All(context.Manufacturers.ToList(), m => Assert.Equal("transformed", m.Name));
    }



    /// <summary>
    /// Verifies the SeedWithRandom(int, Func with index) overload applies the indexed transform.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_indexed_func_overload_applies_the_transform()
    {
        using var sut = NewBuilder().UseInMemory();

        await using var context = await sut
            .SeedWithRandom<CoverageManufacturer>(3, (m, i) => { m.Id = i + 100; return m; })
            .BuildAsync();

        var ids = context.Manufacturers.Select(m => m.Id).OrderBy(id => id).ToList();
        Assert.Equal(new[] { 100, 101, 102 }, ids);
    }



    /// <summary>
    /// Verifies that foreign keys on randomly-seeded entities are reconciled against the model:
    /// a required FK is wired to a seeded principal and an optional FK with no principal is nulled,
    /// so the build does not violate SQLite's foreign-key constraints.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_reconciles_required_and_optional_foreign_keys()
    {
        using var sut = NewBuilder().UseSqlite();

        await using var context = await sut
            .SeedWithRandom<CoverageManufacturer>(2)
            .SeedWithRandom<CoverageWidget>(3)
            .BuildAsync();

        var manufacturerIds = context.Manufacturers.Select(m => m.Id).ToHashSet();
        var widgets = context.Widgets.ToList();

        Assert.Equal(3, widgets.Count);
        Assert.All(widgets, w => Assert.Contains(w.ManufacturerId, manufacturerIds));
        Assert.All(widgets, w => Assert.Null(w.SupplierId));
    }



    /// <summary>
    /// Verifies the SqliteForMsSqlServer extension is exercised end-to-end with random seeding.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_works_with_UseSqliteForMsSqlServer()
    {
        using var sut = NewBuilder().UseSqliteForMsSqlServer();

        await using var context = await sut
            .SeedWithRandom<CoverageManufacturer>(2)
            .BuildAsync();

        Assert.Equal(2, context.Manufacturers.Count());
    }



    /// <summary>
    /// Verifies SeedWithRandom throws when no random-entity provider has been configured.
    /// </summary>
    [Fact]
    public void SeedWithRandom_when_no_provider_configured_throws_InvalidOperationException()
    {
        using var sut = new DbContextBuilder<CoverageContext>().UseInMemory();

        var ex = Assert.Throws<InvalidOperationException>(() => sut.SeedWithRandom<CoverageManufacturer>(1));
        Assert.Contains("UseAutoFixture", ex.Message, StringComparison.Ordinal);
    }



    /// <summary>
    /// Verifies all three SeedWithRandom overloads reject a count below one.
    /// </summary>
    [Fact]
    public void SeedWithRandom_when_count_is_less_than_one_throws_ArgumentOutOfRangeException()
    {
        using var sut = NewBuilder();

        Assert.Throws<ArgumentOutOfRangeException>(() => sut.SeedWithRandom<CoverageManufacturer>(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.SeedWithRandom<CoverageManufacturer>(0, m => m));
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.SeedWithRandom<CoverageManufacturer>(0, (m, _) => m));
    }



    /// <summary>
    /// Verifies SeedWith with an IEnumerable seeds the rows.
    /// </summary>
    [Fact]
    public async Task SeedWith_enumerable_seeds_the_rows()
    {
        var rows = new[] { new CoverageManufacturer { Id = 1, Name = "a" }, new CoverageManufacturer { Id = 2, Name = "b" } };
        using var sut = new DbContextBuilder<CoverageContext>().UseInMemory();

        await using var context = await sut.SeedWith(rows.AsEnumerable()).BuildAsync();

        Assert.Equal(2, context.Manufacturers.Count());
    }



    /// <summary>
    /// Verifies SeedWith rejects null, a string element type, and (params) a null item.
    /// </summary>
    [Fact]
    public void SeedWith_rejects_null_string_type_and_null_items()
    {
        using var sut = new DbContextBuilder<CoverageContext>();

        Assert.Throws<ArgumentNullException>(() => sut.SeedWith((IEnumerable<CoverageManufacturer>)null!));
        Assert.Throws<ArgumentException>(() => sut.SeedWith(new[] { "not an entity" }.AsEnumerable()));
        Assert.Throws<ArgumentNullException>(() => sut.SeedWith((CoverageManufacturer[])null!));
        Assert.Throws<ArgumentException>(() => sut.SeedWith(new CoverageManufacturer { Id = 1 }, null!));
    }



    /// <summary>
    /// Verifies the params overload seeds the rows.
    /// </summary>
    [Fact]
    public async Task SeedWith_params_seeds_the_rows()
    {
        using var sut = new DbContextBuilder<CoverageContext>().UseInMemory();

        await using var context = await sut
            .SeedWith(new CoverageManufacturer { Id = 1, Name = "a" }, new CoverageManufacturer { Id = 2, Name = "b" })
            .BuildAsync();

        Assert.Equal(2, context.Manufacturers.Count());
    }



    /// <summary>
    /// Verifies the singleton overload seeds one row and rejects null and string instances.
    /// </summary>
    [Fact]
    public async Task SeedWith_singleton_seeds_one_row_and_validates()
    {
        using var sut = new DbContextBuilder<CoverageContext>().UseInMemory();

        await using var context = await sut.SeedWith(new CoverageManufacturer { Id = 1, Name = "a" }).BuildAsync();
        Assert.Equal(1, context.Manufacturers.Count());

        using var other = new DbContextBuilder<CoverageContext>();
        Assert.Throws<ArgumentNullException>(() => other.SeedWith((CoverageManufacturer)null!));
        Assert.Throws<ArgumentException>(() => other.SeedWith<object>("a string"));
    }



    /// <summary>
    /// Verifies UseDbContextOptionsBuilder accepts a builder and rejects null.
    /// </summary>
    [Fact]
    public async Task UseDbContextOptionsBuilder_is_honored_and_rejects_null()
    {
        var options = new DbContextOptionsBuilder<CoverageContext>().UseInMemoryDatabase("explicit-options");
        using var sut = new DbContextBuilder<CoverageContext>().UseDbContextOptionsBuilder(options);

        await using var context = await sut.SeedWith(new CoverageManufacturer { Id = 1 }).BuildAsync();
        Assert.Equal(1, context.Manufacturers.Count());

        using var other = new DbContextBuilder<CoverageContext>();
        Assert.Throws<ArgumentNullException>(() => other.UseDbContextOptionsBuilder(null!));
    }



    /// <summary>
    /// Verifies selecting a SQLite provider more than once works (exercises the model-customizer
    /// de-duplication path that only runs when a SQLite extension is applied a second time).
    /// </summary>
    [Fact]
    public async Task Selecting_a_Sqlite_provider_twice_works()
    {
        using var sut = NewBuilder()
            .UseSqlite()
            .UseSqliteForMsSqlServer();

        await using var context = await sut
            .SeedWithRandom<CoverageManufacturer>(2)
            .BuildAsync();

        Assert.Equal(2, context.Manufacturers.Count());
    }



    /// <summary>
    /// Verifies BuildAsync throws after the builder is disposed, and Dispose is idempotent.
    /// </summary>
    [Fact]
    public async Task BuildAsync_after_Dispose_throws_ObjectDisposedException()
    {
        var sut = new DbContextBuilder<CoverageContext>().UseSqlite();
        sut.Dispose();
        sut.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => sut.BuildAsync());
    }
}



/// <summary>
/// A deterministic <see cref="ICreateRandomEntities"/> for coverage tests: fills public,
/// settable, non-virtual scalar properties with predictable values and leaves virtual
/// (navigation) and other reference-type members unset — mirroring what a real provider produces,
/// without depending on AutoFixture.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Test double")]
internal sealed class DeterministicRandomEntityCreator : ICreateRandomEntities
{
    private int _seq;

    public IEnumerable<TEntity> CreateRandomEntities<TEntity>(int count)
        where TEntity : class
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Value cannot be less than 1");
        }

        var list = new List<TEntity>(count);
        for (var i = 0; i < count; i++)
        {
            var entity = (TEntity)Activator.CreateInstance(typeof(TEntity))!;
            Fill(entity);
            list.Add(entity);
        }

        return list;
    }



    private void Fill(object entity)
    {
        foreach (var prop in entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var setter = prop.GetSetMethod();
            if (setter is null || setter.IsVirtual)
            {
                // Skip read-only and virtual (navigation) members.
                continue;
            }

            var value = ScalarValue(prop.PropertyType);
            if (value is not null)
            {
                prop.SetValue(entity, value);
            }
        }
    }



    private object? ScalarValue(Type type)
    {
        var t = Nullable.GetUnderlyingType(type) ?? type;
        var n = ++_seq;

        if (t == typeof(string)) return $"value-{n}";
        if (t == typeof(int)) return n;
        if (t == typeof(long)) return (long)n;
        if (t == typeof(short)) return (short)n;
        if (t == typeof(byte)) return (byte)(n % 256);
        if (t == typeof(decimal)) return (decimal)n;
        if (t == typeof(double)) return (double)n;
        if (t == typeof(float)) return (float)n;
        if (t == typeof(bool)) return true;
        if (t == typeof(Guid)) return new Guid(n, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        if (t == typeof(DateTime)) return new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(n);
        if (t.IsEnum) return Enum.GetValues(t).GetValue(0);

        // Reference/complex types (navigation properties) are left unset.
        return null;
    }
}



[ExcludeFromCodeCoverage(Justification = "Test model")]
internal sealed class CoverageManufacturer
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}



[ExcludeFromCodeCoverage(Justification = "Test model")]
internal sealed class CoverageSupplier
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}



[ExcludeFromCodeCoverage(Justification = "Test model")]
internal class CoverageWidget
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int ManufacturerId { get; set; }

    // Virtual so the random-entity double leaves it unset (only the scalar FK is populated).
    public virtual CoverageManufacturer? Manufacturer { get; set; }

    public int? SupplierId { get; set; }

    public virtual CoverageSupplier? Supplier { get; set; }
}



[ExcludeFromCodeCoverage(Justification = "Test model")]
internal sealed class CoverageContext(DbContextOptions<CoverageContext> options) : DbContext(options)
{
    public DbSet<CoverageManufacturer> Manufacturers => Set<CoverageManufacturer>();

    public DbSet<CoverageSupplier> Suppliers => Set<CoverageSupplier>();

    public DbSet<CoverageWidget> Widgets => Set<CoverageWidget>();
}
