using Wolfgang.DbContextBuilderCore;
using Xunit;

namespace Wolfgang.DbContextBuilder.Tests.Fuzz;

/// <summary>
/// Fuzz properties for <see cref="AutoFixtureRandomEntityCreator"/> against the
/// <see cref="ICreateRandomEntities"/> count contract. Each runs
/// <see cref="FuzzSettings.CaseCount"/> cases (the weekly workflow sets this to 100,000+).
/// </summary>
[Trait("Category", "Fuzz")]
public sealed class AutoFixtureRandomEntityCreatorFuzzTests
{
    private static readonly ICreateRandomEntities Sut = new AutoFixtureRandomEntityCreator();


    [FuzzProperty]
    public bool CreateRandomEntities_respects_the_count_contract(int count)
        => RandomEntityCreatorProperties.Respects_the_count_contract(Sut, count);
}
