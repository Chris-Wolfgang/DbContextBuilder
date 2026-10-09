using System.Data.Common;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;

namespace Wolfgang.DbContextBuilderEF6.Tests.Unit.Models;

/// <summary>Counts the calls to its SaveChanges and SaveChangesAsync overrides, across instances (#531).</summary>
public class SaveCountingContext : DbContext
{
    public SaveCountingContext(DbConnection connection, bool contextOwnsConnection)
        : base(connection, contextOwnsConnection)
    {
    }



    public static int Saves { get; set; }



    public virtual DbSet<Category> Categories { get; set; } = null!;



    public override int SaveChanges()
    {
        Saves++;
        return base.SaveChanges();
    }



    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saves++;
        return base.SaveChangesAsync(cancellationToken);
    }
}
