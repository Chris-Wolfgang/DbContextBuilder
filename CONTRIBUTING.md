# Contributing to Wolfgang.DbContextBuilder

Thank you for your interest in contributing to **Wolfgang.DbContextBuilder**! We welcome contributions to help improve this project.

## How Can You Contribute?

You can contribute in several ways:
- Reporting bugs
- Suggesting enhancements
- Submitting pull requests for new features or bug fixes
- Improving documentation
- Writing or improving tests

**Please note:** Before coding anything please check with me first by entering an issue and getting approval for it. PRs are more likely to get merged if I have agreed to the changes.

---

## Getting Started

1. **Fork the repository** and clone it locally.
2. **Enable the pre-commit secret scan** (once per clone). The repository ships a
   [gitleaks](https://github.com/gitleaks/gitleaks) hook in `.githooks/pre-commit` that blocks
   commits containing secrets (credentials, tokens, private keys); CI runs the same scan, so
   enabling it locally only saves you a failed PR check:
   ```sh
   git config core.hooksPath .githooks
   ```
   Install the `gitleaks` CLI: `winget install gitleaks` (Windows), `brew install gitleaks` (macOS), or a
   binary from the [releases page](https://github.com/gitleaks/gitleaks/releases) (Linux). Without it, the
   hook prints a warning and lets the commit through. `git commit --no-verify` skips it for one commit.
3. **Create a new branch** for your feature or bug fix:
   ```sh
   git checkout -b your-feature-name
   ```
4. **Make your changes** and commit them with clear messages:
   ```sh
   git commit -m "Describe your changes"
   ```
5. **Push your branch** to your fork:
   ```sh
   git push origin your-feature-name
   ```
6. **Open a pull request** describing your changes.

7. **PR Checks:**  
   Once you create a pull request (PR), several Continuous Integration (CI) steps will run automatically. These may include:
   - Building the project
   - Running automated tests, with per-assembly coverage gates (95% for src, 100% for test assemblies)
   - Running static analysis with multiple static analyzers (see list below), plus security scans
   - Checking for a changelog fragment and for a protected-file mix (see *Pull Requests*)

   CI does **not** run a formatting check: run `pwsh ./scripts/format.ps1` yourself before pushing.

   **It is important to make sure that all CI steps pass before your PR can be merged.**
   - If any CI step fails, please review the error messages and update your PR as needed.
   - Maintainers will review your PR once all checks have passed.

---

## Code Quality Standards

This project maintains **extremely high code quality standards** through multiple layers of static analysis and automated enforcement.

### The Analyzers

All code is analyzed by these seven tools during build, and `src/` projects that carry
`PublicAPI.*.txt` files get an eighth (listed last):

1. **Microsoft.CodeAnalysis.NetAnalyzers** (Built-in .NET SDK)
   - Correctness, performance, and security rules
   - Latest analysis level enabled

2. **Roslynator.Analyzers**
   - 500+ refactoring and code quality rules
   - Advanced C# pattern detection

3. **AsyncFixer**
   - Detects common async/await anti-patterns (AsyncFixer01–05)
   - Flags missing or incorrect cancellation-token propagation
   - Prevents fire-and-forget async calls (`async void` outside event handlers)
   - NOTE: `ConfigureAwait()` enforcement is handled by Meziantou's
     MA0004 / SonarAnalyzer S3216 / CA2007, not by AsyncFixer.

4. **Microsoft.VisualStudio.Threading.Analyzers**
   - Thread safety enforcement
   - Async method naming conventions
   - Deadlock prevention

5. **Microsoft.CodeAnalysis.BannedApiAnalyzers**
   - Blocks usage of APIs listed in `BannedSymbols.txt`
   - Enforces async-first patterns (see below)

6. **Meziantou.Analyzer**
   - Comprehensive code quality checks
   - Performance optimizations
   - Best practice enforcement

7. **SonarAnalyzer.CSharp**
   - Industry-standard code analysis
   - Security vulnerability detection
   - Code smell identification

8. **Microsoft.CodeAnalysis.PublicApiAnalyzers** (`src/` projects with `PublicAPI.Shipped.txt` / `PublicAPI.Unshipped.txt`)
   - Every public API change must be recorded in `PublicAPI.Unshipped.txt` (RS0016/RS0017)
   - Core's files are copied into each `-Core-EFx` project; update all of them together

### Async-First Enforcement

This library **prohibits synchronous blocking calls** via `BannedSymbols.txt`. The following APIs are **banned**:

#### ❌ Blocking Async Operations
```csharp
// Banned - blocks threads
task.Wait();
task.Result;
Task.WaitAll(tasks);

// Required - truly async
await task;
await Task.WhenAll(tasks);
```

#### ❌ Synchronous I/O
```csharp
// Banned
File.ReadAllText(path);
stream.Read(buffer, 0, count);
streamReader.ReadLine();

// Required
await File.ReadAllTextAsync(path);
await stream.ReadAsync(buffer, 0, count);
await streamReader.ReadLineAsync();
```

#### ❌ Thread Blocking
```csharp
// Banned
Thread.Sleep(1000);
Console.ReadLine();

// Required
await Task.Delay(1000);
// Avoid blocking console reads in async code
```

#### ❌ Obsolete/Insecure APIs
```csharp
// Banned
var client = new WebClient();
var formatter = new BinaryFormatter();
var now = DateTime.Now; // Use DateTimeOffset

// Required
var client = new HttpClient();
// Use System.Text.Json.JsonSerializer
var now = DateTimeOffset.UtcNow;
```

**Why?** This ensures all code is **truly asynchronous** and **non-blocking**, providing optimal performance in async contexts.

---

## Build and Test Instructions

### Prerequisites
- .NET 10.0 SDK or later (required for the repo's net10.0 target; older SDKs cannot load the csproj)
- PowerShell Core (optional, for formatting scripts)

### Build the Project

```bash
# Restore NuGet packages
dotnet restore

# Build in Release configuration (enforces all analyzers)
dotnet build --configuration Release
```

**Note:** Release builds treat all analyzer warnings as errors (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`). Debug builds allow warnings to facilitate development.

### Run Tests

```bash
# Run all unit tests
dotnet test --configuration Release

# Run with coverage (if configured)
dotnet test --collect:"XPlat Code Coverage"
```

### Code Formatting

This project uses `.editorconfig` for consistent code style:

```bash
# Format all code
dotnet format

# Check formatting without changes (CI mode)
dotnet format --verify-no-changes

# PowerShell formatting script
pwsh ./scripts/format.ps1
```

See [docs/README-FORMATTING.md](docs/README-FORMATTING.md) for detailed formatting rules.

---

## .editorconfig Rules

Key style rules enforced:

- **Indentation:** 4 spaces (C#), 2 spaces (XML/JSON)
- **Line endings:** LF (Unix-style)
- **Charset:** UTF-8
- **Trim trailing whitespace:** Yes
- **Final newline:** Yes
- **Braces:** New line style (Allman)
- **Naming:** PascalCase for public members, camelCase for parameters/locals
- **File-scoped namespaces:** the project convention (`.editorconfig` reports block-scoped ones as a suggestion, not an error)
- **`var` preferences:** Use for built-in types and when type is obvious
- **Null checks:** Prefer pattern matching (`is null`, `is not null`)

View the complete configuration in [.editorconfig](.editorconfig).

---

## Guidelines

- Follow the coding style used in the project.
- Write clear, concise commit messages.
- Add relevant tests for new features or bug fixes.
- Document any public APIs with XML documentation comments.
- Ensure all analyzer warnings are addressed (they're treated as errors in Release builds).
- Use async/await patterns - no blocking calls allowed.
- Include `CancellationToken` parameters in async methods where appropriate.
- Add every new project to `Wolfgang.DbContextBuilder.slnx`. The solution-wide gates (Stage 2 build, CodeQL, InspectCode, release validation, Stryker) only see projects listed there.

---

## Pull Requests

- **Changelog fragment.** A PR that changes anything under `src/` must add one file to
  `changelog/unreleased/` (first line `type: breaking|feature|fix|docs|internal`, then a one-sentence
  user-facing description; see [changelog/unreleased/README.md](changelog/unreleased/README.md)),
  or carry the `no-changelog` label. The *Changelog Fragment Check* job fails otherwise.
- **Protected files go in their own PR.** A PR may change protected configuration files
  (`.editorconfig`, `Directory.Build.props/targets`, `BannedSymbols.txt`, workflows,
  `coverlet.runsettings`, `.config/dotnet-tools.json`, ... — the full list is in
  [docs/WORKFLOW_SECURITY.md](docs/WORKFLOW_SECURITY.md)) **or** other files, never both; the
  *Protected Files Guard* fails a mixed PR. Split the configuration change into a separate PR.
- Ensure your pull request passes all tests and analyzer checks.
- Respond to review feedback in a timely manner.
- Reference related issues in your pull request description.
- Keep changes focused and atomic - one feature/fix per PR.
- Update documentation if you change public APIs.

---

## Code of Conduct

Please be respectful and considerate in all interactions. See [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) for our community guidelines.

---

Thank you for contributing! 🎉
