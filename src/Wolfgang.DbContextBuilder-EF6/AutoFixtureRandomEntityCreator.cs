using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;

namespace Wolfgang.DbContextBuilderEF6;



/// <summary>
/// Provides an API to create random entities for seeding databases.
/// </summary>
internal class AutoFixtureRandomEntityCreator : ICreateRandomEntities
{
    public AutoFixtureRandomEntityCreator()
    {
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
    /// Creates an instance of <see cref="AutoFixtureRandomEntityCreator"/> using the specified Fixture
    /// for creating random entities.
    /// </summary>
    /// <param name="fixture">The fixture to use when creating random entities</param>
    public AutoFixtureRandomEntityCreator(Fixture fixture)
    {
        Fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }


    /// <summary>
    /// The AutoFixture Fixture instance used to create random data.
    /// </summary>
    public Fixture Fixture { get; } = new();



    /// <summary>
    /// Creates the specified number of entities of type TEntity with random data.
    /// </summary>
    /// <param name="count">The number of entities to create</param>
    /// <typeparam name="TEntity">The type of entity to create</typeparam>
    /// <returns>An IEnumerable{TEntity}</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 1.</exception>
    public IEnumerable<TEntity> CreateRandomEntities<TEntity>(int count)
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
            if (fixture == null)
            {
                throw new ArgumentNullException(nameof(fixture));
            }

            if (!fixture.Behaviors.OfType<OmitOnRecursionBehavior>().Any())
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
            if (fixture == null)
            {
                throw new ArgumentNullException(nameof(fixture));
            }

            if (fixture.Customizations.OfType<IgnoreVirtualMembers>().Any())
            {
                return;
            }
            fixture.Customizations.Add(new IgnoreVirtualMembers());
        }
    }



    internal class IgnoreVirtualMembers : ISpecimenBuilder
    {
        public object? Create(object request, ISpecimenContext context)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

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
