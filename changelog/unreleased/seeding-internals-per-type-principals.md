type: internal

Seeding internals do less repeated work: foreign-key reconciliation resolves the candidate principals once per principal type instead of per foreign key per row, and the EF6 builder shares one validation loop between its `SeedWith` overloads and one count guard between its `SeedWithRandom` overloads.
