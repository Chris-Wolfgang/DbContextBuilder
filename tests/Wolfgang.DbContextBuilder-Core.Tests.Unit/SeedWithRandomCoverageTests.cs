using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Xunit;

namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Exercises the <c>SeedWithRandom</c> / foreign-key-reconciliation paths of
/// <see cref="DbContextBuilder{T}"/> using a deterministic in-repo
/// <see cref="ICreateRandomEntities"/> double (no AutoFixture dependency, so no Core-assembly
/// type collision). This lives in the shared Core test source and is linked into every EF-version
/// wrapper, so it runs on net8.0+ runtimes — where the coverage collector works — and therefore
/// covers <c>DbContextBuilder&lt;T&gt;</c> for the net6.0/net7.0-targeted Core-EF6/EF7 assemblies,
/// whose AutoFixture-backed integration suite only runs on the net6.0/net7.0 diagonal (where the
/// collector emits nothing).
/// </summary>
public class SeedWithRandomCoverageTests
{
    private static DbContextBuilder<CoverageContext> NewBuilder() =>
        new DbContextBuilder<CoverageContext>().UseCustomRandomEntityCreator(new DeterministicRandomEntityCreator());



    /// <summary>
    /// Verifies SeedWithRandom seeds the requested number of rows via the configured provider.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_seeds_the_requested_count()
    {
        using var sut = NewBuilder().UseInMemory();

        await using var context = await sut
            .SeedWithRandom<CoverageManufacturer>(5)
            .BuildAsync();

        Assert.Equal(5, context.Manufacturers.Count());
    }



    /// <summary>
    /// Verifies the SeedWithRandom(int, Func) overload applies the transform to each entity.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_func_overload_applies_the_transform()
    {
        using var sut = NewBuilder().UseInMemory();

        await using var context = await sut
            .SeedWithRandom<CoverageManufacturer>(3, m => { m.Name = "transformed"; return m; })
            .BuildAsync();

        Assert.Equal(3, context.Manufacturers.Count());
        Assert.All(context.Manufacturers.ToList(), m => Assert.Equal("transformed", m.Name));
    }



    /// <summary>
    /// Verifies the SeedWithRandom(int, Func with index) overload applies the indexed transform.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_indexed_func_overload_applies_the_transform()
    {
        using var sut = NewBuilder().UseInMemory();

        await using var context = await sut
            .SeedWithRandom<CoverageManufacturer>(3, (m, i) => { m.Id = i + 100; return m; })
            .BuildAsync();

        var ids = context.Manufacturers.Select(m => m.Id).OrderBy(id => id).ToList();
        Assert.Equal(new[] { 100, 101, 102 }, ids);
    }



    /// <summary>
    /// Verifies that foreign keys on randomly-seeded entities are reconciled against the model:
    /// a required FK is wired to a seeded principal and an optional FK with no principal is nulled,
    /// so the build does not violate SQLite's foreign-key constraints.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_reconciles_required_and_optional_foreign_keys()
    {
        using var sut = NewBuilder().UseSqlite();

        await using var context = await sut
            .SeedWithRandom<CoverageManufacturer>(2)
            .SeedWithRandom<CoverageWidget>(3)
            .BuildAsync();

        var manufacturerIds = context.Manufacturers.Select(m => m.Id).ToHashSet();
        var widgets = context.Widgets.ToList();

        Assert.Equal(3, widgets.Count);
        Assert.All(widgets, w => Assert.Contains(w.ManufacturerId, manufacturerIds));
        Assert.All(widgets, w => Assert.Null(w.SupplierId));
    }



    /// <summary>
    /// Verifies the SqliteForMsSqlServer extension is exercised end-to-end with random seeding.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_works_with_UseSqliteForMsSqlServer()
    {
        using var sut = NewBuilder().UseSqliteForMsSqlServer();

        await using var context = await sut
            .SeedWithRandom<CoverageManufacturer>(2)
            .BuildAsync();

        Assert.Equal(2, context.Manufacturers.Count());
    }



    /// <summary>
    /// Verifies SeedWithRandom throws when no random-entity provider has been configured.
    /// </summary>
    [Fact]
    public void SeedWithRandom_when_no_provider_configured_throws_InvalidOperationException()
    {
        using var sut = new DbContextBuilder<CoverageContext>().UseInMemory();

        var ex = Assert.Throws<InvalidOperationException>(() => sut.SeedWithRandom<CoverageManufacturer>(1));
        Assert.Equal
        (
            "SeedWithRandom requires a random-entity provider, but none is configured. " +
            "Call UseAutoFixture() (add the Wolfgang.DbContextBuilder.AutoFixture package), " +
            "UseBogus() (add the Wolfgang.DbContextBuilder.Bogus package), or " +
            "UseCustomRandomEntityCreator(...) before SeedWithRandom.",
            ex.Message
        );
    }



    /// <summary>
    /// Verifies all three SeedWithRandom overloads reject a count below one.
    /// </summary>
    [Fact]
    public void SeedWithRandom_when_count_is_less_than_one_throws_ArgumentOutOfRangeException()
    {
        using var sut = NewBuilder();

        var plain = Assert.Throws<ArgumentOutOfRangeException>(() => sut.SeedWithRandom<CoverageManufacturer>(0));
        var func = Assert.Throws<ArgumentOutOfRangeException>(() => sut.SeedWithRandom<CoverageManufacturer>(0, m => m));
        var indexed = Assert.Throws<ArgumentOutOfRangeException>(() => sut.SeedWithRandom<CoverageManufacturer>(0, (m, _) => m));

        // The builder's own message, not one from the creator it would otherwise reach.
        Assert.All(new[] { plain, func, indexed }, ex => Assert.StartsWith("Count must be greater than 0", ex.Message, StringComparison.Ordinal));
        // The rejected value is reported, as both random-data providers do (#571).
        Assert.All(new[] { plain, func, indexed }, ex => Assert.Equal(0, ex.ActualValue));
    }



    /// <summary>
    /// Verifies SeedWith with an IEnumerable seeds the rows.
    /// </summary>
    [Fact]
    public async Task SeedWith_enumerable_seeds_the_rows()
    {
        var rows = new[] { new CoverageManufacturer { Id = 1, Name = "a" }, new CoverageManufacturer { Id = 2, Name = "b" } };
        using var sut = new DbContextBuilder<CoverageContext>().UseInMemory();

        await using var context = await sut.SeedWith(rows.AsEnumerable()).BuildAsync();

        Assert.Equal(2, context.Manufacturers.Count());
    }



    /// <summary>
    /// Verifies SeedWith(IEnumerable) rejects a null sequence.
    /// </summary>
    [Fact]
    public void SeedWith_IEnumerable_when_null_throws_ArgumentNullException()
    {
        using var sut = new DbContextBuilder<CoverageContext>();

        Assert.Throws<ArgumentNullException>(() => sut.SeedWith((IEnumerable<CoverageManufacturer>)null!));
    }



