using System.Data.Common;
using System.Data.Entity;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Wolfgang.DbContextBuilderEF6.Tests.Unit.Models;

/// <summary>
/// Counts the calls to its SaveChanges and SaveChangesAsync overrides, across instances (#531), into
/// the counter the current test installed with <see cref="StartCounting"/>.
/// </summary>
public class SaveCountingContext : DbContext
{
    public SaveCountingContext(DbConnection connection, bool contextOwnsConnection)
        : base(connection, contextOwnsConnection)
    {
    }



    // Per test, not process-wide (#597): AsyncLocal flows each test's counter only into the calls on
    // that test's execution context, so parallel tests cannot reset or bump each other's count.
    private static readonly AsyncLocal<StrongBox<int>?> _saves = new AsyncLocal<StrongBox<int>?>();



    /// <summary>Installs a fresh counter for the current test and returns it.</summary>
    public static StrongBox<int> StartCounting()
    {
        var saves = new StrongBox<int>();
        _saves.Value = saves;
        return saves;
    }



    public virtual DbSet<Category> Categories { get; set; } = null!;



    public override int SaveChanges()
    {
        _saves.Value!.Value++;
        return base.SaveChanges();
    }



    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        _saves.Value!.Value++;
        return base.SaveChangesAsync(cancellationToken);
    }
}
