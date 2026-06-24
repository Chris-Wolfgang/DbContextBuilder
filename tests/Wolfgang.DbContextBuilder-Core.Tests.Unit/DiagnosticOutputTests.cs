using AdventureWorks.Models;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Tests for <see cref="DbContextBuilder{T}.UseDiagnosticOutput"/>.
/// </summary>
public class DiagnosticOutputTests
{
    private static Address MakeAddress(int id) => new()
    {
        AddressId = id,
        AddressLine1 = $"{id} Main St",
        City = "Townsville",
        StateProvinceId = 1,
        PostalCode = "00001",
        Rowguid = Guid.NewGuid(),
        ModifiedDate = DateTime.UtcNow,
    };



    /// <summary>
    /// Verifies that UseDiagnosticOutput writes a summary line reporting the seeded row count.
    /// </summary>
    [Fact]
    public async Task UseDiagnosticOutput_writes_a_seed_summary()
    {
        var lines = new List<string>();
        using var sut = new DbContextBuilder<AdventureWorksDbContext>().UseInMemory();

        await using var context = await sut
            .UseDiagnosticOutput(lines.Add)
            .SeedWith(MakeAddress(1), MakeAddress(2))
            .BuildAsync();

        Assert.Contains(lines, line => line.Contains("seeded 2 entity row(s)", StringComparison.Ordinal));
    }



    /// <summary>
    /// Verifies that EF Core's own diagnostic logs (not just the summary line) are captured.
    /// </summary>
    [Fact]
    public async Task UseDiagnosticOutput_captures_ef_core_logs()
    {
        var lines = new List<string>();
        using var sut = new DbContextBuilder<AdventureWorksDbContext>().UseInMemory();

        await using var context = await sut
            .UseDiagnosticOutput(lines.Add)
            .SeedWith(MakeAddress(1))
            .BuildAsync();

        // At least one captured line is an EF Core log rather than the builder's own summary.
        Assert.Contains(lines, line => !line.StartsWith("DbContextBuilder<", StringComparison.Ordinal));
    }



    /// <summary>
    /// Verifies that UseDiagnosticOutput returns the builder so calls can be chained.
    /// </summary>
    [Fact]
    public void UseDiagnosticOutput_returns_the_builder_for_chaining()
    {
        using var sut = new DbContextBuilder<AdventureWorksDbContext>().UseInMemory();

        var result = sut.UseDiagnosticOutput(_ => { });

        Assert.IsType<DbContextBuilder<AdventureWorksDbContext>>(result);
    }



    /// <summary>
    /// Verifies that UseDiagnosticOutput with a null sink throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void UseDiagnosticOutput_when_passed_null_throws_ArgumentNullException()
    {
        using var sut = new DbContextBuilder<AdventureWorksDbContext>().UseInMemory();

        var ex = Assert.Throws<ArgumentNullException>(() => sut.UseDiagnosticOutput(null!));
        Assert.Equal("writeLine", ex.ParamName);
    }
}
