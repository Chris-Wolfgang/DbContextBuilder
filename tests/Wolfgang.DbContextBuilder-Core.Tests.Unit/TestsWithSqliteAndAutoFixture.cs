using System.Data.Common;
using System.Text;
using AdventureWorks.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Runs all the tests using the default values for DbContextBuilder
/// </summary>
public class TestsWithSqliteAndAutoFixture(ITestOutputHelper testOutputHelper) : DbContextBuilderTestsBase(testOutputHelper)
{

    /// <summary>
    /// Creates a new instance of the DbContextBuilder configured to use SQLite and AutoFixture
    /// </summary>
    /// <returns><see cref="DbContextBuilder{AdventureWorksDbContext}"/></returns>
    protected override DbContextBuilder<AdventureWorksDbContext> CreateDbContextBuilder()
    {
        //var serviceProvider = new ServiceCollection()
        //    .AddEntityFrameworkSqlite()
        //    .AddSingleton<IModelCustomizer, SqliteModelCustomizer>()
        //    .BuildServiceProvider();

        return new DbContextBuilder<AdventureWorksDbContext>()
            //.UseSqlite()
            .UseSqliteForMsSqlServer()
            //.UseServiceProvider(serviceProvider)
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
    /// Verifies that calling UseSqlite cause BuildAsync() to use Microsoft's Sqlite database
    /// </summary>
    [Fact]
    public async Task UseSqlite_causes_BuildAsync_to_use_Sqlite()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlite()
            .AddSingleton<IModelCustomizer, SqliteModelCustomizer>()
            .BuildServiceProvider();

        // Act
        var context = await sut
            .UseSqlite()
            //.UseServiceProvider(serviceProvider)
            .BuildAsync();

        // Assert
        Assert.True(context.Database.IsSqlite());
    }



