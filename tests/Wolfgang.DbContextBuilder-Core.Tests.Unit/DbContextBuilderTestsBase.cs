using System.Text;
using AdventureWorks.Models;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;


namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// A base class that contains all the common unit tests for DbContextBuilder.
/// </summary>
public abstract class DbContextBuilderTestsBase(ITestOutputHelper testOutputHelper)
{

#pragma warning disable IDE0052
	private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;
#pragma warning restore IDE0052

	/// <summary>
    /// Creates an instance of DbContextBuilder with specific database
    /// and random entity generator to be used in the tests
    /// </summary>
    /// <returns></returns>
    protected abstract DbContextBuilder<AdventureWorksDbContext> CreateDbContextBuilder();



    /// <summary>
    /// Verifies that the test project can create an instance of DbContextBuilder can be created.
    /// </summary>
    [Fact]
    public void Can_create_instance_of_DbContextBuilder()
    {
        var builder = new DbContextBuilder<AdventureWorksDbContext>();
        Assert.NotNull(builder);
    }



    /// <summary>
    /// Verifies that calling Build on the DbContextBuilder returns an instance of the specified DbContext type.
    /// </summary>
    [Fact]
    public async Task Calling_Build_returns_instance_of_specified_context()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        // Act
        await using var context = await sut.BuildAsync();

