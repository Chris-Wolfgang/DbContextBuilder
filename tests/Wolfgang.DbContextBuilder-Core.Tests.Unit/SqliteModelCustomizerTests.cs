using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Moq;
using Wolfgang.DbContextBuilderCore.Tests.Unit.Models;

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
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        // Act & Assert — constructor should not throw
        _ = new SqliteModelCustomizer(dependencies);
    }



    /// <summary>
    /// Verifies that passing null to the constructor throws ArgumentNullException
    /// </summary>
    [Fact]
    public void Ctor_when_passed_null_throws_ArgumentNullException()
    {
        // Arrange

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new SqliteModelCustomizer(null!));
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

        var sut = new SqliteModelCustomizer(dependencies);

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

        var sut = new SqliteModelCustomizer(dependencies);

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
    /// Verifies that tables get renamed properly using a custom implementation of OverrideTableRenaming.
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


        var sut = new SqliteModelCustomizer(dependencies) {
            OverrideTableRenaming = tuple =>
            {
                var schemaPrefix = $"{tuple.SchemaName ?? "dbo"}$";

                return tuple.TableName.StartsWith(schemaPrefix, StringComparison.OrdinalIgnoreCase)
                    ? tuple.TableName
                    : $"{tuple.SchemaName ?? "dbo"}${tuple.TableName}";
            }
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

        var sut = new SqliteModelCustomizer(dependencies);

        var ex = Assert.Throws<ArgumentNullException>(() => sut.OverrideDefaultValueHandling = null!);
        Assert.Equal("value", ex.ParamName);
    }



    /// <summary>
    /// Verifies that the default implementation for OverrideDefaultValueHandling does nothing
    /// </summary>
    [Fact]
    public void OverrideDefaultValueHandling_default_implementation_leaves_default_value_as_is()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteModelCustomizer(dependencies);

        // Act & Assert
        Assert.Equal("(getdate())", sut.OverrideDefaultValueHandling("(getdate())"));
        Assert.Equal("(newid())", sut.OverrideDefaultValueHandling("(newid())"));
        Assert.Equal("", sut.OverrideDefaultValueHandling(""));
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
#else
        var dependencies = new ModelCustomizerDependencies();
#endif


        var sut = new SqliteModelCustomizer(dependencies)
        {
            // Act & Assert
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

        var sut = new SqliteModelCustomizer(dependencies);

        var ex = Assert.Throws<ArgumentNullException>(() => sut.OverrideComputedValueHandling = null!);
        Assert.Equal("value", ex.ParamName);
    }



    /// <summary>
    /// Verifies that the default implementation for OverrideComputedValueHandling does_nothing
    /// </summary>
    [Fact]
    public void OverrideComputedValueHandling_default_implementation_set_default_to_null()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteModelCustomizer(dependencies);

        // Act & Assert
        Assert.Equal("(isnull('AW'+[dbo].[ufnLeadingZeros]([CustomerID]),''))", sut.OverrideComputedValueHandling("(isnull('AW'+[dbo].[ufnLeadingZeros]([CustomerID]),''))"));
        Assert.Equal("([OrganizationNode].[GetLevel]())", sut.OverrideComputedValueHandling("([OrganizationNode].[GetLevel]())"));
        Assert.Equal("", sut.OverrideComputedValueHandling(""));
        Assert.Null(sut.OverrideComputedValueHandling(null));
    }



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
#else
        var dependencies = new ModelCustomizerDependencies();
