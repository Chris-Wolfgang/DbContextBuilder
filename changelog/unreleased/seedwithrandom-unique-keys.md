type: fix

`SeedWithRandom` no longer fails intermittently with "another instance with the same key value is already being tracked". A random entity creator fills integer primary keys like any other integer, so two entities could share a key; the chance grew with the count, to about 5% at 100 entities. Randomly seeded entities now keep their random key only while it is unique; a colliding one gets the lowest unused value. Keys given via `SeedWith` are never changed, and foreign keys follow the final keys.
