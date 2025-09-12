using AdventureWorks.Models;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Runs all the tests using the default values for DbContextBuilder
/// </summary>
public class TestsWithDefaults : DbContextBuilderTestsBase
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override DbContextBuilder<AdventureWorksDbContext> CreateDbContextBuilder()
    {
        return new DbContextBuilder<AdventureWorksDbContext>();
    }



    /// <summary>
    /// 
    /// </summary>
    protected override string ExpectedDatabaseProvider => "Microsoft.EntityFrameworkCore.InMemory";
}