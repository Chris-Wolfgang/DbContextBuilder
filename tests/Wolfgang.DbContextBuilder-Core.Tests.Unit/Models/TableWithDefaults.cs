namespace Wolfgang.DbContextBuilderCore.Tests.Unit.Models;

// Test-only EF entity POCO — properties are populated by EF Core / the
// DbContextBuilder seeding paths via reflection. R# cannot see external
// reflection consumers so it reports the setters as never used.
// ReSharper disable UnusedAutoPropertyAccessor.Global
internal class TableWithDefaults
{
    public int Id { get; set; }
    public DateTime ModifiedDate { get; set; }
    public Guid Rowguid { get; set; }
}