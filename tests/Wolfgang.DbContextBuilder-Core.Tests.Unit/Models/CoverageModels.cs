using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

// Shared by the Core suites (SeedWithRandomCoverageTests) and the AutoFixture suite
// (ForeignKeyAutoWireTests), which link this file (#602).



internal sealed class CoverageManufacturer
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}



internal sealed class CoverageSupplier
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}



// Test-only EF entity POCO — properties are populated by the random-entity
// creator via reflection. R# cannot see reflection consumers and reports the
// scalar FK setters as unused.
// ReSharper disable UnusedAutoPropertyAccessor.Global
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



internal sealed class CoverageContext(DbContextOptions<CoverageContext> options) : DbContext(options)
{
    public DbSet<CoverageManufacturer> Manufacturers => Set<CoverageManufacturer>();

    public DbSet<CoverageSupplier> Suppliers => Set<CoverageSupplier>();

    public DbSet<CoverageWidget> Widgets => Set<CoverageWidget>();
}
