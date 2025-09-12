using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;

namespace Wolfgang.DbContextBuilderCore;

// TODO Create an interface for this class
// TODO Create implementations for AutoFixture and Bogus
// TODO Allow user to pass in their own implementation to DbContextBuilder
// TODO Allow user to configure AutoFixture or Bogus
// TODO Add tests



/// <summary>
/// Provides an API to generate random entities for seeding databases.
/// </summary>
internal class RandomEntityGenerator
{

    private readonly Fixture _fixture = new();

    public RandomEntityGenerator()
    {
        _fixture.Customize<DateOnly>(o => o.FromFactory((DateTime dt) => DateOnly.FromDateTime(dt)));
        _fixture.Customize<TimeOnly>(o => o.FromFactory((DateTime dt) => TimeOnly.FromDateTime(dt)));

        _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _fixture = new Fixture();
        _fixture.Customize(new NoCircularReferencesCustomization());
        _fixture.Customize(new IgnoreVirtualMembersCustomization());
    }



    /// <summary>
    /// Creates the specified number of entities of type TEntity with random data.
    /// </summary>
    /// <param name="count">The number of entities to create</param>
    /// <typeparam name="TEntity">The type of entity to create</typeparam>
    /// <returns>An IEnumerable{TEntity}</returns>
    internal IEnumerable<TEntity> CreateRandomEntities<TEntity>(int count)
        where TEntity : class
        => count < 1
            ? throw new ArgumentOutOfRangeException(nameof(count), count, "Value cannot be less than 1")
            : _fixture
                .Build<TEntity>()
                .CreateMany(count)
                .ToList();



    /// <summary>
    /// Creates the specified number of entities, passing each one to the specified function
    /// for additional initialization and configuration before being returned to the caller.
    /// </summary>
    /// <param name="count">The number of entities to create</param>
    /// <param name="func">A function that receives the newly created entity for further customization</param>
    /// <typeparam name="TEntity">The type of entity to create</typeparam>
    /// <returns>An IEnumerable{TEntity}</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    /// <remarks>
    /// This overload is useful for when the randomly created would have unrealistic values that you want to change.
    /// For example, a Customer entity would have a CustomerNumber and you want to generate a realistic value for that property.
    /// </remarks>
    /// <example>
    /// var builder = new DbContextBuilder{AppDbContext}()
    ///     .UseInMemory()
    ///     .SeedWithRandom{Customer}(count: 5, c => {c.CustomerNumber = GenerateRandomCustomerNumber(); return c; });
    /// </example>
    public IEnumerable<TEntity> CreateRandomEntities<TEntity>(int count, Func<TEntity, TEntity> func)
        where TEntity : class
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Value cannot be less than 1");
        }

        throw new NotImplementedException();
    }



    internal class NoCircularReferencesCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(behavior => fixture.Behaviors.Remove(behavior));
        }
    }

    internal class IgnoreVirtualMembersCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customizations.Add(new IgnoreVirtualMembers());
        }
    }

    internal class IgnoreVirtualMembers : ISpecimenBuilder
    {
        public object? Create(object request, ISpecimenContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var propertyInfo = request as PropertyInfo;
            if (propertyInfo == null)
            {
                return new NoSpecimen();
            }

            if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsVirtual)
            {
                return null;
            }

            return new NoSpecimen();
        }
    }
}