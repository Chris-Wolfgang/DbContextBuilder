namespace Wolfgang.DbContextBuilderCore.Tests.Unit;

/// <summary>
/// Provides a collection of overrides that apply to how Sqlite handles creating the database
/// </summary>
/// <remarks>
/// Not all database engines support the same set of features and so it is sometimes necessary
/// to override the default behavior for the DbContext you are trying to create so that it is
/// compatible with Sqlite.
/// </remarks>
public record SqliteOverrides
{
}