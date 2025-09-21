using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Wolfgang.DbContextBuilderCore;

/// <summary>
/// An implementation of SqliteModelCustomizer specifically configured to handle a DbContext
/// designed for Microsoft SQL Server databases. 
/// </summary>
/// <remarks>
/// This class includes overrides to map common default value SQL functions from SQL Server to
/// their SQLite equivalents.
/// </remarks>
public class SqliteForMsSqlServerModelCustomizer : SqliteModelCustomizer
{
    /// <summary>
    /// Creates a new instance of the <see cref="SqliteForMsSqlServerModelCustomizer"/> class.
    /// </summary>
    /// <param name="dependencies"></param>
    /// <remarks>
    /// This class should not be created directly but rather added to the service collection by
    /// calling the UseSqliteForMsSqlServerModelCustomizer method on the DbContextBuilder class
    /// </remarks>
    public SqliteForMsSqlServerModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies)
    {
        DefaultValueMap.Add("(newid())", "lower(hex(randomblob(16)))");
        DefaultValueMap.Add("(getdate())", "datetime('now')");
    }
}
