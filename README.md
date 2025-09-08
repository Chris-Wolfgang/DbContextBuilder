# DbContextBuilder

Uses the `Builder pattern` to create Entity Framework Core and classic `DbContext` instances
using an in-memory database for testing purposes. With DbContextBuilder, you can easily set up
a DbContext with predefined data, making it ideal for unit tests and integration tests without
having to rely on an actual database who data can be changed or deleted over time

## Features

- Create DbContext instances with an in-memory database. Be default DbContextBuilder
uses Sqlite in-memory database provider. However, you can use other databases by passing
in your own ``DbConnection`

- Add your own data to the DbContext using the `SeedWith<T>` method

- Easily add random data using the `SeedWithRandom<T>` method. This method will create random
data in your database to simulate real-world scenarios where a database will have more than
just the data you seed with.


## Installation
You can install the DbContextBuilder package via NuGet. Run the following command in the Package Manager Console:
```bash
Install-Package Wolfgang.DbContextBuilder
```

## Usage

Here's a simple example of how to use DbContextBuilder to create a DbContext with seeded data:
```csharp
// Create a DbContext with seeded randome data and your test data
var context = new DbContextBuilder<YourDbContext>()
	.SeedWithRandom<YourEntity>(10)		// Seed with 10 random entities
	.SeedWith(new YourEntity
		{
			Id = 1,
			Name = "Test Entity"
		}
	)									// Seed with specific data
	.SeedWithRandom<YourEntity>(5)		// Seed with 5 random entities
	.Build();							// Build the DbContext instance

// Use the context in your tests
var sut = new YourService(context);

```
