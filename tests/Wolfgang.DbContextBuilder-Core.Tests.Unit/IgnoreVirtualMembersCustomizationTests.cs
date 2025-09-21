using AutoFixture;
using static Wolfgang.DbContextBuilderCore.AutoFixtureRandomEntityGenerator;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;
/// <summary>
/// Test suite to verify the correctness of IgnoreVirtualMembersCustomization
/// </summary>
public class IgnoreVirtualMembersCustomizationTests
{
    /// <summary>
    /// Verifies that an instance of IgnoreVirtualMembersCustomization can be created.
    /// </summary>
    [Fact]
    public void Can_create_instance_of_IgnoreVirtualMembersCustomization()
    {
        // Arrange

        // Act
        // ReSharper disable once UseDiscardAssignment
        var unused = new IgnoreVirtualMembersCustomization();
    }



    /// <summary>
    /// Verifies that calling Customize and passing null throws ArgumentNullException
    /// </summary>
    [Fact]
    public void Customize_when_passed_null_throws_ArgumentNullException()
    {
        // Arrange
        var sut = new IgnoreVirtualMembersCustomization();

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.Customize(null!));
        Assert.Equal("fixture", ex.ParamName);
    }



    /// <summary>
    /// Verifies that calling Customize adds IgnoreVirtualMembers to Customizations if it doesn't exist
    /// </summary>
    [Fact]
    public void Customize_when_Customizations_does_not_contain_IgnoreVirtualMembers_adds_it()
    {
        // Arrange
        var sut = new IgnoreVirtualMembersCustomization();
        var fixture = new Fixture();
        fixture.Customizations.Clear();
        Assert.Empty(fixture.Customizations.OfType<IgnoreVirtualMembers>());

        // Act
        sut.Customize(fixture);

        // Assert
        Assert.NotEmpty(fixture.Customizations.OfType<IgnoreVirtualMembers>());
    }



    /// <summary>
    /// Verifies that calling Customize does not add IgnoreVirtualMembers to Customizations
    /// if it already exists
    /// </summary>
    [Fact]
    public void Customize_when_Customizations_does_contain_IgnoreVirtualMembers_does_not_adds_it_again()
    {
        // Arrange
        var sut = new IgnoreVirtualMembersCustomization();
        var fixture = new Fixture();
        fixture.Customizations.Clear();
        fixture.Customizations.Add(new IgnoreVirtualMembers());
        var countBefore = fixture.Customizations.OfType<IgnoreVirtualMembers>().Count();
        Assert.Equal(1, countBefore);

        // Act
        sut.Customize(fixture);

        // Assert
        var countAfter = fixture.Customizations.OfType<IgnoreVirtualMembers>().Count();
        Assert.Equal(1, countAfter);
    }





    /// <summary>
    /// Verifies IgnoreVirtualMembersCustomization ignores virtual properties
    /// </summary>
    [Fact]
    public void IgnoreVirtualMembersCustomization_ShouldIgnoreVirtualProperties()
    {
        // Arrange
        var fixture = new Fixture();
        fixture.Customize(new IgnoreVirtualMembersCustomization());

        // Act
        var result = fixture.Create<TestClass>();

        // Assert
        Assert.Null(result.VirtualProperty);
        Assert.NotNull(result.NonVirtualProperty);
    }



    /// <summary>
    /// Verifies IgnoreVirtualMembersCustomization ignores virtual methods
    /// </summary>
    [Fact]
    public void IgnoreVirtualMembersCustomization_ShouldIgnoreVirtualMethods()
    {
        // Arrange
        var fixture = new Fixture();
        fixture.Customize(new IgnoreVirtualMembersCustomization());

        // Act
        var result = fixture.Create<TestClass>();

        // Assert
        Assert.Equal(0, result.VirtualMethod());
        Assert.NotEqual(0, result.NonVirtualMethod());
    }


    // ReSharper disable once ClassNeverInstantiated.Local
    private class TestClass
    {
        public virtual string? VirtualProperty { get; set; }
        public string NonVirtualProperty { get; set; } = "NonVirtual";
        public virtual int VirtualMethod() => 0;
        public int NonVirtualMethod() => 42;
    }
}