    /// <summary>
    /// Verifies SeedWith(IEnumerable) rejects a string element type.
    /// </summary>
    [Fact]
    public void SeedWith_IEnumerable_when_TEntity_is_string_throws_ArgumentException()
    {
        using var sut = new DbContextBuilder<CoverageContext>();

        var ex = Assert.Throws<ArgumentException>(() => sut.SeedWith(new[] { "not an entity" }.AsEnumerable()));

        Assert.StartsWith("The type of TEntity cannot be string", ex.Message, StringComparison.Ordinal);
    }



    /// <summary>
    /// Verifies SeedWith(params) rejects a null array.
    /// </summary>
    [Fact]
    public void SeedWith_params_when_null_throws_ArgumentNullException()
    {
        using var sut = new DbContextBuilder<CoverageContext>();

        Assert.Throws<ArgumentNullException>(() => sut.SeedWith((CoverageManufacturer[])null!));
    }



    /// <summary>
    /// Verifies SeedWith(params) rejects a null item.
    /// </summary>
    [Fact]
    public void SeedWith_params_when_an_item_is_null_throws_ArgumentException()
    {
        using var sut = new DbContextBuilder<CoverageContext>();

        var ex = Assert.Throws<ArgumentException>(() => sut.SeedWith(new CoverageManufacturer { Id = 1 }, null!));

        Assert.StartsWith("One of the entities is null", ex.Message, StringComparison.Ordinal);
    }



    /// <summary>
    /// Verifies the params overload seeds the rows.
    /// </summary>
    [Fact]
    public async Task SeedWith_params_seeds_the_rows()
    {
        using var sut = new DbContextBuilder<CoverageContext>().UseInMemory();

        await using var context = await sut
            .SeedWith(new CoverageManufacturer { Id = 1, Name = "a" }, new CoverageManufacturer { Id = 2, Name = "b" })
            .BuildAsync();

        Assert.Equal(2, context.Manufacturers.Count());
    }



    /// <summary>
    /// Verifies the singleton overload seeds one row.
    /// </summary>
    [Fact]
    public async Task SeedWith_singleton_seeds_one_row()
    {
        using var sut = new DbContextBuilder<CoverageContext>().UseInMemory();

        await using var context = await sut.SeedWith(new CoverageManufacturer { Id = 1, Name = "a" }).BuildAsync();

        Assert.Equal(1, context.Manufacturers.Count());
    }



    /// <summary>
    /// Verifies the singleton overload rejects a null entity.
    /// </summary>
    [Fact]
    public void SeedWith_singleton_when_null_throws_ArgumentNullException()
    {
        using var sut = new DbContextBuilder<CoverageContext>();

        Assert.Throws<ArgumentNullException>(() => sut.SeedWith((CoverageManufacturer)null!));
    }



    /// <summary>
    /// Verifies the singleton overload rejects a string even when TEntity is widened to object.
    /// </summary>
    [Fact]
    public void SeedWith_singleton_when_TEntity_is_widened_to_object_rejects_a_string()
    {
        using var sut = new DbContextBuilder<CoverageContext>();

        var ex = Assert.Throws<ArgumentException>(() => sut.SeedWith<object>("a string"));

        Assert.StartsWith("One of the entities passed in is of type string", ex.Message, StringComparison.Ordinal);
    }



    /// <summary>
    /// Verifies the singleton overload, given an <c>IEnumerable&lt;object&gt;</c> at runtime, seeds every
    /// item. The argument is typed as <see cref="object"/> so overload resolution picks the singleton
    /// overload; a <c>List&lt;object&gt;</c> would bind to the <c>IEnumerable&lt;TEntity&gt;</c> overload instead.
    /// </summary>
    [Fact]
    public async Task SeedWith_singleton_given_a_sequence_seeds_every_item()
    {
        object sequence = new List<object> { new CoverageManufacturer { Id = 1, Name = "a" }, new CoverageManufacturer { Id = 2, Name = "b" } };
        using var sut = new DbContextBuilder<CoverageContext>().UseInMemory();

        await using var context = await sut.SeedWith(sequence).BuildAsync();

        Assert.Equal(2, context.Manufacturers.Count());
    }



    /// <summary>
    /// Verifies the singleton overload rejects a sequence containing a string, and rejects it
    /// atomically: the valid item before it is not seeded either.
    /// </summary>
    [Fact]
    public async Task SeedWith_singleton_given_a_sequence_with_a_string_throws_and_seeds_nothing()
    {
        object sequence = new List<object> { new CoverageManufacturer { Id = 1, Name = "a" }, "not an entity" };
        using var sut = new DbContextBuilder<CoverageContext>().UseInMemory();

        var ex = Assert.Throws<ArgumentException>(() => sut.SeedWith(sequence));
        Assert.Equal("entity", ex.ParamName);
        Assert.StartsWith("One of the entities passed in is of type string", ex.Message, StringComparison.Ordinal);

        await using var context = await sut.BuildAsync();
        Assert.Equal(0, context.Manufacturers.Count());
    }



    /// <summary>
    /// Verifies the singleton overload rejects a sequence containing a null item at seed time,
    /// atomically, instead of storing it to fail later inside EF (#560).
    /// </summary>
    [Fact]
    public async Task SeedWith_singleton_given_a_sequence_with_a_null_item_throws_and_seeds_nothing()
    {
        var sequence = new List<CoverageManufacturer> { new() { Id = 1, Name = "a" }, null! };
        using var sut = new DbContextBuilder<CoverageContext>().UseInMemory();

        var ex = Assert.Throws<ArgumentException>(() => sut.SeedWith(sequence));
        Assert.Equal("entity", ex.ParamName);
        Assert.StartsWith("One of the entities is null", ex.Message, StringComparison.Ordinal);

        await using var context = await sut.BuildAsync();
        Assert.Equal(0, context.Manufacturers.Count());
    }



    /// <summary>
    /// Verifies the params overload flattens an item that is itself an <c>IEnumerable&lt;object&gt;</c>.
    /// </summary>
    [Fact]
    public async Task SeedWith_params_flattens_a_sequence_item()
    {
        using var sut = new DbContextBuilder<CoverageContext>().UseInMemory();

        await using var context = await sut
            .SeedWith<object>
            (
                new CoverageManufacturer { Id = 1, Name = "a" },
                new List<object> { new CoverageManufacturer { Id = 2, Name = "b" }, new CoverageManufacturer { Id = 3, Name = "c" } }
            )
            .BuildAsync();

        Assert.Equal(3, context.Manufacturers.Count());
    }



