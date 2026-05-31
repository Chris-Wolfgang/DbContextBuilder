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



    /// <summary>
    /// Verifies that DefaultValueMap starts as an empty dictionary on a freshly-constructed
    /// SqliteModelCustomizer (consumers add their own SQL Server → SQLite default-value
    /// mappings).
    /// </summary>
    [Fact]
    public void DefaultValueMap_is_empty_on_construction()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        // Act
        var sut = new SqliteModelCustomizer(dependencies);

        // Assert
        Assert.Empty(sut.DefaultValueMap);
    }



    /// <summary>
    /// Verifies that DefaultValueMap is keyed case-insensitively (OrdinalIgnoreCase) so SQL
    /// Server defaults like "(GETDATE())" and "(getdate())" both match the same entry.
    /// </summary>
    [Fact]
    public void DefaultValueMap_key_lookup_is_case_insensitive()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteModelCustomizer(dependencies);
        sut.DefaultValueMap["(GETDATE())"] = "datetime('now')";

        // Act & Assert — the same key in different casings should resolve to the same value
        Assert.True(sut.DefaultValueMap.ContainsKey("(GETDATE())"));
        Assert.True(sut.DefaultValueMap.ContainsKey("(getdate())"));
        Assert.True(sut.DefaultValueMap.ContainsKey("(GetDate())"));
        Assert.Equal("datetime('now')", sut.DefaultValueMap["(getdate())"]);
    }



    /// <summary>
    /// Verifies that the default <see cref="SqliteModelCustomizer.OverrideDefaultValueHandling"/>
    /// implementation returns the mapped replacement when the input value is present in
    /// DefaultValueMap.
    /// </summary>
    [Fact]
    public void OverrideDefaultValueHandling_default_impl_uses_DefaultValueMap_when_value_is_mapped()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteModelCustomizer(dependencies);
        sut.DefaultValueMap["(my_custom_sproc())"] = "datetime('now', '+1 day')";

        // Act
        var result = sut.OverrideDefaultValueHandling("(my_custom_sproc())");

        // Assert
        Assert.Equal("datetime('now', '+1 day')", result);
    }



    /// <summary>
    /// Verifies that the default <see cref="SqliteModelCustomizer.OverrideDefaultValueHandling"/>
    /// implementation honours case-insensitive lookups against DefaultValueMap (matching the
    /// dictionary's StringComparer.OrdinalIgnoreCase comparer).
    /// </summary>
    [Fact]
    public void OverrideDefaultValueHandling_default_impl_matches_DefaultValueMap_case_insensitively()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteModelCustomizer(dependencies);
        sut.DefaultValueMap["(GETDATE())"] = "datetime('now')";

        // Act & Assert — same logical value, different casings
        Assert.Equal("datetime('now')", sut.OverrideDefaultValueHandling("(GETDATE())"));
        Assert.Equal("datetime('now')", sut.OverrideDefaultValueHandling("(getdate())"));
        Assert.Equal("datetime('now')", sut.OverrideDefaultValueHandling("(GetDate())"));
    }



    /// <summary>
    /// Verifies that DefaultValueMap is mutable in-place — adding an entry after
    /// construction takes effect for subsequent OverrideDefaultValueHandling calls
    /// (the property exposes the live dictionary, not a snapshot).
    /// </summary>
    [Fact]
    public void DefaultValueMap_additions_take_effect_immediately()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteModelCustomizer(dependencies);

        // Act & Assert — the base SqliteModelCustomizer's default OverrideDefaultValueHandling
        // returns the input unchanged when no mapping exists (only null is special-cased to
        // null). After adding a mapping, the same input now returns the mapped value. The
        // null-on-miss behaviour mentioned in some commits applies to the derived
        // SqliteForMsSqlServerModelCustomizer, not the base class tested here.
        Assert.Equal("(my_runtime_added())", sut.OverrideDefaultValueHandling("(my_runtime_added())"));
        sut.DefaultValueMap["(my_runtime_added())"] = "1";
        Assert.Equal("1", sut.OverrideDefaultValueHandling("(my_runtime_added())"));
    }



    /// <summary>
    /// Verifies that the default <see cref="SqliteModelCustomizer.OverrideManyToManyTableHandling"/>
    /// implementation does NOT rename an entity with 3 foreign keys. The heuristic targets pure
    /// many-to-many join tables (exactly 2 FKs, 0 navigations); entities with additional FKs
    /// represent richer relationships (e.g. an OrderItem that joins Order, Product, AND Warehouse)
    /// and must keep their declared table name.
    /// </summary>
    [Fact]
    public void OverrideManyToManyTableHandling_default_implementation_does_not_rename_entity_with_three_foreign_keys()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteModelCustomizer(dependencies);

        var modelBuilder = new ModelBuilder();

        modelBuilder.Entity("Order", b =>
        {
            b.Property<int>("Id");
            b.HasKey("Id");
            b.ToTable("Orders");
        });

        modelBuilder.Entity("Product", b =>
        {
            b.Property<int>("Id");
            b.HasKey("Id");
            b.ToTable("Products");
        });

        modelBuilder.Entity("Warehouse", b =>
        {
            b.Property<int>("Id");
            b.HasKey("Id");
            b.ToTable("Warehouses");
        });

        modelBuilder.Entity("OrderItem", b =>
        {
            b.Property<int>("OrderId");
            b.Property<int>("ProductId");
            b.Property<int>("WarehouseId");
            b.HasKey("OrderId", "ProductId", "WarehouseId");
            b.ToTable("OrderItems");
            b.HasOne("Order").WithMany().HasForeignKey("OrderId");
            b.HasOne("Product").WithMany().HasForeignKey("ProductId");
            b.HasOne("Warehouse").WithMany().HasForeignKey("WarehouseId");
        });

        var entity = modelBuilder.Model.FindEntityType("OrderItem")!;

        // Precondition — 3 FKs
        Assert.Equal(3, entity.GetForeignKeys().Count());

        // Act
        sut.OverrideManyToManyTableHandling(entity);

        // Assert — table name unchanged
        Assert.Equal("OrderItems", entity.GetTableName());
    }



    /// <summary>
    /// Verifies that the default <see cref="SqliteModelCustomizer.OverrideManyToManyTableHandling"/>
    /// implementation does NOT rename an entity that has exactly 2 foreign keys but also has at
    /// least one navigation property. Navigations indicate EF treats the entity as first-class
    /// (not a pure many-to-many join), so its declared table name must be preserved.
    /// </summary>
    [Fact]
    public void OverrideManyToManyTableHandling_default_implementation_does_not_rename_entity_with_navigations()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteModelCustomizer(dependencies);

        var modelBuilder = new ModelBuilder();

        modelBuilder.Entity("Author", b =>
        {
            b.Property<int>("Id");
            b.HasKey("Id");
            b.ToTable("Authors");
        });

        modelBuilder.Entity("Book", b =>
        {
            b.Property<int>("Id");
            b.HasKey("Id");
            b.ToTable("Books");
        });

        // Authorship is conceptually a join (2 FKs to Author + Book) but also models
        // first-class data (e.g. a Role string). EF surfaces it with navigations on
        // both sides; the heuristic must not flatten its table name.
        modelBuilder.Entity("Authorship", b =>
        {
            b.Property<int>("AuthorId");
            b.Property<int>("BookId");
            b.Property<string>("Role");
            b.HasKey("AuthorId", "BookId");
            b.ToTable("Authorships");
            b.HasOne("Author", "Author").WithMany().HasForeignKey("AuthorId");
            b.HasOne("Book", "Book").WithMany().HasForeignKey("BookId");
        });

        var entity = modelBuilder.Model.FindEntityType("Authorship")!;

        // Precondition — exactly 2 FKs but navigations present
        Assert.Equal(2, entity.GetForeignKeys().Count());
        Assert.NotEmpty(entity.GetNavigations());

        // Act
        sut.OverrideManyToManyTableHandling(entity);

        // Assert — table name unchanged
        Assert.Equal("Authorships", entity.GetTableName());
    }



    /// <summary>
    /// Verifies the default <see cref="SqliteModelCustomizer.OverrideManyToManyTableHandling"/>
    /// behaviour on a self-referencing many-to-many join: an Employee.Subordinates collection
    /// produces a join entity whose two foreign keys both point back to Employee. The heuristic
    /// matches (2 FKs, 0 navigations) and produces a deterministic name of <c>Employee_Employee</c>.
    /// This pins the behaviour so future refactors don't silently change the self-reference name.
    /// </summary>
    [Fact]
    public void OverrideManyToManyTableHandling_default_implementation_handles_self_referencing_join()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteModelCustomizer(dependencies);

        var modelBuilder = new ModelBuilder();

        modelBuilder.Entity("Employee", b =>
        {
            b.Property<int>("Id");
            b.HasKey("Id");
            b.ToTable("Employees");
        });

        // Self-referencing join: both FKs point to Employee. EF generates this for
        // an Employee.Subordinates many-to-many relationship.
        modelBuilder.Entity("EmployeeSubordinate", b =>
        {
            b.Property<int>("ManagerId");
            b.Property<int>("SubordinateId");
            b.HasKey("ManagerId", "SubordinateId");
            b.ToTable("EmployeeSubordinates");
            b.HasOne("Employee").WithMany().HasForeignKey("ManagerId");
            b.HasOne("Employee").WithMany().HasForeignKey("SubordinateId");
        });

        var entity = modelBuilder.Model.FindEntityType("EmployeeSubordinate")!;

        // Precondition — heuristic should match: 2 FKs, 0 navigations
        Assert.Equal(2, entity.GetForeignKeys().Count());
        Assert.Empty(entity.GetNavigations());

        // Act
        sut.OverrideManyToManyTableHandling(entity);

        // Assert — both FK principals are Employee, so the rename produces Employee_Employee.
        // Consumers who want a more descriptive name (Employee_Subordinate, EmployeeHierarchy,
        // etc.) should assign a custom OverrideManyToManyTableHandling delegate.
        Assert.Equal("Employees_Employees", entity.GetTableName());
    }



    /// <summary>
    /// Documents the happy path explicitly under the "exactly 2 FKs, 0 navigations" precondition
    /// using the new XML doc convention (the original
    /// <c>OverrideManyToManyTableHandling_default_implementation_renames_join_table</c> test
    /// covers the same scenario; this duplication is intentional for documentation value and
    /// will survive any rename of the original).
    /// </summary>
    [Fact]
    public void OverrideManyToManyTableHandling_default_implementation_renames_pure_join_with_two_fks_and_no_navigations()
    {
        // Arrange
#if EF_CORE_6
        var finder = new Mock<IDbSetFinder>().Object;
        var dependencies = new ModelCustomizerDependencies(finder);
#else
        var dependencies = new ModelCustomizerDependencies();
#endif

        var sut = new SqliteModelCustomizer(dependencies);

        var modelBuilder = new ModelBuilder();

        modelBuilder.Entity("Tag", b =>
        {
            b.Property<int>("Id");
            b.HasKey("Id");
            b.ToTable("Tags");
        });

        modelBuilder.Entity("Post", b =>
        {
            b.Property<int>("Id");
            b.HasKey("Id");
            b.ToTable("Posts");
        });

        modelBuilder.Entity("PostTag", b =>
        {
            b.Property<int>("PostId");
            b.Property<int>("TagId");
            b.HasKey("PostId", "TagId");
            b.ToTable("PostTags");
            b.HasOne("Post").WithMany().HasForeignKey("PostId");
            b.HasOne("Tag").WithMany().HasForeignKey("TagId");
        });

        var entity = modelBuilder.Model.FindEntityType("PostTag")!;

        // Precondition — heuristic match
        Assert.Equal(2, entity.GetForeignKeys().Count());
        Assert.Empty(entity.GetNavigations());

        // Act
        sut.OverrideManyToManyTableHandling(entity);

        // Assert — renamed to {LeftPrincipalTable}_{RightPrincipalTable}
        Assert.Equal("Posts_Tags", entity.GetTableName());
    }
}
