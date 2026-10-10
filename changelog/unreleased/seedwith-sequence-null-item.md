type: fix

`SeedWith(entity)` given a sequence that contains a null item (for example `SeedWith(new List<Product> { p, null })`) now throws `ArgumentException` at seed time, as the `params` overload does, instead of failing later inside EF Core with an unhelpful `ArgumentNullException`. Applies to the EF Core and EF6 builders.
