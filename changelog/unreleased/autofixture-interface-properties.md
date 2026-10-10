type: fix

The AutoFixture random-entity creators (`Wolfgang.DbContextBuilder.AutoFixture` and `-EF6`) now populate a non-virtual property that implements an interface member (for example `IAuditable.CreatedAt`); the compiler emits those as virtual final, and the creators skipped them as navigations.
