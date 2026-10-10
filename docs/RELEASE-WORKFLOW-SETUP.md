# Release Workflow Setup Guide

This guide explains how to configure a repository to use the standard `release.yaml` workflow. The same checklist applies whether you are bootstrapping a new repo from `repo-template` or auditing an existing one.

## Overview

The release workflow triggers when you **publish a GitHub Release** and implements a comprehensive validation and automatic deployment process that:
- ✅ Tests all target frameworks per test project on Windows
- ✅ Enforces per-assembly line coverage: 95% for src assemblies, 100% for test assemblies
- ✅ Validates NuGet package integrity with smoke tests
- ✅ Automatically publishes to NuGet.org after validation passes
- ✅ Eliminates duplicate build work for faster releases

## Required Configuration

Complete the following one-time setup so that the workflow can publish releases:

### Configure NuGet Trusted Publishing (no API key)

The `publish-nuget` job does not use a stored API key. It runs with `id-token: write`, and the
`NuGet/login` action exchanges the run's GitHub OIDC token for a temporary push key that expires
in about an hour; nothing long-lived is kept in the repository. No `NUGET_API_KEY` secret is
needed (or read).

**Location:** nuget.org → your account → **Trusted Publishing** → **Add policy**

1. **Repository owner / repository:** this repository (`Chris-Wolfgang/DbContextBuilder`).
2. **Workflow file:** `release.yaml`.
3. **Environment:** leave empty (the job does not use a GitHub environment).
4. **Packages:** the package IDs the release publishes — all nine `Wolfgang.DbContextBuilder*` IDs.

The `user:` input of the `NuGet/login` step in `release.yaml` must be the nuget.org account that
owns the policy.

### Optional: `SECURITY_ALERTS_TOKEN` for the security-alerts workflow

`security-alerts.yml` turns open code-scanning, secret-scanning and Dependabot alerts into issues.
`GITHUB_TOKEN` cannot read secret-scanning alerts (and Dependabot readability varies by account),
so add a repository secret **`SECURITY_ALERTS_TOKEN`**: a fine-grained personal access token with
only **Secret scanning alerts: read**, **Dependabot alerts: read** and **Metadata: read**. One
token may cover all your repositories. Without it, those alert kinds are skipped with a notice.
The workflow file's header comment has the details.

### Verify Branch Protection Rules

**Location:** Settings → Branches → main (or Settings → Rules → Rulesets)

> **Note:** `scripts/Setup-BranchRuleset.ps1`, which creates the ruleset interactively, lives in
> `repo-template` and is not copied into this repository. This repository has
> `scripts/Fix-BranchRuleset.ps1`, which repairs an existing ruleset's required checks. Otherwise
> configure the settings manually using the checklist below.

Ensure the following settings are enabled:

- ✅ **Require a pull request before merging**
  - **Single developer repos:** 0 approvals (default)
  - **Multi-developer repos:** 1+ approvals (recommended)
- ✅ **Require status checks to pass before merging**
  - Required checks should include the following status check contexts:
    - "Stage 1: Linux Tests (.NET 5.0-10.0) + Coverage Gate"
    - "Stage 2: Windows Tests (.NET 5.0-10.0, Framework 4.6.2-4.8.1)"
    - "Stage 3: macOS Tests (.NET 6.0-10.0)"
  - The "5.0" in the Stage 1/2 names is historical (the jobs no longer install the .NET 5 SDK).
    The names are required-check contexts, so renaming a job and the ruleset must happen together.
    - "Security Scan (DevSkim)"
    - "Security Scan (CodeQL)"
- ✅ **Require branches to be up to date before merging**
- ✅ **Require conversation resolution before merging**
- ✅ **Do not allow bypassing the above settings** (recommended, even for admins)
- ✅ **Restrict deletions**
- ✅ **Require linear history** (optional but recommended)

**What this does:** Ensures all code merged to `main` has passed comprehensive validation, preventing broken releases.

## Testing the Release Workflow

After completing the setup, test the workflow by creating a GitHub Release:

1. Go to your repository's **Releases** page
2. Click **"Draft a new release"**
3. Choose or create a tag (e.g., `v0.0.1-test`)
4. Add a title and description (optional for a test)
5. Check **"Set as a pre-release"** for test releases
6. Click **"Publish release"**

The workflow triggers automatically when the release is published.

### Expected Workflow Behavior

Six jobs run (see *Workflow Architecture* below for the order):

1. **validate-release** — checks the tag equals `<Version>` in `src/Version.props`, and that no src
   csproj sets its own. It then runs every test project on every target framework with coverage, and
   enforces 95 % per src assembly and 100 % per test assembly (each assembly that ran must have a
   coverage row).
2. **pack-and-validate** — generates the third-party notices, packs, smoke-tests installing each
   package, generates the CycloneDX SBOM and uploads the packages.
3. **verify-docs-build** — builds the docfx site (metadata + build) without deploying, so a release
   never ships with broken docs.
