# DbContextBuilder Copilot Coding Agent Instructions

Always reference these instructions first and only fallback to search or additional context gathering if the information provided here is incomplete or found to be incorrect.

## Repository Summary

**DbContextBuilder** is a .NET library family that uses the Builder pattern to create Entity Framework Core (and classic EF 6) DbContext instances with an in-memory database for testing purposes. The library allows developers to easily set up DbContext instances with predefined and random data, making it ideal for unit tests and integration tests without relying on actual databases.

**Repository Type**: .NET Library (multi-package family)  
**Target Platform**: net10.0 for the canonical Core package; per-EF-version wrappers target net6.0–net10.0; classic EF 6 targets net462–net481  
**Primary Language**: C#  
**NuGet Packages**: a family of 10 — `Wolfgang.DbContextBuilder-Core`, the per-EF-version wrappers `-Core-EF6/7/8/9/10`, classic `-EF6`, the shared `.Abstractions`, and the random-data providers `.AutoFixture` and `.Bogus` (each adds a `UseAutoFixture()`/`UseBogus()` extension; the EF Core packages no longer bundle a random-data provider).  
**Main Features**: Builder pattern, in-memory/SQLite database seeding, pluggable random data generation, seed profiles, diagnostic output

## Build and Validation Instructions

### Prerequisites
- .NET 10.0.x SDK (required for the net10.0 targets; always verify: `dotnet --version`)
- ReportGenerator tool: `dotnet tool install -g dotnet-reportgenerator-globaltool`
- DevSkim CLI: `dotnet tool install --global Microsoft.CST.DevSkim.CLI`

### Complete Build and Test Workflow
Run these commands in the repository root in this exact order:

1. **Restore Dependencies** (2 seconds):
   ```bash
   dotnet restore
   ```

2. **Build Solution** (4-5 seconds):
   ```bash
   dotnet build --no-restore --configuration Release
   ```

3. **Run Tests with Coverage** (3-4 seconds):
   ```bash
   find ./tests -type f -name '*Test*.csproj' | while read proj; do
     echo "Testing $proj"
     dotnet test "$proj" --no-build --configuration Release --collect:"XPlat Code Coverage" --results-directory "./TestResults"
   done
   ```

4. **Generate Coverage Reports** (< 1 second):
   ```bash
   reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:"Html;TextSummary;MarkdownSummaryGithub;CsvSummary"
   ```

5. **Check Coverage Threshold** (manual verification):
   ```bash
   cat CoverageReport/Summary.txt
   ```
   - **REQUIREMENT**: the coverage gate is **per-module (assembly), ≥ 90%** — every shippable
     package's coverage is checked individually, aggregated across all test projects

6. **Security Scanning** (< 1 second):
   ```bash
   # Clean artifacts first to avoid false positives
   rm -rf CoverageReport TestResults
   devskim analyze --source-code . -f text --output-file devskim-results.txt -E
   ```

### Critical Build Requirements
- **Code Coverage**: Minimum 80% line coverage required for CI pipeline
- **Security Scanning**: DevSkim must pass with exit code 0 (no errors)
- **Test Pattern**: Test projects must match `*Test*.csproj` in `/tests` folder
- **Analyzer Rules**: CA1707 (underscores in names) is suppressed for test methods

### Build Performance Expectations
- **Total build time**: ~10-12 seconds for complete workflow
- **Test execution**: ~3-4 seconds (5 tests)
- **Coverage generation**: < 1 second
- **Security scan**: < 1 second (on clean source)
- **NEVER CANCEL**: Even though builds are fast, always allow 30+ seconds timeout

## Project Structure and Navigation

### Directory Layout
```
DbContextBuilder/
├── DbContextBuilder.sln              # Main solution file
├── src/                              # Library source code
│   └── Wolfgang.DbContextBuilder/    # Main library project
│       ├── Wolfgang.DbContextBuilder.csproj
│       └── Class1.cs                 # DbContextBuilder<T> implementation
├── tests/                            # Test projects
│   └── Wolfgang.DbContextBuilder.Tests/
│       ├── Wolfgang.DbContextBuilder.Tests.csproj
│       └── UnitTest1.cs              # DbContextBuilder tests
├── .editorconfig                     # Code style rules (CA1707 suppressed)
├── .github/workflows/pr.yaml         # CI/CD pipeline
└── README.md                         # Library documentation
```

### Key Files and Their Purpose
- **`src/Wolfgang.DbContextBuilder/Class1.cs`**: Main `DbContextBuilder<T>` class implementation
- **`tests/Wolfgang.DbContextBuilder.Tests/UnitTest1.cs`**: Comprehensive test suite (5 tests)
- **`.editorconfig`**: Code style configuration with CA1707 suppressed for test methods
- **`DbContextBuilder.sln`**: Solution file containing both projects

