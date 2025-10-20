using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Suite of tests to verify SqliteForMsSqlServerModelCustomizer functionality.
/// </summary>
public class SqliteForMsSqlServerModelCustomizerTests
{

    /// <summary>
    /// Verifies that an instance of SqliteForMsSqlServerModelCustomizer can be created.
    /// </summary>
    [Fact]
    public void Can_create_instance_of_SqliteForMsSqlServerModelCustomizer()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        // Act & Assert
        // ReSharper disable once UnusedVariable
        var sut = new SqliteForMsSqlServerModelCustomizer(dependencies);
    }



    /// <summary>
    /// Verifies that passing null to the constructor throws ArgumentNullException
    /// </summary>
    [Fact]
    public void Ctor_when_passed_null_throws_ArgumentNullException()
    {
        // Arrange

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new SqliteForMsSqlServerModelCustomizer(null!));
        Assert.Equal("dependencies", ex.ParamName);
    }



    /// <summary>
    /// Verifies that setting OverrideTableRenaming property to null throws ArgumentNullException
    /// </summary>
    [Fact]
    public void OverrideTableRenameRenaming_when_set_to_null_throws_ArgumentNullException()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif


        var sut = new SqliteForMsSqlServerModelCustomizer(dependencies);

        var ex = Assert.Throws<ArgumentNullException>(() => sut.OverrideTableRenaming = null!);
        Assert.Equal("value", ex.ParamName);
    }



    /// <summary>
    /// Verifies that tables get renamed properly using default implementation of OverrideTableRenaming.
    /// </summary>
    [Fact]
    public void OverrideTableRenaming_can_rename_table_with_default_implementation()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteForMsSqlServerModelCustomizer(dependencies);

        // Act & Assert
        var table1 = (SchemaName: "Personnel", TableName: "Person");
        var renamedTable1 = sut.OverrideTableRenaming(table1);
        Assert.Equal("Personnel_Person", renamedTable1);

        var table2 = (SchemaName: (string?)null, TableName: "Person");
        var renamedTable2 = sut.OverrideTableRenaming(table2);
        Assert.Equal("dbo_Person", renamedTable2);

        var table3 = (SchemaName: "dbo", TableName: "Person");
        var renamedTable3 = sut.OverrideTableRenaming(table3);
        Assert.Equal("dbo_Person", renamedTable3);

        // Verify that recursive renaming is avoided
        var table4 = (SchemaName: "dbo", TableName: "dbo_Person");
        var renamedTable4 = sut.OverrideTableRenaming(table4);
        Assert.Equal("dbo_Person", renamedTable4);
    }



    /// <summary>
    /// Verifies that OverrideTableRenaming renames table properly using a custom implementation 
    /// </summary>
    [Fact]
    public void OverrideTableRenaming_can_rename_table_with_custom_implementation()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteForMsSqlServerModelCustomizer(dependencies);

        sut.OverrideTableRenaming = tuple =>
        {
            var schemaPrefix = $"{tuple.SchemaName ?? "dbo"}$";

            return tuple.TableName.StartsWith(schemaPrefix, StringComparison.OrdinalIgnoreCase)
                ? tuple.TableName
                : $"{tuple.SchemaName ?? "dbo"}${tuple.TableName}";
        };

        // Act & Assert
        var table1 = (SchemaName: "Personnel", TableName: "Person");
        var renamedTable1 = sut.OverrideTableRenaming(table1);
        Assert.Equal("Personnel$Person", renamedTable1);

        var table2 = (SchemaName: (string?)null, TableName: "Person");
        var renamedTable2 = sut.OverrideTableRenaming(table2);
        Assert.Equal("dbo$Person", renamedTable2);

        var table3 = (SchemaName: "dbo", TableName: "Person");
        var renamedTable3 = sut.OverrideTableRenaming(table3);
        Assert.Equal("dbo$Person", renamedTable3);

        // Verify that recursive renaming is avoided
        var table4 = (SchemaName: "dbo", TableName: "dbo$Person");
        var renamedTable4 = sut.OverrideTableRenaming(table4);
        Assert.Equal("dbo$Person", renamedTable4);
    }



    /// <summary>
    /// Verifies that setting OverrideDefaultValueHandling property to null throws ArgumentNullException
    /// </summary>
    [Fact]
    public void OverrideDefaultValueHandling_when_set_to_null_throws_ArgumentNullException()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteForMsSqlServerModelCustomizer(dependencies);

        var ex = Assert.Throws<ArgumentNullException>(() => sut.OverrideDefaultValueHandling = null!);
        Assert.Equal("value", ex.ParamName);
    }



    /// <summary>
    /// Verifies that the default implementation for OverrideDefaultValueHandling does
    /// override known SQL Server functions with SQLite equivalents.
    /// </summary>
    [Fact]
    public void OverrideDefaultValueHandling_default_implementation_overrides_known_SqlServer_values_with_Sqlite_equivalent()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteForMsSqlServerModelCustomizer(dependencies);

        // Act & Assert
        Assert.Equal("datetime('now')", sut.OverrideDefaultValueHandling("(getdate())"));
        Assert.Equal("lower(hex(randomblob(16)))", sut.OverrideDefaultValueHandling("(newid())"));
    }



    /// <summary>
    /// Verifies that the default implementation for OverrideDefaultValueHandling does
    /// override known SQL Server functions with SQLite equivalents.
    /// </summary>
    [Fact]
    public void OverrideDefaultValueHandling_default_implementation_overrides_unknown_SqlServer_values_with_null()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteForMsSqlServerModelCustomizer(dependencies);

        // Act & Assert
        Assert.Null(sut.OverrideDefaultValueHandling("(unknown value)"));
        Assert.Null(sut.OverrideDefaultValueHandling("(another unknown)"));
        Assert.Null(sut.OverrideDefaultValueHandling(""));
        Assert.Null(sut.OverrideDefaultValueHandling(null));
    }



    /// <summary>
    /// Verifies OverrideDefaultValueHandling honors custom implementation when specified.
    /// </summary>
    [Fact]
    public void OverrideDefaultValueHandling_uses_custom_method_when_provided()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
        var sut = new SqliteForMsSqlServerModelCustomizer(dependencies);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        // Act & Assert
        var sut = new SqliteForMsSqlServerModelCustomizer(dependencies)
        {
            OverrideDefaultValueHandling = _ => null
        };

        Assert.Null(sut.OverrideDefaultValueHandling("(getdate())"));
        Assert.Null(sut.OverrideDefaultValueHandling("(newid())"));
        Assert.Null(sut.OverrideDefaultValueHandling(""));
        Assert.Null(sut.OverrideDefaultValueHandling(null));
    }





    /// <summary>
    /// Verifies that setting OverrideComputedValueHandling property to null throws ArgumentNullException
    /// </summary>
    [Fact]
    public void OverrideComputedValueHandling_when_set_to_null_throws_ArgumentNullException()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteForMsSqlServerModelCustomizer(dependencies);

        var ex = Assert.Throws<ArgumentNullException>(() => sut.OverrideComputedValueHandling = null!);
        Assert.Equal("value", ex.ParamName);
    }



    ///// <summary>
    ///// Verifies that the default implementation for OverrideComputedValueHandling does_nothing
    ///// </summary>
    //[Fact]
    //public void OverrideComputedValueHandling_default_implementation_set_default_to_null()
    //{
    //    // Arrange
    //    var finder = new Mock<IDbSetFinder>().Object;
    //    var dependencies = new ModelCustomizerDependencies(finder);
    //    var sut = new SqliteForMsSqlServerModelCustomizer(dependencies);

    //    // Act & Assert
    //    Assert.Equal("(isnull('AW'+[dbo].[ufnLeadingZeros]([CustomerID]),''))", sut.OverrideComputedValueHandling("(isnull('AW'+[dbo].[ufnLeadingZeros]([CustomerID]),''))"));
    //    Assert.Equal("([OrganizationNode].[GetLevel]())", sut.OverrideComputedValueHandling("([OrganizationNode].[GetLevel]())"));
    //    Assert.Equal("", sut.OverrideComputedValueHandling(""));
    //    Assert.Equal(null, sut.OverrideComputedValueHandling(null));
    //}



    /// <summary>
    /// Verifies OverrideComputedValueHandling honors custom implementation when specified.
    /// </summary>
    [Fact]
    public void OverrideComputedValueHandling_uses_custom_method_when_provided()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
        var sut = new SqliteForMsSqlServerModelCustomizer(dependencies);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif


        // Act & Assert
        var sut = new SqliteForMsSqlServerModelCustomizer(dependencies)
        {
            OverrideComputedValueHandling = _ => null
        };

        Assert.Null(sut.OverrideComputedValueHandling("(isnull('AW'+[dbo].[ufnLeadingZeros]([CustomerID]),''))"));
        Assert.Null(sut.OverrideComputedValueHandling("([OrganizationNode].[GetLevel]())"));
        Assert.Null(sut.OverrideComputedValueHandling(""));
        Assert.Null(sut.OverrideComputedValueHandling(null));
    }
}
