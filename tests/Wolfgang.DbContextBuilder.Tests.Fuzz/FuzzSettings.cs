using FsCheck.Xunit;

namespace Wolfgang.DbContextBuilder.Tests.Fuzz;

/// <summary>
/// Shared configuration for the continuous-fuzz suite. The per-property case count
/// is read from the <c>FUZZ_CASES</c> environment variable: the weekly fuzz workflow
/// sets it to a generous budget (100,000+), while an unset value defaults to a modest
/// <see cref="DefaultCaseCount"/> so an incidental full-solution <c>dotnet test</c>
/// (or a quick local run) stays fast. The actual fuzzing happens in <c>fuzz.yaml</c>.
/// </summary>
internal static class FuzzSettings
{
    // Modest default so a whole-solution test run that happens to include this
    // project isn't dragged down; fuzz.yaml overrides via FUZZ_CASES.
    internal const int DefaultCaseCount = 1_000;

    internal static int CaseCount { get; } =
        int.TryParse(Environment.GetEnvironmentVariable("FUZZ_CASES"), out var n) && n > 0
            ? n
            : DefaultCaseCount;
}

/// <summary>
/// <see cref="PropertyAttribute"/> whose <c>MaxTest</c> defaults to
/// <see cref="FuzzSettings.CaseCount"/> — the continuous-fuzz case budget.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class FuzzPropertyAttribute : PropertyAttribute
{
    public FuzzPropertyAttribute()
    {
        MaxTest = FuzzSettings.CaseCount;
    }
}
