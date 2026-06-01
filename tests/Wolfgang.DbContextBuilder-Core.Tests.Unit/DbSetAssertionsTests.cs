using Microsoft.EntityFrameworkCore;
using Wolfgang.DbContextBuilderCore.Assertions;
using Wolfgang.DbContextBuilderCore.Tests.Unit.Models;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Tests for the fluent DbSet assertion extensions under
/// <see cref="DbContextAssertionsExtensions"/> / <see cref="DbSetAssertions{TEntity}"/>.
/// All assertions are exercised against the EF Core InMemory provider seeded with a few
/// rows of TableWithDefaults to keep the test focused on the assertion logic, not on
/// AdventureWorks seeding or relational SQLite quirks.
/// </summary>
public class DbSetAssertionsTests
{

    private static async Task<BasicContext> CreateSeededContextAsync(int rowCount)
    {
        using var builder = new DbContextBuilder<BasicContext>().UseInMemory();
        var seeds = new TableWithDefaults[rowCount];
        for (var i = 0; i < rowCount; i++)
        {
            seeds[i] = new TableWithDefaults { Id = i + 1 };
        }
        builder.SeedWith(seeds);
        return await builder.BuildAsync();
    }



    /// <summary>
    /// Verifies HaveCount passes when the actual row count matches.
    /// </summary>
    [Fact]
    public async Task HaveCount_when_count_matches_does_not_throw()
    {
        await using var context = await CreateSeededContextAsync(3);

        await context.Set<TableWithDefaults>().Should().HaveCount(3);
    }



    /// <summary>
    /// Verifies HaveCount throws when the actual count differs, and the message identifies
    /// the entity type plus both expected and actual values.
    /// </summary>
    [Fact]
    public async Task HaveCount_when_count_differs_throws_with_informative_message()
    {
        await using var context = await CreateSeededContextAsync(3);

        var ex = await Assert.ThrowsAsync<DbContextAssertionException>(
            async () => await context.Set<TableWithDefaults>().Should().HaveCount(5));

        Assert.Contains("TableWithDefaults", ex.Message, StringComparison.Ordinal);
        Assert.Contains("5 entities", ex.Message, StringComparison.Ordinal);
        Assert.Contains("found 3", ex.Message, StringComparison.Ordinal);
    }



    /// <summary>
    /// Verifies BeEmpty passes when the DbSet has no rows.
    /// </summary>
    [Fact]
    public async Task BeEmpty_when_set_is_empty_does_not_throw()
    {
        await using var context = await CreateSeededContextAsync(0);

        await context.Set<TableWithDefaults>().Should().BeEmpty();
    }



    /// <summary>
    /// Verifies BeEmpty throws when the DbSet has rows.
    /// </summary>
    [Fact]
    public async Task BeEmpty_when_set_has_rows_throws()
    {
        await using var context = await CreateSeededContextAsync(2);

        var ex = await Assert.ThrowsAsync<DbContextAssertionException>(
            async () => await context.Set<TableWithDefaults>().Should().BeEmpty());

        Assert.Contains("be empty", ex.Message, StringComparison.Ordinal);
        Assert.Contains("found 2", ex.Message, StringComparison.Ordinal);
    }



    /// <summary>
    /// Verifies NotBeEmpty passes when the DbSet has any rows.
    /// </summary>
    [Fact]
    public async Task NotBeEmpty_when_set_has_rows_does_not_throw()
    {
        await using var context = await CreateSeededContextAsync(1);

        await context.Set<TableWithDefaults>().Should().NotBeEmpty();
    }



    /// <summary>
    /// Verifies NotBeEmpty throws when the DbSet is empty.
    /// </summary>
    [Fact]
    public async Task NotBeEmpty_when_set_is_empty_throws()
    {
        await using var context = await CreateSeededContextAsync(0);

        var ex = await Assert.ThrowsAsync<DbContextAssertionException>(
            async () => await context.Set<TableWithDefaults>().Should().NotBeEmpty());

        Assert.Contains("at least one entity", ex.Message, StringComparison.Ordinal);
    }



    /// <summary>
    /// Verifies Contain passes when at least one row matches the predicate.
    /// </summary>
    [Fact]
    public async Task Contain_when_predicate_matches_does_not_throw()
    {
        await using var context = await CreateSeededContextAsync(3);

        await context.Set<TableWithDefaults>().Should().Contain(t => t.Id == 2);
    }



    /// <summary>
    /// Verifies Contain throws when no rows match the predicate, and the message includes
    /// the total row count for context.
    /// </summary>
    [Fact]
    public async Task Contain_when_no_match_throws_with_total_count_in_message()
    {
        await using var context = await CreateSeededContextAsync(3);

        var ex = await Assert.ThrowsAsync<DbContextAssertionException>(
            async () => await context.Set<TableWithDefaults>().Should().Contain(t => t.Id == 99));

        Assert.Contains("TableWithDefaults", ex.Message, StringComparison.Ordinal);
        Assert.Contains("among 3 entities", ex.Message, StringComparison.Ordinal);
    }



    /// <summary>
    /// Verifies NotContain passes when no rows match the predicate.
    /// </summary>
    [Fact]
    public async Task NotContain_when_no_match_does_not_throw()
    {
        await using var context = await CreateSeededContextAsync(3);

        await context.Set<TableWithDefaults>().Should().NotContain(t => t.Id == 999);
    }



