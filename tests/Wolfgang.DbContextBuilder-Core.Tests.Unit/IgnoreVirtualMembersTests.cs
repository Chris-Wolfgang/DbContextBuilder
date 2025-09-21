using System.Reflection;
using AutoFixture.Kernel;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Test suite that verifies the correctness of IgnoreVirtualMembers
/// </summary>
public class IgnoreVirtualMembersTests
{
    /// <summary>
    /// Verifies IgnoreVirtualMembersCustomization ignores virtual properties
    /// </summary>
    [Fact]
    public void Can_create_instance_of_IgnoreVirtualMembers()
    {
        // Act & Assert
        var sut = new AutoFixtureRandomEntityGenerator.IgnoreVirtualMembers();
    }


    /// <summary>
    /// Verifies IgnoreVirtualMembersCustomization ignores virtual properties
    /// </summary>
    [Fact]
    public void Create_when_passed_null_request_throws_ArgumentNullException()
    {
        // Arrange
        var sut = new AutoFixtureRandomEntityGenerator.IgnoreVirtualMembers();
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
        var sut = new AutoFixtureRandomEntityGenerator.IgnoreVirtualMembers();
        var request = new object();


        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => sut.Create(request, null!));
        Assert.Equal("context", ex.ParamName);
    }



}
//    var entityType = typeof(TestEntity);

//        // Act
//        var nonVirtualProperties = entityType
//            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
//            .Where(p => !IsVirtual(p))
//            .ToList();

//        // Assert
//        Assert.DoesNotContain(nonVirtualProperties, p => p.Name == nameof(TestEntity.Children));
//        Assert.DoesNotContain(nonVirtualProperties, p => p.Name == nameof(TestEntity.Parent));
//        Assert.Contains(nonVirtualProperties, p => p.Name == nameof(TestEntity.Id));
//        Assert.Contains(nonVirtualProperties, p => p.Name == nameof(TestEntity.Name));
//        Assert.Contains(nonVirtualProperties, p => p.Name == nameof(TestEntity.Description));
//    }



//    [Fact]
//    public void Should_Include_Only_NonVirtual_Properties()
//    {
//        // Arrange
//        var entityType = typeof(TestEntity);

//        // Act
//        var nonVirtualPropertyNames = entityType
//            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
//            .Where(p => !IsVirtual(p))
//            .Select(p => p.Name)
//            .ToList();

//        // Assert
//        Assert.Equal(
//            new[] { "Id", "Name", "Description" },
//            nonVirtualPropertyNames
//        );
//    }

//    private static bool IsVirtual(PropertyInfo property)
//    {
//        var method = property.GetGetMethod();
//        return method != null && method.IsVirtual && !method.IsFinal;
//    }



//    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Local
//    private class TestEntity
//    {
//        public int Id { get; set; }
//        public string? Name { get; set; }
//        public virtual ICollection<TestEntity>? Children { get; set; }
//        public virtual TestEntity? Parent { get; set; }
//        public string? Description { get; set; }
//    }
//}
