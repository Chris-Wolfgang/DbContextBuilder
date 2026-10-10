type: fix

The exception messages for a database that cannot be created now give current advice: the EF Core builder points at `UseDiagnosticOutput(...)` for capturing EF Core's log, and the EF6 builder no longer suggests a missing `(DbConnection, bool)` constructor, a case that throws `MissingMethodException` before that message can occur.
