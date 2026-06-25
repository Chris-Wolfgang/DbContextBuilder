using AutoFixture;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Test to verify correctness of NoCircularReferencesCustomization
/// </summary>
public class NoCircularReferencesCustomizationTests
{


    /// <summary>
    /// Verifies that an instance of NoCircularReferencesCustomization can be created.
    /// </summary>
    [Fact]
    public void Can_create_instance_of_NoCircularReferencesCustomization()
    {
        // Arrange
        
        // Act
        var unused = new AutoFixtureRandomEntityCreator.NoCircularReferencesCustomization();
    }



    /// <summary>
    /// Verifies that calling Customize and passing null throws ArgumentNullException
    /// </summary>
    [Fact]
    public void Customize_when_passed_null_throws_ArgumentNullException()
    {
        // Arrange
        var sut = new AutoFixtureRandomEntityCreator.NoCircularReferencesCustomization();

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() =>sut.Customize(null!));
        Assert.Equal("fixture", ex.ParamName);
    }



    /// <summary>
    /// Verifies that Create adds OmitOnRecursionBehavior to the behaviors if it doesn't exist
    /// </summary>
    [Fact]
    public void Customize_when_Behaviors_does_not_contain_NoCircularReferencesGuard_adds_it()
    {
        // Arrange
        var sut = new AutoFixtureRandomEntityCreator.NoCircularReferencesCustomization();
        var fixture = new Fixture();
        fixture.Behaviors.Clear();

        // Act
        sut.Customize(fixture);

        // Assert
        Assert.NotEmpty(fixture.Behaviors.OfType<OmitOnRecursionBehavior>());
    }



    /// <summary>
    /// Verifies that when Create is called if Behaviors already contains OmitOnRecursionBehavior it does not add a second occurrence
    /// </summary>
    [Fact]
    public void Customize_when_Behaviors_contains_NoCircularReferencesGuard_does_not_add_second_occurrence()
    {
        // Arrange
        var sut = new AutoFixtureRandomEntityCreator.NoCircularReferencesCustomization();
        var fixture = new Fixture();
        fixture.Behaviors.Clear();
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        var countBefore = fixture.Behaviors.OfType<OmitOnRecursionBehavior>().Count();
        Assert.Equal(1, countBefore);

        // Act
        sut.Customize(fixture);

        // Assert
        var countAfter = fixture.Behaviors.OfType<OmitOnRecursionBehavior>().Count();
        Assert.Equal(1, countAfter);
    }



    /// <summary>
    /// Verifies that when Create is called if Behaviors contains ThrowingRecursionBehavior it is removed
    /// </summary>
    [Fact]
    public void Customize_when_Behaviors_contains_ThrowingRecursionBehavior_removes_it()
    {
        // Arrange
        var sut = new AutoFixtureRandomEntityCreator.NoCircularReferencesCustomization();
        var fixture = new Fixture();
        fixture.Behaviors.Clear();
        fixture.Behaviors.Add(new ThrowingRecursionBehavior());
        var countBefore = fixture.Behaviors.OfType<ThrowingRecursionBehavior>().Count();
        Assert.Equal(1, countBefore);

        // Act
        sut.Customize(fixture);

        // Assert
        var countAfter = fixture.Behaviors.OfType<ThrowingRecursionBehavior>().Count();
        Assert.Equal(0, countAfter);
    }


    /// <summary>
    /// Verifies that when Create is called if Behaviors contains ThrowingRecursionBehavior it is removed
    /// </summary>
    [Fact]
    public void Customize_when_Behaviors_does_not_contain_ThrowingRecursionBehavior_it_does_error()
    {
        // Arrange
        var sut = new AutoFixtureRandomEntityCreator.NoCircularReferencesCustomization();
        var fixture = new Fixture();
        fixture.Behaviors.Clear();
        var countBefore = fixture.Behaviors.OfType<ThrowingRecursionBehavior>().Count();
        Assert.Equal(0, countBefore);

        // Act
        sut.Customize(fixture);

        // Assert
        var countAfter = fixture.Behaviors.OfType<ThrowingRecursionBehavior>().Count();
        Assert.Equal(0, countAfter);
    }


}
