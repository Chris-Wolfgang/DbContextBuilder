using System;
using System.Linq;
using AutoFixture;
using AutoFixture.Kernel;
using Xunit;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Test suite that verifies the correctness of IgnoreVirtualMembers
/// </summary>
public class IgnoreVirtualMembersTests
{
    /// <summary>
    /// Verifies that Create throws ArgumentNullException naming <c>request</c> when the request is null.
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
    /// Verifies that Create throws ArgumentNullException naming <c>context</c> when the context is null.
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


    /// <summary>
    /// #561: a non-virtual property that implicitly implements an interface member is emitted as
    /// virtual final. It is a scalar, not a navigation, so the builder leaves it to AutoFixture.
    /// </summary>
    [Fact]
    public void Create_when_property_implements_an_interface_returns_NoSpecimen()
    {
        var sut = new AutoFixtureRandomEntityCreator.IgnoreVirtualMembers();
        var context = new SpecimenContext(new Fixture());
        var interfaceProp = typeof(InterfaceImplementingRow).GetProperty(nameof(InterfaceImplementingRow.Code))!;

        var result = sut.Create(interfaceProp, context);

        Assert.True(interfaceProp.GetMethod!.IsVirtual && interfaceProp.GetMethod.IsFinal);
        Assert.IsType<NoSpecimen>(result);
    }



    /// <summary>
    /// #561: the creator populates an interface-implementing scalar property.
    /// </summary>
    [Fact]
    public void CreateRandomEntities_populates_a_property_that_implements_an_interface()
    {
        var sut = new AutoFixtureRandomEntityCreator();

        var row = sut.CreateRandomEntities<InterfaceImplementingRow>(1).First();

        Assert.False(string.IsNullOrEmpty(row.Code));
    }
}


/// <summary>An interface whose implementation the compiler emits as virtual final (#561).</summary>
internal interface IHasCode
{
    string Code { get; set; }
}



/// <summary>Implements <see cref="IHasCode"/> with a plain, non-virtual auto-property (#561).</summary>
internal sealed class InterfaceImplementingRow : IHasCode
{
    public string Code { get; set; } = string.Empty;
}

