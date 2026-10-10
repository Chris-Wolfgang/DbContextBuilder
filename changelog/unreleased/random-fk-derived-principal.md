type: fix

`SeedWithRandom` now wires a required foreign key whose principal is an inheritance base type (TPH/TPT) to a seeded instance of a derived type; it used to match principals by exact runtime type only and leave the key random.
