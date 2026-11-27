type: internal

`BogusRandomEntityCreator` builds its `Faker<TEntity>` once per entity type and reuses it (a seeded creator builds a fresh one per call, so the seed never reaches the shared instance), instead of registering its type and enum rules on every `CreateRandomEntities` call.
