type: feature

The classic EF6 `DbContextBuilder<T>` is now `IDisposable`: disposing it disposes its context creator (for Effort, the in-memory connection), re-selecting a provider with `UseEffort()` disposes the creator it replaces, and a disposed builder throws `ObjectDisposedException`.
