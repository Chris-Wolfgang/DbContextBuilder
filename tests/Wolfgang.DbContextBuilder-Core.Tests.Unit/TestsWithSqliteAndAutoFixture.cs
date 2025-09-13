using AdventureWorks.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Runs all the tests using the default values for DbContextBuilder
/// </summary>
public class TestsWithSqliteAndAutoFixture : DbContextBuilderTestsBase
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

        // Ignore unsupported or problematic properties
        modelBuilder.Entity(typeof(Employee)).Ignore("OrganizationLevel");
        modelBuilder.Entity(typeof(Customer)).Ignore("AccountNumber");
        modelBuilder.Entity(typeof(SalesOrderHeader)).Ignore("SalesOrderNumber");

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

        // Rename tables and fix relationships
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var originalTableName = entityType.GetTableName();
            var schema = entityType.GetSchema();
            var schemaPrefix = string.IsNullOrEmpty(schema) ? "dbo" : schema;

            // Avoid recursive renaming
            if (!originalTableName.StartsWith($"{schemaPrefix}_"))
            {
                var newTableName = $"{schemaPrefix}_{originalTableName}";
                entityType.SetTableName(newTableName);
            }

            entityType.SetSchema(null); // Strip schema for SQLite

            // Rewrite computed columns and default values
            foreach (var property in entityType.GetProperties())
            {
                var sql = property.GetComputedColumnSql();


                if (!string.IsNullOrEmpty(sql))
                {
                    Console.WriteLine($"ComputerColumn: [{sql}]");

                    var rewrittenSql = sql.Replace("ISNULL", "IFNULL", StringComparison.OrdinalIgnoreCase)
                                          .Replace("N'", "'", StringComparison.OrdinalIgnoreCase)
                                          .Replace("+", "||", StringComparison.OrdinalIgnoreCase)
                                          .Replace("CONVERT", "CAST", StringComparison.OrdinalIgnoreCase);
                    property.SetComputedColumnSql(rewrittenSql);
                }

                var defaultValueSql = property.GetDefaultValueSql();

                Console.WriteLine($"DefaultValue: [{defaultValueSql}]");

                if (!string.IsNullOrEmpty(defaultValueSql) &&
                    defaultValueSql.Contains("newid", StringComparison.OrdinalIgnoreCase))
                {
                    property.SetDefaultValueSql("lower(hex(randomblob(16)))");
                }

                if (!string.IsNullOrEmpty(defaultValueSql) &&
                    defaultValueSql.Contains("getdate", StringComparison.OrdinalIgnoreCase))
                {
                    property.SetDefaultValueSql("datetime('now')");
                }
            }

            // Rename many-to-many join tables
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
                Console.WriteLine($"🔗 Renamed join table: {entityType.Name} → {joinName}");
            }

            // Fix foreign key principal table names (avoid recursive renaming)
            foreach (var fk in foreignKeys)
            {
                var principal = fk.PrincipalEntityType;
                var principalSchema = principal.GetSchema() ?? "dbo";
                var principalTable = principal.GetTableName();

                if (!principalTable.StartsWith($"{principalSchema}_"))
                {
                    var expectedTableName = $"{principalSchema}_{principalTable}";
                    principal.SetTableName(expectedTableName);
                }

                principal.SetSchema(null);
            }
        }
    }

    public  void CustomizeV4(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        if (!context.Database.IsSqlite())
            return;

        // Ignore unsupported or problematic properties
        modelBuilder.Entity(typeof(Employee)).Ignore("OrganizationLevel");
        modelBuilder.Entity(typeof(Customer)).Ignore("AccountNumber");
        modelBuilder.Entity(typeof(SalesOrderHeader)).Ignore("SalesOrderNumber");

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

        // Rename tables and fix relationships
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var originalTableName = entityType.GetTableName();
            var schema = entityType.GetSchema();
            var schemaPrefix = string.IsNullOrEmpty(schema) ? "dbo" : schema;

            // Avoid recursive renaming
            if (!originalTableName.StartsWith($"{schemaPrefix}_"))
            {
                var newTableName = $"{schemaPrefix}_{originalTableName}";
                entityType.SetTableName(newTableName);
            }

            entityType.SetSchema(null); // Strip schema for SQLite

            // Rewrite computed columns and default values
            foreach (var property in entityType.GetProperties())
            {
                var sql = property.GetComputedColumnSql();


                if (!string.IsNullOrEmpty(sql) && sql.Contains("ISNULL", StringComparison.OrdinalIgnoreCase))
                {
                    var rewrittenSql = sql.Replace("ISNULL", "IFNULL", StringComparison.OrdinalIgnoreCase)
                                          .Replace("N'", "'", StringComparison.OrdinalIgnoreCase)
                                          .Replace("+", "||", StringComparison.OrdinalIgnoreCase)
                                          .Replace("CONVERT", "CAST", StringComparison.OrdinalIgnoreCase);
                    property.SetComputedColumnSql(rewrittenSql);
                }

                var defaultValueSql = property.GetDefaultValueSql();
                if (!string.IsNullOrEmpty(defaultValueSql) &&
                    defaultValueSql.Contains("newid", StringComparison.OrdinalIgnoreCase))
                {
                    property.SetDefaultValueSql("lower(hex(randomblob(16)))");
                }

                if (!string.IsNullOrEmpty(defaultValueSql) &&
                    defaultValueSql.Contains("getdate", StringComparison.OrdinalIgnoreCase))
                {
                    property.SetDefaultValueSql("datetime('now')");
                }
            }

            // Rename many-to-many join tables
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
                Console.WriteLine($"🔗 Renamed join table: {entityType.Name} → {joinName}");
            }

            // Fix foreign key principal table names (avoid recursive renaming)
            foreach (var fk in foreignKeys)
            {
                var principal = fk.PrincipalEntityType;
                var principalSchema = principal.GetSchema() ?? "dbo";
                var principalTable = principal.GetTableName();

                if (!principalTable.StartsWith($"{principalSchema}_"))
                {
                    var expectedTableName = $"{principalSchema}_{principalTable}";
                    principal.SetTableName(expectedTableName);
                }

                principal.SetSchema(null);
            }
        }
    }

    
    
    public void CustomizeV3(ModelBuilder modelBuilder, DbContext context)
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
            var originalTableName = entityType.GetTableName();
            var schema = entityType.GetSchema();
            var schemaPrefix = string.IsNullOrEmpty(schema) ? "dbo" : schema;
            var newTableName = $"{schemaPrefix}_{originalTableName}";

            entityType.SetTableName(newTableName);
            entityType.SetSchema(null); // Strip schema for SQLite

            // Fix computed columns and default values
            foreach (var property in entityType.GetProperties().ToList())
            {
                var sql = property.GetComputedColumnSql();
                if (!string.IsNullOrEmpty(sql) && sql.Contains("ISNULL", StringComparison.OrdinalIgnoreCase))
                {
                    var rewrittenSql = sql.Replace("ISNULL", "IFNULL", StringComparison.OrdinalIgnoreCase)
                                          .Replace("N'", "'", StringComparison.OrdinalIgnoreCase)
                                          .Replace("+", "||", StringComparison.OrdinalIgnoreCase)
                                          .Replace("CONVERT", "CAST", StringComparison.OrdinalIgnoreCase);
                    property.SetComputedColumnSql(rewrittenSql);
                }

                var defaultValueSql = property.GetDefaultValueSql();
                if (!string.IsNullOrEmpty(defaultValueSql) &&
                    defaultValueSql.Contains("newid", StringComparison.OrdinalIgnoreCase))
                {
                    property.SetDefaultValueSql("lower(hex(randomblob(16)))");
                }
            }

            // Rename many-to-many join tables
            var foreignKeys = entityType.GetForeignKeys().ToList();
            var navigations = entityType.GetNavigations().ToList();

            if (foreignKeys.Count == 2 && navigations.Count == 0)
            {
                var left = foreignKeys[0].PrincipalEntityType;
                var right = foreignKeys[1].PrincipalEntityType;

                var leftName = $"{left.GetSchema() ?? "dbo"}_{left.GetTableName()}";
                var rightName = $"{right.GetSchema() ?? "dbo"}_{right.GetTableName()}";

                var joinName = $"{leftName}_{rightName}";
                entityType.SetTableName(joinName);
                Console.WriteLine($"🔗 Renamed join table: {entityType.Name} → {joinName}");
            }

            // Fix foreign key references to renamed principal tables
            foreach (var fk in foreignKeys)
            {
                var principal = fk.PrincipalEntityType;
                var principalSchema = principal.GetSchema() ?? "dbo";
                var principalTable = principal.GetTableName();
                var expectedTableName = $"{principalSchema}_{principalTable}";

                principal.SetTableName(expectedTableName);
                principal.SetSchema(null);
            }
        }
    }


    public void CustomizeV2(ModelBuilder modelBuilder, DbContext context)
    {
        if (!context.Database.IsSqlite())
            return;

        // Ignore problematic properties
        modelBuilder.Entity(typeof(Employee)).Ignore("OrganizationLevel");
        modelBuilder.Entity(typeof(Customer)).Ignore("AccountNumber");
        modelBuilder.Entity(typeof(SalesOrderHeader)).Ignore("SalesOrderNumber");

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

        // Apply schema-prefix renaming and patch SQL Server-specific expressions
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var originalTableName = entityType.GetTableName();
            var schema = entityType.GetSchema();

            var schemaPrefix = string.IsNullOrEmpty(schema) ? "dbo" : schema;
            var newTableName = $"{schemaPrefix}_{originalTableName}";

            entityType.SetTableName(newTableName);
            entityType.SetSchema(null); // Strip schema for SQLite


            // Handle many-to-many join tables
            var foreignKeys = entityType.GetForeignKeys().ToList();
            var navigation = entityType.GetNavigations().ToList();

            // Heuristic: likely a join table if it has exactly 2 FKs and no other navigation
            if (foreignKeys.Count == 2 && navigation.Count == 0)
            {
                var left = foreignKeys[0].PrincipalEntityType;
                var right = foreignKeys[1].PrincipalEntityType;

                var leftName = $"{left.GetSchema() ?? "dbo"}_{left.GetTableName()}";
                var rightName = $"{right.GetSchema() ?? "dbo"}_{right.GetTableName()}";

                var joinName = $"{leftName}_{rightName}";
                entityType.SetTableName(joinName);
                entityType.SetSchema(null);

                Console.WriteLine($"🔗 Renamed join table: {entityType.ClrType?.Name ?? entityType.Name} → {joinName}");
            }

            foreach (var property in entityType.GetProperties().ToList())
            {
                // Rewrite computed columns using ISNULL → IFNULL
                var sql = property.GetComputedColumnSql();
                if (!string.IsNullOrEmpty(sql) && sql.Contains("ISNULL", StringComparison.OrdinalIgnoreCase))
                {
                    var rewrittenSql = sql.Replace("ISNULL", "IFNULL", StringComparison.OrdinalIgnoreCase)
                        .Replace("N'", "'", StringComparison.OrdinalIgnoreCase)
                        .Replace("+", "||", StringComparison.OrdinalIgnoreCase)
                        .Replace("CONVERT", "CAST", StringComparison.OrdinalIgnoreCase);
                    property.SetComputedColumnSql(rewrittenSql);
                }

                // Rewrite default values using NEWID() → lower(hex(randomblob(16)))
                var defaultValueSql = property.GetDefaultValueSql();
                if (!string.IsNullOrEmpty(defaultValueSql) &&
                    defaultValueSql.Contains("newid", StringComparison.OrdinalIgnoreCase))
                {
                    property.SetDefaultValueSql("lower(hex(randomblob(16)))");
                }
            }
        }
    }



    public void CustomizeV1(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        if (!context.Database.IsSqlite())
            return;

        modelBuilder.Entity(typeof(Employee)).Ignore("OrganizationLevel");
        modelBuilder.Entity(typeof(Customer)).Ignore("AccountNumber");
        modelBuilder.Entity(typeof(SalesOrderHeader)).Ignore("SalesOrderNumber");

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {

            var originalTableName = entityType.GetTableName();
            var schema = entityType.GetSchema();

            // Default to "dbo" if no schema is defined
            var schemaPrefix = string.IsNullOrEmpty(schema) ? "dbo" : schema;

            // Rename table to schema_tablename
            var newTableName = $"{schemaPrefix}_{originalTableName}";
            entityType.SetTableName(newTableName);

            // Remove schema to avoid SQLite warnings
            entityType.SetSchema(null);


            foreach (var property in entityType.GetProperties().ToList())
            {
                var sql = property.GetComputedColumnSql();
                if (!string.IsNullOrEmpty(sql) && sql.Contains("ISNULL", StringComparison.OrdinalIgnoreCase))
                {
                    var rewrittenSql = sql.Replace("ISNULL", "IFNULL", StringComparison.OrdinalIgnoreCase);
                    property.SetComputedColumnSql(rewrittenSql);
                }

                var defaultValueSql = property.GetDefaultValueSql();
                if (!string.IsNullOrEmpty(defaultValueSql) &&
                    defaultValueSql.Contains("newid", StringComparison.OrdinalIgnoreCase))
                {
                    property.SetDefaultValueSql("lower(hex(randomblob(16)))");
                }
            }
        }
    }
}

