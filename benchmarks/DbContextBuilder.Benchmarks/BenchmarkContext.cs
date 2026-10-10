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



/// <summary>
/// Dependent of <see cref="BenchmarkEntity"/> with a required foreign key, used by the
/// foreign-key reconciliation scenario.
/// </summary>
public class BenchmarkChild
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Required foreign key to <see cref="BenchmarkEntity"/>.</summary>
    public int ParentId { get; set; }

    /// <summary>Navigation; virtual so AutoFixture leaves it unset and only the key is reconciled.</summary>
    public virtual BenchmarkEntity? Parent { get; set; }
}



/// <summary>
/// Context with <see cref="BenchmarkEntity"/> principals and <see cref="BenchmarkChild"/>
/// dependents. Separate from <see cref="BenchmarkContext"/> so the existing series keep
/// measuring the same one-entity model.
/// </summary>
public sealed class BenchmarkForeignKeyContext(DbContextOptions<BenchmarkForeignKeyContext> options)
    : DbContext(options)
{
    /// <summary>The principal set.</summary>
    public DbSet<BenchmarkEntity> Entities => Set<BenchmarkEntity>();

    /// <summary>The dependent set.</summary>
    public DbSet<BenchmarkChild> Children => Set<BenchmarkChild>();
}