4. **publish-nuget** — `NuGet/login` (OIDC) and `dotnet nuget push` of every package.
5. **trigger-docs** — starts the docs deploy for the release, only after publishing succeeded.
6. **update-release-artifacts** — writes the reproducible-build manifest, attests build provenance
   for the packages and attaches packages, SBOM, coverage report, manifest and provenance bundle
   to the GitHub Release.

### Monitoring the Workflow

- **Actions Tab:** Shows workflow progress in real-time
- **Artifacts:** Each job uploads artifacts (coverage reports, packages)
- **Releases:** Check the Releases page after successful completion

## Troubleshooting

### `NuGet/login` or `dotnet nuget push` fails with 401 / 403

**Problem:** The `publish-nuget` job fails at the NuGet login or the push.

**Solution:**
1. **401:** nuget.org has no Trusted Publishing policy matching this repository and `release.yaml`
   (or the `user:` in the login step is not the policy owner's account). Add or fix the policy.
2. **403:** the policy exists but does not cover the package being pushed. Add that package ID to
   the policy.
3. Re-run the failed job from the Actions tab (do not re-publish the release).

### Tests Fail on Specific Framework

**Problem:** Tests pass on some frameworks but fail on others (e.g., net462).

**Solution:**
1. Check the test logs for framework-specific issues
2. Fix compatibility issues in your code
3. Test locally: `dotnet test --framework net462`
4. Push fix, then re-publish the release (or re-run the workflow from the Actions tab)

### Coverage Below Threshold (95% src, 100% tests)

**Problem:** Workflow fails at coverage validation step.

**Solution:**
1. Review `CoverageReport/Summary.txt` artifact
2. Add tests for uncovered code paths
3. Ensure tests run on all frameworks
4. Push fix, then re-publish the release (or re-run the workflow from the Actions tab)

### Smoke Test Fails to Install Package

**Problem:** Package packs successfully but fails smoke test installation.

**Solution:**
1. Check package dependencies in `.csproj`
2. Verify framework compatibility in `<TargetFrameworks>`
3. Test locally: `dotnet pack` then try installing in a test project
4. Fix packaging issues and re-publish the release (or re-run the workflow from the Actions tab)

## Production Release Checklist

Before creating a production GitHub Release (e.g., `v1.0.0`):

- [ ] All tests pass on all platforms (pr.yaml workflow)
- [ ] Code coverage meets the thresholds: 95% per src assembly, 100% per test assembly
- [ ] Security scan shows no critical issues
- [ ] `<Version>` bumped in `src/Version.props` (the one place; the src csprojs import it)
- [ ] `CHANGELOG.md` updated with release notes (if applicable)
- [ ] All PRs merged to `main` branch
- [ ] Local build succeeds: `dotnet build --configuration Release`
- [ ] Local tests pass: `dotnet test --configuration Release`

**Create a production release:**
1. Go to your repository's **Releases** page
2. Click **"Draft a new release"**
3. Choose or create the version tag (e.g., `v1.0.0`) targeting `main`
4. Add a title and release notes
5. Click **"Publish release"**

**After workflow completes:**
- [ ] Verify packages appear on NuGet.org
- [ ] Test installing package from NuGet.org in a clean project
- [ ] Announce release (if applicable)

## Workflow Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  Trigger: Published GitHub Release                          │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
validate-release ── tag/version check, tests on every TFM, coverage gate
        │
        ▼
pack-and-validate ── notices, pack, smoke install, SBOM
        │
        ├──────────────► verify-docs-build ── docfx metadata + build (no deploy)
        │                         │
        ▼                         ▼
publish-nuget  (needs pack-and-validate AND verify-docs-build) ── OIDC login, push
        │
        ├──────────────► trigger-docs  (needs validate-release, publish-nuget)
        │
        ▼
update-release-artifacts  (needs validate-release, pack-and-validate, publish-nuget)
   ── reproducible-build manifest, provenance attestation, release assets
```

## Key Improvements Over Previous Workflow

| Issue | Before | After |
|-------|--------|-------|
| **Framework Coverage** | Default framework only | All frameworks (net6.0-10.0, net462-481) |
| **Code Coverage** | Not enforced | 95% src / 100% test assemblies enforced |
| **Package Validation** | None | Smoke test installation |
| **Deployment** | Incomplete publish script | Automatic publishing after validation |
| **Publishing credentials** | Long-lived API key secret | NuGet Trusted Publishing (OIDC, short-lived key) |
| **GitHub Releases** | Not used as trigger | Workflow triggered by published release |
| **Build Efficiency** | Duplicate builds in each job | Build once per job with dependencies |
| **Test Logging** | No logger parameter | Console logging with verbosity |
| **Permissions** | Read-only | Write access for releases |

## Support

If you encounter issues not covered in this guide:

1. Check the Actions tab of this repository on GitHub for detailed logs
2. Review artifacts uploaded by failed jobs
3. Consult the [GitHub Actions documentation](https://docs.github.com/en/actions)
4. Open an issue in this repository with:
   - Workflow run URL
   - Error message and logs
   - Steps to reproduce