    /// <summary>
    /// Verifies the params overload rejects a string item.
    /// </summary>
    [Fact]
    public void SeedWith_params_with_a_string_item_throws_ArgumentException()
    {
        using var sut = new DbContextBuilder<CoverageContext>();

        var ex = Assert.Throws<ArgumentException>(() => sut.SeedWith<object>(new CoverageManufacturer { Id = 1 }, "not an entity"));

        Assert.Equal("entities", ex.ParamName);
        Assert.StartsWith("One of the entities passed in is of type string", ex.Message, StringComparison.Ordinal);
    }



    /// <summary>
    /// Verifies a database that cannot be created surfaces as DbContextBuilder's own
    /// <see cref="InvalidOperationException"/> with the EF failure as its inner exception, not
    /// EF's bare error. An entity with no primary key cannot be modelled; EF validates the model
    /// lazily, and the first thing in <c>BuildAsync</c> to touch it is <c>EnsureCreatedAsync</c>.
    /// (Options with no provider do not work here: the builder supplies InMemory.)
    /// </summary>
    [Fact]
    public async Task BuildAsync_when_the_database_cannot_be_created_wraps_the_EF_failure()
    {
        using var sut = new DbContextBuilder<KeylessContext>().UseInMemory();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.BuildAsync());

        Assert.StartsWith("DbContextBuilder failed to create the in-memory database", ex.Message, StringComparison.Ordinal);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }



    /// <summary>
    /// #515: random integer primary keys that collide are made unique, so EF can track every
    /// entity. Before the fix this build threw an identity conflict.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_makes_colliding_primary_keys_unique()
    {
        using var sut = new DbContextBuilder<CoverageContext>()
            .UseInMemory()
            .UseCustomRandomEntityCreator(new CollidingKeyRandomEntityCreator());

        await using var context = await sut.SeedWithRandom<CoverageManufacturer>(5).BuildAsync();

        var ids = context.Manufacturers.Select(m => m.Id).ToList();
        Assert.Equal(5, ids.Distinct().Count());
        Assert.Contains(CollidingKeyRandomEntityCreator.Key, ids);
    }



    /// <summary>
    /// #515: a key given via SeedWith is never changed; random entities that collide with it
    /// are renumbered around it.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_keeps_SeedWith_keys_and_renumbers_colliding_random_ones()
    {
        using var sut = new DbContextBuilder<CoverageContext>()
            .UseInMemory()
            .UseCustomRandomEntityCreator(new CollidingKeyRandomEntityCreator());

        await using var context = await sut
            .SeedWith(new CoverageManufacturer { Id = CollidingKeyRandomEntityCreator.Key, Name = "explicit" })
            .SeedWithRandom<CoverageManufacturer>(3)
            .BuildAsync();

        Assert.Equal(4, context.Manufacturers.Select(m => m.Id).Distinct().Count());
        Assert.Equal("explicit", context.Manufacturers.Single(m => m.Id == CollidingKeyRandomEntityCreator.Key).Name);
    }



    /// <summary>
    /// #515: keys are made unique before foreign keys are reconciled, so dependents point at the
    /// renumbered principals. SQLite enforces the constraint, so a stale FK would fail the build.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_reconciles_foreign_keys_to_renumbered_principals()
    {
        using var sut = new DbContextBuilder<CoverageContext>()
            .UseSqlite()
            .UseCustomRandomEntityCreator(new CollidingKeyRandomEntityCreator());

        await using var context = await sut
            .SeedWithRandom<CoverageManufacturer>(3)
            .SeedWithRandom<CoverageWidget>(4)
            .BuildAsync();

        var manufacturerIds = context.Manufacturers.Select(m => m.Id).ToHashSet();
        Assert.Equal(3, manufacturerIds.Count);
        Assert.Equal(4, context.Widgets.Select(w => w.Id).Distinct().Count());
        Assert.All(context.Widgets.ToList(), widget => Assert.Contains(widget.ManufacturerId, manufacturerIds));
    }



    /// <summary>
    /// #515: a base type and a derived type share the root's primary key, so keys must be unique
    /// across the whole hierarchy, not per CLR type. Grouping by runtime type left a base and a
    /// derived entity both on the colliding key, and EF refused to track the second.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_makes_primary_keys_unique_across_an_inheritance_hierarchy()
    {
        using var sut = new DbContextBuilder<HierarchyContext>()
            .UseInMemory()
            .UseCustomRandomEntityCreator(new CollidingKeyRandomEntityCreator());

        await using var context = await sut
            .SeedWithRandom<HierarchyAnimal>(2)
            .SeedWithRandom<HierarchyDog>(2)
            .BuildAsync();

        var animals = context.Set<HierarchyAnimal>().ToList();
        Assert.Equal(4, animals.Select(animal => animal.Id).Distinct().Count());
        Assert.All(animals, animal => Assert.StartsWith("value-", animal.Name, StringComparison.Ordinal));
        Assert.Equal(2, animals.OfType<HierarchyDog>().Count(dog => dog.Breed.StartsWith("value-", StringComparison.Ordinal)));
    }



    /// <summary>
    /// #599: a REQUIRED foreign key whose principal type was never seeded is left at the value the
    /// creator (here, the transform) produced, as documented; an optional one is cleared.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_when_a_required_principal_is_not_seeded_leaves_the_FK_untouched()
    {
        using var sut = NewBuilder().UseInMemory();

        await using var context = await sut
            .SeedWithRandom<CoverageWidget>(3, widget => { widget.ManufacturerId = 4242; return widget; })
            .BuildAsync();

        var widgets = context.Widgets.ToList();
        Assert.Equal(3, widgets.Count);
        Assert.All(widgets, widget => Assert.Equal(4242, widget.ManufacturerId));
        Assert.All(widgets, widget => Assert.Null(widget.SupplierId));
    }



    /// <summary>
    /// #599: reconciliation skips a SHADOW foreign key (no CLR property to write), as documented,
    /// even when a principal is seeded: the build succeeds and the shadow FK stays unset.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_leaves_a_shadow_foreign_key_untouched()
    {
        using var sut = new DbContextBuilder<ShadowForeignKeyContext>()
            .UseCustomRandomEntityCreator(new DeterministicRandomEntityCreator())
            .UseInMemory();

        await using var context = await sut
            .SeedWith(new ShadowOwner { Id = 1, Name = "owner" })
            .SeedWithRandom<ShadowChild>(2)
            .BuildAsync();

        var children = context.Set<ShadowChild>().ToList();
        Assert.Equal(2, children.Count);
        Assert.All(children, child => Assert.Null(context.Entry(child).Property<int?>("ShadowOwnerId").CurrentValue));
        Assert.All(children, child => Assert.Null(child.Owner));
    }



    /// <summary>
    /// #599: long, short and byte primary keys are made unique like int keys: colliding random
    /// keys are renumbered through Convert.ChangeType into the key's own type.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_makes_long_short_and_byte_primary_keys_unique()
    {
        using var sut = new DbContextBuilder<NarrowKeyContext>()
            .UseCustomRandomEntityCreator(new CollidingTypedKeyRandomEntityCreator())
            .UseInMemory();

        await using var context = await sut
            .SeedWithRandom<LongKeyRow>(3)
            .SeedWithRandom<ShortKeyRow>(3)
            .SeedWithRandom<ByteKeyRow>(3)
            .BuildAsync();

        var longIds = context.Set<LongKeyRow>().Select(row => row.Id).ToList();
        var shortIds = context.Set<ShortKeyRow>().Select(row => row.Id).ToList();
        var byteIds = context.Set<ByteKeyRow>().Select(row => row.Id).ToList();
        Assert.Equal(3, longIds.Distinct().Count());
        Assert.Equal(3, shortIds.Distinct().Count());
        Assert.Equal(3, byteIds.Distinct().Count());
        Assert.Contains(7L, longIds);
        Assert.Contains((short)7, shortIds);
        Assert.Contains((byte)7, byteIds);
    }



    /// <summary>
    /// #569: a required FK whose principal is an inheritance base is wired to a seeded instance
    /// of a derived type. Matching principals by exact runtime type missed it and left the FK
    /// random, which SQLite's foreign-key check then rejected.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_reconciles_a_foreign_key_to_a_seeded_derived_principal()
    {
        using var sut = new DbContextBuilder<HierarchyCollarContext>()
            .UseCustomRandomEntityCreator(new DeterministicRandomEntityCreator())
            .UseSqlite();

        await using var context = await sut
            .SeedWith(new HierarchyDog { Id = 41, Name = "Rex", Breed = "Collie" })
            .SeedWithRandom<HierarchyCollar>(2)
            .BuildAsync();

        var collars = context.Set<HierarchyCollar>().Include(collar => collar.Animal).ToList();
        Assert.Equal(2, collars.Count);
        Assert.All(collars, collar => Assert.Equal(41, collar.AnimalId));
        Assert.All(collars, collar => Assert.IsType<HierarchyDog>(collar.Animal));
    }



    /// <summary>
    /// #530: a creator that leaves a store-generated key at 0 means "let EF generate it". Keeping
    /// 0 for the first entity and numbering the rest from 1 made EF generate 1 for it, colliding
    /// with the assigned 1. Every unset key is now assigned, so no key is left for EF to generate.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_assigns_keys_a_creator_left_at_the_default()
    {
        using var sut = new DbContextBuilder<CoverageContext>()
            .UseInMemory()
            .UseCustomRandomEntityCreator(new CollidingKeyRandomEntityCreator(0));

        await using var context = await sut.SeedWithRandom<CoverageManufacturer>(5).BuildAsync();

        Assert.Equal
        (
            new[] { 1, 2, 3, 4, 5 },
            context.Manufacturers.Select(m => m.Id).OrderBy(id => id).ToArray()
        );
    }



    /// <summary>
    /// #530: a SeedWith entity left at 0 would also get an EF-generated key that can collide with
    /// an assigned random one, so it is assigned the lowest unused value as well. A SeedWith key
    /// that is set is still never changed.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_assigns_an_unset_SeedWith_key_and_keeps_a_set_one()
    {
        using var sut = new DbContextBuilder<CoverageContext>()
            .UseInMemory()
            .UseCustomRandomEntityCreator(new CollidingKeyRandomEntityCreator(0));

        await using var context = await sut
            .SeedWith(new CoverageManufacturer { Name = "unset" }, new CoverageManufacturer { Id = 2, Name = "set" })
            .SeedWithRandom<CoverageManufacturer>(2)
            .BuildAsync();

        var manufacturers = context.Manufacturers.ToList();
        Assert.Equal(4, manufacturers.Select(m => m.Id).Distinct().Count());
        Assert.DoesNotContain(0, manufacturers.Select(m => m.Id));
        Assert.Equal(2, manufacturers.Single(m => string.Equals(m.Name, "set", StringComparison.Ordinal)).Id);
    }



    /// <summary>
    /// #530: on a key EF never generates, 0 is an ordinary value, so one random entity keeps it
    /// and only the colliding ones are renumbered.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_keeps_0_as_a_real_key_when_EF_never_generates_it()
    {
        using var sut = new DbContextBuilder<ManualKeyContext>()
            .UseInMemory()
            .UseCustomRandomEntityCreator(new CollidingKeyRandomEntityCreator(0));

        await using var context = await sut.SeedWithRandom<CoverageManufacturer>(3).BuildAsync();

        Assert.Equal
        (
            new[] { 0, 1, 2 },
            context.Set<CoverageManufacturer>().Select(m => m.Id).OrderBy(id => id).ToArray()
        );
    }



