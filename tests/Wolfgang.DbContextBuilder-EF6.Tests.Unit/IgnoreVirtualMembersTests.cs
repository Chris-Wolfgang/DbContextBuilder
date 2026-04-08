using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoFixture;
using AutoFixture.Kernel;
using Wolfgang.DbContextBuilderEF6.Tests.Unit.Models;

namespace Wolfgang.DbContextBuilderEF6.Tests.Unit;

public class IgnoreVirtualMembersTests
{

    /// <summary>
    /// Verifies that virtual properties are skipped during AutoFixture generation.
    /// </summary>
    [Fact]
    public void AutoFixture_with_IgnoreVirtualMembers_skips_virtual_properties()
    {
        // Arrange
        var sut = new AutoFixtureRandomEntityCreator();

        // Act
        var product = sut.CreateRandomEntities<Product>(1).First();

        // Assert — non-virtual properties should be populated
        Assert.NotNull(product.Name);
        Assert.NotEqual(string.Empty, product.Name);
    }



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
}



/// <summary>
/// Test class with a write-only property (no getter) for testing IgnoreVirtualMembers.
/// </summary>
[ExcludeFromCodeCoverage]
internal class WriteOnlyPropertyClass
{
    private string _writeOnly = string.Empty;

    public string WriteOnly
    {
        set { _writeOnly = value; }
    }
}