### Dependencies
**Main Library** (`src/Wolfgang.DbContextBuilder/`):
- `Microsoft.EntityFrameworkCore` (9.0.8)
- `Microsoft.EntityFrameworkCore.Sqlite` (9.0.8) 
- `Microsoft.EntityFrameworkCore.InMemory` (9.0.8)

**Test Project** (`tests/Wolfgang.DbContextBuilder.Tests/`):
- `Microsoft.EntityFrameworkCore.InMemory` (9.0.8)
- `xunit` (test framework)
- Project reference to main library

## Development Workflow

### Adding New Features
1. **Always run full build first** to ensure clean baseline
2. **Implement in** `src/Wolfgang.DbContextBuilder/Class1.cs`
3. **Add tests in** `tests/Wolfgang.DbContextBuilder.Tests/UnitTest1.cs`
4. **Run build and test cycle** after each change
5. **Verify coverage stays ≥ 80%** before committing

### Testing DbContextBuilder Functionality
The library provides these main methods:
- `SeedWith<T>(entity)`: Add specific test data
- `SeedWithRandom<T>(count)`: Generate random entities with unique IDs
- `UseConnection(connection)`: Use custom database connection
- `UseDatabaseName(name)`: Custom database name for InMemory provider
- `Build()`: Create configured DbContext instance

### Test Scenarios to Validate
Always test these user scenarios after making changes:
1. **Basic seeding**: Create DbContext with specific entity data
2. **Random data**: Generate entities with unique random IDs
3. **Combined seeding**: Mix specific and random data
4. **Error handling**: Invalid inputs (null entities, zero/negative counts)
5. **Custom connections**: Use different database providers

### Example Usage Validation
Test this code pattern works correctly:
```csharp
var context = new DbContextBuilder<YourDbContext>()
    .SeedWithRandom<YourEntity>(10)
    .SeedWith(new YourEntity { Id = 1, Name = "Test Entity" })
    .SeedWithRandom<YourEntity>(5)
    .Build();
```

## CI/CD Pipeline (`.github/workflows/pr.yaml`)

### Pipeline Steps
1. **Setup .NET 8.0.x** on Ubuntu Latest
2. **Restore dependencies** (`dotnet restore`)
3. **Build Release configuration** (`dotnet build --no-restore --configuration Release`)
4. **Run tests with coverage** (find all `*Test*.csproj` files)
5. **Generate coverage reports** (ReportGenerator)
6. **Check 80% coverage threshold** (fails if below)
7. **Security scan** (DevSkim)
8. **Upload artifacts** (coverage reports, security results)

### Known CI Issues
- **Coverage below 80%**: Currently at ~76%, will fail CI until improved
- **False positives**: DevSkim may flag generated coverage report files
- **Build isolation**: Workflow only runs if `github.repository != 'Chris-Wolfgang/repo-template'`

## Common Tasks and Troubleshooting

### Increasing Code Coverage
Current coverage is 76.4%. To reach 80%:
1. Add tests for uncovered branches in `DbContextBuilder<T>`
2. Test the `UseConnection()` and `UseDatabaseName()` methods
3. Add edge case tests for reflection-based ID setting
4. Verify full method coverage for all public methods

### Security Scan Issues
If DevSkim fails:
1. **Clean artifacts first**: `rm -rf CoverageReport TestResults`
2. **Check DevSkim output**: `cat devskim-results.txt`
3. **Common issues**: Generated files, hardcoded URLs, setTimeout calls
4. **Resolution**: Move problematic files to `.gitignore` or address findings

### Test Failures
Common test issues:
1. **Duplicate entity IDs**: Fixed by random ID generation in `SeedWithRandom`
2. **Database connection**: Uses InMemory provider, not SQLite
3. **Entity tracking**: Each test uses unique database name

### Dependencies and Packages
To add new Entity Framework providers:
```bash
cd src/Wolfgang.DbContextBuilder
dotnet add package Microsoft.EntityFrameworkCore.SqlServer  # Example
```

## Validation Checklist
Before committing changes, always verify:
- [ ] `dotnet restore` succeeds (< 5 seconds)
- [ ] `dotnet build --configuration Release` succeeds (< 10 seconds) 
- [ ] All tests pass (`dotnet test --configuration Release`)
- [ ] Coverage ≥ 80% (`cat CoverageReport/Summary.txt`)
- [ ] Security scan passes (`devskim analyze` exit code 0)
- [ ] Manual functionality test with sample DbContext

This library enables rapid Entity Framework testing with minimal setup. Focus on maintaining the builder pattern simplicity while ensuring robust in-memory database functionality.