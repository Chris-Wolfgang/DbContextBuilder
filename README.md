# DbContextBuilder

Uses the `Builder pattern` to create Entity Framework Core and classic `DbContext` instances
using an in-memory database for testing purposes. With DbContextBuilder, you can easily set up
a DbContext with predefined data, making it ideal for unit tests and integration tests without
having to rely on an actual database whose data can be changed or deleted over time

## Features

- Create DbContext instances with an in-memory database. By default, DbContextBuilder
uses the EF Core InMemory provider. Use `.UseSqlite()` for a SQLite in-memory database,
or pass in your own `DbContextOptionsBuilder` for other providers.

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
// Create a DbContext with seeded random data and your test data
var context = await new DbContextBuilder<YourDbContext>()
	// Seed with 10 random entities
	.SeedWithRandom<YourEntity>(10)

	// Seed with specific data
	.SeedWith
	(
		new YourEntity
		{
			Id = 1,
			Name = "Test Entity"
		}
	)

	// Seed with 5 random entities
	.SeedWithRandom<YourEntity>(5)

	// Build the DbContext instance
	.BuildAsync();							

// Use the context in your tests
var sut = new YourService(context);

```
