using System;
using System.Linq;
using Wolfgang.DbContextBuilderEF6.Tests.Unit.Models;
using Xunit;

namespace Wolfgang.DbContextBuilderEF6.Tests.Unit;

public class AutoFixtureRandomEntityCreatorTests
{

    /// <summary>
    /// Verifies that CreateRandomEntities creates the specified number of entities.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void CreateRandomEntities_creates_specified_number_of_entities(int count)
    {
        // Arrange
        var sut = new AutoFixtureRandomEntityCreator();

        // Act
        var result = sut.CreateRandomEntities<Product>(count).ToList();

        // Assert
        Assert.Equal(count, result.Count);
    }



    /// <summary>
    /// Verifies that CreateRandomEntities throws when count is less than 1.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateRandomEntities_when_count_less_than_1_throws_ArgumentOutOfRangeException(int count)
    {
        // Arrange
        var sut = new AutoFixtureRandomEntityCreator();

        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => sut.CreateRandomEntities<Product>(count).ToList());
        Assert.Equal("count", ex.ParamName);
        Assert.StartsWith("Value cannot be less than 1", ex.Message, StringComparison.Ordinal);
    }



    /// <summary>
    /// Verifies that created entities have populated properties.
    /// </summary>
    [Fact]
    public void CreateRandomEntities_creates_entities_with_populated_properties()
    {
        // Arrange
        var sut = new AutoFixtureRandomEntityCreator();

        // Act
        var result = sut.CreateRandomEntities<Product>(1).Single();

        // Assert
        Assert.NotNull(result.Name);
        Assert.NotEqual(string.Empty, result.Name);
        Assert.NotEqual(0m, result.Price);
    }



    /// <summary>
    /// Verifies that a custom Fixture can be provided.
    /// </summary>
    [Fact]
    public void Constructor_with_fixture_uses_provided_fixture()
    {
        // Arrange
        var fixture = new AutoFixture.Fixture();
        var sut = new AutoFixtureRandomEntityCreator(fixture);

        // Act & Assert
        Assert.Same(fixture, sut.Fixture);
    }



    /// <summary>
    /// Verifies that passing null fixture throws.
    /// </summary>
    [Fact]
    public void Constructor_with_null_fixture_throws_ArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AutoFixtureRandomEntityCreator(null!));
    }



    /// <summary>
    /// A type that references itself would make AutoFixture's default ThrowingRecursionBehavior
    /// throw; the default constructor's customization omits the recursion instead.
    /// </summary>
    [Fact]
    public void CreateRandomEntities_when_a_type_references_itself_omits_the_recursion()
    {
        var sut = new AutoFixtureRandomEntityCreator();

        var node = Assert.Single(sut.CreateRandomEntities<SelfReferencingNode>(1));

        Assert.NotEqual(0, node.Id);
        Assert.Null(node.Parent?.Parent);
    }



    /// <summary>
    /// Virtual members (EF navigation properties) are left unset by the default constructor, so
    /// random entities do not drag in random object graphs.
    /// </summary>
    [Fact]
    public void CreateRandomEntities_leaves_virtual_members_unset()
    {
        var sut = new AutoFixtureRandomEntityCreator();

        var entity = Assert.Single(sut.CreateRandomEntities<EntityWithVirtualMember>(1));

        Assert.NotEmpty(entity.Name);
        Assert.Null(entity.Navigation);
    }
}



/// <summary>A type whose non-virtual property refers to its own type.</summary>
public class SelfReferencingNode
{
    public int Id { get; set; }

    public SelfReferencingNode? Parent { get; set; }
}



/// <summary>A type with a virtual member, like an EF navigation property.</summary>
public class EntityWithVirtualMember
{
    public string Name { get; set; } = string.Empty;

    public virtual Category? Navigation { get; set; }
}
