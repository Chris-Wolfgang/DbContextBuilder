using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Wolfgang.DbContextBuilderCore;

/// <summary>
/// Overrides the default model creation process in the DbContext{T} with configurations suitable for SQLite.
/// </summary>
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
public class SqliteModelCustomizer : ModelCustomizer
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
    public Func<(string? SchemaName, string TableName), string> OverrideTableRenaming
    {
        get =>
            _overrideTableRenameRenaming ??= t =>
            {
                var schemaPrefix = t.SchemaName ?? "dbo";

                // Avoid recursive renaming
                return t.TableName.StartsWith($"{schemaPrefix}_", StringComparison.OrdinalIgnoreCase)
                    ? t.TableName // Table has already been renamed so just return it
                    : $"{schemaPrefix}_{t.TableName}"; // Rename table by prefixing schema name
            };
        set => _overrideTableRenameRenaming = value ?? throw new ArgumentNullException(nameof(value));
    }


    private Func<string?, string?>? _overrideComputedValueHandling;
    /// <summary>
    /// This method is called for each column that has a computed value defined in the model.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <remarks>
    /// The default handling for this is not leave the computed value as is. However,
    /// Sqlite has limited support for computed values and the functions used in the model may
    /// be incompatible with SQLite. You can override this behavior by assigning a custom value
    /// </remarks>
    public Func<string?, string?> OverrideComputedValueHandling
    {
        get => _overrideComputedValueHandling ??= defaultValue => defaultValue;
        set => _overrideComputedValueHandling = value ?? throw new ArgumentNullException(nameof(value));
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
    public SqliteModelCustomizer(ModelCustomizerDependencies dependencies)
        : base(dependencies) => ArgumentNullException.ThrowIfNull(dependencies);

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
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentNullException.ThrowIfNull(context);

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
            
            RenameTable(entityType, OverrideTableRenaming, originalTableName, originalSchemaName);

            foreach (var property in entityType.GetProperties())
            {
                OverrideComputedValue(property);

                OverrideDefaultValue(property);
            }

            OverrideManyToManyTableHandling(entityType);
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
        if (!string.Equals(newTableName, originalTableName, StringComparison.Ordinal))
        {
            entityType.SetTableName(newTableName);
        }

        if (originalSchemaName != null)
        {
#pragma warning disable MA0003 // EF extension method parameter name varies across versions
            entityType.SetSchema(null);
#pragma warning restore MA0003
        }
    }



    private void OverrideDefaultValue(IMutableProperty property)
    {
        var currentDefaultValue = property.GetDefaultValueSql();
        var newDefaultValue = OverrideDefaultValueHandling(currentDefaultValue);
        if (!string.Equals(currentDefaultValue, newDefaultValue, StringComparison.Ordinal))
        {
            property.SetDefaultValueSql(newDefaultValue);
        }
    }


    private void OverrideComputedValue(IMutableProperty property)
    {
        var originalComputedValueSql = property.GetComputedColumnSql();
        var newComputedValueSql = OverrideComputedValueHandling(originalComputedValueSql);
        if (!string.Equals(originalComputedValueSql, newComputedValueSql, StringComparison.Ordinal))
        {
            property.SetComputedColumnSql(newComputedValueSql);
        }

    }



    private Action<IMutableEntityType>? _overrideManyToManyTableHandling;

    /// <summary>
    /// This action is called for each entity type to handle many-to-many join table renaming.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <remarks>
    /// The default implementation uses a heuristic to detect many-to-many join tables:
    /// an entity type with exactly two foreign keys and no navigations is assumed to be a join table.
    /// When detected, the table is renamed to "{LeftTable}_{RightTable}". You can override this
    /// behavior by assigning a custom implementation to this property.
    /// </remarks>
    public Action<IMutableEntityType> OverrideManyToManyTableHandling
    {
        get =>
            _overrideManyToManyTableHandling ??= DefaultOverrideManyToManyTableHandling;
        set => _overrideManyToManyTableHandling = value ?? throw new ArgumentNullException(nameof(value));
    }



    private static void DefaultOverrideManyToManyTableHandling(IMutableEntityType entityType)
    {
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
        }
    }
}