using System;
using System.Linq;
using AutoFixture;

namespace Wolfgang.DbContextBuilderEF6.Tests.Unit;

public class NoCircularReferencesCustomizationTests
{

    /// <summary>
    /// Verifies that NoCircularReferencesCustomization adds OmitOnRecursionBehavior.
    /// </summary>
    [Fact]
    public void Customize_adds_OmitOnRecursionBehavior()
    {
        // Arrange
        var fixture = new Fixture();
        var sut = new AutoFixtureRandomEntityCreator.NoCircularReferencesCustomization();

        // Act
        sut.Customize(fixture);

        // Assert
        Assert.Contains(fixture.Behaviors, b => b is OmitOnRecursionBehavior);
    }



    /// <summary>
    /// Verifies that NoCircularReferencesCustomization removes ThrowingRecursionBehavior.
    /// </summary>
    [Fact]
    public void Customize_removes_ThrowingRecursionBehavior()
    {
        // Arrange
        var fixture = new Fixture();
        var sut = new AutoFixtureRandomEntityCreator.NoCircularReferencesCustomization();

        // Act
        sut.Customize(fixture);

        // Assert
        Assert.DoesNotContain(fixture.Behaviors, b => b is ThrowingRecursionBehavior);
    }



    /// <summary>
    /// Verifies that calling Customize twice does not add duplicate OmitOnRecursionBehavior.
    /// </summary>
    [Fact]
    public void Customize_called_twice_does_not_add_duplicate_OmitOnRecursionBehavior()
    {
        // Arrange
        var fixture = new Fixture();
        var sut = new AutoFixtureRandomEntityCreator.NoCircularReferencesCustomization();

        // Act
        sut.Customize(fixture);
        sut.Customize(fixture);

        // Assert
        var count = fixture.Behaviors.OfType<OmitOnRecursionBehavior>().Count();
        Assert.Equal(1, count);
    }



    /// <summary>
    /// Verifies that Customize throws when fixture is null.
    /// </summary>
    [Fact]
    public void Customize_when_fixture_is_null_throws_ArgumentNullException()
    {
        // Arrange
        var sut = new AutoFixtureRandomEntityCreator.NoCircularReferencesCustomization();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => sut.Customize(null!));
    }
}
