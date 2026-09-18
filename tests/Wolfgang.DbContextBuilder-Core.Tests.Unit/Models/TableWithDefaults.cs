using System.Diagnostics.CodeAnalysis;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit.Models;

// Test-only EF entity POCO — properties are populated by EF Core / the
// DbContextBuilder seeding paths via reflection. R# cannot see external
// reflection consumers so it reports the setters as never used.
// ReSharper disable UnusedAutoPropertyAccessor.Global
//
// Compiled here and linked (<Compile Link>) into the AutoFixture and EF6-EF10
// variant test projects; those only ever set Id, so the instrumented test
// assemblies score this POCO at 0-33%. No logic to cover - excluded.
[ExcludeFromCodeCoverage]
internal class TableWithDefaults
{
    public int Id { get; set; }
    public DateTime ModifiedDate { get; set; }
    public Guid Rowguid { get; set; }
}