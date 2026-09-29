using System;
using Microsoft.EntityFrameworkCore;
using Wolfgang.DbContextBuilderCore;

// End-to-end SourceLink "step into" fixture. The debugger sets a breakpoint on the
// marked line below and issues a step-into (the F11 a consumer would press). If
// SourceLink is intact the debugger resolves the library's real source (from GitHub)
// inside DbContextBuilder<T>.UseInMemory, instead of a decompiled placeholder.
// UseInMemory is a plain, non-async public method with a real body, which makes it
// a clean and stable step-into target.

using var builder = new DbContextBuilder<FixtureContext>();
builder.UseInMemory(); // STEP_INTO_TARGET
Console.WriteLine("ok");

// MA0048/S3903: a repo-wide Directory.Build.props override was deliberately avoided here -
// Directory.Build.props/.targets are protected filenames at any depth (Protected Files Guard),
// and a nested one under .github/ tripped a stale, root-only pattern in a second, older
// protected-file check elsewhere in pr.yaml that hasn't caught up to that "any depth" rule.
// Suppressing the two warnings this fixture-only, non-shipping file trips is simpler and safer
// than fixing a shared, protected workflow just for this fixture's convenience.
#pragma warning disable MA0048, S3903
internal sealed class FixtureContext : DbContext
{
}
#pragma warning restore MA0048, S3903
