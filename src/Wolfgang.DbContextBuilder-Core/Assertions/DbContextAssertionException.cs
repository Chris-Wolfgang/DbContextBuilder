namespace Wolfgang.DbContextBuilderCore.Assertions;

/// <summary>
/// Thrown when a <see cref="DbSetAssertions{TEntity}"/> assertion fails. Carries an
/// EF-aware failure message that identifies the entity type and the specific check that
/// did not pass.
/// </summary>
public class DbContextAssertionException : Exception
{
    /// <summary>Initializes a new instance with no message or inner exception.</summary>
    public DbContextAssertionException() { }

    /// <summary>Initializes a new instance with the specified message.</summary>
    /// <param name="message">The failure message describing what assertion did not hold.</param>
    public DbContextAssertionException(string message) : base(message) { }

    /// <summary>Initializes a new instance with the specified message and inner exception.</summary>
    /// <param name="message">The failure message describing what assertion did not hold.</param>
    /// <param name="innerException">The exception that caused the assertion to fail.</param>
    public DbContextAssertionException(string message, Exception innerException) : base(message, innerException) { }
}