#if EF_CORE_8_OR_GREATER
    /// <summary>
    /// #530 review: with <c>HasSentinel(-1)</c> (EF Core 8+), -1 means "let EF generate it", so
    /// entities left at -1 are assigned keys; 0 is not treated as unset.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_assigns_keys_left_at_a_custom_sentinel()
    {
        using var sut = new DbContextBuilder<SentinelKeyContext>()
            .UseInMemory()
            .UseCustomRandomEntityCreator(new CollidingKeyRandomEntityCreator(-1));

        await using var context = await sut.SeedWithRandom<CoverageManufacturer>(3).BuildAsync();

        Assert.Equal
        (
            new[] { 1, 2, 3 },
            context.Set<CoverageManufacturer>().Select(m => m.Id).OrderBy(id => id).ToArray()
        );
    }



    /// <summary>
    /// #530 review: with a custom sentinel, 0 is an ordinary key, so one random entity keeps it
    /// and only the colliding ones are renumbered.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_keeps_0_as_a_real_key_when_the_sentinel_is_not_0()
    {
        using var sut = new DbContextBuilder<SentinelKeyContext>()
            .UseInMemory()
            .UseCustomRandomEntityCreator(new CollidingKeyRandomEntityCreator(0));

        await using var context = await sut.SeedWithRandom<CoverageManufacturer>(3).BuildAsync();

        Assert.Equal
        (
            new[] { 0, 1, 2 },
            context.Set<CoverageManufacturer>().Select(m => m.Id).OrderBy(id => id).ToArray()
        );
    }



    /// <summary>
    /// #530 review: the sentinel is never assigned. With <c>HasSentinel(1)</c>, handing out 1 would
    /// make EF generate over it, so assignment starts at 2.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_never_assigns_the_sentinel_itself()
    {
        using var sut = new DbContextBuilder<PositiveSentinelKeyContext>()
            .UseInMemory()
            .UseCustomRandomEntityCreator(new CollidingKeyRandomEntityCreator(1));

        await using var context = await sut.SeedWithRandom<CoverageManufacturer>(2).BuildAsync();

        Assert.Equal
        (
            new[] { 2, 3 },
            context.Set<CoverageManufacturer>().Select(m => m.Id).OrderBy(id => id).ToArray()
        );
    }
