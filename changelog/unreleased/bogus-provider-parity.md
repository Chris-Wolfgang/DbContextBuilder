type: feature

`BogusRandomEntityCreator` gains a `BogusRandomEntityCreator(int seed)` constructor for reproducible data, and now populates `DateOnly`, `TimeOnly`, the nullable forms of every supported value type, and settable enum properties; its count guard (and the builder's `SeedWithRandom` guard) now report the rejected value, and Bogus uses the same message as the AutoFixture provider.
