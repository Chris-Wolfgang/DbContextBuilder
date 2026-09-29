using Wolfgang.DbContextBuilderCore;

namespace Wolfgang.DbContextBuilder.Tests.Fuzz;

/// <summary>
/// Direct, deterministic unit tests for <see cref="RandomEntityCreatorProperties"/> itself -
/// pinning the two branches FsCheck's randomized case generation doesn't reliably reach at the
/// default (modest, local) case count: the out-of-bound magnitude short-circuit, and the
/// "the creator should have thrown but didn't" failure path.
/// </summary>
public sealed class RandomEntityCreatorPropertiesTests
{
    [Theory]
    [InlineData(5001)]
    [InlineData(-5001)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Respects_the_count_contract_is_vacuously_true_beyond_the_magnitude_bound(int count)
    {
        var result = RandomEntityCreatorProperties.Respects_the_count_contract(new BogusRandomEntityCreator(), count);

        Assert.True(result);
    }


    [Fact]
    public void Respects_the_count_contract_returns_false_when_a_creator_fails_to_throw()
    {
        var result = RandomEntityCreatorProperties.Respects_the_count_contract(new CreatorThatNeverThrows(), count: 0);

        Assert.False(result);
    }


    // A deliberately broken ICreateRandomEntities: violates the documented contract by
    // returning an empty sequence instead of throwing for count < 1. Exists only to prove
    // Respects_the_count_contract actually detects a contract violation instead of passing
    // vacuously - the "should have thrown" branch has no other way to execute during a normal,
    // passing test run.
    private sealed class CreatorThatNeverThrows : ICreateRandomEntities
    {
        public IEnumerable<TEntity> CreateRandomEntities<TEntity>(int count) where TEntity : class
            => [];
    }
}
