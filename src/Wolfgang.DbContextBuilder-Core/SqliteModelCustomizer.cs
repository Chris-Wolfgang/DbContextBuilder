using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Wolfgang.DbContextBuilderCore;

/// <summary>
/// Overrides the default model creation process in the DbContext{T} with configurations suitable for SQLite.
/// </summary>
/// <param name="dependencies"></param>
/// <remarks>
/// Unless the production database you are testing is also SQLite, there will be differences between
/// your database and the context's configuration definition and SQLite's capabilities. This class
/// provides some basic overrides to make your DbContext work in SQLite. This class provides some
/// basic capabilities like,
///   1. Renaming tables to avoid schema issues since SQLite does not support schemas.
///   2. Removing computed values for columns since SQLite may not support the same functions.
///
/// You can override the functionality provided in this class or if you will be frequently working
/// in the same database engine, you can create your own ModelCustomizer, either from scratch or
/// derived from this one, and override the desired functionality. Once you created you can reuse
/// it over and over again. For example, you want to create a SqliteForOracleModelCustomizer that
/// has overrides to make an DbContext created for Oracle work in SQLite.
/// </remarks>
public class SqliteModelCustomizer(ModelCustomizerDependencies dependencies)
    : ModelCustomizer(dependencies)
{



    private Func<(string? SchemaName, string TableName), string>? _overrideTableRenameRenaming;
    /// <summary>
    /// This method is called for each table in the database to rename the table since SQLite
    /// does not support schemas.  
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <remarks>
    ///The default implementation renames the table by prefixing
    /// the schema name to the table name. For example, a table named "Person" in schema
    /// Personnel would be renamed to "Personnel_Person". If the table doesn't have a schema
    /// "dbo" is used as the schema name. You can override this behavior by assigning a
    /// custom implementation to this property
    /// </remarks>
    public Func<(string? SchemaName, string TableName), string> OverrideTableRenameRenaming
    {
        get =>
            _overrideTableRenameRenaming ??= t =>
            {
                var schemaPrefix = t.SchemaName ?? "dbo";

                // Avoid recursive renaming
                return t.TableName.StartsWith($"{schemaPrefix}_", StringComparison.InvariantCultureIgnoreCase)
                    ? t.TableName // Table has already been renamed so just return it
                    : $"{schemaPrefix}_{t.TableName}"; // Rename table by prefixing schema name
            };
        set => _overrideTableRenameRenaming = value ?? throw new ArgumentNullException(nameof(value));
    }



    /// <summary>
    /// A dictionary where the key is the default value as defined in the model and the value is the
    /// value to replace it with.
    /// </summary>
    /// <remarks>
    /// Examples of a default values that may need to be replaced it getdate() and newid() which would
    /// be replaced with datetime('now') and lower(hex(randomblob(16))) respectively.
    /// </remarks>
    public IDictionary<string, string> DefaultValueMap { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);


    private Func<string?, string?>? _overrideDefaultValueHandling;
    /// <summary>
    /// This method is called for each column that has a default value defined in the model.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <remarks>
    /// The default implementation checks the DefaultValueMap dictionary to see if the existing default value
    /// is contained in the dictionary and if so, replaces it with the value in the dictionary. If the
    /// current default is not in the dictionary, it is left unchanged. You can override this behavior
    /// by assigning a custom implementation to this property 
    /// </remarks>
    public Func<string?, string?> OverrideDefaultValueHandling
    {
        get =>
            _overrideDefaultValueHandling ??= defaultValue =>
            {
                if (defaultValue == null)
                {
                    return defaultValue;
                }

                return DefaultValueMap.TryGetValue(defaultValue, out var newDefaultValue)
                    ? newDefaultValue
                    : defaultValue;
            };
        set => _overrideDefaultValueHandling = value ?? throw new ArgumentNullException(nameof(value));
    }



    /// <summary>
    /// Overrides the default model creation process in the DbContext{T} with configurations suitable for SQLite.
    /// </summary>
    /// <param name="modelBuilder"></param>
    /// <param name="context"></param>
    public override void Customize
    (
        ModelBuilder modelBuilder,
        DbContext context
    )
    {
        base.Customize(modelBuilder, context);

        if (!context.Database.IsSqlite())
        {
            return;
        }

        // Rename all tables, override default values and computed value and fix relationships
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var originalTableName = entityType.GetTableName();
            var originalSchemaName = entityType.GetSchema();


            RenameTable(entityType, OverrideTableRenameRenaming, originalTableName, originalSchemaName);

            foreach (var property in entityType.GetProperties())
            {
                OverrideComputedValue(property);

                OverrideDefaultValue(property);
            }

            { // TODO Override join table handling
                // Heuristic: rename many-to-many join tables
                var foreignKeys = entityType.GetForeignKeys().ToList();
                var navigation = entityType.GetNavigations().ToList();

                if (foreignKeys.Count == 2 && navigation.Count == 0)
                {
                    var left = foreignKeys[0].PrincipalEntityType;
                    var right = foreignKeys[1].PrincipalEntityType;

                    var leftName = left.GetTableName();
                    var rightName = right.GetTableName();

                    var joinName = $"{leftName}_{rightName}";
                    entityType.SetTableName(joinName);
                }
            }
        }
    }



    private static void RenameTable
    (
        IMutableEntityType entityType,
        Func<(string?, string), string?> getNewName,
        string? originalTableName,
        string? originalSchemaName
    )
    {
        if (originalTableName == null)
        {
            throw new InvalidOperationException($"Entity type {entityType.Name} has no table name");
        }

        var newTableName = getNewName((originalSchemaName, originalTableName));
        if (newTableName != originalSchemaName)
        {
            entityType.SetTableName(newTableName);
        }

        if (originalSchemaName != null)
        {
            entityType.SetSchema(null);
        }
    }



    private void OverrideDefaultValue(IMutableProperty property)
    {
        var currentDefaultValue = property.GetDefaultValueSql();
        var newDefaultValue = OverrideDefaultValueHandling(currentDefaultValue);
        if (currentDefaultValue != newDefaultValue)
        {
            property.SetDefaultValueSql(newDefaultValue);
        }
    }



    private static void OverrideComputedValue(IMutableProperty property)
    {
        //if (computedColumns.TryGetValue((originalSchemaName, originalTableName, property.Name), out var value))
        //{
        //    property.SetComputedColumnSql(value);
        //}
        //else
        //{
        //    var sql = property.GetComputedColumnSql();
        //    if (!string.IsNullOrEmpty(sql))
        //    {
        //        var rewrittenSql = sql.Replace("ISNULL", "IFNULL", StringComparison.OrdinalIgnoreCase)
        //                .Replace("N'", "'", StringComparison.OrdinalIgnoreCase)
        //                .Replace("+", "||", StringComparison.OrdinalIgnoreCase)
        //            //.Replace("CONVERT", "CAST", StringComparison.OrdinalIgnoreCase)
        //            //.Replace("[dbo].", "")
        //            //.Replace("dbo.", "")
        //            ;
        //        property.SetComputedColumnSql(rewrittenSql);
        //    }
        //}
        property.SetComputedColumnSql(null!);
    }
}