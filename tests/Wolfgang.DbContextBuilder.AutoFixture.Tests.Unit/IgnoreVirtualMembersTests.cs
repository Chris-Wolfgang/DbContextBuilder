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
    /// A sealed override of a virtual navigation is virtual AND final, like an interface
    /// implementation, but it is still a navigation: the builder must keep omitting it (#561 review).
    /// </summary>
    [Fact]
    public void Create_when_property_is_a_sealed_override_returns_null()
    {
        var sut = new AutoFixtureRandomEntityCreator.IgnoreVirtualMembers();
        var context = new SpecimenContext(new Fixture());
        var sealedOverride = typeof(SealedOverrideRow).GetProperty(nameof(SealedOverrideRow.Parent))!;

        var result = sut.Create(sealedOverride, context);

        Assert.True(sealedOverride.GetMethod!.IsVirtual && sealedOverride.GetMethod.IsFinal);
        Assert.Null(result);
    }



    /// <summary>
    /// End to end: the creator leaves a sealed-override navigation unset.
    /// </summary>
    [Fact]
    public void CreateRandomEntities_leaves_a_sealed_override_navigation_null()
    {
        var sut = new AutoFixtureRandomEntityCreator();

        var row = sut.CreateRandomEntities<SealedOverrideRow>(1).First();

        Assert.Null(row.Parent);
    }



    /// <summary>
    /// An interface implementation declared on a base class and read through a derived type is
    /// still a scalar the builder leaves to AutoFixture (#561 review).
    /// </summary>
    [Fact]
    public void Create_when_inherited_property_implements_an_interface_returns_NoSpecimen()
    {
        var sut = new AutoFixtureRandomEntityCreator.IgnoreVirtualMembers();
        var context = new SpecimenContext(new Fixture());
        var inherited = typeof(DerivedInterfaceRow).GetProperty(nameof(DerivedInterfaceRow.Code))!;

        var result = sut.Create(inherited, context);

        Assert.IsType<NoSpecimen>(result);
    }



    /// <summary>
    /// End to end: the creator populates an interface implementation inherited from a base class.
    /// </summary>
    [Fact]
    public void CreateRandomEntities_populates_an_inherited_interface_property()
    {
        var sut = new AutoFixtureRandomEntityCreator();

        var row = sut.CreateRandomEntities<DerivedInterfaceRow>(1).First();

        Assert.False(string.IsNullOrEmpty(row.Code));
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



/// <summary>Declares an overridable navigation for <see cref="SealedOverrideRow"/> to seal (#561 review).</summary>
internal abstract class NavigationRowBase
{
    public abstract InterfaceImplementingRow? Parent { get; set; }
}



/// <summary>Seals the inherited navigation: its getter is virtual and final (#561 review).</summary>
internal sealed class SealedOverrideRow : NavigationRowBase
{
    public sealed override InterfaceImplementingRow? Parent { get; set; }
}



/// <summary>Implements <see cref="IHasCode"/> on a base class (#561 review).</summary>
internal class InterfaceRowBase : IHasCode
{
    public string Code { get; set; } = string.Empty;
}



/// <summary>Inherits <see cref="InterfaceRowBase"/>'s interface implementation (#561 review).</summary>
internal sealed class DerivedInterfaceRow : InterfaceRowBase
{
}