#endif



    /// <summary>
    /// UseCustomRandomEntityCreator rejects a null creator up front.
    /// </summary>
    [Fact]
    public void UseCustomRandomEntityCreator_when_creator_is_null_throws_ArgumentNullException()
    {
        using var sut = new DbContextBuilder<CoverageContext>();

        var ex = Assert.Throws<ArgumentNullException>(() => sut.UseCustomRandomEntityCreator(null!));
        Assert.Equal("creator", ex.ParamName);
    }



    /// <summary>
    /// Both transform overloads reject a null transform, even at the smallest valid count.
    /// </summary>
    [Fact]
    public void SeedWithRandom_transform_overloads_when_func_is_null_throws_ArgumentNullException()
    {
        using var sut = NewBuilder().UseInMemory();

        Assert.Equal("func", Assert.Throws<ArgumentNullException>(() => sut.SeedWithRandom<CoverageManufacturer>(1, (Func<CoverageManufacturer, CoverageManufacturer>)null!)).ParamName);
        Assert.Equal("func", Assert.Throws<ArgumentNullException>(() => sut.SeedWithRandom<CoverageManufacturer>(1, (Func<CoverageManufacturer, int, CoverageManufacturer>)null!)).ParamName);
    }



    /// <summary>
    /// Both transform overloads accept a count of exactly 1, the smallest valid count.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_transform_overloads_accept_a_count_of_1()
    {
        using var sut = NewBuilder().UseInMemory();

        await using var context = await sut
            .SeedWithRandom<CoverageManufacturer>(1, m => m)
            .SeedWithRandom<CoverageManufacturer>(1, (m, _) => m)
            .BuildAsync();

        Assert.Equal(2, context.Manufacturers.Count());
    }



    /// <summary>
    /// #515: entities from the transform overloads are randomly seeded too, so their colliding keys
    /// are made unique; untracked, both builds would throw the identity conflict.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_transform_overloads_make_colliding_keys_unique()
    {
        using var plain = new DbContextBuilder<CoverageContext>().UseInMemory().UseCustomRandomEntityCreator(new CollidingKeyRandomEntityCreator());
        await using var fromFunc = await plain.SeedWithRandom<CoverageManufacturer>(3, m => m).BuildAsync();
        Assert.Equal(3, fromFunc.Manufacturers.Select(m => m.Id).Distinct().Count());

        using var indexed = new DbContextBuilder<CoverageContext>().UseInMemory().UseCustomRandomEntityCreator(new CollidingKeyRandomEntityCreator());
        await using var fromIndexed = await indexed.SeedWithRandom<CoverageManufacturer>(3, (m, _) => m).BuildAsync();
        Assert.Equal(3, fromIndexed.Manufacturers.Select(m => m.Id).Distinct().Count());
    }



    /// <summary>
    /// Only a single-property integer key is renumbered. A composite key whose first part repeats
    /// but whose parts together are unique is left exactly as the creator made it.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_leaves_a_composite_key_alone()
    {
        using var sut = new DbContextBuilder<CompositeKeyContext>().UseInMemory().UseCustomRandomEntityCreator(new CompositeKeyCreator());

        await using var context = await sut.SeedWithRandom<CompositeKeyRow>(3).BuildAsync();

        var rows = context.Set<CompositeKeyRow>().OrderBy(r => r.B).ToList();
        Assert.All(rows, r => Assert.Equal(CompositeKeyCreator.FirstPart, r.A));
        Assert.Equal(new[] { 1, 2, 3 }, rows.Select(r => r.B).ToArray());
        Assert.All(rows, r => Assert.Equal("row", r.Name));
    }



    /// <summary>
    /// A key that is not an integer (here a Guid) is not renumbered: building must not try to read
    /// it as a number.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_leaves_a_non_integer_key_alone()
    {
        using var sut = new DbContextBuilder<GuidKeyContext>().UseInMemory().UseCustomRandomEntityCreator(new DeterministicRandomEntityCreator());

        await using var context = await sut.SeedWithRandom<GuidKeyRow>(2).BuildAsync();

        var rows = context.Set<GuidKeyRow>().ToList();
        Assert.Equal(2, rows.Select(r => r.Id).Distinct().Count());
        Assert.All(rows, r => Assert.StartsWith("value-", r.Name, StringComparison.Ordinal));
    }



    /// <summary>
    /// Without random entities, keys are left to EF: a <c>SeedWith</c> entity left at the unset
    /// key gets the key the model's value generator produces, not one DbContextBuilder picks.
    /// Assigning one would override EF's own key generation, such as an identity column on a
    /// real database (#531).
    /// </summary>
    [Fact]
    public async Task SeedWith_alone_leaves_an_unset_key_to_the_models_value_generator()
    {
        using var sut = new DbContextBuilder<GeneratedKeyContext>().UseInMemory();

        await using var context = await sut.SeedWith(new GeneratedKeyRow { Name = "seeded" }).BuildAsync();

        var row = Assert.Single(context.Set<GeneratedKeyRow>().ToList());
        Assert.Equal(FixedKeyGenerator.Key, row.Id);
        Assert.Equal("seeded", row.Name);
    }



    /// <summary>
    /// BuildAsync saves only when there is seed data, so a build without any does not run the
    /// context's SaveChanges override (auditing, timestamps) or its interceptors (#531).
    /// </summary>
    [Fact]
    public async Task BuildAsync_without_seed_data_does_not_call_SaveChanges()
    {
        var saves = SaveCountingContext.StartCounting();
        using var sut = new DbContextBuilder<SaveCountingContext>().UseInMemory();

        await using var context = await sut.BuildAsync();

        Assert.Equal(0, saves.Value);
    }



    /// <summary>
    /// The counterpart of <see cref="BuildAsync_without_seed_data_does_not_call_SaveChanges"/>:
    /// with seed data, BuildAsync saves once, through the context's override.
    /// </summary>
    [Fact]
    public async Task BuildAsync_with_seed_data_calls_SaveChanges_once()
    {
        var saves = SaveCountingContext.StartCounting();
        using var sut = new DbContextBuilder<SaveCountingContext>().UseInMemory();

        await using var context = await sut.SeedWith(new SaveCountingRow()).BuildAsync();

        Assert.Equal(1, saves.Value);
        Assert.NotEqual(0, Assert.Single(context.Set<SaveCountingRow>().ToList()).Id);
    }



    /// <summary>
    /// Seeding a type that is not part of the model fails with EF's own error, not a
    /// NullReferenceException from the foreign-key pass skipping its missing entity type.
    /// </summary>
    [Fact]
    public async Task BuildAsync_when_a_random_entity_type_is_not_in_the_model_throws_InvalidOperationException()
    {
        using var sut = NewBuilder().UseInMemory();

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SeedWithRandom<UnmappedRow>(1).BuildAsync());
    }



    /// <summary>
    /// A self-referencing optional foreign key with no other row of its type to point at is cleared,
    /// not wired to the row itself.
    /// </summary>
    [Fact]
    public async Task SeedWithRandom_clears_a_self_reference_with_no_other_principal()
    {
        using var sut = new DbContextBuilder<SelfReferenceContext>().UseInMemory().UseCustomRandomEntityCreator(new DeterministicRandomEntityCreator());

        await using var context = await sut.SeedWithRandom<SelfReferenceRow>(1).BuildAsync();

        var row = Assert.Single(context.Set<SelfReferenceRow>().ToList());
        Assert.NotEqual(0, row.Id);
        Assert.Null(row.ParentId);
        Assert.Null(row.Parent);
    }



    /// <summary>
    /// Disposing the builder disposes the context creator it owns (here SQLite's, which holds an
    /// open connection).
    /// </summary>
    [Fact]
    public void Dispose_disposes_the_context_creator()
    {
        var sut = new DbContextBuilder<CoverageContext>().UseSqlite();
        var creator = Assert.IsType<SqliteDbContextCreator>(sut.CreateDbContext);

        sut.Dispose();

        Assert.True(creator.IsDisposed);
    }



    /// <summary>
    /// UseSqliteForMsSqlServer's model customizer is applied when the context is built: a SQL Server
    /// default ((getdate()), as scaffolded) only works on SQLite once the customizer has rewritten it.
    /// </summary>
    [Fact]
    public async Task UseSqliteForMsSqlServer_applies_its_model_customizer_when_building()
    {
        using var sut = new DbContextBuilder<SqlServerDefaultsContext>().UseSqliteForMsSqlServer();

        await using var context = await sut.SeedWith(new SqlServerDefaultsRow { Id = 1, Name = "a" }).BuildAsync();

        var row = Assert.Single(context.Set<SqlServerDefaultsRow>().ToList());
        Assert.Equal("a", row.Name);
        Assert.NotEqual(default, row.CreatedAt);
    }



    /// <summary>
    /// Verifies UseDbContextOptionsBuilder's builder is the one BuildAsync uses.
    /// </summary>
    [Fact]
    public async Task UseDbContextOptionsBuilder_is_honored()
    {
        var options = new DbContextOptionsBuilder<CoverageContext>()
            .UseInMemoryDatabase("explicit-options")
            .EnableSensitiveDataLogging();
        using var sut = new DbContextBuilder<CoverageContext>().UseDbContextOptionsBuilder(options);

        await using var context = await sut.SeedWith(new CoverageManufacturer { Id = 1 }).BuildAsync();
        Assert.Equal(1, context.Manufacturers.Count());

        // Only the caller's builder turned sensitive-data logging on, so this proves it was the one used.
        Assert.True(context.GetService<IDbContextOptions>().FindExtension<CoreOptionsExtension>()!.IsSensitiveDataLoggingEnabled);
    }



    /// <summary>
    /// Verifies UseDbContextOptionsBuilder rejects null.
    /// </summary>
    [Fact]
    public void UseDbContextOptionsBuilder_when_null_throws_ArgumentNullException()
    {
        using var sut = new DbContextBuilder<CoverageContext>();

        Assert.Throws<ArgumentNullException>(() => sut.UseDbContextOptionsBuilder(null!));
    }



    /// <summary>
    /// Verifies selecting a SQLite provider more than once works (exercises the model-customizer
    /// de-duplication path that only runs when a SQLite extension is applied a second time).
    /// </summary>
    [Fact]
    public async Task Selecting_a_Sqlite_provider_twice_works()
    {
        using var sut = NewBuilder()
            .UseSqlite()
            .UseSqliteForMsSqlServer();

        await using var context = await sut
            .SeedWithRandom<CoverageManufacturer>(2)
            .BuildAsync();

        Assert.Equal(2, context.Manufacturers.Count());
    }



    /// <summary>
    /// Verifies BuildAsync throws after the builder is disposed, and Dispose is idempotent.
    /// </summary>
    [Fact]
    public async Task BuildAsync_after_Dispose_throws_ObjectDisposedException()
    {
        var sut = new DbContextBuilder<CoverageContext>().UseSqlite();
        sut.Dispose();
        sut.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => sut.BuildAsync());
    }



    /// <summary>
    /// Verifies a widget seeded with both of its principals round-trips through SQLite, which
    /// enforces the foreign keys, and that both navigations load back to the seeded principals.
    /// </summary>
    [Fact]
    public async Task SeedWith_a_widget_and_both_principals_round_trips_both_navigations()
    {
        using var sut = new DbContextBuilder<CoverageContext>().UseSqlite();

        await using var context = await sut
            .SeedWith(new CoverageManufacturer { Id = 1, Name = "Acme" })
            .SeedWith(new CoverageSupplier { Id = 2, Name = "Bolts Ltd" })
            .SeedWith(new CoverageWidget { Id = 3, Name = "Cog", ManufacturerId = 1, SupplierId = 2 })
            .BuildAsync();

        var widget = context.Widgets
            .Include(w => w.Manufacturer)
            .Include(w => w.Supplier)
            .Single();
        var supplier = context.Suppliers.Single();

        Assert.Equal("Acme", widget.Manufacturer?.Name);
        Assert.Equal("Bolts Ltd", widget.Supplier?.Name);
        Assert.Equal(2, supplier.Id);
    }



    /// <summary>
    /// Verifies the deterministic creator honours the <see cref="ICreateRandomEntities"/> contract
    /// by rejecting a count below one, as the real providers do.
    /// </summary>
    [Fact]
    public void DeterministicRandomEntityCreator_when_count_is_less_than_one_throws_ArgumentOutOfRangeException()
    {
        var creator = new DeterministicRandomEntityCreator();

        Assert.Throws<ArgumentOutOfRangeException>(() => creator.CreateRandomEntities<CoverageManufacturer>(0));
    }



    /// <summary>
    /// Verifies the deterministic creator fills every scalar type it supports with a non-default
    /// value and leaves a non-string reference-type property unset.
    /// </summary>
    [Fact]
    public void DeterministicRandomEntityCreator_fills_every_scalar_type_and_leaves_references_unset()
    {
        var creator = new DeterministicRandomEntityCreator();

        var entity = creator.CreateRandomEntities<CoverageAllScalars>(1).Single();

        Assert.NotEqual(0L, entity.Long);
        Assert.NotEqual((short)0, entity.Short);
        Assert.NotEqual((byte)0, entity.Byte);
        Assert.NotEqual(0m, entity.Decimal);
        Assert.NotEqual(0d, entity.Double);
        Assert.NotEqual(0f, entity.Float);
        Assert.True(entity.Bool);
        Assert.NotEqual(Guid.Empty, entity.Guid);
        Assert.True(entity.DateTime > new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        Assert.Equal(CoverageColor.Green, entity.Color);
        Assert.NotNull(entity.NullableInt);
        Assert.Null(entity.Link);
    }
}