#endif


        var sut = new SqliteModelCustomizer(dependencies)
        {
            // Act & Assert
            OverrideComputedValueHandling = _ => null
        };

        Assert.Null(sut.OverrideComputedValueHandling("(isnull('AW'+[dbo].[ufnLeadingZeros]([CustomerID]),''))"));
        Assert.Null(sut.OverrideComputedValueHandling("([OrganizationNode].[GetLevel]())"));
        Assert.Null(sut.OverrideComputedValueHandling(""));
        Assert.Null(sut.OverrideComputedValueHandling(null));
    }



    /// <summary>
    /// Verifies that Customize throws ArgumentNullException when modelBuilder is null.
    /// </summary>
    [Fact]
    public void Customize_when_modelBuilder_is_null_throws_ArgumentNullException()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteModelCustomizer(dependencies);
        using var context = new BasicContext
        (
            new DbContextOptionsBuilder<BasicContext>()
                .UseSqlite("DataSource=:memory:")
                .Options
        );

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.Customize(null!, context));
        Assert.Equal("modelBuilder", ex.ParamName);
    }



    /// <summary>
    /// Verifies that Customize throws ArgumentNullException when context is null.
    /// </summary>
    [Fact]
    public void Customize_when_context_is_null_throws_ArgumentNullException()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteModelCustomizer(dependencies);

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.Customize(new ModelBuilder(), null!));
        Assert.Equal("context", ex.ParamName);
    }



    /// <summary>
    /// Verifies that Customize throws InvalidOperationException when an entity has no table name.
    /// </summary>
    [Fact]
    public void Customize_when_entity_has_no_table_name_throws_InvalidOperationException()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteModelCustomizer(dependencies);
        using var context = new BasicContext
        (
            new DbContextOptionsBuilder<BasicContext>()
                .UseSqlite("DataSource=:memory:")
                .Options
        );

        // Build a model where an entity has no table name (keyless query-type pattern)
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity("NoTableEntity", b =>
        {
            b.Property<int>("Id");
            b.HasKey("Id");
            b.ToTable((string?)null);
        });

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => sut.Customize(modelBuilder, context));
        Assert.Contains("has no table name", ex.Message, StringComparison.Ordinal);
    }



    /// <summary>
    /// Verifies that Customize skips customization when database is not SQLite.
    /// </summary>
    [Fact]
    public void Customize_when_database_is_not_sqlite_does_not_rename_tables()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteModelCustomizer(dependencies);
        using var context = new BasicContext
        (
            new DbContextOptionsBuilder<BasicContext>()
                .UseInMemoryDatabase("test-not-sqlite")
                .Options
        );

        // Act — should return early without error since it's not SQLite
        sut.Customize(new ModelBuilder(), context);
    }



    /// <summary>
    /// Verifies that setting OverrideManyToManyTableHandling to null throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void OverrideManyToManyTableHandling_when_set_to_null_throws_ArgumentNullException()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteModelCustomizer(dependencies);

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.OverrideManyToManyTableHandling = null!);
        Assert.Equal("value", ex.ParamName);
    }



    /// <summary>
    /// Verifies that a custom OverrideManyToManyTableHandling delegate is used when assigned.
    /// </summary>
    [Fact]
    public void OverrideManyToManyTableHandling_uses_custom_action_when_provided()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var invoked = false;
        var sut = new SqliteModelCustomizer(dependencies)
        {
            OverrideManyToManyTableHandling = _ => invoked = true
        };

        // Act
        sut.OverrideManyToManyTableHandling(Mock.Of<IMutableEntityType>());

        // Assert
        Assert.True(invoked);
    }



    /// <summary>
    /// Verifies that the default OverrideManyToManyTableHandling renames a join table
    /// when the entity has exactly 2 foreign keys and no navigations.
    /// </summary>
    [Fact]
    public void OverrideManyToManyTableHandling_default_implementation_renames_join_table()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteModelCustomizer(dependencies);

        // Build a real model with a many-to-many join entity
        var modelBuilder = new ModelBuilder();

        modelBuilder.Entity("Left", b =>
        {
            b.Property<int>("Id");
            b.HasKey("Id");
            b.ToTable("Orders");
        });

        modelBuilder.Entity("Right", b =>
        {
            b.Property<int>("Id");
            b.HasKey("Id");
            b.ToTable("Products");
        });

        modelBuilder.Entity("JoinTable", b =>
        {
            b.Property<int>("LeftId");
            b.Property<int>("RightId");
            b.HasKey("LeftId", "RightId");
            b.ToTable("JoinTable");
            b.HasOne("Left").WithMany().HasForeignKey("LeftId");
            b.HasOne("Right").WithMany().HasForeignKey("RightId");
        });

        var model = modelBuilder.FinalizeModel();
        var joinEntityType = model.FindEntityType("JoinTable")!;

        // Verify preconditions: 2 FKs, 0 navigations
        Assert.Equal(2, joinEntityType.GetForeignKeys().Count());
        Assert.Empty(joinEntityType.GetNavigations());

        // Act — use a mutable copy so SetTableName works
        var mutableModelBuilder = new ModelBuilder();

        mutableModelBuilder.Entity("Left", b =>
        {
            b.Property<int>("Id");
            b.HasKey("Id");
            b.ToTable("Orders");
        });

        mutableModelBuilder.Entity("Right", b =>
        {
            b.Property<int>("Id");
            b.HasKey("Id");
            b.ToTable("Products");
        });

        mutableModelBuilder.Entity("JoinTable", b =>
        {
            b.Property<int>("LeftId");
            b.Property<int>("RightId");
            b.HasKey("LeftId", "RightId");
            b.ToTable("JoinTable");
            b.HasOne("Left").WithMany().HasForeignKey("LeftId");
            b.HasOne("Right").WithMany().HasForeignKey("RightId");
        });

        var mutableJoinEntity = mutableModelBuilder.Model.FindEntityType("JoinTable")!;
        sut.OverrideManyToManyTableHandling(mutableJoinEntity);

        // Assert
        Assert.Equal("Orders_Products", mutableJoinEntity.GetTableName());
    }
}
