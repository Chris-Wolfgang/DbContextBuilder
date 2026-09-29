namespace Wolfgang.DbContextBuilder.Tests.Fuzz;

/// <summary>
/// A representative entity shape for fuzzing <see cref="Wolfgang.DbContextBuilderCore.ICreateRandomEntities"/>
/// implementations — a public parameterless constructor plus a spread of the common scalar
/// property types both <c>BogusRandomEntityCreator</c> and <c>AutoFixtureRandomEntityCreator</c>
/// document rules for.
/// </summary>
public sealed class FuzzEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid ExternalId { get; set; }
}
