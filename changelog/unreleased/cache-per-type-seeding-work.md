type: internal

Random seeding resolves EF model metadata once per entity type instead of once per seeded row, and `AutoFixtureRandomEntityCreator(Fixture)` no longer allocates a `Fixture` it discards. A benchmark covers seeding random rows with foreign keys.
