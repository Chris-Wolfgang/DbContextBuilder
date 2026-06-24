using AdventureWorks.Models;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Tests for <see cref="DbContextBuilder{T}.UseSeedProfile"/> and
/// <see cref="ISeedProfile{T}"/>.
/// </summary>
public class SeedProfileTests
{
    private sealed class TwoAddressesProfile : ISeedProfile<AdventureWorksDbContext>
    {
        public void Apply(DbContextBuilder<AdventureWorksDbContext> builder) =>
            builder.SeedWith
            (
                MakeAddress(901, "1 First St"),
                MakeAddress(902, "2 Second St")
            );
    }

    private sealed class OneAddressProfile : ISeedProfile<AdventureWorksDbContext>
    {
        public void Apply(DbContextBuilder<AdventureWorksDbContext> builder) =>
            builder.SeedWith(MakeAddress(903, "3 Third St"));
    }

    private static Address MakeAddress(int id, string line1) => new()
    {
        AddressId = id,
        AddressLine1 = line1,
        City = "Townsville",
        StateProvinceId = 1,
        PostalCode = "00001",
        Rowguid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };



    /// <summary>
    /// Verifies that UseSeedProfile applies the seed data declared by the profile.
    /// </summary>
    [Fact]
    public async Task UseSeedProfile_applies_the_profiles_seed_data()
    {
        using var sut = new DbContextBuilder<AdventureWorksDbContext>().UseInMemory();

        await using var context = await sut
            .UseSeedProfile(new TwoAddressesProfile())
            .BuildAsync();

        Assert.Equal(2, context.Addresses.Count());
    }



    /// <summary>
    /// Verifies that UseSeedProfile returns the builder so calls can be chained.
    /// </summary>
    [Fact]
    public void UseSeedProfile_returns_the_builder_for_chaining()
    {
        using var sut = new DbContextBuilder<AdventureWorksDbContext>().UseInMemory();

        var result = sut.UseSeedProfile(new OneAddressProfile());

        Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(result);
    }



    /// <summary>
    /// Verifies that UseSeedProfile with a null profile throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void UseSeedProfile_when_passed_null_throws_ArgumentNullException()
    {
        using var sut = new DbContextBuilder<AdventureWorksDbContext>().UseInMemory();

        var ex = Assert.Throws<ArgumentNullException>(() => sut.UseSeedProfile(null!));
        Assert.Equal("profile", ex.ParamName);
    }



    /// <summary>
    /// Verifies that applying multiple profiles accumulates their seed data.
    /// </summary>
    [Fact]
    public async Task UseSeedProfile_when_multiple_profiles_applied_accumulates_their_seed_data()
    {
        using var sut = new DbContextBuilder<AdventureWorksDbContext>().UseInMemory();

        await using var context = await sut
            .UseSeedProfile(new TwoAddressesProfile())
            .UseSeedProfile(new OneAddressProfile())
            .BuildAsync();

        Assert.Equal(3, context.Addresses.Count());
    }
}
