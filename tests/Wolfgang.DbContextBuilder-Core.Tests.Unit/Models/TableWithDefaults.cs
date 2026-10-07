using System;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit.Models;

// Test-only EF entity POCO — properties are populated by EF Core / the
// DbContextBuilder seeding paths via reflection. R# cannot see external
// reflection consumers so it reports the setters as never used.
// ReSharper disable UnusedAutoPropertyAccessor.Global
//
// Compiled here and linked (<Compile Link>) into the AutoFixture and EF6-EF10
// variant test projects. TestModelRoundTripTests (linked alongside) exercises
// every column in each of them, so the POCO is measured, not excluded.
internal class TableWithDefaults
{
    public int Id { get; set; }
    public DateTime ModifiedDate { get; set; }
    public Guid Rowguid { get; set; }
}