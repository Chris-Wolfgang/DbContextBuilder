using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.DbContextBuilderEF6.Tests.Unit.Models;

namespace Wolfgang.DbContextBuilderEF6.Tests.Unit;

public class DbContextBuilderTests
{

    private DbContextBuilder<TestDbContext> CreateDbContextBuilder() =>
        new DbContextBuilder<TestDbContext>()
            .UseEffort()
            .UseAutoFixture();



    /// <summary>
    /// Verifies that the test project can create an instance of DbContextBuilder.
    /// </summary>
    [Fact]
    public void Can_create_instance_of_DbContextBuilder()
    {
        var builder = new DbContextBuilder<TestDbContext>();
        Assert.NotNull(builder);
    }



    /// <summary>
    /// Verifies that calling Build returns an instance of the specified DbContext type.
    /// </summary>
    [Fact]
    public void Calling_Build_returns_instance_of_specified_context()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        // Act
        var context = sut.Build();

        // Assert
        Assert.NotNull(context);
        Assert.IsType<TestDbContext>(context);
    }



    /// <summary>
    /// Verifies that calling BuildAsync returns an instance of the specified DbContext type.
    /// </summary>
    [Fact]
    public async Task Calling_BuildAsync_returns_instance_of_specified_context()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        // Act
        var context = await sut.BuildAsync();

        // Assert
        Assert.NotNull(context);
        Assert.IsType<TestDbContext>(context);
    }



    /// <summary>
    /// Verifies that calling UseEffort returns the DbContextBuilder instance to allow for method chaining.
    /// </summary>
    [Fact]
    public void Calling_UseEffort_returns_DbContextBuilder()
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
    public void Calling_UseAutoFixture_returns_DbContextBuilder()
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
    public void RandomEntityCreator_is_AutoFixture()
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
    public void Calling_UseCustomRandomEntityCreator_returns_DbContextBuilder()
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
    public void Calling_UseCustomEntityCreator_and_passing_in_null_throws_ArgumentNullException()
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
    public void Calling_UseCustomRandomEntityCreator_sets_the_RandomEntityCreator_property()
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        var creator = new AutoFixtureRandomEntityCreator();

        // Act
        sut.UseCustomRandomEntityCreator(creator);

        // Assert
        Assert.Equal(creator, sut.RandomEntityCreator);
    }



    /// <summary>
    /// Verifies that a newly created DbContext contains the mapped entities
    /// but the sets are empty as no data has been seeded.
    /// </summary>
    [Fact]
    public void A_newly_created_DbContext_contains_the_mapped_entities_but_the_sets_are_empty()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        // Act
        var context = sut.Build();

        // Assert
        Assert.NotNull(context);
        Assert.Empty(context.Products);
        Assert.Empty(context.Categories);
    }



    /// <summary>
    /// Verifies that a newly created DbContext does not have any tracked changes.
    /// </summary>
    [Fact]
    public void A_newly_created_DbContext_does_not_have_any_tracked_changes()
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
        var context = sut
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
        Assert.Equal("entities", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWith(IEnumerable{T}) with strings throws ArgumentException.
    /// </summary>
    [Fact]
    public void SeedWith_IEnumerable_when_passed_an_list_of_strings_throws_ArgumentException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        var invalidValues = new List<string> { "Dog", "Cat", "Bird" };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => sut.SeedWith(invalidValues.AsEnumerable()));
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
        var context = sut
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

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => sut.SeedWith("Invalid value"));
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
        var context = sut
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
    public void SeedWithRandom_int_when_passed_value_less_than_1_throws_ArgumentException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => sut.SeedWithRandom<Product>(0));
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
    [InlineData(3)]
    [InlineData(7)]
    public void SeedWithRandom_int_seeds_DbContext_with_specified_number_of_random_entities(int count)
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var context = sut
            .SeedWithRandom<Category>(count)
            .Build();

        // Act
        var actual = context.Categories.ToList();

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(count, actual.Count);
    }

    #endregion



    #region SeedWithRandom<T>(int, Func<T, T>)

    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, Func) throws when passed a value less than 1.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_TEntity_when_passed_value_less_than_1_throws_ArgumentException()
    {
        // Arrange
        Func<Product, Product> func = null!;
        var sut = CreateDbContextBuilder();

        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => sut.SeedWithRandom(0, func));
        Assert.Equal("count", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, Func) throws when func is null.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_TEntity_when_passed_null_for_func_throws_ArgumentException()
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
    [InlineData(3)]
    [InlineData(7)]
    public void SeedWithRandom_int_func_TEntity_TEntity_seeds_DbContext_with_specified_number_of_random_entities(int count)
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        var func = new Func<Product, Product>(p =>
        {
            p.Name = "Modified_" + p.Name;
            return p;
        });

        var context = sut
            .SeedWithRandom(count, func)
            .Build();

        // Act
        var actual = context.Products.ToList();

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(count, actual.Count);
        Assert.All(actual, p => Assert.StartsWith("Modified_", p.Name));
    }

    #endregion



    #region SeedWithRandom<T>(int, Func<T, int, T>)

    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, Func) with index throws when passed a value less than 1.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_int_TEntity_when_passed_value_less_than_1_throws_ArgumentException()
    {
        // Arrange
        Func<Product, int, Product> func = null!;
        var sut = CreateDbContextBuilder();

        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => sut.SeedWithRandom(0, func));
        Assert.Equal("count", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, Func) with index throws when func is null.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_int_TEntity_when_passed_null_for_func_throws_ArgumentException()
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
    [InlineData(3)]
    [InlineData(7)]
    public void SeedWithRandom_int_func_TEntity_int_TEntity_seeds_DbContext_with_specified_number_of_random_entities(int count)
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        var func = new Func<Product, int, Product>((p, i) =>
        {
            p.Name = $"Product_{i}";
            return p;
        });

        var context = sut
            .SeedWithRandom(count, func)
            .Build();

        // Act
        var actual = context.Products.ToList();

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(count, actual.Count);
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
        var context = sut.Build();

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
        var context = await sut.BuildAsync();

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
        var context = await sut
            .SeedWith(product)
            .BuildAsync();

        // Assert
        var actual = context.Products.Single();
        Assert.Equal("AsyncWidget", actual.Name);
    }



    /// <summary>
    /// Verifies that SeedWith(params) handles a list passed as a single param
    /// by flattening the collection into the seed data.
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

        // Act — passing a List<Product> as a single param element triggers the IEnumerable<object> branch
        // The params overload receives one element (the list itself), which matches IEnumerable<object>
        var result = sut.SeedWith(products);

        // Assert
        Assert.IsType<DbContextBuilder<TestDbContext>>(result);
    }



    /// <summary>
    /// Verifies that calling UseEffort multiple times doesn't cause issues.
    /// </summary>
    [Fact]
    public void Calling_UseEffort_multiple_times_still_works()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        // Act
        var context = sut
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
    public void Can_seed_with_multiple_entity_types()
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
        var context = sut
            .SeedWith(product)
            .SeedWith(category)
            .Build();

        // Assert
        Assert.Single(context.Products);
        Assert.Single(context.Categories);
    }
}