    /// <summary>
    /// Verifies NotContain throws when at least one row matches the predicate, and the
    /// message reports the matching count.
    /// </summary>
    [Fact]
    public async Task NotContain_when_predicate_matches_throws_with_count()
    {
        await using var context = await CreateSeededContextAsync(3);

        var ex = await Assert.ThrowsAsync<DbContextAssertionException>(
            async () => await context.Set<TableWithDefaults>().Should().NotContain(t => t.Id >= 1));

        Assert.Contains("3 matching entities", ex.Message, StringComparison.Ordinal);
    }



    /// <summary>
    /// Verifies AllSatisfy passes when every row satisfies the predicate.
    /// </summary>
    [Fact]
    public async Task AllSatisfy_when_every_row_matches_does_not_throw()
    {
        await using var context = await CreateSeededContextAsync(3);

        await context.Set<TableWithDefaults>().Should().AllSatisfy(t => t.Id > 0);
    }



    /// <summary>
    /// Verifies AllSatisfy on an empty set is vacuously true (does not throw) — matches the
    /// LINQ All semantic.
    /// </summary>
    [Fact]
    public async Task AllSatisfy_when_set_is_empty_does_not_throw()
    {
        await using var context = await CreateSeededContextAsync(0);

        await context.Set<TableWithDefaults>().Should().AllSatisfy(t => t.Id == 1234567);
    }



    /// <summary>
    /// Verifies AllSatisfy throws when at least one row fails, and the message tells the
    /// reader how many of how many failed.
    /// </summary>
    [Fact]
    public async Task AllSatisfy_when_some_rows_fail_throws_with_failure_count()
    {
        await using var context = await CreateSeededContextAsync(3);

        var ex = await Assert.ThrowsAsync<DbContextAssertionException>(
            async () => await context.Set<TableWithDefaults>().Should().AllSatisfy(t => t.Id == 1));

        Assert.Contains("3 entities", ex.Message, StringComparison.Ordinal);
        Assert.Contains("2 of 3 failed", ex.Message, StringComparison.Ordinal);
    }



    /// <summary>
    /// Verifies the chain returns this from each assertion so multiple checks can be
    /// composed in a single await without throwing.
    /// </summary>
    [Fact]
    public async Task Chained_assertions_compose_cleanly_when_all_pass()
    {
        await using var context = await CreateSeededContextAsync(3);

        await (await (await context.Set<TableWithDefaults>().Should()
            .HaveCount(3))
            .NotBeEmpty())
            .AllSatisfy(t => t.Id > 0);
    }



    /// <summary>
    /// Verifies the IQueryable overload of <c>.Should()</c> works against a filtered query
    /// — letting callers assert on subsets without materializing them first.
    /// </summary>
    [Fact]
    public async Task Should_on_filtered_IQueryable_asserts_against_the_filter()
    {
        await using var context = await CreateSeededContextAsync(5);

        await context.Set<TableWithDefaults>().Where(t => t.Id <= 3).Should().HaveCount(3);
    }



    /// <summary>
    /// Verifies all three <see cref="DbContextAssertionException"/> constructors behave as
    /// the standard <see cref="Exception"/> pattern requires (default message, supplied
    /// message, message + inner exception).
    /// </summary>
    [Fact]
    public void DbContextAssertionException_constructors_behave_as_expected()
    {
        var defaultEx = new DbContextAssertionException();
        Assert.NotNull(defaultEx.Message);

        var msgEx = new DbContextAssertionException("boom");
        Assert.Equal("boom", msgEx.Message);

        var inner = new InvalidOperationException("inner");
        var wrappedEx = new DbContextAssertionException("outer", inner);
        Assert.Equal("outer", wrappedEx.Message);
        Assert.Same(inner, wrappedEx.InnerException);
    }



    /// <summary>
    /// The DbSet variant of <c>Should()</c> must reject a null set argument.
    /// </summary>
    [Fact]
    public void Should_on_DbSet_when_set_is_null_throws_ArgumentNullException()
    {
        DbSet<TableWithDefaults>? set = null;

        var ex = Assert.Throws<ArgumentNullException>(() => set!.Should());
        Assert.Equal("set", ex.ParamName);
    }



    /// <summary>
    /// The IQueryable variant of <c>Should()</c> must reject a null query argument.
    /// </summary>
    [Fact]
    public void Should_on_IQueryable_when_query_is_null_throws_ArgumentNullException()
    {
        IQueryable<TableWithDefaults>? query = null;

        var ex = Assert.Throws<ArgumentNullException>(() => query!.Should());
        Assert.Equal("query", ex.ParamName);
    }



    /// <summary>
    /// Contain/NotContain/AllSatisfy must reject a null predicate via
    /// <see cref="ArgumentNullException.ThrowIfNull(object?, string?)"/>.
    /// </summary>
    [Fact]
    public async Task Predicate_assertions_when_predicate_is_null_throw_ArgumentNullException()
    {
        await using var context = await CreateSeededContextAsync(1);
        var assertions = context.Set<TableWithDefaults>().Should();

        await Assert.ThrowsAsync<ArgumentNullException>(() => assertions.Contain(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => assertions.NotContain(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => assertions.AllSatisfy(null!));
    }
}
