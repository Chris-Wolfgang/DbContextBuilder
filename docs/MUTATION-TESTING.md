# Mutation testing

Mutation testing seeds deliberate faults ("mutants") into the source and checks that the
test suite catches them. A **survived** mutant is a change in behavior that no test noticed:
a gap that line coverage cannot show, since a line can run in a test without any assertion
depending on it.

This repository runs [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction/)
as an enforced quality bar (#300), configured by `stryker-config.json` and run by
`.github/workflows/stryker.yaml`.

## The floors

Two floors apply, and a run fails if either is missed:

- **Repository floor:** `thresholds.break` in `stryker-config.json` makes `dotnet stryker`
  exit non-zero when the overall mutation score drops below it.
- **Per-project floors:** `mutation-floors.json` gives every project under `src/` its own
  floor, and the workflow checks each project's score against it. A repository-wide score
  can hide one package far under the line, so each shipped package is judged on its own. A
  `src/` project with tested mutants but no entry in the file fails the run, so a new package
  cannot slip in unchecked.

The scores below are from the latest full run (see "Measuring honestly" below), CI run
37784524025 on 2026-10-08, Stryker 5.0.0:

| Project | Valid mutants | Score | Floor |
|---|---|---|---|
| `src/Wolfgang.DbContextBuilder-Core` | 1125 | 93.33 % | 90 % |
| `src/Wolfgang.DbContextBuilder-EF6` | 96 | 94.79 % | 92 % |
| `src/Wolfgang.DbContextBuilder.AutoFixture` | 27 | 96.30 % ¹ | 92 % |
| `src/Wolfgang.DbContextBuilder.Bogus` | 7 | 100.00 % ¹ | 100 % |
| **Repository (`break`)** | 1255 | **91.71 %** ² | **88 %** |

¹ Measured in project mode (see "Packages measured in project mode" below), locally with the
same configuration, 2026-10-08. That run's solution mode under-measured them at 22.22 % and
57.14 % (#552).
² That run still mutated AutoFixture and Bogus in solution mode. `break` now applies to the
umbrella run alone, which leaves them out: Core and EF6 together scored 93.45 % in it.

`low` / `high` in `stryker-config.json` (90 % / 95 %) only color the report. Stryker requires
`low` to be at least `break`.

Each floor sits a few points under its measured score so small run-to-run variation does not
fail a run, while a real regression (a deleted assertion, untested new behavior) does. For
example, a mutant that makes a test much slower can exceed Stryker's timeout on a slow
runner (counted as detected) but finish in time, and survive, on a faster one; a mutant
whose only killing test is flaky can also flip between killed and survived. For a package
with only a handful of mutants (Bogus has 7) one mutant moves the score by many points, so
its floor allows no further survivors.

**Policy: ratchet the floors up, never down.** As survivors are killed and a score climbs,
raise that floor to lock the gain in. Lowering a floor to turn a red run green defeats the
point; close the test gap instead. The pull-request gate enforces this: a PR whose
`mutation-floors.json` lowers or removes a floor fails. The one exception so far was a
correction, not a ratchet: the original floor of 85 % was set on an inflated score (below).

## Packages measured in project mode

`AutoFixture.Tests.Unit` and `Bogus.Tests.Unit` target net6.0 to net10.0 and reference a
different `-Core-EF*` project for each framework. Stryker's solution mode cannot measure the
packages they test: it runs every framework's build of those tests but puts the mutated
package into the net10.0 build only, so the kills are lost (#552). Those two packages are
left out of the root config's `mutate` list, and each test project has its own
`stryker-config.json`; the workflow runs Stryker in that directory, in project mode, after the
umbrella run, and judges the report against the same `mutation-floors.json`. A package with a
test project like that needs the same treatment.

## Measuring honestly

Until 2026-10-05 the workflow used Stryker's default `coverage-analysis: perTest`. In CI that
mode logs "test coverage capture failed", after which a mutant the tests do not kill runs
more tests than its timeout allows and is recorded as `Timeout`, which Stryker counts as
detected. Survivors were being scored as kills: the same code measured 97.59 % that way and
78.39 % with `coverage-analysis: perTestInIsolation`, which reports 258 more survivors
(#544). The workflow now uses `perTestInIsolation`. A full run takes about 93 minutes.

When a score looks too good, check the share of `Timeout` mutants: a large share is a sign
that survivors are being hidden.

## How it runs

| Trigger | Scope | What it does |
|---|---|---|
| Weekly (Sunday 06:00 UTC) and `workflow_dispatch` | Every mutant | Enforces the floors, charts the score on the docs site under `/dev/stryker/`, and keeps one rolling `kind:mutation-survives` issue listing the surviving and uncovered mutants |
| Pull requests to `main` that touch `src/**`, `tests/**/*.cs`, `tests/**/*.csproj`, `stryker-config.json`, `mutation-floors.json` or the workflow | `--since:<base commit>` | Enforces the floors on the mutants the PR's changes bring into scope (a project's floor applies only when the PR brings some of its mutants into scope), using `stryker-config.json` and `mutation-floors.json` from the base branch so a PR cannot lower its own floors; fails a PR that lowers or removes a floor in `mutation-floors.json` |

A full run takes about 105 minutes on a Windows runner (Windows, so the .NET Framework
targets compile), which is why pull requests use `--since`. What a PR run covers:

- Mutants in the `.cs` files the PR changed.
- Mutants covered by the tests in any test file the PR changed. Editing a widely used test
  file therefore brings in much of the suite.
- **Every** mutant if the PR changes a non-`.cs` file that is not ignored, such as a
  `.csproj` or props file. Stryker cannot tell what such a change affects. Docs,
  changelog fragments, `PublicAPI.*.txt`, `mutation-floors.json` and `.github/` are ignored through
  `since.ignore-changes-in` in `stryker-config.json`.
- A PR whose changes leave no mutant in scope passes with a notice.

Dependabot PRs skip the PR gate: their `.csproj` bumps would make every run a full run,
and the weekly run covers what a dependency change does to the score.

The PR gate is not a required status check, and must not become one: a path-filtered
workflow that does not run leaves a required check "Expected" forever.

## Equivalent mutants

Some mutants change the code without changing behavior any test could observe. Those
cannot be killed. They are never suppressed with comments in the code; instead:

- Where a whole family is equivalent and a safe, general exclusion exists, it is excluded in
  `stryker-config.json`. Today that is only `ConfigureAwait(false)` to `true`, excluded by
  `"ignore-methods": ["ConfigureAwait"]`.
- Every other equivalent mutant, such as a null guard that the next EF call repeats with the
  same exception, stays in the survivors issue with its triage written down there, so the
  next reader does not redo it.

## Running locally

```bash
dotnet tool install --global dotnet-stryker --version 5.0.0
dotnet stryker --config-file stryker-config.json
```

A full local run is slow. To work on one file, mutate only that file:

```bash
dotnet stryker --config-file stryker-config.json --mutate "**/DbSetAssertions.cs"
```

Or test only what your branch changed, as the PR gate does:

```bash
dotnet stryker --config-file stryker-config.json --since:main
```

Run `--since` from a normal clone, not a `git worktree`. In a worktree, Stryker 5.0.0
reported the changed files but tested none of their mutants for the same commits that a
clone scored correctly. Keep the checkout path short too: under a deep path the .NET
Framework build fails with `MSB3501` path-length errors.

The HTML report under `StrykerOutput/**/reports/` lists every surviving mutant with its
file, line and the change Stryker made. That is the worklist for raising the score.
