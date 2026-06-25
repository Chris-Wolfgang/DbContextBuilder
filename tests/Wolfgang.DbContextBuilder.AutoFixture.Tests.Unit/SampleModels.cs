using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// A small standalone entity used to exercise <see cref="ICreateRandomEntities"/> implementations
/// without taking a dependency on the AdventureWorks model.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Test model")]
public class SampleEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Quantity { get; set; }
}



[ExcludeFromCodeCoverage(Justification = "Test model")]
public class SampleDbContext(DbContextOptions<SampleDbContext> options) : DbContext(options)
{
    public DbSet<SampleEntity> Samples => Set<SampleEntity>();
}
