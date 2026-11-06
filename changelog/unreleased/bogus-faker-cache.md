type: internal

`BogusRandomEntityCreator` builds its `Faker<TEntity>` once per entity type and reuses it, instead of registering the twelve type rules on every `CreateRandomEntities` call.
