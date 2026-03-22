using System;
using System.Linq;
using AutoFixture;

namespace Wolfgang.DbContextBuilderEF6.Tests.Unit;

public class IgnoreVirtualMembersCustomizationTests
{

    /// <summary>
    /// Verifies that IgnoreVirtualMembersCustomization adds IgnoreVirtualMembers to fixture.
    /// </summary>
    [Fact]
    public void Customize_adds_IgnoreVirtualMembers()
    {
        // Arrange
        var fixture = new Fixture();
        var sut = new AutoFixtureRandomEntityCreator.IgnoreVirtualMembersCustomization();

        // Act
        sut.Customize(fixture);

        // Assert
        Assert.Contains
        (
            fixture.Customizations,
            c => c is AutoFixtureRandomEntityCreator.IgnoreVirtualMembers
        );
    }



    /// <summary>
    /// Verifies that calling Customize twice does not add duplicate IgnoreVirtualMembers.
    /// </summary>
    [Fact]
    public void Customize_called_twice_does_not_add_duplicate_IgnoreVirtualMembers()
    {
        // Arrange
        var fixture = new Fixture();
        var sut = new AutoFixtureRandomEntityCreator.IgnoreVirtualMembersCustomization();

        // Act
        sut.Customize(fixture);
        sut.Customize(fixture);

        // Assert
        var count = fixture.Customizations
            .OfType<AutoFixtureRandomEntityCreator.IgnoreVirtualMembers>()
            .Count();
        Assert.Equal(1, count);
    }



    /// <summary>
    /// Verifies that Customize throws when fixture is null.
    /// </summary>
    [Fact]
    public void Customize_when_fixture_is_null_throws_ArgumentNullException()
    {
        // Arrange
        var sut = new AutoFixtureRandomEntityCreator.IgnoreVirtualMembersCustomization();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => sut.Customize(null!));
    }
}