/// <summary>
/// A deterministic <see cref="ICreateRandomEntities"/> for coverage tests: fills public,
/// settable, non-virtual scalar properties with predictable values and leaves virtual
/// (navigation) and other reference-type members unset — mirroring what a real provider produces,
/// without depending on AutoFixture.
/// </summary>
internal sealed class DeterministicRandomEntityCreator : ICreateRandomEntities
{
    private int _seq;

    public IEnumerable<TEntity> CreateRandomEntities<TEntity>(int count)
        where TEntity : class
    {
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Value cannot be less than 1");
        }

        var list = new List<TEntity>(count);
        for (var i = 0; i < count; i++)
        {
            var entity = (TEntity)Activator.CreateInstance(typeof(TEntity))!;
            Fill(entity);
            list.Add(entity);
        }

        return list;
    }



    private void Fill(object entity)
    {
        foreach (var prop in entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var setter = prop.GetSetMethod();
            if (setter is null || setter.IsVirtual)
            {
                // Skip read-only and virtual (navigation) members.
                continue;
            }

            var value = ScalarValue(prop.PropertyType);
            if (value is not null)
            {
                prop.SetValue(entity, value);
            }
        }
    }



    private object? ScalarValue(Type type)
    {
        var t = Nullable.GetUnderlyingType(type) ?? type;
        var n = ++_seq;

        if (t == typeof(string))
        {
            return $"value-{n}";
        }

        if (t == typeof(int))
        {
            return n;
        }

        if (t == typeof(long))
        {
            return (long)n;
        }

        if (t == typeof(short))
        {
            return (short)n;
        }

        if (t == typeof(byte))
        {
            return (byte)(n % 256);
        }

        if (t == typeof(decimal))
        {
            return (decimal)n;
        }

        if (t == typeof(double))
        {
            return (double)n;
        }

        if (t == typeof(float))
        {
            return (float)n;
        }

        if (t == typeof(bool))
        {
            return true;
        }

        if (t == typeof(Guid))
        {
            return new Guid(n, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        if (t == typeof(DateTime))
        {
            return new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(n);
        }

        if (t.IsEnum)
        {
            return Enum.GetValues(t).GetValue(0);
        }

        // Reference/complex types (navigation properties) are left unset.
        return null;
    }
}



// Not 0-based, so default(CoverageColor) is not a named value and an unset property is visible.
internal enum CoverageColor
{
    Green = 1,
    Red = 2,
}



// One property of every scalar type DeterministicRandomEntityCreator handles, plus a non-string
// reference type it must leave unset. Used directly, not through EF.
internal sealed class CoverageAllScalars
{
    public long Long { get; set; }

    public short Short { get; set; }

    public byte Byte { get; set; }

    public decimal Decimal { get; set; }

    public double Double { get; set; }

    public float Float { get; set; }

    public bool Bool { get; set; }

    public Guid Guid { get; set; }

    public DateTime DateTime { get; set; }

    public CoverageColor Color { get; set; }

    public int? NullableInt { get; set; }

    public Uri? Link { get; set; }
}



// No members, so no executable lines to leave uncovered (#450 allows no test-model exclusions).
// It has no key either, which is the point: EF cannot model it.
internal sealed class KeylessEntity;



internal sealed class KeylessContext(DbContextOptions<KeylessContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<KeylessEntity>();
}



#if EF_CORE_8_OR_GREATER
/// <summary>Maps <see cref="CoverageManufacturer"/> with a generated key whose sentinel is -1 (#530).</summary>
internal sealed class SentinelKeyContext(DbContextOptions<SentinelKeyContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<CoverageManufacturer>().Property(m => m.Id).HasSentinel(-1);
}



/// <summary>Maps <see cref="CoverageManufacturer"/> with a generated key whose sentinel is 1 (#530).</summary>
internal sealed class PositiveSentinelKeyContext(DbContextOptions<PositiveSentinelKeyContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<CoverageManufacturer>().Property(m => m.Id).HasSentinel(1);
}
#endif



/// <summary>A row keyed by two integers (#531).</summary>
internal sealed class CompositeKeyRow
{
    public int A { get; set; }

    public int B { get; set; }

    public string Name { get; set; } = string.Empty;
}



/// <summary>Maps <see cref="CompositeKeyRow"/> with the composite key (A, B).</summary>
internal sealed class CompositeKeyContext(DbContextOptions<CompositeKeyContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<CompositeKeyRow>().HasKey(r => new { r.A, r.B });
}



/// <summary>Gives every row the same first key part and a distinct second one.</summary>
internal sealed class CompositeKeyCreator : ICreateRandomEntities
{
    internal const int FirstPart = 7;



    public IEnumerable<TEntity> CreateRandomEntities<TEntity>(int count)
        where TEntity : class =>
        Enumerable.Range(1, count)
            .Select(i => (TEntity)(object)new CompositeKeyRow { A = FirstPart, B = i, Name = "row" })
            .ToList();
}



/// <summary>A row keyed by a <see cref="Guid"/> (#531).</summary>
internal sealed class GuidKeyRow
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}



