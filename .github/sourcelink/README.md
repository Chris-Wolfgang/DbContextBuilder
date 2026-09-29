# SourceLink step-into verification

Fixtures for [`sourcelink-stepinto.yaml`](../workflows/sourcelink-stepinto.yaml),
which proves the *debugger* half of SourceLink end-to-end: that pressing **F11**
in a consumer steps into the library's **real source** (resolved via the
SourceLink map in the PDB), not a decompiled placeholder.

This complements [`sourcelink.yaml`](../workflows/sourcelink.yaml), which proves
every document in the PDB resolves to real GitHub content via
`dotnet sourcelink test`. Together they cover the full chain F11 depends on:
SourceLink map in the PDB → source-file resolution → GitHub raw-URL fetch.

## How it works

1. Build [`consumer/`](consumer/) with `-c Debug -p:ContinuousIntegrationBuild=true`.
   Its `ProjectReference` compiles the library with the **same SourceLink map a
   released package ships** (deterministic `/_/…` source roots that map to GitHub
   raw URLs), and Debug/non-optimized codegen so the step-into target isn't
   reordered away.
2. Run [`verify_stepinto.py`](verify_stepinto.py) — it drives
   [netcoredbg](https://github.com/Samsung/netcoredbg) over its MI interface to
   break in the consumer, step into `DbContextBuilder<T>.UseInMemory`, and assert
   the resulting frame is the library's SourceLink-mapped source file with
   symbols loaded. If SourceLink or the sequence points were broken the step
   would land with no source and the script exits non-zero.

## Why a ProjectReference, not the packed NuGet

netcoredbg is the only CI-scriptable .NET debugger, and it does **not** reliably
pair a *package*-sourced assembly with its symbol-package (`.snupkg`) PDB — it
reports `symbols-loaded=0` and cannot step into it (verified during development
on the AuditTrail reference implementation this was ported from). A
`ProjectReference` built with `ContinuousIntegrationBuild=true` produces a
**byte-identical SourceLink PDB**, so the SourceLink map under test is the same
one that ships; only the delivery path differs. The packed-package symbol/URL
resolution is covered by `sourcelink.yaml`.

## Files

- `consumer/StepIntoConsumer.csproj` / `Program.cs` — the fixture consumer. It
  references `Wolfgang.DbContextBuilder-Core` and the break line is marked
  `STEP_INTO_TARGET`. Inherits the repo's own analyzers rather than an
  isolation-stub `Directory.Build.props`/`.targets` (that pattern hit a stale,
  root-only protected-file check elsewhere in `pr.yaml`) — the couple of
  warnings that come from that are suppressed inline instead.
- `verify_stepinto.py` — the debugger driver (exit 0 = step-into resolved real
  source).

## Scope

Scheduled + manual, not a PR gate — debugger automation is heavier and slightly
less deterministic than a unit test, and SourceLink's raw URLs resolve only
against a pushed commit.
