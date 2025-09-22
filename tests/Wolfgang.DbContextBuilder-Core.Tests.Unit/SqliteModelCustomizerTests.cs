using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Suite of tests to verify SqliteModelCustomizer functionality.
/// </summary>
public class SqliteModelCustomizerTests
{

    /// <summary>
    /// Verifies that an instance of SqliteModelCustomizer can be created.
    /// </summary>
    [Fact]
    public void Can_create_instance_of_SqliteModelCustomizer()
    {
        // Arrange
        var finder = new DbSetFinder();
        var dependencies = new Microsoft.EntityFrameworkCore.Infrastructure.ModelCustomizerDependencies(finder);

        // Act & Assert
        // ReSharper disable once UnusedVariable
        var sut = new SqliteModelCustomizer(dependencies);
    }



    /// <summary>
    /// Verifies that passing null to the constructor throws ArgumentNullException
    /// </summary>
    [Fact]
    public void Cannot_create_instance_of_SqliteModelCustomizer_with_null_dependencies()
    {
        // Arrange

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new SqliteModelCustomizer(null!));
        Assert.Equal("dependencies", ex.ParamName);
    }



    /// <summary>
    /// Verifies that setting OverrideTableRenameRenaming property to null throws ArgumentNullException
    /// </summary>
    [Fact]
    public void OverrideTableRenameRenaming_when_set_to_null_throws_ArgumentNullException()
    {
        // Arrange
        var finder = new DbSetFinder();
        var dependencies = new Microsoft.EntityFrameworkCore.Infrastructure.ModelCustomizerDependencies(finder);
        var sut = new SqliteModelCustomizer(dependencies);

        var ex = Assert.Throws<ArgumentNullException>(() => sut.OverrideTableRenameRenaming = null!);
        Assert.Equal("value", ex.ParamName);
    }



    /// <summary>
    /// Verifies that tables get renamed properly using default implementation of OverrideTableRenameRenaming.
    /// </summary>
    [Fact]
    public void Can_rename_table_with_default_implementation()
    {
        // Arrange
        var finder = new DbSetFinder();
        var dependencies = new Microsoft.EntityFrameworkCore.Infrastructure.ModelCustomizerDependencies(finder);
        var sut = new SqliteModelCustomizer(dependencies);

        // Act & Assert
        var table1 = (SchemaName: "Personnel", TableName: "Person");
        var renamedTable1 = sut.OverrideTableRenameRenaming(table1);
        Assert.Equal("Personnel_Person", renamedTable1);

        var table2 = (SchemaName: (string?)null, TableName: "Person");
        var renamedTable2 = sut.OverrideTableRenameRenaming(table2);
        Assert.Equal("dbo_Person", renamedTable2);

        var table3 = (SchemaName: "dbo", TableName: "Person");
        var renamedTable3 = sut.OverrideTableRenameRenaming(table3);
        Assert.Equal("dbo_Person", renamedTable3);

        // Verify that recursive renaming is avoided
        var table4 = (SchemaName: "dbo", TableName: "dbo_Person");
        var renamedTable4 = sut.OverrideTableRenameRenaming(table4);
        Assert.Equal("dbo_Person", renamedTable4);
    }



    /// <summary>
    /// Verifies that tables get renamed properly using a custom implementation of OverrideTableRenameRenaming.
    /// </summary>
    [Fact]
    public void Can_rename_table_with_custom_implementation()
    {
        // Arrange
        var finder = new DbSetFinder();
        var dependencies = new Microsoft.EntityFrameworkCore.Infrastructure.ModelCustomizerDependencies(finder);
        var sut = new SqliteModelCustomizer(dependencies);

        // Act
        sut.OverrideTableRenameRenaming = tuple =>
        {
            var schemaPrefix = $"{tuple.SchemaName ?? "dbo"}$";

            return tuple.TableName.StartsWith(schemaPrefix, StringComparison.OrdinalIgnoreCase)
                ? tuple.TableName
                : $"{tuple.SchemaName ?? "dbo"}${tuple.TableName}";
        };

        // Assert
        var table1 = (SchemaName: "Personnel", TableName: "Person");
        var renamedTable1 = sut.OverrideTableRenameRenaming(table1);
        Assert.Equal("Personnel$Person", renamedTable1);

        var table2 = (SchemaName: (string?)null, TableName: "Person");
        var renamedTable2 = sut.OverrideTableRenameRenaming(table2);
        Assert.Equal("dbo$Person", renamedTable2);

        var table3 = (SchemaName: "dbo", TableName: "Person");
        var renamedTable3 = sut.OverrideTableRenameRenaming(table3);
        Assert.Equal("dbo$Person", renamedTable3);

        // Verify that recursive renaming is avoided
        var table4 = (SchemaName: "dbo", TableName: "dbo$Person");
        var renamedTable4 = sut.OverrideTableRenameRenaming(table4);
        Assert.Equal("dbo$Person", renamedTable4);
    }



}
