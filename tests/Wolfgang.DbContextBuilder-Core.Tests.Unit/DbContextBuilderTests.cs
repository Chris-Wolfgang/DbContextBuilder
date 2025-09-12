using AdventureWorks.Models;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// 
/// </summary>
public class DbContextBuilderTests
{
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
	public void Calling_Build_returns_instance_of_specified_context()
	{
		// Arrange
		var sut = new DbContextBuilder<AdventureWorksDbContext>();

		// Act
		var context = sut.Build();

		// Assert
		Assert.NotNull(context);
		Assert.IsType<AdventureWorksDbContext>(context);
	}



	/// <summary>
	/// Verifies that calling Build multiple times on the DbContextBuilder returns multiple distinct instances of the specified DbContext type.
	/// </summary>
	[Fact]
	public void Can_create_multiple_instances_of_specified_context()
	{
		// Arrange
		var sut = new DbContextBuilder<AdventureWorksDbContext>();

		// Act
		var context1 = sut.Build();
		var context2 = sut.Build();

		// Assert
		Assert.NotNull(context1);
		Assert.NotNull(context2);
		Assert.IsType<AdventureWorksDbContext>(context1);
		Assert.IsType<AdventureWorksDbContext>(context2);
		Assert.NotSame(context1, context2);
	}



	/// <summary>
	/// Verifies that the default database provider is Microsoft.EntityFrameworkCore.InMemory.
	/// </summary>
	/// <remarks>Other providers could include Sqlite</remarks>
	[Fact]
	public void Default_database_provider_is_Microsoft_EntityFrameworkCore_InMemory()
	{

		// Arrange
		var sut = new DbContextBuilder<AdventureWorksDbContext>();

		// Act
		var context = sut.Build();

		// Assert
		var provider = context.Database.ProviderName;
		Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", provider);
	}



	/// <summary>
	/// Verifies that calling UseSqlite changes the database provider to Microsoft.EntityFrameworkCore.Sqlite.
	/// </summary>
	[Fact]
	public void Calling_UseSqlite_changes_database_provider_to_Sqlite()
	{
		// Arrange
		var sut = new DbContextBuilder<AdventureWorksDbContext>();

		// Act
		sut.UseSqlite();
		var context = sut.Build();

		// Assert
		var provider = context.Database.ProviderName;
		Assert.Equal("Microsoft.EntityFrameworkCore.Sqlite", provider);
	}



	/// <summary>
	/// Verifies that calling UseSqlite returns the DbContextBuilder instance to allow for method chaining.
	/// </summary>
	[Fact]
	public void Calling_UseSqlite_returns_DbContextBuilder()
	{
		// Arrange
		var sut = new DbContextBuilder<AdventureWorksDbContext>();

		// Act
		var result = sut.UseSqlite();

		// Assert
		Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(result);
	}



	/// <summary>
	/// Verifies that calling UseInMemory changes the database provider to Microsoft.EntityFrameworkCore.InMemory.
	/// </summary>
	/// <remarks>Currently this is the default so there is no need to explicitly call it</remarks>
	[Fact]
	public void Calling_UseInMemory_changes_database_provider_to_InMemory()
	{
		// Arrange
		var sut = new DbContextBuilder<AdventureWorksDbContext>();

		// Act
		sut.UseSqlite();
		var context = sut.Build();

		// Switch from default to Sqlite first to prove the change
		var provider = context.Database.ProviderName;
		Assert.NotEqual("Microsoft.EntityFrameworkCore.InMemory", provider);


		sut.UseInMemory();
		context = sut.Build();

		// Assert
		provider = context.Database.ProviderName;
		Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", provider);
	}



	/// <summary>
	/// Verifies that calling UseInMemory returns the DbContextBuilder instance to allow for method chaining.
	/// </summary>
	[Fact]
	public void Calling_UseInMemory_returns_DbContextBuilder()
	{
		// Arrange
		var sut = new DbContextBuilder<AdventureWorksDbContext>();

		// Act
		var result = sut.UseInMemory();

		// Assert
		Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(result);
	}



