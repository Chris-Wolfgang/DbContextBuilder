using Wolfgang.DbContextBuilderCore;

namespace Wolfgang.DbContextBuilder.Tests.Fuzz;

/// <summary>
/// Property bodies shared by every <see cref="ICreateRandomEntities"/> implementation's fuzz
/// suite. <c>BogusRandomEntityCreator</c> and <c>AutoFixtureRandomEntityCreator</c> are two
/// independent implementations of the same interface, and the interface's XML doc makes an
/// explicit contract claim ("Count must be greater than 0", "returns exactly <paramref
/// name="count"/> entities") - these properties fuzz both implementations against that one
/// contract, rather than duplicating the assertions per class.
/// </summary>
internal static class RandomEntityCreatorProperties
{
    /// <summary>
    /// <paramref name="count"/> is fuzzed across the full <see cref="int"/> range by the
    /// caller; both the below-the-floor and at-or-above-the-floor branches of the documented
    /// contract are exercised by the same property. Bounds the magnitude generating output
    /// defensively - materializing entities is real (reflection-driven) work per item, unlike
    /// AuditTrail's pure string-fuzzing properties this suite is modeled on, so an
    /// FsCheck-shrink-search excursion to an extreme count must not turn into a multi-minute
    /// (or hung) CI run. Values outside the bound are vacuously satisfied (returns true),
    /// matching the established "not applicable, so true" pattern for out-of-scope inputs in
    /// a boolean FsCheck property.
    /// </summary>
    public static bool Respects_the_count_contract(ICreateRandomEntities creator, int count)
    {
        ArgumentNullException.ThrowIfNull(creator);

        const int magnitudeBound = 5_000;
        if (count > magnitudeBound || count < -magnitudeBound)
        {
            return true;
        }

        if (count < 1)
        {
            try
            {
                _ = creator.CreateRandomEntities<FuzzEntity>(count).ToList();
                return false; // should have thrown
            }
            catch (ArgumentOutOfRangeException)
            {
                return true;
            }
        }

        var items = creator.CreateRandomEntities<FuzzEntity>(count).ToList();
        return items.Count == count && items.TrueForAll(item => item is not null);
    }
}
