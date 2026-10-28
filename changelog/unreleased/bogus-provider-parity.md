type: feature

`BogusRandomEntityCreator` gains a `BogusRandomEntityCreator(int seed)` constructor for reproducible data (dates included: a seeded creator measures them from a fixed reference date, not the current time), and now populates `DateOnly`, `TimeOnly`, the nullable forms of every supported value type, and settable enum properties (an enum type with no defined values is left at its default); its count guard (and the builder's `SeedWithRandom` guard) now report the rejected value, and Bogus uses the same message as the AutoFixture provider.
