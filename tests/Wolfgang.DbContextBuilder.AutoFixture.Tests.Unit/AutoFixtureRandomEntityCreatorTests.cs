using System;
using AutoFixture;
using Xunit;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Tests for the AutoFixtureRandomEntityCreator class
/// </summary>
public class AutoFixtureRandomEntityCreatorTests : ICreateRandomEntitiesTestsBase
{
	/// <summary>
	/// Creates an instance of  <see cref="AutoFixtureRandomEntityCreator"/> to be tested.
	/// </summary>
	/// <returns></returns>
	/// <exception cref="NotImplementedException"></exception>
	protected override ICreateRandomEntities CreateRandomEntityCreator() => new AutoFixtureRandomEntityCreator();



	/// <summary>
	/// Verifies that the Fixture property is not null
	/// </summary>
	[Fact]
	public void Fixture_when_default_ctor_is_used_is_not_null()
	{
		// Arrange
		var sut = new AutoFixtureRandomEntityCreator();

		// Act & Assert
		Assert.NotNull(sut.Fixture);
	}



	/// <summary>
	/// Verifies that passing null to the constructor throws an ArgumentNullException
	/// </summary>
	[Fact]
	public void Ctor_Fixture_when_passed_null_throws_ArgumentNullException()
	{
		// Act & Assert
		var ex = Assert.Throws<ArgumentNullException>(() => new AutoFixtureRandomEntityCreator(null!));
		Assert.Equal("fixture", ex.ParamName);
	}



	/// <summary>
	/// Verifies that the value passed into the constructor is assigned to the Fixture property
	/// </summary>
	[Fact]
	public void Ctor_when_passed_a_fixture_assigns_it_to_the_Fixture_property()
	{
		// Arrange
		var fixture = new Fixture();

		// Act
		var sut = new AutoFixtureRandomEntityCreator(fixture);

		// Assert
		Assert.Equal(fixture, sut.Fixture);
	}



	/// <summary>
	/// AutoFixture 4.x cannot create <see cref="DateOnly"/> on its own: it calls the year/month/day
	/// constructor with random integers, which throws. The default constructor registers a factory
	/// built on a random <see cref="DateTime"/>, so entities with such properties can be seeded.
	/// </summary>
	/// <remarks>
	/// <see cref="TimeOnly"/> is not tested here: AutoFixture builds it through its ticks constructor
	/// even without the factory, so the factory changes how the value is made, not whether it is,
	/// and any value, midnight included, is a valid result.
	/// </remarks>
	[Fact]
	public void CreateRandomEntities_creates_DateOnly_properties()
	{
		var sut = new AutoFixtureRandomEntityCreator();

		var entity = Assert.Single(sut.CreateRandomEntities<DatedEntity>(1));

		Assert.NotEqual(default, entity.Day);
	}



	/// <summary>
	/// A type that references itself through a non-virtual property would make AutoFixture's default
	/// ThrowingRecursionBehavior throw; the default constructor's customization omits the recursion.
	/// </summary>
	[Fact]
	public void CreateRandomEntities_when_a_type_references_itself_omits_the_recursion()
	{
		var sut = new AutoFixtureRandomEntityCreator();

		var node = Assert.Single(sut.CreateRandomEntities<SelfReferencingNode>(1));

		Assert.NotEqual(0, node.Id);
		Assert.Null(node.Parent?.Parent);
	}



	/// <summary>
	/// A count below 1 is rejected with a message that says what the limit is.
	/// </summary>
	[Fact]
	public void CreateRandomEntities_when_count_is_less_than_1_reports_the_limit()
	{
		var sut = new AutoFixtureRandomEntityCreator();

		var ex = Assert.Throws<ArgumentOutOfRangeException>(() => sut.CreateRandomEntities<SelfReferencingNode>(0));

		Assert.StartsWith("Value cannot be less than 1", ex.Message, StringComparison.Ordinal);
	}
}



/// <summary>A type with a <see cref="DateOnly"/> property.</summary>
public class DatedEntity
{
	public DateOnly Day { get; set; }
}



/// <summary>A type whose non-virtual property refers to its own type.</summary>
public class SelfReferencingNode
{
	public int Id { get; set; }

	public SelfReferencingNode? Parent { get; set; }
}