	/// <summary>
	/// Verifies that a newly created DbContext contains the mapped entities
	/// as they are defined in the EF configuration, but the sets are empty
	/// as no data has been seeded.
	/// </summary>
	[Fact]
    public void A_newly_created_DbContext_contains_the_mapped_entities_but_the_sets_are_empty()
	{
		// Arrange
		var sut = new DbContextBuilder<AdventureWorksDbContext>();

		// Act
		var context = sut.Build();

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
    public void A_newly_created_DbContext_does_not_have_any_tracked_changes()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

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
                City = "Othertown",
                StateProvinceId = 2,
                PostalCode = "67890",
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.UtcNow
            }
        };

        // Act
        var context = sut
            .SeedWith(expectedAddresses)
            .Build();


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
        var sut = new DbContextBuilder<AdventureWorksDbContext>();
        IEnumerable<Address> records = null!;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.SeedWith(records));
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
		var sut = new DbContextBuilder<AdventureWorksDbContext>();

        var addresses = new List<Address>
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
                City = "Othertown",
                StateProvinceId = 2,
                PostalCode = "67890",
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.UtcNow
            }
        };

        // Act
        var result = sut.SeedWith(addresses.AsEnumerable());

        // Assert
        Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(result);
    }



    /// <summary>
    /// Verifies that a newly created DbContext contains the data it was seeded with.
    /// </summary>
    [Fact]
    public void SeedsWith_IEnumerable_seeds_DbContext_with_specified_data()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

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
                City = "Othertown",
                StateProvinceId = 2,
                PostalCode = "67890",
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.UtcNow
            }
        };

        // Act
        var actualAddresses = sut.SeedWith(expectedAddresses.AsEnumerable())
            .Build()
            .Addresses
            .OrderBy(a => a.AddressId)
            .ToList();

        // Assert
		Assert.Equivalent(expectedAddresses, actualAddresses);
    }



    /// <summary>
    /// Verifies that calling SeedWith(params T[]) returns the DbContextBuilder instance to allow for method chaining.
    /// </summary>
    /// <remarks>This does not yet verify that the data is actually seeded into the context.</remarks>
    [Fact]
    public void SeedWith_params_returns_DbContextBuild()
    {

        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        var address1 = new Address()
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
 
        var address2 = new Address()
            {
                AddressId = 2,
                AddressLine1 = "456 Oak St",
                City = "Othertown",
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
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        Address[] addresses = null!;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.SeedWith(addresses!));
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
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        var address1 = new Address()
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


        var address2 = new Address()
        {
            AddressId = 2,
            AddressLine1 = "456 Oak St",
            City = "Othertown",
            StateProvinceId = 2,
            PostalCode = "67890",
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DateTime.UtcNow
        };


        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(
            () => sut.SeedWith(address1, (Address)null!, address2));
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
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        var addressList = new Address[]
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
                City = "Othertown",
                StateProvinceId = 2,
                PostalCode = "67890",
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.UtcNow
            }
        };


        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(
            () => sut.SeedWith((Address[])null!, addressList));
        Assert.Equal("entities", ex.ParamName);
    }



    /// <summary>
    /// Verifies that a newly created DbContext contains the data it was seeded with.
    /// </summary>
    [Fact]
    public void SeedsWith_params_seeds_DbContext_with_specified_data()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

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
                City = "Othertown",
                StateProvinceId = 2,
                PostalCode = "67890",
                Rowguid = Guid.NewGuid(),
                ModifiedDate = DateTime.UtcNow
            }
        };

        // Act
        var actualAddresses = sut.SeedWith(expectedAddresses[0], expectedAddresses[1])
            .Build()
            .Addresses
            .OrderBy(a => a.AddressId)
            .ToList();

        // Assert
        Assert.Equivalent(expectedAddresses, actualAddresses);
    }



    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int) throws ArgumentOutOfRangeException when passed a value less than 1.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_when_passed_value_less_than_1_throws_ArgumentException()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

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
        var sut = new DbContextBuilder<AdventureWorksDbContext>();
        const int count = 5;

        // Act
        var result = sut.SeedWithRandom<Address>(count);
        
        // Assert
        Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(result);
    }
    

    
    /// <summary>
    /// Verifies that a newly created DbContext contains the specified number of randomly generated entities.
    /// </summary>
    [Theory]
    [InlineData(7)]
    [InlineData(17)]
    public void SeedWithRandom_int_seeds_DbContext_with_specified_number_of_random_entities(int count)
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        // Act
        var actualAddresses = sut
            .SeedWithRandom<Address>(count)
            .Build()
            .Addresses
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
       var func = new Func<Address, Address>(a => a);
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

       // Act & Assert
       var ex = Assert.Throws<ArgumentOutOfRangeException>(() => sut.SeedWithRandom<Address>(0, func));
       Assert.Equal("count", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, func{TEntity, TEntity}) throws ArgumentOutOfRangeException when passed a value less than 1.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_TEntity_when_passed_null_for_func_throws_ArgumentException()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();
        Func<Address, Address> func = null!;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.SeedWithRandom<Address>(17, func));
        Assert.Equal("func", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, func{TEntity, TEntity}) returns the DbContextBuilder instance to allow for method chaining.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_TEntity_returns_DbContextBuilder()
    {
       // Arrange
       var sut = new DbContextBuilder<AdventureWorksDbContext>();
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
    public void SeedWithRandom_int_func_TEntity_TEntity_seeds_DbContext_with_specified_number_of_random_entities(int count)
    {
        var addressId = 1000;
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();
        var func = new Func<Address, Address>(a =>
        {
            a.AddressId = ++addressId;
            return a;
        });

        // Act
        var actualAddresses = sut
          .SeedWithRandom<Address>(count, func)
          .Build()
          .Addresses
          .ToList();

      // Assert
      Assert.NotNull(actualAddresses);
      Assert.Equal(count, actualAddresses.Count);
    }



    /// <summary>
    /// Verifies that a newly created DbContext contains the specified number of randomly generated entities.
    /// </summary>
    [Theory]
    [InlineData(7)]
    [InlineData(17)]
    public void SeedWithRandom_int_func_TEntity_TEntity_seeds_DbContext_with_specified_values(int count)
    {
        const int startingId = 1001;

        var addressId = startingId;
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();
        var func = new Func<Address, Address>(a =>
        {
            a.AddressId = addressId++;
            return a;
        });

        // Act
        var actualAddresses = sut
            .SeedWithRandom<Address>(count, func)
            .Build()
            .Addresses
            .ToList();

        // Assert

        var expectedAddressIds = Enumerable
            .Range(startingId, count)
            .ToList();

        var actualAddressIds = actualAddresses
            .Select(a => a.AddressId)
            .ToList();

        Assert.Equal(expectedAddressIds, actualAddressIds);
    }



    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, func{TEntity, int, TEntity}) throws ArgumentOutOfRangeException when passed a value less than 1.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_int_TEntity_when_passed_value_less_than_1_throws_ArgumentException()
    {
        // Arrange
        var func = new Func<Address, int, Address>((a, i) => a);
        var sut = new DbContextBuilder<AdventureWorksDbContext>();

        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => sut.SeedWithRandom<Address>(0, func));
        Assert.Equal("count", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, func{TEntity, int, TEntity}) throws ArgumentOutOfRangeException when passed a value less than 1.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_int_TEntity_when_passed_null_for_func_throws_ArgumentException()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();
        Func<Address, int, Address> func = null!;

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.SeedWithRandom<Address>(17, func));
        Assert.Equal("func", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling SeedWithRandom{T}(int, func{TEntity, TEntity}) returns the DbContextBuilder instance to allow for method chaining.
    /// </summary>
    [Fact]
    public void SeedWithRandom_int_func_TEntity_int_TEntity_returns_DbContextBuilder()
    {
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();
        const int count = 5;
        var func = new Func<Address, int, Address>((a, i) => a);

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
    public void SeedWithRandom_int_func_TEntity_int_TEntity_seeds_DbContext_with_specified_number_of_random_entities(int count)
    {
        const int startingId = 1001;
        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();
        var func = new Func<Address, int, Address>((a, i) =>
        {
            a.AddressId = startingId + i;
            return a;
        });

        // Act
        var actualAddresses = sut
          .SeedWithRandom<Address>(count, func)
          .Build()
          .Addresses
          .ToList();

        // Assert
        Assert.NotNull(actualAddresses);
        Assert.Equal(count, actualAddresses.Count);
    }



    /// <summary>
    /// Verifies that a newly created DbContext contains the specified number of randomly generated entities.
    /// </summary>
    [Theory]
    [InlineData(7)]
    [InlineData(17)]
    public void SeedWithRandom_int_func_TEntity_int_TEntity_seeds_DbContext_with_specified_values(int count)
    {
        const int startingId = 1001;

        // Arrange
        var sut = new DbContextBuilder<AdventureWorksDbContext>();
        var func = new Func<Address, int,Address>((a, i) =>
        {
            a.AddressId = startingId + i;
            return a;
        });

        // Act
        var actualAddresses = sut
            .SeedWithRandom(count, func)
            .Build()
            .Addresses
            .ToList();

        // Assert

        var expectedAddressIds = Enumerable
            .Range(startingId, count)
            .ToList();

        var actualAddressIds = actualAddresses
            .Select(a => a.AddressId)
            .ToList();

        Assert.Equal(expectedAddressIds, actualAddressIds);
    }


}
