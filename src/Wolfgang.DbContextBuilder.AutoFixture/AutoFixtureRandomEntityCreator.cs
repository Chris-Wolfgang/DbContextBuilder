using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;

namespace Wolfgang.DbContextBuilderCore;



/// <summary>
/// An <see cref="ICreateRandomEntities"/> implementation backed by
/// <see href="https://github.com/AutoFixture/AutoFixture">AutoFixture</see>. It populates the
/// scalar properties of an entity with random values while omitting virtual (navigation)
/// members and breaking circular references, so seeding does not pull in object graphs.
/// </summary>
/// <remarks>
/// Configure the builder to use it with <c>UseAutoFixture()</c> (the convenience extension) or
/// <c>UseCustomRandomEntityCreator(new AutoFixtureRandomEntityCreator())</c>.
/// </remarks>
public class AutoFixtureRandomEntityCreator : ICreateRandomEntities
{
    /// <summary>
    /// Creates an instance backed by a new <see cref="AutoFixture.Fixture"/> configured with the
    /// customizations this creator relies on (DateOnly/TimeOnly support, recursion omission, and
    /// virtual-member exclusion).
    /// </summary>
    public AutoFixtureRandomEntityCreator()
    {
        // AutoFixture 4.x does not have built-in support for DateOnly and TimeOnly. Add factories
        // that convert from a generated DateTime so AutoFixture can produce these types.
        Fixture.Customize<DateOnly>(o => o.FromFactory((DateTime dt) => DateOnly.FromDateTime(dt)));
        Fixture.Customize<TimeOnly>(o => o.FromFactory((DateTime dt) => TimeOnly.FromDateTime(dt)));

        // Prevents issues with circular references — the customization itself handles
        // swapping ThrowingRecursionBehavior for OmitOnRecursionBehavior idempotently.
        Fixture.Customize(new NoCircularReferencesCustomization());
        Fixture.Customize(new IgnoreVirtualMembersCustomization());
    }



    /// <summary>
    /// Creates an instance of <see cref="AutoFixtureRandomEntityCreator"/> using the specified Fixture
    /// for creating random entities.
    /// </summary>
    /// <param name="fixture">The fixture to use when creating random entities</param>
    /// <exception cref="ArgumentNullException"><paramref name="fixture"/> is null.</exception>
    public AutoFixtureRandomEntityCreator(Fixture fixture) =>
        Fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));


    /// <summary>
    /// The AutoFixture Fixture instance used to create random data.
    /// </summary>
    public Fixture Fixture { get; } = new();



    /// <summary>
    /// Creates the specified number of entities of type TEntity with random data.
    /// </summary>
    /// <param name="count">The number of entities to create</param>
    /// <typeparam name="TEntity">The type of entity to create</typeparam>
    /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TEntity"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 1.</exception>
    public IEnumerable<TEntity> CreateRandomEntities<TEntity>(int count)
        where TEntity : class
        => count < 1
            ? throw new ArgumentOutOfRangeException(nameof(count), count, "Value cannot be less than 1")
            : Fixture
                .Build<TEntity>()
                .CreateMany(count)
                .ToList();



    /// <summary>
    /// An AutoFixture <see cref="ICustomization"/> that swaps the default
    /// <see cref="ThrowingRecursionBehavior"/> for <see cref="OmitOnRecursionBehavior"/> so that
    /// circular object graphs are truncated rather than throwing. Applying it more than once is a
    /// no-op.
    /// </summary>
    public class NoCircularReferencesCustomization : ICustomization
    {
        /// <summary>
        /// Applies the recursion-omitting behavior to <paramref name="fixture"/>.
        /// </summary>
        /// <param name="fixture">The fixture to customize.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fixture"/> is null.</exception>
        public void Customize(IFixture fixture)
        {
            ArgumentNullException.ThrowIfNull(fixture);

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



    /// <summary>
    /// An AutoFixture <see cref="ICustomization"/> that registers <see cref="IgnoreVirtualMembers"/>
    /// so that virtual (navigation) properties are left unset. Applying it more than once is a
    /// no-op.
    /// </summary>
    public class IgnoreVirtualMembersCustomization : ICustomization
    {
        /// <summary>
        /// Registers the <see cref="IgnoreVirtualMembers"/> specimen builder on <paramref name="fixture"/>.
        /// </summary>
        /// <param name="fixture">The fixture to customize.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fixture"/> is null.</exception>
        public void Customize(IFixture fixture)
        {
            ArgumentNullException.ThrowIfNull(fixture);
            if (fixture.Customizations.OfType<IgnoreVirtualMembers>().Any())
            {
                return;
            }
            fixture.Customizations.Add(new IgnoreVirtualMembers());
        }
    }



    /// <summary>
    /// An AutoFixture <see cref="ISpecimenBuilder"/> that returns <c>null</c> for virtual
    /// properties (so navigation members are not populated) and defers all other requests.
    /// </summary>
    public class IgnoreVirtualMembers : ISpecimenBuilder
    {
        /// <summary>
        /// Returns <c>null</c> when <paramref name="request"/> is a virtual property, otherwise a
        /// <see cref="NoSpecimen"/> so other builders handle the request.
        /// </summary>
        /// <param name="request">The specimen request.</param>
        /// <param name="context">The specimen context.</param>
        /// <returns><c>null</c> for a virtual property; otherwise a <see cref="NoSpecimen"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="request"/> or <paramref name="context"/> is null.</exception>
        public object? Create(object request, ISpecimenContext context)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(context);

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
