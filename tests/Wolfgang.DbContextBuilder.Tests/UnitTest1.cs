using Microsoft.EntityFrameworkCore;
using Wolfgang.DbContextBuilder;

namespace Wolfgang.DbContextBuilder.Tests;

// Test entity
public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Test DbContext
public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<TestEntity> TestEntities { get; set; }
}

public class DbContextBuilderTests
{
    [Fact]
    public void Build_CreatesDbContextWithSeedData()
    {
        // Arrange
        var testEntity = new TestEntity { Id = 1, Name = "Test Entity" };

        // Act
        var context = new DbContextBuilder<TestDbContext>()
            .SeedWith(testEntity)
            .Build();

        // Assert
        Assert.NotNull(context);
        Assert.Single(context.TestEntities);
        
        var entity = context.TestEntities.First();
        Assert.Equal(1, entity.Id);
        Assert.Equal("Test Entity", entity.Name);
        
        context.Dispose();
    }

    [Fact]
    public void SeedWithRandom_CreatesSpecifiedNumberOfEntities()
    {
        // Arrange & Act
        var context = new DbContextBuilder<TestDbContext>()
            .SeedWithRandom<TestEntity>(5)
            .Build();

        // Assert
        Assert.NotNull(context);
        Assert.Equal(5, context.TestEntities.Count());
        
        context.Dispose();
    }

    [Fact]
    public void Build_CombinesSeedDataAndRandomData()
    {
        // Arrange
        var testEntity = new TestEntity { Id = 1, Name = "Specific Entity" };

        // Act
        var context = new DbContextBuilder<TestDbContext>()
            .SeedWith(testEntity)
            .SeedWithRandom<TestEntity>(3)
            .Build();

        // Assert
        Assert.NotNull(context);
        Assert.Equal(4, context.TestEntities.Count());
        
        // Check that our specific entity is there
        var specificEntity = context.TestEntities.FirstOrDefault(e => e.Name == "Specific Entity");
        Assert.NotNull(specificEntity);
        Assert.Equal(1, specificEntity.Id);
        
        context.Dispose();
    }

    [Fact]
    public void SeedWithRandom_ThrowsForInvalidCount()
    {
        // Arrange
        var builder = new DbContextBuilder<TestDbContext>();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.SeedWithRandom<TestEntity>(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.SeedWithRandom<TestEntity>(-1));
    }

    [Fact]
    public void SeedWith_ThrowsForNullEntity()
    {
        // Arrange
        var builder = new DbContextBuilder<TestDbContext>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.SeedWith<TestEntity>(null!));
    }
}