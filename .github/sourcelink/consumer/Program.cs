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

internal sealed class FixtureContext : DbContext
{
}
