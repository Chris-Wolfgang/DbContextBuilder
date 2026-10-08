using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Tests that <c>SeedWithRandom</c> reconciles foreign keys on the random entities so they
/// satisfy referential constraints (verified against SQLite, which enforces them).
/// </summary>
public class ForeignKeyAutoWireTests
{
    /// <summary>
    /// Verifies that a required FK on a randomly-seeded dependent is wired to a seeded
    /// principal, and an optional FK with no seeded principal is nulled — so the build does
    /// not violate SQLite's foreign-key constraints.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_wires_required_fk_to_seeded_principal_and_nulls_optional_fk()
    {
        using var sut = new DbContextBuilder<FactoryContext>().UseSqlite().UseAutoFixture();

        await using var context = await sut
            .SeedWithRandom<Manufacturer>(2)
            .SeedWithRandom<Widget>(3)
            .BuildAsync();

        var manufacturerIds = context.Manufacturers.Select(m => m.Id).ToHashSet();
        var widgets = context.Widgets.ToList();

        Assert.Equal(3, widgets.Count);
        Assert.All(widgets, widget => Assert.Contains(widget.ManufacturerId, manufacturerIds));
        Assert.All(widgets, widget => Assert.Null(widget.SupplierId));
    }



    /// <summary>
    /// Verifies that entities added with <c>SeedWith</c> are never reconciled — their
    /// explicit foreign-key values are preserved exactly.
    /// </summary>
    [Fact]
    public async Task SeedWith_explicit_entities_are_left_untouched()
    {
        var manufacturer = new Manufacturer { Id = 100, Name = "Acme" };
        var widget = new Widget { Id = 1, Name = "Cog", ManufacturerId = 100, SupplierId = 55 };

        using var sut = new DbContextBuilder<FactoryContext>().UseInMemory();

        await using var context = await sut
            .SeedWith(manufacturer)
            .SeedWith(widget)
            .BuildAsync();

        var stored = context.Widgets.Single();
        Assert.Equal(100, stored.ManufacturerId);
        Assert.Equal(55, stored.SupplierId);
    }



    /// <summary>
    /// Verifies a widget seeded with both of its principals round-trips through SQLite, which
    /// enforces the foreign keys, and that both navigations load back to the seeded principals.
    /// </summary>
    [Fact]
    public async Task SeedWith_a_widget_and_both_principals_round_trips_both_navigations()
    {
        using var sut = new DbContextBuilder<FactoryContext>().UseSqlite();

        await using var context = await sut
            .SeedWith(new Manufacturer { Id = 1, Name = "Acme" })
            .SeedWith(new Supplier { Id = 2, Name = "Bolts Ltd" })
            .SeedWith(new Widget { Id = 3, Name = "Cog", ManufacturerId = 1, SupplierId = 2 })
            .BuildAsync();

        var widget = context.Widgets
            .Include(w => w.Manufacturer)
            .Include(w => w.Supplier)
            .Single();
        var supplier = context.Suppliers.Single();

        Assert.Equal("Acme", widget.Manufacturer?.Name);
        Assert.Equal("Bolts Ltd", widget.Supplier?.Name);
        Assert.Equal(2, supplier.Id);
    }
}



internal class Manufacturer
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}



internal class Supplier
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}



internal class Widget
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int ManufacturerId { get; set; }

    // Virtual so the AutoFixture creator ignores it (leaving only the scalar FK random).
    public virtual Manufacturer? Manufacturer { get; set; }

    public int? SupplierId { get; set; }

    public virtual Supplier? Supplier { get; set; }
}



internal class FactoryContext(DbContextOptions<FactoryContext> options) : DbContext(options)
{
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();

    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<Widget> Widgets => Set<Widget>();
}
