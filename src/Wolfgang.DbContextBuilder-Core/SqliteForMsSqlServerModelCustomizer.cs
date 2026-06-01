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
    /// <param name="dependencies">The dependencies for the model customizer.</param>
    /// <remarks>
    /// This class should not be created directly but rather added to the service collection by
    /// calling the UseSqliteForMsSqlServerModelCustomizer method on the DbContextBuilder class
    /// </remarks>
    public SqliteForMsSqlServerModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies)
    {
        DefaultValueMap.Add("(newid())", "lower(hex(randomblob(16)))");
        DefaultValueMap.Add("(getdate())", "datetime('now')");

        // OverrideDefaultValueHandling returns null on a dictionary miss — intentional and
        // a deliberate divergence from the base SqliteModelCustomizer (which returns the
        // original defaultValue on miss). Rationale: an un-mapped SQL Server SQL function
        // (e.g. NEWSEQUENTIALID(), SYSUTCDATETIME(), or a user-defined function) would
        // succeed at model compile time on SQLite but fail at runtime when EF emits the
        // default-value SQL into the CREATE TABLE statement. Returning null drops the
        // default, letting SQLite use its column-type default — safer than passing through
        // a function SQLite doesn't recognize. Callers who need a specific mapping should
        // add it to DefaultValueMap.
        OverrideDefaultValueHandling = s =>
        {
            if (s == null)
            {
                return s;
            }

            return DefaultValueMap.TryGetValue(s, out var mappedValue)
                ? mappedValue
                : null;
        };

        // OverrideComputedValueHandling strips ALL computed values. SQLite supports
        // computed columns but only with a constrained SQL grammar; SQL-Server computed
        // expressions almost always reference functions SQLite doesn't have. Dropping
        // them is safer than passing them through — the column becomes a normal column,
        // and tests can seed an explicit value if they need one.
        OverrideComputedValueHandling = _ => null;
    }
}
