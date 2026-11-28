# AdventureWorks for EF Core 11

This project holds the same `AdventureWorksDbContext` and 90 entity classes
as `../AdventureWorks-EF10/`, built against **EF Core 11** (`net11.0`,
EF Core packages pinned to `[11.0.0,12.0.0)`).

The `Models/Generated/` files are a copy of the EF Core 10 scaffold, not a
fresh `dotnet ef dbcontext scaffold` run: the scaffold needs the
AdventureWorks2022 SQL Server database, which the machine that added this
project (#820) did not have. The EF Core 10 output compiles unchanged on
EF Core 11. To regenerate it with EF Core 11 tooling, run the command in
`../AdventureWorks-EF10/SCAFFOLDING.md` with `dotnet-ef 11.x` and
`--project extra-projects/AdventureWorks-EF11/AdventureWorks-EF11.csproj`.

See `../AdventureWorks-EF8/SCAFFOLDING.md` for the flag choices, the
skipped tables and the architectural notes; they apply here unchanged.

The EF Core 10 project pins `System.Security.Cryptography.Pkcs` for a NuGet
Audit advisory. This one does not: EF Core SqlServer 11 restores Pkcs
10.0.13, which already has the fix.
