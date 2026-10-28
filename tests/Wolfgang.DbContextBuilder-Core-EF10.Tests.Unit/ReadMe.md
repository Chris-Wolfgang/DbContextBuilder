This project contains no source of its own. It links the test files of the
Wolfgang.DbContextBuilder-Core.Tests.Unit project (`<Compile Include=... Link=...>` items in its csproj)
and runs them against EF Core 10.
This project is contained in a separate folder so that when it is built it doesn't cause conflicting package restores.
