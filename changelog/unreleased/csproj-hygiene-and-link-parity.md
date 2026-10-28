type: internal

The `-Core-EF6` to `-Core-EF10` packages now carry a description that names their EF Core major version and providers, the redundant per-project `CheckEolTargetFramework` overrides are gone (so the .NET end-of-life notice reaches the net8.0+ builds again), and a new `scripts/compile-link-parity.ps1` checks that every EF Core wrapper links all of the shared source files.
