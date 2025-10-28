using AutoFixture.Kernel;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Test suite that verifies the correctness of IgnoreVirtualMembers
/// </summary>
public class IgnoreVirtualMembersTests
{
    /// <summary>
    /// Verifies can create a instances of IgnoreVirtualMembers
    /// </summary>
    [Fact]
    public void Can_create_instance_of_IgnoreVirtualMembers()
    {
        // Act & Assert
        var sut = new AutoFixtureRandomEntityCreator.IgnoreVirtualMembers();
    }


    /// <summary>
    /// Verifies IgnoreVirtualMembersCustomization ignores virtual properties
    /// </summary>
    [Fact]
    public void Create_when_passed_null_request_throws_ArgumentNullException()
    {
        // Arrange
        var sut = new AutoFixtureRandomEntityCreator.IgnoreVirtualMembers();
        var builder = new FixedBuilder(new object());
        var context = new SpecimenContext(builder);

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.Create(null!, context));
        Assert.Equal("request", ex.ParamName);

    }



    /// <summary>
    /// Verifies IgnoreVirtualMembersCustomization ignores virtual properties
    /// </summary>
    [Fact]
    public void Create_when_passed_null_context_throws_ArgumentNullException()
    {
        // Arrange
        var sut = new AutoFixtureRandomEntityCreator.IgnoreVirtualMembers();
        var request = new object();


        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.Create(request, null!));
        Assert.Equal("context", ex.ParamName);
    }



}
