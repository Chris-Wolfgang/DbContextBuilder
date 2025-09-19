using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;

namespace Wolfgang.DbContextBuilderCore;



/// <summary>
/// Provides an API to generate random entities for seeding databases.
/// </summary>
internal class AutoFixtureRandomEntityGenerator : IGenerateRandomEntities
{
    public AutoFixtureRandomEntityGenerator()
    {
        // AutoFixture 4.x does not have built in support for DateOnly and TimeOnly. Version is supposed to 
        Fixture.Customize<DateOnly>(o => o.FromFactory((DateTime dt) => DateOnly.FromDateTime(dt)));
        Fixture.Customize<TimeOnly>(o => o.FromFactory((DateTime dt) => TimeOnly.FromDateTime(dt)));

        // Prevents issues with circular references
        Fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        Fixture.Customize(new NoCircularReferencesCustomization());
        Fixture.Customize(new IgnoreVirtualMembersCustomization());
    }



    /// <summary>
    /// Creates an instance of AutoFixtureRandomEntityGenerator using the specified Fixture
    /// for generating random entities.
    /// </summary>
    /// <param name="fixture">The fixture to use when generating random entities</param>
    public AutoFixtureRandomEntityGenerator(Fixture fixture)
    {
        Fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }



    /// <summary>
    /// The AutoFixture Fixture instance used to generate random data.
    /// </summary>
    public Fixture Fixture { get; } = new();



    /// <summary>
    /// Creates the specified number of entities of type TEntity with random data.
    /// </summary>
    /// <param name="count">The number of entities to create</param>
    /// <typeparam name="TEntity">The type of entity to create</typeparam>
    /// <returns>An IEnumerable{TEntity}</returns>
    public IEnumerable<TEntity> GenerateRandomEntities<TEntity>(int count)
        where TEntity : class
        => count < 1
            ? throw new ArgumentOutOfRangeException(nameof(count), count, "Value cannot be less than 1")
            : Fixture
                .Build<TEntity>()
                .CreateMany(count)
                .ToList();



    internal class NoCircularReferencesCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            if (fixture.Behaviors.OfType<OmitOnRecursionBehavior>().Any())
            {
                fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            }

            fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
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
                throw new ArgumentNullException(nameof(context));
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