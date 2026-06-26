using AutoFixture;

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
}