        // Assert
        Assert.NotNull(context);
        Assert.IsType<AdventureWorksDbContext>(context);
    }



    /// <summary>
    /// Verifies that calling Build multiple times on the DbContextBuilder returns multiple distinct instances of the specified DbContext type.
    /// </summary>
    [Fact]
    public async Task Can_create_multiple_instances_of_specified_context()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        // Act
        await using var context1 = await sut.BuildAsync();
        await using var context2 = await sut.BuildAsync();

        // Assert
        Assert.NotNull(context1);
        Assert.NotNull(context2);
        Assert.IsType<AdventureWorksDbContext>(context1);
        Assert.IsType<AdventureWorksDbContext>(context2);
        Assert.NotSame(context1, context2);
    }



    /// <summary>
    /// Verifies that calling UseInMemory returns the DbContextBuilder instance to allow for method chaining.
    /// </summary>
    [Fact]
    public void Calling_UseInMemory_returns_DbContextBuilder()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        // Act
        var result = sut.UseInMemory();

        // Assert
        Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(result);
    }



    /// <summary>
    /// Verifies that calling UseSqlite returns the DbContextBuilder instance to allow for method chaining.
    /// </summary>
    [Fact]
    public void Calling_UseSqlite_returns_DbContextBuilder()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        // Act
        var result = sut.UseSqlite();

        // Assert
        Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(result);
    }

    

    /// <summary>
    /// Verifies that calling UseAutoFixture returns the DbContextBuilder instance to allow for method chaining.
    /// </summary>
    [Fact]
    public void Calling_UseAutoFixture_returns_DbContextBuilder()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        // Act
        var result = sut.UseAutoFixture();

        // Assert
        Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(result);
    }


    
    /// <summary>
    /// Verifies that calling UseCustomRandomEntityGenerator returns the
    /// DbContextBuilder instance to allow for method chaining.
    /// </summary>
    [Fact]
    public void Calling_UseCustomRandomEntityGenerator_returns_DbContextBuilder()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var generator = new AutoFixtureRandomEntityGenerator();

        // Act
        var result = sut.UseCustomRandomEntityGenerator(generator);

        // Assert
        Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(result);
    }



    /// <summary>
    /// Verifies that calling UseCustomRandomEntityGenerator and passing null
    /// throws ArgumentNullException
    /// </summary>
    [Fact]
    public void Calling_UseCustomRandomEntityGenerator_and_passing_in_null_throws_ArgumentNullException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException> (() => sut.UseCustomRandomEntityGenerator(null!));
        Assert.Equal("generator", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling UseCustomRandomEntityGenerator sets Fixture
    /// property to the value passed in
    /// </summary>
    [Fact]
    public void Calling_UseCustomRandomEntityGenerator_sets_the_RandomEntityGenerator_property()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var generator = new AutoFixtureRandomEntityGenerator();

        // Act
        sut.UseCustomRandomEntityGenerator(generator);

        // Assert
        Assert.Equal(generator, sut.RandomEntityGenerator);
    }



    /// <summary>
    /// Verifies that a newly created DbContext contains the mapped entities
    /// as they are defined in the EF configuration, but the sets are empty
    /// as no data has been seeded.
    /// </summary>
    [Fact]
    public async Task A_newly_created_DbContext_contains_the_mapped_entities_but_the_sets_are_empty()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        // Act
        await using var context = await sut
            .BuildAsync();

        // Assert
        Assert.NotNull(context);
        Assert.Empty(context.Addresses);
        Assert.Empty(context.People);
        Assert.Empty(context.Vendors);
    }



    /// <summary>
    /// Verifies that a newly created DbContext does not have any tracked changes.
    /// </summary>
    /// <remarks>
    /// By default, if you add entities to a DbContext, they are tracked as Added. This
    /// ensures that the internals of the Build method is not returning a DbContext with
    /// items that are already being tracked since in a production environment, a newly created
    /// DbContext would not have anything being tracked.
    /// </remarks>
    [Fact]
    public async Task A_newly_created_DbContext_does_not_have_any_tracked_changes()
    {
        // Arrange
        var sut = CreateDbContextBuilder();


        var country = new CountryRegion
        {
            CountryRegionCode = "US",
            Name = "United States",
            ModifiedDate = DateTime.UtcNow
        };

        var territory = new SalesTerritory
        {
            TerritoryId = 1,
            Name = "Northeast",
            CountryRegionCode = "US",
            Group = "North America",
            SalesYtd = 0,
            SalesLastYear = 0,
            CostYtd = 0,
            CostLastYear = 0,
            ModifiedDate = DateTime.UtcNow
        };

        var expectedStates = new List<StateProvince>
        {
            new()
            {
                Name = "New York",
                StateProvinceId = 1,
                Rowguid = Guid.NewGuid(),
                StateProvinceCode = "NY",
                CountryRegionCode = "US",
                TerritoryId = 1
            },
            new()
            {
                Name = "New Jersey",
                StateProvinceId = 2,
                Rowguid = Guid.NewGuid(),
                StateProvinceCode = "NJ",
                CountryRegionCode = "US",
                TerritoryId = 1
            }
        };


        var expectedAddresses = new List<Address>
        {
            new()
            {
                AddressId = 1,
                AddressLine1 = "123 Main St",
                AddressLine2 = "Apt 4B",
                City = "Anytown",
                StateProvinceId = 1,
                PostalCode = "12345",
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.UtcNow
            },
            new()
            {
                AddressId = 2,
                AddressLine1 = "456 Oak St",
                City = "Another town",
                StateProvinceId = 2,
                PostalCode = "67890",
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.UtcNow
            }
        };

        // Act
        await using var context = await sut
            .SeedWith(country)
            .SeedWith(territory)
            .SeedWith(expectedStates)
            .SeedWith(expectedAddresses)
            .BuildAsync();


        // Assert
        Assert.False(context.ChangeTracker.HasChanges());
        Assert.Empty(context.ChangeTracker.Entries());
    }



    /// <summary>
    /// Verifies that passing null into SeedWith(IEnumerable{T}) throws an ArgumentNullException.
    /// </summary>
    [Fact]
    public void SeedWith_IEnumerable_when_passed_null_throws_ArgumentNullException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        IEnumerable<Address> entities = null!;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.SeedWith(entities));
        Assert.Equal("entities", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWith(IEnumerable{T}) returns the DbContextBuilder instance to allow for method chaining.
    /// </summary>
    /// <remarks>This does not yet verify that the data is actually seeded into the context.</remarks>
    [Fact]
    public void SeedWith_IEnumerable_returns_DbContextBuild()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var countries = new[]
        {
            new CountryRegion
            {
                CountryRegionCode = "US",
                Name = "United States",
                ModifiedDate = DateTime.UtcNow
            }
        };

        // Act
        var result = sut.SeedWith(countries.AsEnumerable());

        // Assert
        Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(result);
    }



    /// <summary>
    /// Verifies that a newly created DbContext contains the data it was seeded with.
    /// </summary>
    [Fact]
    public async Task SeedsWith_IEnumerable_seeds_DbContext_with_specified_data()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var expectedCountry = new CountryRegion
        {
            CountryRegionCode = "US",
            Name = "United States",
            ModifiedDate = DateTime.UtcNow,
            StateProvinces = [],
            SalesTerritories = [],
            CountryRegionCurrencies = [],
        };
        var seedCountry = expectedCountry with { };

        // Act
        await using var context = await sut
            .SeedWith(seedCountry)
            .BuildAsync();


        // Assert

        var actualCountry = context
            .CountryRegions
            .Single();
        actualCountry.ModifiedDate = DateTime.SpecifyKind(actualCountry.ModifiedDate, DateTimeKind.Utc);

        Assert.Equivalent(expectedCountry, actualCountry);
    }



    /// <summary>
    /// Verifies that calling SeedWith(params T[]) returns the DbContextBuilder instance to allow for method chaining.
    /// </summary>
    /// <remarks>This does not yet verify that the data is actually seeded into the context.</remarks>
    [Fact]
    public void SeedWith_params_returns_DbContextBuild()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var address1 = new Address
        {
            AddressId = 1,
            AddressLine1 = "123 Main St",
            AddressLine2 = "Apt 4B",
            City = "Anytown",
            StateProvinceId = 1,
            PostalCode = "12345",
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DateTime.UtcNow
        };

        var address2 = new Address
        {
            AddressId = 2,
            AddressLine1 = "456 Oak St",
            City = "Another town",
            StateProvinceId = 2,
            PostalCode = "67890",
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DateTime.UtcNow
        };

        // Act
        var result = sut.SeedWith(address1, address2);

        // Assert
        Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(result);
    }



    /// <summary>
    /// Verifies that calling SeedWith(params T[]) and passing in null throws an ArgumentNullException.
    /// </summary>
    [Fact]
    public void SeedWith_params_when_passed_null_throws_ArgumentNullException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        Address[] addresses = null!;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.SeedWith(addresses));
        Assert.Equal("entities", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWith(params T[]) with a list of values
    /// i.e. SeedWith(address1, address2, address3), null throws ArgumentException
    /// if any of the values is null.
    /// </summary>
    /// <remarks>This does not yet verify that the data is actually seeded into the context.</remarks>
    [Fact]
    public void SeedWith_params_when_passed_list_of_values_and_one_is_null_throws_ArgumentException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var address1 = new Address
        {
            AddressId = 1,
            AddressLine1 = "123 Main St",
            AddressLine2 = "Apt 4B",
            City = "Anytown",
            StateProvinceId = 1,
            PostalCode = "12345",
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DateTime.UtcNow
        };


        var address2 = new Address
        {
            AddressId = 2,
            AddressLine1 = "456 Oak St",
            City = "Another town",
            StateProvinceId = 2,
            PostalCode = "67890",
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DateTime.UtcNow
        };


        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => sut.SeedWith(address1, null!, address2));
        Assert.Equal("entities", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWith(params T[]) with a mix of lists,
    /// throws an ArgumentException if any of the values is null.
    /// </summary>
    [Fact]
    public void SeedWith_params_when_passed_mix_of_values_and_list_of_values_and_one_is_null_throws_ArgumentException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var addressList = new[]
        {
            new Address
            {
                AddressId = 1,
                AddressLine1 = "123 Main St",
                AddressLine2 = "Apt 4B",
                City = "Anytown",
                StateProvinceId = 1,
                PostalCode = "12345",
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.UtcNow
            },
            new Address
            {
                AddressId = 2,
                AddressLine1 = "456 Oak St",
                City = "Another town",
                StateProvinceId = 2,
                PostalCode = "67890",
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.UtcNow
            }
        };


        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => sut.SeedWith(null!, addressList));
        Assert.Equal("entities", ex.ParamName);
    }



    /// <summary>
    /// Verifies that when calling SeedWith(params T[]), if any of the elements in
    /// the array is null throws an ArgumentException.
    /// </summary>
    [Fact]
    public void SeedWith_params_when_passed_an_array_of_values_and_one_of_the_elements_is_null_throws_ArgumentException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var addressList = new[]
        {
            new Address
            {
                AddressId = 1,
                AddressLine1 = "123 Main St",
                AddressLine2 = "Apt 4B",
                City = "Anytown",
                StateProvinceId = 1,
                PostalCode = "12345",
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.UtcNow
            },

            null, // Verify method checks for null values

            new Address
            {
                AddressId = 2,
                AddressLine1 = "456 Oak St",
                City = "Another town",
                StateProvinceId = 2,
                PostalCode = "67890",
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.UtcNow
            }
        };


        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => sut.SeedWith<Address>(addressList!));
        Assert.Equal("entities", ex.ParamName);
    }



    /// <summary>
    /// Verifies that when calling SeedWith(params T[]), if any of the elements in
    /// the array is a string, throws an ArgumentException.
    /// </summary>
    /// <remarks>
    /// This test is needed because the params keyword allows passing in an array of objects
    /// with the restriction that the object must be classes and string is a class
    /// </remarks>
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
    public async Task SeedsWith_params_seeds_DbContext_with_specified_data()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var expectedCountry = new List<CountryRegion>
        {
            new()
            {
                CountryRegionCode = "US",
                Name = "United States",
                ModifiedDate = DateTime.UtcNow,
                StateProvinces = [],
                SalesTerritories = [],
                CountryRegionCurrencies = [],
            },
            new()
            {
                CountryRegionCode = "CA",
                Name = "Canada",
                ModifiedDate = DateTime.UtcNow,
                StateProvinces = [],
                SalesTerritories = [],
                CountryRegionCurrencies = [],
            },
            new()
            {
                CountryRegionCode = "MX",
                Name = "Mexico",
                ModifiedDate = DateTime.UtcNow,
                StateProvinces = [],
                SalesTerritories = [],
                CountryRegionCurrencies = [],
            }
        };
        var seedCountry = expectedCountry.Select(c => c with { });

        // Act
        await using var context = await sut
            .SeedWith(seedCountry.AsEnumerable())
            .BuildAsync();


        // Assert

        var actualCountry = context
            .CountryRegions
            .ToList()
            // Sqlite store the data time as unknown so we need to tell .net that it is UTC
            .Select(c => c with { ModifiedDate = DateTime.SpecifyKind(c.ModifiedDate, DateTimeKind.Utc) })
            .ToArray();

        Assert.Equivalent(expectedCountry, actualCountry);
    }



    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int) throws ArgumentOutOfRangeException when passed a value less than 1.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_when_passed_value_less_than_1_throws_ArgumentException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => sut.SeedWithRandom<Address>(0));
        Assert.Equal("count", ex.ParamName);
    }


    
    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int) returns the DbContextBuilder instance to allow for method chaining.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_returns_DbContextBuilder()
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        const int count = 5;

        // Act
        var result = sut.SeedWithRandom<Address>(count);

		// Assert
		_ = Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(result);
    }



    /// <summary>
    /// Verifies that a newly created DbContext contains the specified number of randomly generated entities.
    /// </summary>
    [Theory]
    [InlineData(7)]
    [InlineData(17)]
    public async Task SeedWithRandom_int_seeds_DbContext_with_specified_number_of_random_entities(int count)
    {
        // Arrange
        var sut = CreateDbContextBuilder();

        var context = await sut
            .SeedWithRandom<BusinessEntity>(count)
            .BuildAsync();


        // Act
        var actualAddresses = context
            .BusinessEntities
            .ToList();

        // Assert
        Assert.NotNull(actualAddresses);
        Assert.Equal(count, actualAddresses.Count);
    }



    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, func{TEntity, TEntity}) throws ArgumentOutOfRangeException when passed a value less than 1.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_TEntity_when_passed_value_less_than_1_throws_ArgumentException()
    {
        // Arrange
        Func<Address, Address> func = null!; 

        var sut = CreateDbContextBuilder();

        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => sut.SeedWithRandom(0, func));
        Assert.Equal("count", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, func{TEntity, TEntity}) throws ArgumentOutOfRangeException when passed a value less than 1.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_TEntity_when_passed_null_for_func_throws_ArgumentException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        Func<Address, Address> func = null!;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.SeedWithRandom(17, func));
        Assert.Equal("func", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, func{TEntity, TEntity}) returns the DbContextBuilder instance to allow for method chaining.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_TEntity_returns_DbContextBuilder()
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        const int count = 5;
        var func = new Func<Address, Address>(a => a);

        // Act
        var result = sut.SeedWithRandom(count, func);

        // Assert
        Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(result);
    }



    /// <summary>
    /// Verifies that a newly created DbContext contains the specified number of randomly generated entities.
    /// </summary>
    [Theory]
    [InlineData(7)]
    [InlineData(17)]
    public async Task SeedWithRandom_int_func_TEntity_TEntity_seeds_DbContext_with_specified_number_of_random_entities(int count)
    {
        var startingId = 1000;
        // Arrange
        var sut = CreateDbContextBuilder();
        var func = new Func<Person, Person>(a =>
        {
            a.BusinessEntityId = ++startingId;
            a.AdditionalContactInfo = null;
            a.BusinessEntity = new BusinessEntity
            {
                BusinessEntityId = ++startingId
            };
            return a;
        });

        var context = await sut
            .SeedWithRandom(count, func)
            .BuildAsync();


        // Act
        var actualPeople = context
            .People
            .ToList();

        // Assert
        Assert.NotNull(actualPeople);
        Assert.Equal(count, actualPeople.Count);
    }



    /// <summary>
    /// Verifies that a newly created DbContext contains the specified number of randomly generated entities.
    /// </summary>
    [Theory]
    [InlineData(7)]
    [InlineData(17)]
    public async Task SeedWithRandom_int_func_TEntity_TEntity_seeds_DbContext_with_specified_values(int count)
    {
        // Arrange
        const int startingId = 1001;

        var businessEntityId = startingId;

        var sut = CreateDbContextBuilder();
        var func = new Func<Person, Person>(a =>
        {
            a.BusinessEntityId = businessEntityId;
            a.AdditionalContactInfo = null;
            a.BusinessEntity = new BusinessEntity
            {
                BusinessEntityId = businessEntityId
            };

            businessEntityId++;

            return a;
        });

        var context = await sut
            .SeedWithRandom(count, func)
            .BuildAsync();


        // Act
        var actualPeople = context
            .People
            .ToList();

        // Assert

        var expectedBusinessEntityIds = Enumerable
            .Range(startingId, count)
            .ToList();

        var actualBusinessEntityId = actualPeople
            .Select(a => a.BusinessEntityId)
            .ToList();

        Assert.Equal(expectedBusinessEntityIds, actualBusinessEntityId);
    }



    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, func{TEntity, int, TEntity}) throws ArgumentOutOfRangeException when passed a value less than 1.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_int_TEntity_when_passed_value_less_than_1_throws_ArgumentException()
    {
        // Arrange
        Func<Address, int, Address> func = null!;
        var sut = CreateDbContextBuilder();

        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => sut.SeedWithRandom(0, func));
        Assert.Equal("count", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, func{TEntity, int, TEntity}) throws ArgumentOutOfRangeException when passed a value less than 1.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_int_TEntity_when_passed_null_for_func_throws_ArgumentException()
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        Func<Address, int, Address> func = null!;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.SeedWithRandom(17, func));
        Assert.Equal("func", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, func{TEntity, TEntity}) returns the DbContextBuilder instance to allow for method chaining.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_int_TEntity_returns_DbContextBuilder()
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        const int count = 5;
        var func = new Func<Address, int, Address>((a, _) => a);

        // Act
        var result = sut.SeedWithRandom(count, func);

        // Assert
        Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(result);
    }



    /// <summary>
    /// Verifies that a newly created DbContext contains the specified number of randomly generated entities.
    /// </summary>
    [Theory]
    [InlineData(7)]
    [InlineData(17)]
    public async Task SeedWithRandom_int_func_TEntity_int_TEntity_seeds_DbContext_with_specified_number_of_random_entities(int count)
    {
        const int startingId = 1001;
        // Arrange
        var sut = CreateDbContextBuilder();
        var func = new Func<Person, int, Person>((a, i) =>
        {
            a.BusinessEntityId = startingId + i;
            a.AdditionalContactInfo = null;
            a.BusinessEntity = new BusinessEntity
            {
                BusinessEntityId = startingId + i
            };
            return a;
        });

        var context = await sut
            .SeedWithRandom(count, func)
            .BuildAsync();


        // Act
        var actualPeople = context
            .People
            .ToList();

        // Assert
        Assert.NotNull(actualPeople);
        Assert.Equal(count, actualPeople.Count);
    }



    /// <summary>
    /// Verifies that a newly created DbContext contains the specified number of randomly generated entities.
    /// </summary>
    [Theory]
    [InlineData(7)]
    [InlineData(17)]
    public async Task SeedWithRandom_int_func_TEntity_int_TEntity_seeds_DbContext_with_specified_values(int count)
    {
        const int startingId = 1001;
        // Arrange
        var sut = CreateDbContextBuilder();
        var func = new Func<Person, int, Person>((a, i) =>
        {
            a.BusinessEntityId = startingId + i;
            a.AdditionalContactInfo = null;
            a.BusinessEntity = new BusinessEntity
            {
                BusinessEntityId = startingId + i
            };
            return a;
        });

        var context = await sut
            .SeedWithRandom(count, func)
            .BuildAsync();


        // Act
        var actualPeople = context
            .People
            .ToList();

        // Assert
        Assert.NotNull(actualPeople);
        Assert.Equal(count, actualPeople.Count);
    }



    /// <summary>
    /// Verifies that calling UseDbContextOptionsBuilder and passing null throws ArgumentNullException
    /// </summary>
    [Fact]
    public void Calling_UseDbContextOptionsBuilder_when_passed_null_throws_ArgumentNullException()
    {
        // Arrange

        var sut = CreateDbContextBuilder();

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.UseDbContextOptionsBuilder(null!));
        Assert.Equal("dbContextOptionsBuilder", ex.ParamName);


    }



    /// <summary>
    /// Verifies that UseDbContextOptionsBuilder returns the DbContextBuilder{T} so it can be chained to other calls
    /// </summary>
    [Fact]
    public void Calling_UseDbContextOptionsBuilder_returns_DbContextBuild()
    {
        // Arrange
        var sut = CreateDbContextBuilder();
        var optionsBuilder = new DbContextOptionsBuilder<AdventureWorksDbContext>();

        // Act & Assert
        Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(sut.UseDbContextOptionsBuilder(optionsBuilder));
    }



    /// <summary>
    /// Verifies that BuildAsync uses the DbOptionBuilder passed in by UseDbContextOptionsBuilder
    /// </summary>
    [Fact]
    public async Task UseDbContextOptionsBuilder_after_calling_BuildAsync_uses_DbOptionBuilder_passed_in()
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
        Assert.True(buffer.Length > 0, "Buffer length was expected to be greater than 0");
    }
}