using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoFixture;
using AutoFixture.Kernel;
using Wolfgang.DbContextBuilderEF6.Tests.Unit.Models;
using Xunit;

namespace Wolfgang.DbContextBuilderEF6.Tests.Unit;

public class IgnoreVirtualMembersTests
{

    /// <summary>
    /// Verifies that the IgnoreVirtualMembers specimen builder returns NoSpecimen for non-property requests.
    /// </summary>
    [Fact]
    public void IgnoreVirtualMembers_returns_NoSpecimen_for_non_property_request()
    {
        // Arrange
        var sut = new AutoFixtureRandomEntityCreator.IgnoreVirtualMembers();
        var context = new SpecimenContext(new Fixture());

        // Act
        var result = sut.Create("not a property", context);

        // Assert
        Assert.IsType<NoSpecimen>(result);
    }



    /// <summary>
    /// Verifies that the IgnoreVirtualMembers specimen builder returns null for virtual properties.
    /// </summary>
    [Fact]
    public void IgnoreVirtualMembers_returns_null_for_virtual_property()
    {
        // Arrange
        var sut = new AutoFixtureRandomEntityCreator.IgnoreVirtualMembers();
        var context = new SpecimenContext(new Fixture());
        var virtualProp = typeof(TestDbContext).GetProperty(nameof(TestDbContext.Products))!;

        // Act
        var result = sut.Create(virtualProp, context);

        // Assert
        Assert.Null(result);
    }



    /// <summary>
    /// Verifies that the IgnoreVirtualMembers specimen builder returns NoSpecimen for non-virtual properties.
    /// </summary>
    [Fact]
    public void IgnoreVirtualMembers_returns_NoSpecimen_for_non_virtual_property()
    {
        // Arrange
        var sut = new AutoFixtureRandomEntityCreator.IgnoreVirtualMembers();
        var context = new SpecimenContext(new Fixture());
        var nonVirtualProp = typeof(Product).GetProperty(nameof(Product.Name))!;

        // Act
        var result = sut.Create(nonVirtualProp, context);

        // Assert
        Assert.IsType<NoSpecimen>(result);
    }



    /// <summary>
    /// Verifies that IgnoreVirtualMembers.Create throws when request is null.
    /// </summary>
    [Fact]
    public void IgnoreVirtualMembers_Create_when_request_is_null_throws_ArgumentNullException()
    {
        // Arrange
        var sut = new AutoFixtureRandomEntityCreator.IgnoreVirtualMembers();
        var context = new SpecimenContext(new Fixture());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => sut.Create(null!, context));
    }



    /// <summary>
    /// Verifies that IgnoreVirtualMembers.Create throws when context is null.
    /// </summary>
    [Fact]
    public void IgnoreVirtualMembers_Create_when_context_is_null_throws_ArgumentNullException()
    {
        // Arrange
        var sut = new AutoFixtureRandomEntityCreator.IgnoreVirtualMembers();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => sut.Create("request", null!));
    }



    /// <summary>
    /// Verifies that IgnoreVirtualMembers returns NoSpecimen for a write-only property (no getter).
    /// </summary>
    [Fact]
    public void IgnoreVirtualMembers_returns_NoSpecimen_for_write_only_property()
    {
        // Arrange
        var sut = new AutoFixtureRandomEntityCreator.IgnoreVirtualMembers();
        var context = new SpecimenContext(new Fixture());
        var writeOnlyProp = typeof(WriteOnlyPropertyClass).GetProperty(nameof(WriteOnlyPropertyClass.WriteOnly))!;

        // Act
        var result = sut.Create(writeOnlyProp, context);

        // Assert
        Assert.IsType<NoSpecimen>(result);
    }



    /// <summary>
    /// Verifies the premise of the write-only test above: <see cref="WriteOnlyPropertyClass.WriteOnly"/>
    /// has no getter and a working, non-virtual setter, so its NoSpecimen result comes from the
    /// missing getter and not from the virtual-member rule.
    /// </summary>
    [Fact]
    public void WriteOnlyPropertyClass_WriteOnly_has_no_getter_and_a_working_non_virtual_setter()
    {
        var writeOnlyProp = typeof(WriteOnlyPropertyClass).GetProperty(nameof(WriteOnlyPropertyClass.WriteOnly))!;
        var instance = new WriteOnlyPropertyClass();

        writeOnlyProp.SetValue(instance, "value");

        Assert.Null(writeOnlyProp.GetGetMethod());
        Assert.False(writeOnlyProp.GetSetMethod()!.IsVirtual);
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



/// <summary>
/// Test class with a write-only property (no getter) for testing IgnoreVirtualMembers.
/// The write-only shape and the backing-field-never-read are the WHOLE POINT of the
/// test: we're verifying the customization's behaviour on this exact declaration
/// pattern. Sonar S2376 / S4487 and R# NotAccessedField.Local would eliminate the
/// pattern under test.
/// </summary>
[SuppressMessage("Minor Code Smell", "S2376:Write-only properties should not be used", Justification = "The write-only shape is the test fixture.")]
[SuppressMessage("Minor Code Smell", "S4487:Unread \"private\" fields should be removed", Justification = "Backing field is deliberately unread; the write-only property is under test.")]
internal class WriteOnlyPropertyClass
{
    // ReSharper disable once NotAccessedField.Local
    private string _writeOnly = string.Empty;

    public string WriteOnly
    {
        set { _writeOnly = value; }
    }
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
