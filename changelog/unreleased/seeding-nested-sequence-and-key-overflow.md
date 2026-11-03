type: fix

`SeedWith(params)` and `SeedWith(IEnumerable<T>)` now reject a nested sequence that contains a null or a string with `ArgumentException`, like the single-entity overload already did, on both the EF Core and EF6 builders; and when a randomly-seeded `byte` or `short` primary key runs out of unused values, `BuildAsync` throws an `InvalidOperationException` that names the entity and key instead of a bare `OverflowException`.
