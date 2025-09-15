using AdventureWorks.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Runs all the tests using the default values for DbContextBuilder
/// </summary>
public class TestsWithSqliteAndAutoFixture(ITestOutputHelper testOutputHelper) : DbContextBuilderTestsBase(testOutputHelper)
{

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override DbContextBuilder<AdventureWorksDbContext> CreateDbContextBuilder()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlite()
            .AddSingleton<IModelCacheKeyFactory, SqliteModelCacheKeyFactory>()
            .AddSingleton<IModelCustomizer, SqliteModelCustomizer>()
            .BuildServiceProvider();

        return new DbContextBuilder<AdventureWorksDbContext>()
            .UseSqlite()
            .UseServiceProvider(serviceProvider)
            .UseAutoFixture();
    }


    /// <summary>
    /// The SQL statement to retrieve the schema and names for the tables in the database
    /// </summary>
    protected override string SelectTablesCommandText => "SELECT null AS SchemaName, name AS TableName FROM sqlite_master WHERE type = 'table'";


    /// <summary>
    /// Verifies that UseSqlite returns a DbContext{T} for chaining additional calls
    /// </summary>
    [Fact]
    public void UseSqlite_returns_DbContextBuilder()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        // Act & Assert
        Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(sut.UseSqlite());
    }



    /// <summary>
    /// Verifies that UseAutoFixture returns a DbContext{T} for chaining additional calls
    /// </summary>
    [Fact]
    public void UseAutoFixture_returns_DbContextBuilder()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        // Act & Assert
        Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(sut.UseAutoFixture());
    }



    /// <summary>
    /// Verifies that the RandomEntityGenerator used is an instance of AutoFixtureRandomEntityGenerator
    /// </summary>
    [Fact]
    public void RandomEntityGenerator_is_AutoFixture()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        // Act & Assert
        Assert.IsType<AutoFixtureRandomEntityGenerator>(sut.RandomEntityGenerator);

    }



    /// <summary>
    /// Verifies that the database used Microsoft's Sqlite database
    /// </summary>
    [Fact]
    public async Task Database_is_Sqlite()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlite()
            .AddSingleton<IModelCacheKeyFactory, SqliteModelCacheKeyFactory>()
            .AddSingleton<IModelCustomizer, SqliteModelCustomizer>()
            .BuildServiceProvider();

        // Act
        var context = await sut
            .UseSqlite()
            .UseServiceProvider(serviceProvider)
            .BuildAsync();

        // Assert
        Assert.True(context.Database.IsSqlite());
    }

    

    /// <summary>
    /// 
    /// </summary>
    [Fact]
    public async Task Services_passed_to_UseServiceProvider_are_used__when_creating_the_database()
    {

        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlite()
            .AddSingleton<IModelCacheKeyFactory, SqliteModelCacheKeyFactory>()
            .AddSingleton<IModelCustomizer, SqliteModelCustomizer>()
            .BuildServiceProvider();

        // Act
        await CreateDbContextBuilder()
            .UseServiceProvider(serviceProvider)
            .BuildAsync();

    }
}



internal class SqliteModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        return new SqliteModelCacheKey(context, designTime);
    }

    private sealed class SqliteModelCacheKey(DbContext context, bool designTime) : ModelCacheKey(context, designTime)
    {
    }
}



internal class SqliteModelCustomizer(ModelCustomizerDependencies dependencies) : ModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        if (!context.Database.IsSqlite())
            return;

        // Force inclusion of known entities if needed
        var knownTypes = new[]
        {
        typeof(Address),
        typeof(Customer),
        typeof(Employee),
        typeof(SalesOrderHeader),
        typeof(SalesOrderDetail),
        typeof(Product),
        typeof(StateProvince),
        // Add more as needed
    };

        foreach (var type in knownTypes)
        {
            if (modelBuilder.Model.FindEntityType(type) == null)
            {
                modelBuilder.Entity(type);
            }
        }

        // Rename all tables and fix relationships
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var originalTableName = entityType.GetTableName() ?? "";
            var schema = entityType.GetSchema() ?? "";
            var schemaPrefix = string.IsNullOrEmpty(schema) ? "dbo" : schema;

            // Avoid recursive renaming
            if (!originalTableName.StartsWith($"{schemaPrefix}_"))
            {
                var newTableName = $"{schemaPrefix}_{originalTableName}";
                entityType.SetTableName(newTableName);

                Console.WriteLine($"[{schema}].[{originalTableName}] to [{newTableName}]");
            }

            entityType.SetSchema(null); // Always strip schema for SQLite

            var computedColumns = new Dictionary<(string Schema, string Table, string Column), string?>()
                {
                    { ("Person", "Person", "OrganizationLevel"), null },
                    { ("HumanResources", "Employee", "OrganizationLevel"), null },
                    { ("Sales", "Customer", "AccountNumber"), null },
                    { ("Sales", "SalesOrderHeader", "SalesOrderNumber"), "(IFNULL('SO' || CAST(\"SalesOrderID\" AS TEXT), '*** ERROR ***'))"},
                    //{ ("", "", ""), null},
                    //{ ("", "", ""), null},
                    //{ ("", "", ""), null},


                };

            // Fix computed columns and default values
            foreach (var property in entityType.GetProperties())
            {
                if (computedColumns.TryGetValue((schema, originalTableName, property.Name), out var value))
                {
                    property.SetComputedColumnSql(value);
                }
                else
                {
                    var sql = property.GetComputedColumnSql();

                    if (!string.IsNullOrEmpty(sql))
                    {
                        var rewrittenSql = sql.Replace("ISNULL", "IFNULL", StringComparison.OrdinalIgnoreCase)
                            .Replace("N'", "'", StringComparison.OrdinalIgnoreCase)
                            .Replace("+", "||", StringComparison.OrdinalIgnoreCase)
                            //.Replace("CONVERT", "CAST", StringComparison.OrdinalIgnoreCase)
                            //.Replace("[dbo].", "")
                            //.Replace("dbo.", "")
                            ;

                        property.SetComputedColumnSql(rewrittenSql);

                        Console.WriteLine($"ComputerColumn: {sql,-40} | Rewrite: [{rewrittenSql,-40}");


                    }
                }

                var defaultValueSql = property.GetDefaultValueSql();

                Console.WriteLine($"DefaultValue: [{defaultValueSql}]");

                if (!string.IsNullOrWhiteSpace(defaultValueSql))
                {
                    if (!string.IsNullOrEmpty(defaultValueSql) &&
                        defaultValueSql.Contains("newid", StringComparison.OrdinalIgnoreCase))
                    {
                        property.SetDefaultValueSql("lower(hex(randomblob(16)))");
                    }
                    else if (defaultValueSql.Contains("getdate()"))
                    {
                        property.SetDefaultValueSql("datetime('now')");
                    }
                }
            }

            // Heuristic: rename many-to-many join tables
            var foreignKeys = entityType.GetForeignKeys().ToList();
            var navigations = entityType.GetNavigations().ToList();

            if (foreignKeys.Count == 2 && navigations.Count == 0)
            {
                var left = foreignKeys[0].PrincipalEntityType;
                var right = foreignKeys[1].PrincipalEntityType;

                var leftName = left.GetTableName();
                var rightName = right.GetTableName();

                var joinName = $"{leftName}_{rightName}";
                entityType.SetTableName(joinName);

                Console.WriteLine($"Renamed join table: {entityType.Name} → {joinName}");
            }
        }

        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(e => !string.IsNullOrWhiteSpace(e.GetSchema())))
        {
            entityType.SetSchema(null); // Strip schema for SQLite
        }
    }
}