/// <summary>Maps <see cref="GuidKeyRow"/>.</summary>
internal sealed class GuidKeyContext(DbContextOptions<GuidKeyContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<GuidKeyRow>();
}



/// <summary>A type no context maps (#531).</summary>
internal sealed class UnmappedRow
{
    public int Id { get; set; }
}



/// <summary>A row with an optional foreign key to its own type (#531).</summary>
internal class SelfReferenceRow
{
    public int Id { get; set; }

    public int? ParentId { get; set; }

    public virtual SelfReferenceRow? Parent { get; set; }
}



/// <summary>Maps <see cref="SelfReferenceRow"/> with ParentId as an optional self-reference.</summary>
internal sealed class SelfReferenceContext(DbContextOptions<SelfReferenceContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<SelfReferenceRow>().HasOne(r => r.Parent).WithMany().HasForeignKey(r => r.ParentId);
}



/// <summary>A row whose CreatedAt has a SQL Server default (#531).</summary>
internal sealed class SqlServerDefaultsRow
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}



/// <summary>Maps <see cref="SqlServerDefaultsRow"/> with a (getdate()) default, as a scaffolded SQL Server model has.</summary>
internal sealed class SqlServerDefaultsContext(DbContextOptions<SqlServerDefaultsContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<SqlServerDefaultsRow>().Property(r => r.CreatedAt).HasDefaultValueSql("(getdate())");
}



/// <summary>Maps <see cref="CoverageManufacturer"/> with a key EF never generates (#530).</summary>
internal sealed class ManualKeyContext(DbContextOptions<ManualKeyContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<CoverageManufacturer>().Property(m => m.Id).ValueGeneratedNever();
}



