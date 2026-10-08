using System;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.DbContextBuilderCore.Tests.Unit.Models;
using Xunit;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Round-trips the shared <see cref="TableWithDefaults"/> test model through
/// <see cref="BasicContext"/>. Linked into every project that links the model, so each test
/// assembly exercises every column it compiles.
/// </summary>
public class TestModelRoundTripTests
{
    /// <summary>
    /// Verifies SeedWith stores every TableWithDefaults column and reads the same values back.
    /// </summary>
    [Fact]
    public async Task SeedWith_TableWithDefaults_round_trips_every_column()
    {
        var modified = new DateTime(2026, 10, 4, 12, 30, 0, DateTimeKind.Utc);
        var rowguid = new Guid("0f8fad5b-d9cb-469f-a165-70867728950e");
        using var sut = new DbContextBuilder<BasicContext>().UseInMemory();

        await using var context = await sut
            .SeedWith(new TableWithDefaults { Id = 7, ModifiedDate = modified, Rowguid = rowguid })
            .BuildAsync();

        var stored = context.Set<TableWithDefaults>().Single();

        Assert.Equal(7, stored.Id);
        Assert.Equal(modified, stored.ModifiedDate);
        Assert.Equal(rowguid, stored.Rowguid);
    }
}
