using AutoFixture;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Tests for the AutoFixtureRandomEntityGenerator class
/// </summary>
public class AutoFixtureRandomEntityGeneratorTests : IGenerateRandomEntitiesTestsBase
{
	/// <summary>
	/// Creates an instance of AutoFixtureRandomEntityGenerator to be tested.
	/// </summary>
	/// <returns></returns>
	/// <exception cref="NotImplementedException"></exception>
	protected override IGenerateRandomEntities CreateRandomEntityGenerator() => new AutoFixtureRandomEntityGenerator();



	/// <summary>
	/// Verifies that the Fixture property is not null
	/// </summary>
	[Fact]
	public void Property_Fixture_is_not_null()
	{
		// Arrange
		var sut = new AutoFixtureRandomEntityGenerator();

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
		var ex = Assert.Throws<ArgumentNullException>(() => new AutoFixtureRandomEntityGenerator(null!));
		Assert.Equal("fixture", ex.ParamName);
	}



	/// <summary>
	/// Verifies that the value passed into the constructor is assigned to the Fixture property
	/// </summary>
	[Fact]
	public void Sets_Fixture_property_to_the_value_passed_into_ctor()
	{
		// Arrange
		var fixture = new Fixture();

		// Act
		var sut = new AutoFixtureRandomEntityGenerator(fixture);

		// Assert
		Assert.Equal(fixture, sut.Fixture);
	}
}