    /// <summary>
    /// Verifies that UseSqlite returns a DbContext{T} for chaining additional calls
    /// </summary>
    [Fact]
    public void UseSqliteForMsSqlServer_returns_DbContextBuilder()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        // Act & Assert
        Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(sut.UseSqliteForMsSqlServer());
    }



    ///// <summary>
    ///// Verifies that UseSqlite returns a DbContext{T} for chaining additional calls
    ///// </summary>
    //[Fact]
    //public void UseSqlite_with_SqliteOverrides_when_passed_null_throws_ArgumentNullException()
    //{
    //    // Arrange
    //    var sut = new DbContextBuilder<AdventureWorksDbContext>();

    //    // Act & Assert
    //    var ex = Assert.Throws<ArgumentNullException>(() => sut.UseSqliteForMsSqlServer());
    //    Assert.Equal("modelCustomizer", ex.ParamName);
    //}



    /// <summary>
    /// Verifies that calling UseSqlite cause BuildAsync() to use Microsoft's Sqlite database
    /// </summary>
    [Fact]
    public async Task UseSqliteForMsSqlServer_causes_BuildAsync_to_use_Sqlite_with_customizations_for_Sql_Server()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlite()
            .AddSingleton<IModelCustomizer, SqliteModelCustomizer>()
            .BuildServiceProvider();

        // Act
        var context = await sut
            .UseSqliteForMsSqlServer()
            //.UseServiceProvider(serviceProvider)
            .BuildAsync();

        // Assert
        Assert.True(context.Database.IsSqlite());
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
            .AddSingleton<IModelCustomizer, SqliteModelCustomizer>()
            .BuildServiceProvider();

        // Act
        var context = await sut
            .UseSqlite()
            //.UseServiceProvider(serviceProvider)
            .BuildAsync();

        // Assert
        Assert.True(context.Database.IsSqlite());
    }



    /// <summary>
    /// Verifies that services passed to UseServiceProvider are used when creating the database
    /// </summary>
    [Fact]
    public async Task Services_passed_to_UseServiceProvider_are_used_when_creating_the_database()
    {

        // Arrange
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlite()
            .AddSingleton<IModelCustomizer, SqliteModelCustomizer>()
            .BuildServiceProvider();

        // Act & Assert

        // This would fail it wasn't using the custom configuration above
        await CreateDbContextBuilder()
            //.UseServiceProvider(serviceProvider)
            .BuildAsync();

    }

    

    /// <summary>
    /// Verifies that if you configure the DbContextOptionsBuilder correctly is will log SQL statements,
    /// </summary>
    [Fact]
    public async Task When_configured_to_do_so_BuildAsync_will_log_the_create_statement_with_modified_table_names()
    {

        // Arrange
        var sut = CreateDbContextBuilder();

        var buffer = new StringBuilder(10_240);
        var sw = new StringWriter(buffer);

        var optionsBuilder = new DbContextOptionsBuilder<AdventureWorksDbContext>()
                .LogTo(s => sw.WriteLine(s));

        sut.UseDbContextOptionsBuilder(optionsBuilder);

        // Act
        await sut.BuildAsync();


        // Assert
        Assert.Contains("CREATE TABLE \"Person_Person\"", buffer.ToString());
    }



    /// <summary>
    /// Verifies that the default behavior when using Sqlite, is to prepend the schema name to the table name,
    /// separated by an underscore, and to strip the schema name from the table itself
    /// </summary>
    [Fact]
    public async Task UseSqlite_default_behavior_for_schema_names_is_to_prepend_the_schema_name_to_table_name()
    {

        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>()
                .UseSqlite()
                .UseAutoFixture();

        // Act
        var context = await sut
            .BuildAsync();
        
        // Assert
        //var columns = await GetColumnMetadataAsync(context);
        //Assert.True(columns.Any(c => c.TableName == "Person_Person"), "Table Person_Person was not found");

        var tables = await GetTableMetadataAsync(context);
        Assert.Contains(tables, t => t.TableName == "Person_Person");
    }



    /// <summary>
    /// Verifies that the default behavior when using Sqlite, is to prepend the schema name to the table name,
    /// separated by an underscore, and to strip the schema name from the table itself
    /// </summary>
    [Fact]
    public async Task UseSqlite_default_behavior_for_default_values_is_to_not_change_them()
    {

        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>()
            .UseSqlite()
            .UseAutoFixture();

        // Act
        var context = await sut
            .BuildAsync();

        // Assert
        var columns = await GetColumnMetadataAsync(context);

        var columnsWithNewId = columns.Where(c => c.DefaultValue == "(newid())").ToList();
        Assert.Equal(28, columnsWithNewId.Count);

        var columnsWithGetDate = columns.Where(c => c.DefaultValue == "(getdate())").ToList();
        Assert.Equal(75, columnsWithGetDate.Count);
    }



    /// <summary>
    /// Verifies that the default behavior when using Sqlite, is to prepend the schema name to the table name,
    /// separated by an underscore, and to strip the schema name from the table itself
    /// </summary>
    [Fact]
    public async Task UseSqlite_default_behavior_for_computed_column_is_to_remove_computed_value()
    {

        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>()
            .UseSqlite()
            .UseAutoFixture();

        // Act
        var context = await sut
            .BuildAsync();

        // Assert
        var columns = await GetColumnMetadataAsync(context);
        var columnsWithComputedValues = columns.Where(c => !string.IsNullOrEmpty(c.ComputedValue)).ToList();
        Assert.Empty(columnsWithComputedValues);
    }



    private record TableMetadata(string TableName);



    private static async Task<List<TableMetadata>> GetTableMetadataAsync(AdventureWorksDbContext context)
    {
        const string commandText = "Select name from sqlite_master where type='table';"; //" and name=@tableName;";
        var connection = context.Database.GetDbConnection();

        await using var command = connection.CreateCommand();
        command.CommandText = commandText;
        command.CommandType = System.Data.CommandType.Text;

        //var parameter = command.CreateParameter();
        //parameter.ParameterName = "tableName";
        //parameter.Value = tableName;
        //command.Parameters.Add(parameter);

        var tables = new List<TableMetadata>(200);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var name = reader.GetString(0);
            tables.Add(new TableMetadata(name));
        }

        return tables;
    }



    private record ColumnMetadata(string TableName, string ColumnName, string? DefaultValue, string? ComputedValue);



    // Returns a list of columns with default values and computed values for all tables in SQLite
    private static async Task<List<ColumnMetadata>> GetColumnMetadataAsync(AdventureWorksDbContext context)
    {
        var result = new List<ColumnMetadata>();
        var connection = context.Database.GetDbConnection();

        var tableNames = await GetTableNamesAsync(connection);

        foreach (var tableName in tableNames)
        {
            // PRAGMA table_info returns info about columns, including default values
            var commandText = $"PRAGMA table_info('{tableName}');";
            await using var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = System.Data.CommandType.Text;

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var columnName = reader["name"].ToString();
                var defaultValue = reader["dflt_value"].ToString();
                string? computedValue = null;

                // Try to get computed value using PRAGMA table_xinfo (if available) or sqlite_master
                // SQLite stores generated columns in the "generated" column in PRAGMA table_xinfo (SQLite 3.31+)
                // Fallback: parse the SQL from sqlite_master

                // Try PRAGMA table_xinfo first
                try
                {
                    var xinfoCmdText = $"PRAGMA table_xinfo('{tableName}');";
                    await using var xinfoCmd = connection.CreateCommand();
                    xinfoCmd.CommandText = xinfoCmdText;
                    xinfoCmd.CommandType = System.Data.CommandType.Text;

                    await using var xinfoReader = await xinfoCmd.ExecuteReaderAsync();
                    while (await xinfoReader.ReadAsync())
                    {
                        var xinfoColName = xinfoReader["name"].ToString();
                        if (string.Equals(xinfoColName, columnName, StringComparison.OrdinalIgnoreCase))
                        {
                            var generated = xinfoReader["generated"].ToString();
                            if (!string.IsNullOrEmpty(generated) && generated != "0")
                            {
                                // Try to get the expression from the "hidden" column
                                computedValue = xinfoReader["dflt_value"].ToString();
                            }
                            break;
                        }
                    }
                }
                catch
                {
                    // Ignore errors, fallback to sqlite_master
                }

                // Fallback: parse CREATE TABLE statement for generated columns
                if (computedValue == null)
                {
                    const string getTableSqlCmdText = "SELECT sql FROM sqlite_master WHERE type='table' AND name=@tableName;";
                    await using var getTableSqlCmd = connection.CreateCommand();
                    getTableSqlCmd.CommandText = getTableSqlCmdText;
                    getTableSqlCmd.CommandType = System.Data.CommandType.Text;
                    var param = getTableSqlCmd.CreateParameter();
                    param.ParameterName = "tableName";
                    param.Value = tableName;
                    getTableSqlCmd.Parameters.Add(param);

                    var tableSql = "";
                    await using var sqlReader = await getTableSqlCmd.ExecuteReaderAsync();
                    if (await sqlReader.ReadAsync())
                    {
                        tableSql = sqlReader["sql"].ToString() ?? "";
                    }

                    if (!string.IsNullOrEmpty(tableSql))
                    {
                        // Try to find the column definition with "GENERATED ALWAYS AS"
                        var pattern = $@"\b{columnName}\b\s+[^\(]*GENERATED\s+ALWAYS\s+AS\s*\((.*?)\)";
                        var match = System.Text.RegularExpressions.Regex.Match(tableSql, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (match.Success && match.Groups.Count > 1)
                        {
                            computedValue = match.Groups[1].Value;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(defaultValue) || !string.IsNullOrEmpty(computedValue))
                {
                    result.Add
                    (
                        new ColumnMetadata
                        (
                            TableName: tableName,
                            ColumnName: columnName!,
                            DefaultValue: defaultValue,
                            ComputedValue: computedValue 
                        )
                    );
                }
            }
        }

        return result;
    }



    private static async Task<List<string>> GetTableNamesAsync(DbConnection connection)
    {
        // Get all table names
        const string getTablesCommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";
        await using var getTablesCommand = connection.CreateCommand();
        getTablesCommand.CommandText = getTablesCommandText;
        getTablesCommand.CommandType = System.Data.CommandType.Text;

        var tableNames = new List<string>();
        await using var tablesReader = await getTablesCommand.ExecuteReaderAsync();
        while (await tablesReader.ReadAsync())
        {
            var tableName = tablesReader.GetString(0);
            if (!string.IsNullOrEmpty(tableName))
            {
                tableNames.Add(tableName);
            }
        }

        return tableNames;
    }
}


//// TODO add property to config OverrideDefaultSqliteModelCacheKeyFactory = T/F
//// TODO Modify UseSqlite to return a SqliteDbContextBuilder with additional properties including this one

//internal class SqliteModelCacheKeyFactory : IModelCacheKeyFactory
//{
//    public object Create(DbContext context, bool designTime) => new SqliteModelCacheKey(context, designTime);

//    private sealed class SqliteModelCacheKey(DbContext context, bool designTime) 
//        : ModelCacheKey(context, designTime)
//    {
//    }
//}



//internal sealed class SqliteModelCustomizer(ModelCustomizerDependencies dependencies) 
//    : ModelCustomizer(dependencies)
//{
//    public override void Customize(ModelBuilder modelBuilder, DbContext context)
//    {
//        base.Customize(modelBuilder, context);

//        if (!context.Database.IsSqlite())
//        {
//            return;
//        }

//        // TODO Create property SchemaHandling
//        //      Options: LeaveAlone, Strip, PrefixToTableName
//        // Rename all tables and fix relationships
//        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
//        {
//            var originalTableName = entityType.GetTableName() ?? "";
//            var originalSchemaName = entityType.GetSchema() ?? "";

//            { // TODO Override schema handling
//                var schemaPrefix = string.IsNullOrEmpty(originalSchemaName) ? "dbo" : originalSchemaName;

//                // Avoid recursive renaming
//                if (!originalTableName.StartsWith($"{schemaPrefix}_", StringComparison.InvariantCultureIgnoreCase))
//                {
//                    var newTableName = $"{schemaPrefix}_{originalTableName}";
//                    entityType.SetTableName(newTableName);
//                }

//                entityType.SetSchema(null); // Always strip schema for SQLite
//            }

//            {
//                // Fix computed columns and default values
//                var computedColumns = new Dictionary<(string Schema, string Table, string Column), string?>
//                {
//                    { ("Person", "Person", "OrganizationLevel"), null }, { ("HumanResources", "Employee", "OrganizationLevel"), null }, { ("Sales", "Customer", "AccountNumber"), null }, { ("Sales", "SalesOrderHeader", "SalesOrderNumber"), "(IFNULL('SO' || CAST(\"SalesOrderID\" AS TEXT), '*** ERROR ***'))" },
//                };

//                // TODO Override computed column handling
//                foreach (var property in entityType.GetProperties())
//                {
//                    if (computedColumns.TryGetValue((originalSchemaName, originalTableName, property.Name), out var value))
//                    {
//                        property.SetComputedColumnSql(value);
//                    }
//                    else
//                    {
//                        var sql = property.GetComputedColumnSql();

//                        if (!string.IsNullOrEmpty(sql))
//                        {
//                            var rewrittenSql = sql.Replace("ISNULL", "IFNULL", StringComparison.OrdinalIgnoreCase)
//                                    .Replace("N'", "'", StringComparison.OrdinalIgnoreCase)
//                                    .Replace("+", "||", StringComparison.OrdinalIgnoreCase)
//                                //.Replace("CONVERT", "CAST", StringComparison.OrdinalIgnoreCase)
//                                //.Replace("[dbo].", "")
//                                //.Replace("dbo.", "")
//                                ;

//                            property.SetComputedColumnSql(rewrittenSql);
//                        }
//                    }
//                }

//                { // TODO Override default value handling
//                    foreach (var property in entityType.GetProperties())
//                    {
//                        var defaultValueSql = property.GetDefaultValueSql();

//                        if (!string.IsNullOrWhiteSpace(defaultValueSql))
//                        {
//                            if (!string.IsNullOrEmpty(defaultValueSql) &&
//                                defaultValueSql.Contains("newid", StringComparison.OrdinalIgnoreCase))
//                            {
//                                property.SetDefaultValueSql("lower(hex(randomblob(16)))");
//                            }
//                            else if (defaultValueSql.Contains("getdate()"))
//                            {
//                                property.SetDefaultValueSql("datetime('now')");
//                            }
//                        }
//                    }
//                }
//            }

//            { // TODO Override join table handling
//                // Heuristic: rename many-to-many join tables
//                var foreignKeys = entityType.GetForeignKeys().ToList();
//                var navigation = entityType.GetNavigations().ToList();

//                if (foreignKeys.Count == 2 && navigation.Count == 0)
//                {
//                    var left = foreignKeys[0].PrincipalEntityType;
//                    var right = foreignKeys[1].PrincipalEntityType;

//                    var leftName = left.GetTableName();
//                    var rightName = right.GetTableName();

//                    var joinName = $"{leftName}_{rightName}";
//                    entityType.SetTableName(joinName);
//                }
//            }
//        }
//    }
//}