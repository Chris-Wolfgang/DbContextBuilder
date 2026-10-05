type: internal

The classic EF6 package's AutoFixture random-entity creator now sets up recursion handling once, through its `NoCircularReferencesCustomization`, instead of also repeating the same two behavior changes by hand. Behavior is unchanged.