/// <summary>Base type of a TPH hierarchy whose types share one primary key (#515).</summary>
internal class HierarchyAnimal
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}



/// <summary>Derived type in <see cref="HierarchyContext"/>; keyed by <see cref="HierarchyAnimal.Id"/>.</summary>
internal sealed class HierarchyDog : HierarchyAnimal
{
    public string Breed { get; set; } = string.Empty;
}



/// <summary>Context mapping <see cref="HierarchyAnimal"/> and <see cref="HierarchyDog"/> as one hierarchy.</summary>
internal sealed class HierarchyContext(DbContextOptions<HierarchyContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HierarchyAnimal>();
        modelBuilder.Entity<HierarchyDog>();
    }
}



/// <summary>Principal of <see cref="ShadowChild"/>'s shadow foreign key (#599).</summary>
internal sealed class ShadowOwner
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}



/// <summary>Has a navigation to <see cref="ShadowOwner"/> but no CLR foreign-key property (#599).</summary>
internal class ShadowChild
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    // Virtual so the random-entity double leaves it unset.
    public virtual ShadowOwner? Owner { get; set; }
}



/// <summary>Maps <see cref="ShadowChild"/>'s optional relationship through a shadow FK (#599).</summary>
internal sealed class ShadowForeignKeyContext(DbContextOptions<ShadowForeignKeyContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShadowOwner>();
        modelBuilder.Entity<ShadowChild>()
            .HasOne(child => child.Owner)
            .WithMany()
            .HasForeignKey("ShadowOwnerId")
            .IsRequired(false);
    }
}



/// <summary>Keyed by <see cref="long"/> (#599).</summary>
internal sealed class LongKeyRow
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;
}



/// <summary>Keyed by <see cref="short"/> (#599).</summary>
internal sealed class ShortKeyRow
{
    public short Id { get; set; }

    public string Name { get; set; } = string.Empty;
}



/// <summary>Keyed by <see cref="byte"/> (#599).</summary>
internal sealed class ByteKeyRow
{
    public byte Id { get; set; }

    public string Name { get; set; } = string.Empty;
}



/// <summary>Maps the three narrow-key rows (#599).</summary>
internal sealed class NarrowKeyContext(DbContextOptions<NarrowKeyContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LongKeyRow>();
        modelBuilder.Entity<ShortKeyRow>();
        modelBuilder.Entity<ByteKeyRow>();
    }
}



/// <summary>
/// Wraps <see cref="DeterministicRandomEntityCreator"/> and sets every generated entity's <c>Id</c>
/// to 7 converted to the key's own type, so long/short/byte keys collide like the int ones (#599).
/// </summary>
internal sealed class CollidingTypedKeyRandomEntityCreator : ICreateRandomEntities
{
    private readonly DeterministicRandomEntityCreator _inner = new();



    public IEnumerable<TEntity> CreateRandomEntities<TEntity>(int count)
        where TEntity : class
    {
        var entities = _inner.CreateRandomEntities<TEntity>(count).ToList();
        var id = typeof(TEntity).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)!;
        var key = Convert.ChangeType(CollidingKeyRandomEntityCreator.Key, id.PropertyType, System.Globalization.CultureInfo.InvariantCulture);
        entities.ForEach(entity => id.SetValue(entity, key));
        return entities;
    }
}



/// <summary>
/// Dependent of the <see cref="HierarchyAnimal"/> base type, with a required FK (#569).
/// </summary>
internal class HierarchyCollar
{
    public int Id { get; set; }

    public int AnimalId { get; set; }

    // Virtual so the random-entity double leaves it unset (only the scalar FK is populated).
    public virtual HierarchyAnimal? Animal { get; set; }
}



/// <summary>
/// Maps <see cref="HierarchyCollar"/> against the <see cref="HierarchyAnimal"/> hierarchy, so the
/// FK's principal type is the base while the seeded principal is a <see cref="HierarchyDog"/>.
/// </summary>
internal sealed class HierarchyCollarContext(DbContextOptions<HierarchyCollarContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HierarchyAnimal>();
        modelBuilder.Entity<HierarchyDog>();
        modelBuilder.Entity<HierarchyCollar>();
    }
}



/// <summary>
/// Wraps <see cref="DeterministicRandomEntityCreator"/> and gives every generated entity the same
/// <c>Id</c>, reproducing #515's primary-key collision deterministically (Bogus only collides by chance).
/// </summary>
internal sealed class CollidingKeyRandomEntityCreator(int key = CollidingKeyRandomEntityCreator.Key) : ICreateRandomEntities
{
    internal const int Key = 7;

    private readonly DeterministicRandomEntityCreator _inner = new();



    public IEnumerable<TEntity> CreateRandomEntities<TEntity>(int count)
        where TEntity : class
    {
        var entities = _inner.CreateRandomEntities<TEntity>(count).ToList();
        var id = typeof(TEntity).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)!;
        entities.ForEach(entity => id.SetValue(entity, key));
        return entities;
    }
}


/// <summary>A row whose key comes from <see cref="FixedKeyGenerator"/> (#531).</summary>
internal sealed class GeneratedKeyRow
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}



/// <summary>A value generator that always produces <see cref="Key"/>, unlike any key DbContextBuilder assigns.</summary>
internal sealed class FixedKeyGenerator : ValueGenerator<int>
{
    public const int Key = 1000;



    public override bool GeneratesTemporaryValues => false;



    public override int Next(EntityEntry entry) => Key;
}



/// <summary>Maps <see cref="GeneratedKeyRow"/> with its key generated by <see cref="FixedKeyGenerator"/>.</summary>
internal sealed class GeneratedKeyContext(DbContextOptions<GeneratedKeyContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<GeneratedKeyRow>().Property(r => r.Id).HasValueGenerator<FixedKeyGenerator>();
}



/// <summary>A row for <see cref="SaveCountingContext"/> (#531).</summary>
internal sealed class SaveCountingRow
{
    public int Id { get; set; }
}



/// <summary>Counts the calls to its SaveChangesAsync override, across instances.</summary>
internal sealed class SaveCountingContext(DbContextOptions<SaveCountingContext> options) : DbContext(options)
{
    // Per test, not process-wide (#597): each test installs its own counter, and AsyncLocal flows
    // it only into the calls made on that test's execution context, so tests running in parallel
    // cannot reset or bump each other's count. The box is a reference, so increments made inside
    // the builder's async calls are visible to the test.
    private static readonly AsyncLocal<StrongBox<int>?> _saves = new();



    internal static StrongBox<int> StartCounting()
    {
        var saves = new StrongBox<int>();
        _saves.Value = saves;
        return saves;
    }



    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<SaveCountingRow>();



    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        _saves.Value!.Value++;
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
