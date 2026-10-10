using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.DbContextBuilderEF6.Tests.Unit.Models;
using Xunit;

namespace Wolfgang.DbContextBuilderEF6.Tests.Unit;

public class DbContextBuilderTests
{
    /// <summary>
    /// The entity counts the SeedWithRandom theories run with (#602).
    /// </summary>
    public static TheoryData<int> SeedCounts => new() { 1, 3, 7 };



    // Build and BuildAsync each create two contexts: the temporary seed context and the one returned.
    private const int ContextsCreatedPerBuild = 2;



    private static DbContextBuilder<TestDbContext> CreateDbContextBuilder() =>
        new DbContextBuilder<TestDbContext>()
            .UseEffort()
            .UseAutoFixture();



    /// <summary>
    /// Verifies that calling Build returns an instance of the specified DbContext type.
    /// </summary>
    [Fact]
    public void Build_when_called_returns_an_instance_of_the_context_type()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        // Act
        using var context = sut.Build();

        // Assert
        Assert.NotNull(context);
        Assert.IsType<TestDbContext>(context);
    }



    /// <summary>
    /// Verifies that calling BuildAsync returns an instance of the specified DbContext type.
    /// </summary>
    [Fact]
    public async Task BuildAsync_when_called_returns_an_instance_of_the_context_type()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        // Act
        using var context = await sut.BuildAsync();

        // Assert
        Assert.NotNull(context);
        Assert.IsType<TestDbContext>(context);
    }



    /// <summary>
    /// Verifies that calling UseEffort returns the DbContextBuilder instance to allow for method chaining.
    /// </summary>
    [Fact]
    public void UseEffort_when_called_returns_the_builder()
    {
        // Arrange
        var sut = new DbContextBuilder<TestDbContext>();

        // Act
        var result = sut.UseEffort();

        // Assert
        Assert.IsType<DbContextBuilder<TestDbContext>>(result);
    }



    /// <summary>
    /// Verifies that calling UseAutoFixture returns the DbContextBuilder instance to allow for method chaining.
    /// </summary>
    [Fact]
    public void UseAutoFixture_when_called_returns_the_builder()
    {
        // Arrange
        var sut = new DbContextBuilder<TestDbContext>();

        // Act
        var result = sut.UseAutoFixture();

        // Assert
        Assert.IsType<DbContextBuilder<TestDbContext>>(result);
    }



    /// <summary>
    /// Verifies that the RandomEntityCreator used is an instance of AutoFixtureRandomEntityCreator.
    /// </summary>
    [Fact]
    public void Ctor_when_called_defaults_RandomEntityCreator_to_AutoFixture()
    {
        // Arrange
        var sut = new DbContextBuilder<TestDbContext>();

        // Act & Assert
        Assert.IsType<AutoFixtureRandomEntityCreator>(sut.RandomEntityCreator);
    }



    /// <summary>
    /// Verifies that calling UseCustomRandomEntityCreator returns the
    /// DbContextBuilder instance to allow for method chaining.
    /// </summary>
    [Fact]
    public void UseCustomRandomEntityCreator_when_called_returns_the_builder()
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        var creator = new AutoFixtureRandomEntityCreator();

        // Act
        var result = sut.UseCustomRandomEntityCreator(creator);

        // Assert
        Assert.IsType<DbContextBuilder<TestDbContext>>(result);
    }



    /// <summary>
    /// Verifies that calling UseCustomRandomEntityCreator and passing null
    /// throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void UseCustomRandomEntityCreator_when_creator_is_null_throws_ArgumentNullException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.UseCustomRandomEntityCreator(null!));
        Assert.Equal("creator", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling UseCustomRandomEntityCreator sets the RandomEntityCreator property.
    /// </summary>
    [Fact]
    public void UseCustomRandomEntityCreator_when_called_sets_the_RandomEntityCreator_property()
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        var creator = new AutoFixtureRandomEntityCreator();

        // Act
        sut.UseCustomRandomEntityCreator(creator);

        // Assert
        Assert.Same(creator, sut.RandomEntityCreator);
    }



    /// <summary>
    /// Verifies that a newly created DbContext contains the mapped entities
    /// but the sets are empty as no data has been seeded.
    /// </summary>
    [Fact]
    public void Build_when_no_seeds_are_provided_returns_a_context_with_mapped_but_empty_sets()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        // Act
        using var context = sut.Build();

        // Assert
        Assert.NotNull(context);
        Assert.Empty(context.Products);
        Assert.Empty(context.Categories);
    }



    /// <summary>
    /// Verifies that a newly created DbContext does not have any tracked changes.
    /// </summary>
    [Fact]
    public void Build_when_called_returns_a_context_with_no_tracked_changes()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var product = new Product
        {
            Name = "Widget",
            Price = 9.99m,
            Quantity = 100,
            CreatedDate = DateTime.UtcNow
        };

        // Act
        using var context = sut
            .SeedWith(product)
            .Build();

        // Assert
        Assert.False(context.ChangeTracker.HasChanges());
    }



    #region SeedWith(IEnumerable<T>)

    /// <summary>
    /// Verifies that passing null into SeedWith(IEnumerable{T}) throws an ArgumentNullException.
    /// </summary>
    [Fact]
    public void SeedWith_IEnumerable_when_passed_null_throws_ArgumentNullException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        IEnumerable<Product> entities = null!;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.SeedWith(entities));
        Assert.Equal("entities", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWith(IEnumerable{T}) with a list containing a null item
    /// throws ArgumentException.
    /// </summary>
    [Fact]
    public void SeedWith_IEnumerable_when_passed_list_of_values_and_one_is_null_throws_ArgumentException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var products = new List<Product>
        {
            new Product { Name = "Widget", Price = 9.99m },
            null!,
            new Product { Name = "Gadget", Price = 19.99m }
        };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => sut.SeedWith(products.AsEnumerable()));
        Assert.StartsWith("One of the entities is null", ex.Message, StringComparison.Ordinal);
        Assert.Equal("entities", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWith(IEnumerable{T}) with strings throws ArgumentException.
    /// </summary>
    [Fact]
    public void SeedWith_IEnumerable_when_passed_a_list_of_strings_throws_ArgumentException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        var invalidValues = new List<string> { "Dog", "Cat", "Bird" };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => sut.SeedWith(invalidValues.AsEnumerable()));
        Assert.StartsWith("The type of TEntity cannot be string", ex.Message, StringComparison.Ordinal);
        Assert.Equal("entities", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWith(IEnumerable{T}) returns the DbContextBuilder for chaining.
    /// </summary>
    [Fact]
    public void SeedWith_IEnumerable_returns_DbContextBuilder()
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        var products = new[] { new Product { Name = "Widget", Price = 9.99m } };

        // Act
        var result = sut.SeedWith(products.AsEnumerable());

        // Assert
        Assert.IsType<DbContextBuilder<TestDbContext>>(result);
    }



    /// <summary>
    /// Verifies that a newly created DbContext contains the data it was seeded with.
    /// </summary>
    [Fact]
    public void SeedWith_IEnumerable_seeds_DbContext_with_specified_data()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var expectedProduct = new Product
        {
            Name = "Widget",
            Price = 9.99m,
            Quantity = 42,
            CreatedDate = DateTime.UtcNow
        };

        // Act
        using var context = sut
            .SeedWith(new[] { expectedProduct }.AsEnumerable())
            .Build();

        // Assert
        var actual = context.Products.Single();
        Assert.Equal(expectedProduct.Name, actual.Name);
        Assert.Equal(expectedProduct.Price, actual.Price);
        Assert.Equal(expectedProduct.Quantity, actual.Quantity);
    }

    #endregion



    #region SeedWith(params T[])

    /// <summary>
    /// Verifies that calling SeedWith(params T[]) returns the DbContextBuilder for chaining.
    /// </summary>
    [Fact]
    public void SeedWith_params_returns_DbContextBuilder()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var product1 = new Product { Name = "Widget", Price = 9.99m };
        var product2 = new Product { Name = "Gadget", Price = 19.99m };

        // Act
        var result = sut.SeedWith(product1, product2);

        // Assert
        Assert.IsType<DbContextBuilder<TestDbContext>>(result);
    }



    /// <summary>
    /// Verifies that calling SeedWith(params T[]) and passing null throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void SeedWith_params_when_passed_null_throws_ArgumentNullException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        Product[] products = null!;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.SeedWith(products));
        Assert.Equal("entities", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWith(params T[]) with a null item throws ArgumentException.
    /// </summary>
    [Fact]
    public void SeedWith_params_when_passed_list_of_values_and_one_is_null_throws_ArgumentException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var product1 = new Product { Name = "Widget", Price = 9.99m };
        var product2 = new Product { Name = "Gadget", Price = 19.99m };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => sut.SeedWith(product1, null!, product2));
        Assert.StartsWith("One of the entities is null", ex.Message, StringComparison.Ordinal);
        Assert.Equal("entities", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWith(params T[]) with strings throws ArgumentException.
    /// </summary>
    [Fact]
    public void SeedWith_params_when_passed_an_array_of_strings_throws_ArgumentException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        // Act & Assert — two string args bind unambiguously to the params overload
        // (a single string arg now binds to the SeedWith(TEntity) singleton overload).
        var ex = Assert.Throws<ArgumentException>(() => sut.SeedWith("Invalid value", "another value"));
        Assert.StartsWith("One of the entities passed in is of type string", ex.Message, StringComparison.Ordinal);
        Assert.Equal("entities", ex.ParamName);
    }



    /// <summary>
    /// Verifies that a newly created DbContext contains the data it was seeded with.
    /// </summary>
    [Fact]
    public void SeedWith_params_seeds_DbContext_with_specified_data()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var product1 = new Product
        {
            Name = "Widget",
            Price = 9.99m,
            Quantity = 42,
            CreatedDate = DateTime.UtcNow
        };

        var product2 = new Product
        {
            Name = "Gadget",
            Price = 19.99m,
            Quantity = 7,
            CreatedDate = DateTime.UtcNow
        };

        // Act
        using var context = sut
            .SeedWith(product1, product2)
            .Build();

        // Assert
        var actual = context.Products.ToList();
        Assert.Equal(2, actual.Count);
    }

    #endregion



    #region SeedWithRandom<T>(int)

    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int) throws when passed a value less than 1.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_when_passed_value_less_than_1_throws_ArgumentOutOfRangeException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => sut.SeedWithRandom<Product>(0));
        Assert.StartsWith("Count must be greater than 0", ex.Message, StringComparison.Ordinal);
        Assert.Equal("count", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int) returns the DbContextBuilder for chaining.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_returns_DbContextBuilder()
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        const int count = 5;

        // Act
        var result = sut.SeedWithRandom<Product>(count);

        // Assert
        Assert.IsType<DbContextBuilder<TestDbContext>>(result);
    }



    /// <summary>
    /// Verifies that a newly created DbContext contains the specified number of randomly created entities.
    /// </summary>
    [Theory]
    [MemberData(nameof(SeedCounts))]
    public void SeedWithRandom_int_seeds_DbContext_with_specified_number_of_random_entities(int count)
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        using var context = sut
            .SeedWithRandom<Category>(count)
            .Build();

        // Act
        var actual = context.Categories.ToList();

        // Assert
        Assert.Equal(count, actual.Count);
    }

    #endregion



    #region SeedWithRandom<T>(int, Func<T, T>)

    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, Func) throws when passed a value less than 1.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_TEntity_when_passed_value_less_than_1_throws_ArgumentOutOfRangeException()
    {
        // Arrange
        Func<Product, Product> func = null!;
        var sut = CreateDbContextBuilder();

        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => sut.SeedWithRandom(0, func));
        Assert.StartsWith("Count must be greater than 0", ex.Message, StringComparison.Ordinal);
        Assert.Equal("count", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, Func) throws when func is null.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_TEntity_when_passed_null_for_func_throws_ArgumentNullException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        Func<Product, Product> func = null!;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.SeedWithRandom(17, func));
        Assert.Equal("func", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, Func) returns the DbContextBuilder for chaining.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_TEntity_returns_DbContextBuilder()
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        const int count = 5;
        var func = new Func<Product, Product>(p => p);

        // Act
        var result = sut.SeedWithRandom(count, func);

        // Assert
        Assert.IsType<DbContextBuilder<TestDbContext>>(result);
    }



    /// <summary>
    /// Verifies that a newly created DbContext contains the specified number of randomly created entities
    /// with the transformation function applied.
    /// </summary>
    [Theory]
    [MemberData(nameof(SeedCounts))]
    public void SeedWithRandom_int_func_TEntity_TEntity_seeds_DbContext_with_specified_number_of_random_entities(int count)
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        var func = new Func<Product, Product>(p =>
        {
            p.Name = "Modified_" + p.Name;
            return p;
        });

        using var context = sut
            .SeedWithRandom(count, func)
            .Build();

        // Act
        var actual = context.Products.ToList();

        // Assert
        Assert.Equal(count, actual.Count);
        Assert.All(actual, p => Assert.StartsWith("Modified_", p.Name, StringComparison.Ordinal));
    }

    #endregion



    #region SeedWithRandom<T>(int, Func<T, int, T>)

    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, Func) with index throws when passed a value less than 1.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_int_TEntity_when_passed_value_less_than_1_throws_ArgumentOutOfRangeException()
    {
        // Arrange
        Func<Product, int, Product> func = null!;
        var sut = CreateDbContextBuilder();

        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => sut.SeedWithRandom(0, func));
        Assert.StartsWith("Count must be greater than 0", ex.Message, StringComparison.Ordinal);
        Assert.Equal("count", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, Func) with index throws when func is null.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_int_TEntity_when_passed_null_for_func_throws_ArgumentNullException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        Func<Product, int, Product> func = null!;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.SeedWithRandom(17, func));
        Assert.Equal("func", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, Func) with index returns the DbContextBuilder for chaining.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_int_TEntity_returns_DbContextBuilder()
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        const int count = 5;
        var func = new Func<Product, int, Product>((p, _) => p);

        // Act
        var result = sut.SeedWithRandom(count, func);

        // Assert
        Assert.IsType<DbContextBuilder<TestDbContext>>(result);
    }



    /// <summary>
    /// Verifies that a newly created DbContext contains the specified number of randomly created entities
    /// with the index-based transformation function applied.
    /// </summary>
    [Theory]
    [MemberData(nameof(SeedCounts))]
    public void SeedWithRandom_int_func_TEntity_int_TEntity_seeds_DbContext_with_specified_number_of_random_entities(int count)
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        var func = new Func<Product, int, Product>((p, i) =>
        {
            p.Name = $"Product_{i}";
            return p;
        });

        using var context = sut
            .SeedWithRandom(count, func)
            .Build();

        // Act
        var actual = context.Products.ToList();

        // Assert — every entity went through the transform with its own index
        Assert.Equal
        (
            Enumerable.Range(0, count).Select(i => $"Product_{i}"),
            actual.Select(p => p.Name).OrderBy(n => int.Parse(n.Substring("Product_".Length), System.Globalization.CultureInfo.InvariantCulture))
        );
    }

    #endregion



    /// <summary>
    /// Verifies that UseAutoFixture with null builder throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void UseAutoFixture_when_passed_null_throws_ArgumentNullException()
    {
        // Arrange
        DbContextBuilder<TestDbContext> builder = null!;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => builder.UseAutoFixture());
        Assert.Equal("builder", ex.ParamName);
    }



    /// <summary>
    /// Verifies that UseEffort with null builder throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void UseEffort_when_passed_null_throws_ArgumentNullException()
    {
        // Arrange
        DbContextBuilder<TestDbContext> builder = null!;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => builder.UseEffort());
        Assert.Equal("builder", ex.ParamName);
    }



    /// <summary>
    /// Verifies that Build without configuring a provider defaults to Effort.
    /// </summary>
    [Fact]
    public void Build_without_configuring_provider_defaults_to_Effort()
    {
        // Arrange
        var sut = new DbContextBuilder<TestDbContext>()
            .UseAutoFixture();

        // Act
        using var context = sut.Build();

        // Assert
        Assert.NotNull(context);
        Assert.IsType<TestDbContext>(context);
    }



    /// <summary>
    /// Verifies that BuildAsync without configuring a provider defaults to Effort.
    /// </summary>
    [Fact]
    public async Task BuildAsync_without_configuring_provider_defaults_to_Effort()
    {
        // Arrange
        var sut = new DbContextBuilder<TestDbContext>()
            .UseAutoFixture();

        // Act
        using var context = await sut.BuildAsync();

        // Assert
        Assert.NotNull(context);
        Assert.IsType<TestDbContext>(context);
    }



    /// <summary>
    /// Verifies that BuildAsync seeds data correctly.
    /// </summary>
    [Fact]
    public async Task BuildAsync_seeds_DbContext_with_specified_data()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var product = new Product
        {
            Name = "AsyncWidget",
            Price = 5.99m,
            Quantity = 10,
            CreatedDate = DateTime.UtcNow
        };

        // Act
        using var context = await sut
            .SeedWith(product)
            .BuildAsync();

        // Assert
        var actual = context.Products.Single();
        Assert.Equal("AsyncWidget", actual.Name);
    }



    /// <summary>
    /// Verifies that SeedWith(params) flattens a list passed as one of its items into the
    /// seed data. Two arguments force the params overload: a lone list binds to the
    /// singleton overload instead, which never reaches the params flattening branch.
    /// </summary>
    [Fact]
    public void SeedWith_params_when_passed_a_list_flattens_into_seed_data()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var products = new List<Product>
        {
            new Product { Name = "Widget", Price = 9.99m, CreatedDate = DateTime.UtcNow },
            new Product { Name = "Gadget", Price = 19.99m, CreatedDate = DateTime.UtcNow }
        };
        var category = new Category { Name = "Electronics" };

        // Act
        using var context = sut
            .SeedWith<object>(products, category)
            .Build();

        // Assert
        Assert.Equal
        (
            new[] { "Gadget", "Widget" },
            context.Products.Select(p => p.Name).OrderBy(n => n).ToArray()
        );
        Assert.Single(context.Categories);
    }



    /// <summary>
    /// Verifies that a lone list binds to the singleton SeedWith overload, which seeds
    /// every item of the sequence rather than the list itself.
    /// </summary>
    [Fact]
    public void SeedWith_singleton_overload_when_passed_a_list_seeds_every_item()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var products = new List<Product>
        {
            new Product { Name = "Widget", Price = 9.99m, CreatedDate = DateTime.UtcNow },
            new Product { Name = "Gadget", Price = 19.99m, CreatedDate = DateTime.UtcNow }
        };

        // Act
        using var context = sut
            .SeedWith(products)
            .Build();

        // Assert
        Assert.Equal
        (
            new[] { "Gadget", "Widget" },
            context.Products.Select(p => p.Name).OrderBy(n => n).ToArray()
        );
    }



    /// <summary>
    /// Verifies that the singleton SeedWith overload rejects a null entity with
    /// ArgumentNullException naming the parameter.
    /// </summary>
    [Fact]
    public void SeedWith_singleton_overload_when_passed_null_throws_ArgumentNullException()
    {
        var sut = new DbContextBuilder<TestDbContext>();

        var ex = Assert.Throws<ArgumentNullException>(() => sut.SeedWith((Product)null!));
        Assert.Equal("entity", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling UseEffort multiple times doesn't cause issues.
    /// </summary>
    [Fact]
    public void UseEffort_when_called_multiple_times_still_builds_a_context()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        // Act
        using var context = sut
            .UseEffort()
            .UseEffort()
            .Build();

        // Assert
        Assert.NotNull(context);
    }



    /// <summary>
    /// Verifies that seeding with multiple entity types works.
    /// </summary>
    [Fact]
    public void SeedWith_when_called_with_different_entity_types_seeds_each_set()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var product = new Product
        {
            Name = "Widget",
            Price = 9.99m,
            Quantity = 42,
            CreatedDate = DateTime.UtcNow
        };

        var category = new Category
        {
            Name = "Electronics"
        };

        // Act
        using var context = sut
            .SeedWith(product)
            .SeedWith(category)
            .Build();

        // Assert
        Assert.Single(context.Products);
        Assert.Single(context.Categories);
    }



    /// <summary>
    /// Regression: the singleton overload accepts a single TEntity, but
    /// <c>List&lt;string&gt;</c> casts to <c>IEnumerable&lt;object&gt;</c> at runtime
    /// (T-covariance for reference types) — so without an element-level check, a list
    /// of strings would slip through as silent seed data. Mirrors the Core builder's
    /// regression test.
    /// </summary>
    [Fact]
    public void SeedWith_singleton_overload_when_passed_a_List_of_strings_throws()
    {
        var sut = new DbContextBuilder<TestDbContext>();
        var stringList = new List<string> { "a", "b", "c" };

        var ex = Assert.Throws<ArgumentException>(() => sut.SeedWith(stringList));
        Assert.StartsWith("One of the entities passed in is of type string", ex.Message, StringComparison.Ordinal);
        Assert.Equal("entity", ex.ParamName);
    }



    /// <summary>
    /// Regression (#560): a list containing a null binds to the singleton overload; the null is
    /// rejected at seed time, atomically, instead of failing later inside EF.
    /// </summary>
    [Fact]
    public void SeedWith_singleton_overload_when_passed_a_List_with_a_null_item_throws_and_seeds_nothing()
    {
        var sut = CreateDbContextBuilder();
        var products = new List<Product> { new Product { Name = "Widget", Price = 9.99m, CreatedDate = DateTime.UtcNow }, null! };

        var ex = Assert.Throws<ArgumentException>(() => sut.SeedWith(products));
        Assert.StartsWith("One of the entities is null", ex.Message, StringComparison.Ordinal);
        Assert.Equal("entity", ex.ParamName);

        using var context = sut.Build();
        Assert.Empty(context.Products);
    }



    /// <summary>
    /// Regression: when the caller widens TEntity to <see cref="object"/> the
    /// static `typeof(TEntity) == typeof(string)` check (the original guard) would
    /// pass through. The runtime <c>entity is string</c> check must still reject.
    /// </summary>
    [Fact]
    public void SeedWith_singleton_overload_when_TEntity_is_widened_to_object_still_rejects_string()
    {
        var sut = new DbContextBuilder<TestDbContext>();

        var ex = Assert.Throws<ArgumentException>(() => sut.SeedWith<object>("not an entity"));
        Assert.StartsWith("One of the entities passed in is of type string", ex.Message, StringComparison.Ordinal);
        Assert.Equal("entity", ex.ParamName);
    }



    /// <summary>
    /// UseAutoFixture replaces a creator set earlier, so it switches back to AutoFixture rather
    /// than being a no-op on top of the default.
    /// </summary>
    [Fact]
    public void UseAutoFixture_replaces_a_previously_set_random_entity_creator()
    {
        var custom = new AutoFixtureRandomEntityCreator(new AutoFixture.Fixture());
        var sut = new DbContextBuilder<TestDbContext>().UseCustomRandomEntityCreator(custom);

        sut.UseAutoFixture();

        Assert.IsType<AutoFixtureRandomEntityCreator>(sut.RandomEntityCreator);
        Assert.NotSame(custom, sut.RandomEntityCreator);
    }



    /// <summary>
    /// Build uses a context creator already set on the builder instead of making a new Effort
    /// one, so a caller-supplied creator is honored.
    /// </summary>
    [Fact]
    public void Build_uses_a_context_creator_already_set_on_the_builder()
    {
        using var creator = new CountingDbContextCreator();
        var sut = new DbContextBuilder<TestDbContext> { CreateDbContext = creator };

        using var context = sut.Build();

        Assert.Equal(ContextsCreatedPerBuild, creator.Created);
        Assert.Same(creator, sut.CreateDbContext);
    }



    /// <summary>
    /// BuildAsync uses a context creator already set on the builder instead of making a new
    /// Effort one, so a caller-supplied creator is honored.
    /// </summary>
    [Fact]
    public async Task BuildAsync_uses_a_context_creator_already_set_on_the_builder()
    {
        using var creator = new CountingDbContextCreator();
        var sut = new DbContextBuilder<TestDbContext> { CreateDbContext = creator };

        using var context = await sut.BuildAsync();

        Assert.Equal(ContextsCreatedPerBuild, creator.Created);
        Assert.Same(creator, sut.CreateDbContext);
    }



    /// <summary>
    /// Build saves only when there is seed data, so a build without any does not run the
    /// context's SaveChanges override (auditing, timestamps) (#531).
    /// </summary>
    [Fact]
    public void Build_without_seed_data_does_not_call_SaveChanges()
    {
        var saves = SaveCountingContext.StartCounting();
        var sut = new DbContextBuilder<SaveCountingContext>();

        using var context = sut.Build();

        Assert.Equal(0, saves.Value);
    }



    /// <summary>
    /// The counterpart of <see cref="Build_without_seed_data_does_not_call_SaveChanges"/>: with
    /// seed data, Build saves once, through the context's override.
    /// </summary>
    [Fact]
    public void Build_with_seed_data_calls_SaveChanges_once()
    {
        var saves = SaveCountingContext.StartCounting();
        var sut = new DbContextBuilder<SaveCountingContext>().SeedWith(new Category { Name = "seeded" });

        using var context = sut.Build();

        Assert.Equal(1, saves.Value);
        Assert.Equal("seeded", Assert.Single(context.Categories).Name);
    }



    /// <summary>
    /// BuildAsync saves only when there is seed data, so a build without any does not run the
    /// context's SaveChangesAsync override (#531).
    /// </summary>
    [Fact]
    public async Task BuildAsync_without_seed_data_does_not_call_SaveChangesAsync()
    {
        var saves = SaveCountingContext.StartCounting();
        var sut = new DbContextBuilder<SaveCountingContext>();

        using var context = await sut.BuildAsync();

        Assert.Equal(0, saves.Value);
    }



    /// <summary>
    /// The counterpart of <see cref="BuildAsync_without_seed_data_does_not_call_SaveChangesAsync"/>:
    /// with seed data, BuildAsync saves once, through the context's override.
    /// </summary>
    [Fact]
    public async Task BuildAsync_with_seed_data_calls_SaveChangesAsync_once()
    {
        var saves = SaveCountingContext.StartCounting();
        var sut = new DbContextBuilder<SaveCountingContext>().SeedWith(new Category { Name = "seeded" });

        using var context = await sut.BuildAsync();

        Assert.Equal(1, saves.Value);
        Assert.Equal("seeded", Assert.Single(context.Categories).Name);
    }



    /// <summary>
    /// #568: a database that cannot be created surfaces from Build as the builder's own
    /// <see cref="InvalidOperationException"/>, with EF's failure as the inner exception.
    /// </summary>
    [Fact]
    public void Build_when_the_database_cannot_be_created_wraps_the_EF_failure()
    {
        using var creator = new DisposedContextCreator();
        var sut = new DbContextBuilder<TestDbContext> { CreateDbContext = creator };

        var ex = Assert.Throws<InvalidOperationException>(() => sut.Build());

        Assert.StartsWith("Failed to create database. See InnerException for details.", ex.Message, StringComparison.Ordinal);
        Assert.IsAssignableFrom<InvalidOperationException>(ex.InnerException);
    }



    /// <summary>
    /// #568: the same wrapping applies to BuildAsync.
    /// </summary>
    [Fact]
    public async Task BuildAsync_when_the_database_cannot_be_created_wraps_the_EF_failure()
    {
        using var creator = new DisposedContextCreator();
        var sut = new DbContextBuilder<TestDbContext> { CreateDbContext = creator };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.BuildAsync());

        Assert.StartsWith("Failed to create database. See InnerException for details.", ex.Message, StringComparison.Ordinal);
        Assert.IsAssignableFrom<InvalidOperationException>(ex.InnerException);
    }
}



/// <summary>
/// Returns contexts that are already disposed, so <c>Database.CreateIfNotExists()</c> throws
/// EF6's <see cref="InvalidOperationException"/> inside the builder's InitializeDatabase (#568).
/// </summary>
internal sealed class DisposedContextCreator : ICreateDbContext
{
    private readonly EffortDbContextCreator _inner = new();



    public TDbContext CreateDbContext<TDbContext>() where TDbContext : System.Data.Entity.DbContext
    {
        var context = _inner.CreateDbContext<TDbContext>();
        context.Dispose();
        return context;
    }



    public void Dispose() => _inner.Dispose();
}



/// <summary>Wraps <see cref="EffortDbContextCreator"/> and counts the contexts it creates.</summary>
internal sealed class CountingDbContextCreator : ICreateDbContext
{
    private readonly EffortDbContextCreator _inner = new();



    public int Created { get; private set; }



    public TDbContext CreateDbContext<TDbContext>() where TDbContext : System.Data.Entity.DbContext
    {
        Created++;
        return _inner.CreateDbContext<TDbContext>();
    }



    public void Dispose() => _inner.Dispose();
}
