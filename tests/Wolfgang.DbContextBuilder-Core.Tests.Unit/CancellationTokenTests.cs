using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Wolfgang.DbContextBuilderCore.Assertions;
using Wolfgang.DbContextBuilderCore.Tests.Unit.Models;
using Xunit;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Tests for the <see cref="CancellationToken"/> overloads (#575).
/// </summary>
public class CancellationTokenTests
{
    /// <summary>
    /// Verifies that BuildAsync with a canceled token throws before doing any work.
    /// </summary>
    [Fact]
    public async Task BuildAsync_with_a_canceled_token_throws_OperationCanceledException()
    {
        var creator = new TokenRecordingCreator();
        using var sut = new DbContextBuilder<BasicContext>().UseCustomDbContextCreator(creator);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sut.BuildAsync(new CancellationToken(canceled: true)));

        Assert.Empty(creator.Tokens);
    }



    /// <summary>
    /// Verifies that BuildAsync passes its token to the context creator, for the seed context and
    /// the returned one.
    /// </summary>
    [Fact]
    public async Task BuildAsync_passes_the_token_to_the_context_creator()
    {
        using var cts = new CancellationTokenSource();
        var creator = new TokenRecordingCreator();
        using var sut = new DbContextBuilder<BasicContext>().UseCustomDbContextCreator(creator);

        await using var context = await sut.BuildAsync(cts.Token);

        Assert.Equal(new[] { cts.Token, cts.Token }, creator.Tokens);
    }



    /// <summary>
    /// Verifies that the parameterless BuildAsync passes CancellationToken.None, and that a creator's
    /// plain overload is not what the builder calls.
    /// </summary>
    [Fact]
    public async Task BuildAsync_without_a_token_passes_None_to_the_token_overload()
    {
        var creator = new TokenRecordingCreator();
        using var sut = new DbContextBuilder<BasicContext>().UseCustomDbContextCreator(creator);

        await using var built = await sut.BuildAsync();
        await using var direct = await creator.CreateDbContextAsync(new DbContextOptionsBuilder<BasicContext>());

        // Two from the build (through the token overload), one from the direct plain call.
        Assert.Equal(new[] { CancellationToken.None, CancellationToken.None, CancellationToken.None }, creator.Tokens);
        Assert.Equal(new[] { true, true, false }, creator.ViaTokenOverload);
    }



    /// <summary>
    /// Verifies that the default implementation of the token overload checks the token and then
    /// calls the original overload, so existing creators keep working.
    /// </summary>
    [Fact]
    public async Task ICreateDbContext_default_token_overload_checks_the_token_then_delegates()
    {
        ICreateDbContext sut = new InMemoryDbContextCreator();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sut.CreateDbContextAsync(new DbContextOptionsBuilder<BasicContext>(), new CancellationToken(canceled: true)));
        await using var context = await sut.CreateDbContextAsync(new DbContextOptionsBuilder<BasicContext>(), CancellationToken.None);

        Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", context.Database.ProviderName);
    }



    /// <summary>
    /// Verifies that every assertion's token overload observes a canceled token.
    /// </summary>
    [Theory]
    [MemberData(nameof(AssertionCalls))]
    public async Task DbSetAssertions_token_overloads_observe_a_canceled_token(string call)
    {
        using var builder = new DbContextBuilder<BasicContext>().UseInMemory().SeedWith(new TableWithDefaults { Id = 1 });
        await using var context = await builder.BuildAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => AssertionCallsByName[call](context.Set<TableWithDefaults>().Should(), new CancellationToken(canceled: true)));
    }



    // TableWithDefaults is internal, so the theory takes the call's name and looks it up here.
    private static readonly Dictionary<string, Func<DbSetAssertions<TableWithDefaults>, CancellationToken, Task>> AssertionCallsByName = new()
    {
        ["HaveCount"] = (should, token) => should.HaveCount(1, token),
        ["BeEmpty"] = (should, token) => should.BeEmpty(token),
        ["NotBeEmpty"] = (should, token) => should.NotBeEmpty(token),
        ["Contain"] = (should, token) => should.Contain(row => row.Id == 1, token),
        ["NotContain"] = (should, token) => should.NotContain(row => row.Id == 2, token),
        ["AllSatisfy"] = (should, token) => should.AllSatisfy(row => row.Id > 0, token),
    };



    /// <summary>
    /// The names of the assertion overloads the canceled-token theory exercises.
    /// </summary>
    public static TheoryData<string> AssertionCalls()
    {
        var data = new TheoryData<string>();
        foreach (var name in AssertionCallsByName.Keys)
        {
            data.Add(name);
        }

        return data;
    }
}



/// <summary>Implements both creator overloads and records the token the builder passes.</summary>
internal sealed class TokenRecordingCreator : ICreateDbContext
{
    private readonly string _databaseName = Guid.NewGuid().ToString();



    public List<CancellationToken> Tokens { get; } = [];



    public List<bool> ViaTokenOverload { get; } = [];



    public Task<TDbContext> CreateDbContextAsync<TDbContext>(DbContextOptionsBuilder<TDbContext> optionsBuilder)
        where TDbContext : DbContext =>
        Create(optionsBuilder, CancellationToken.None, viaTokenOverload: false);



    public Task<TDbContext> CreateDbContextAsync<TDbContext>(DbContextOptionsBuilder<TDbContext> optionsBuilder, CancellationToken cancellationToken)
        where TDbContext : DbContext =>
        Create(optionsBuilder, cancellationToken, viaTokenOverload: true);



    private Task<TDbContext> Create<TDbContext>(DbContextOptionsBuilder<TDbContext> optionsBuilder, CancellationToken cancellationToken, bool viaTokenOverload)
        where TDbContext : DbContext
    {
        Tokens.Add(cancellationToken);
        ViaTokenOverload.Add(viaTokenOverload);
        optionsBuilder.UseInMemoryDatabase(_databaseName);
        return Task.FromResult((TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options)!);
    }
}
