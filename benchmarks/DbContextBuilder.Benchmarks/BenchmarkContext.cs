using Microsoft.EntityFrameworkCore;

namespace Wolfgang.DbContextBuilderCore.Benchmarks;

/// <summary>
/// Minimal entity used by the benchmark scenarios. Kept deliberately small
/// (one scalar property beyond the id) so the per-row cost reflects
/// DbContextBuilder overhead rather than entity-shape overhead.
/// </summary>
public sealed class BenchmarkEntity
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>A representative scalar property.</summary>
    public string Name { get; set; } = string.Empty;
}



/// <summary>
/// Minimal <see cref="DbContext"/> used by the benchmark scenarios. One
/// DbSet of <see cref="BenchmarkEntity"/> is enough to exercise the full
/// build + seed + SaveChanges path without dragging in the cost of a
/// realistic multi-entity schema.
/// </summary>
public sealed class BenchmarkContext(DbContextOptions<BenchmarkContext> options)
    : DbContext(options)
{
    /// <summary>The entity set populated by benchmark seed scenarios.</summary>
    public DbSet<BenchmarkEntity> Entities => Set<BenchmarkEntity>();
}
