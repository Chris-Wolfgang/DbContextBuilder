# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.3.3]

### Changed
- EF Core package dependency version ranges updated

## [0.3.2]

### Changed
- EF Core package dependency version ranges updated

## [0.3.1]

### Changed
- EF Core package dependency version ranges updated

## [0.3.0]

### Added
- EF Core 10 support (`Wolfgang.DbContextBuilder-Core-EF10`)

## [0.2.0]

### Added
- Sqlite in-memory database support via `UseSqlite()` extension method
- `SqliteModelCustomizer` for Sqlite-specific model customization
- `SqliteForMsSqlServerModelCustomizer` for SQL Server model compatibility with Sqlite

## [0.1.0]

### Added
- Initial release
- Builder pattern for creating `DbContext` instances with in-memory databases
- `SeedWith<T>` for seeding specific test data
- `SeedWithRandom<T>` for seeding random test data via AutoFixture
- Support for EF Core 6, 7, 8, and 9
- Support for Entity Framework 6 (classic) on .NET Framework 4.6.2--4.8.1
- `Wolfgang.DbContextBuilder-EF6` package for Entity Framework 6

[Unreleased]: https://github.com/Chris-Wolfgang/DbContextBuilder/compare/v0.3.3...HEAD
[0.3.3]: https://github.com/Chris-Wolfgang/DbContextBuilder/compare/v0.3.2...v0.3.3
[0.3.2]: https://github.com/Chris-Wolfgang/DbContextBuilder/compare/v0.3.1...v0.3.2
[0.3.1]: https://github.com/Chris-Wolfgang/DbContextBuilder/compare/v0.3.0...v0.3.1
[0.3.0]: https://github.com/Chris-Wolfgang/DbContextBuilder/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/Chris-Wolfgang/DbContextBuilder/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/Chris-Wolfgang/DbContextBuilder/releases/tag/v0.1.0
