# Workflow Security

## Overview

This document describes the security measures implemented in the GitHub Actions workflows for this repository, particularly focusing on the PR validation workflow (`.github/workflows/pr.yaml`).

## Security Architecture

### 1. Workflow YAML Protection

**Mechanism**: `pull_request` trigger, with gate integrity in a separate workflow

The PR workflow runs on `pull_request`, so it builds and tests the PR's own code **and the PR's own
copy of the workflow**. The validation a pull request receives is therefore the validation it asks
for, and an edit that weakens it is visible in the same diff a reviewer reads.

Analyzer and build configuration is deliberately **not** the PR's. Before the analyzer stages,
`pr.yaml` re-fetches `.editorconfig` (and nested `*.editorconfig`), `Directory.Build.props`/`.targets`,
`BannedSymbols.txt`, `*.globalconfig`, `*.ruleset`, `*.DotSettings`, `coverlet.runsettings`,
`.config/dotnet-tools.json`, the workflows and the CI scripts from `main`, so a pull request cannot
lower a rule, change what coverage counts or swap a tool version for its own run. That means a PR is **not** validated byte-for-byte as it will
merge - its own copies of those files are ignored. This is defence in depth; the primary control is
the guard below.

```yaml
on:
  pull_request:
    branches:
      - main
```

This replaced a `pull_request_target` model that ran main's copy of the workflow and then checked
out `refs/pull/*/head` into the same jobs. That combination - trusted context plus untrusted code
in one job - is the pwn-request pattern, and OpenSSF Scorecard reports it as a **critical**
`DangerousWorkflow` finding. Running main's YAML is what made the checked-out PR code exploitable,
not what prevented it.

What stops a PR from weakening the checks it is subject to is **`.github/workflows/
protected-files.yaml`**: a `pull_request_target` workflow that never checks out PR code at all. It
lists the PR's changed files through the API and fails any PR mixing a protected file with other
changes, so such a change must arrive as a configuration-only PR that a maintainer reviews on its
own. It fails closed - if it cannot obtain a complete file list, it refuses rather than reporting
"nothing protected changed".

A `pull_request` run also gets a read-only `GITHUB_TOKEN` and no secrets for forks. The single
write scope in `pr.yaml` is `security-events: write` on the SARIF upload job, which checks out the
base commit explicitly and never PR content.

### 2. Configuration File Protection

**Problem**: A pull request supplies its own configuration files (`.editorconfig`, `BannedSymbols.txt`, etc.), which control:
- Code analyzer behavior
- Code quality standards
- Security scanning rules

A malicious PR could modify these files to disable security checks.

**Solution**: `protected-files.yaml` classifies these files as protected and fails any PR that changes one alongside anything else. A configuration change is therefore always reviewed as a standalone PR rather than being silently applied to the run that validates a code change. `pr.yaml` additionally re-fetches several of them from `main` before the analyzer steps, which remains as defence in depth.

**Files `pr.yaml` overwrites from `main`** (every trusted-fetch list in the workflow):
- `.editorconfig` and any nested `*.editorconfig` - code style and analyzer rules
- `Directory.Build.props` / `Directory.Build.targets` - MSBuild properties and targets
- `BannedSymbols.txt` - banned API usage rules
- `*.globalconfig` - global analyzer configuration
- `*.ruleset` - code analysis rulesets
- `*.DotSettings` - ReSharper / InspectCode inspection severities
- `coverlet.runsettings` - what coverage counts
- `.config/dotnet-tools.json` - the CI tool versions `dotnet tool restore` installs
- `.github/workflows/*.yml` and `.github/workflows/*.yaml` - workflow definitions

The fetch fails closed: if `main` cannot be fetched, or a file the workflow requires is missing
from it, the step fails instead of continuing with the PR's copy.

The list above is what `pr.yaml` **overwrites**. The set the guard **protects** is larger, and is
the authoritative one - `.github/workflows/protected-files.yaml` fails any PR that changes one of
these alongside anything else:

| Protected | Why |
|---|---|
| `.editorconfig`, `*.globalconfig`, `*.ruleset`, `*.DotSettings` | analyzer severities |
| `Directory.Build.props` / `.targets`, **at any depth** | MSBuild discovers these per directory; this repo has four |
| `BannedSymbols.txt` | banned-API gate |
| `coverlet.runsettings` | what coverage counts |
| `.config/dotnet-tools.json` | pins every CI tool version |
| `.gitleaks.toml` | secret-scan rules |
| `.github/workflows/*.yml` / `*.yaml` | the checks themselves |
| `.github/license-audit/*.json` | licence allow-list |
| `.github/requirements/*` | pip-installed and executed by the privileged scans |
| `scripts/changelog.ps1`, `tfm-parity.ps1`, `build-pr.ps1`, `third-party-notices.ps1` | CI executes them |

Matching is case-insensitive, because Windows and ReSharper resolve names that way.

Dependabot is exempted from the overwrite step (its bumps to `Directory.Build.props` are
legitimate), and from the guard.

The guard is a required check, not a sandbox: it prevents a protected change landing, not a PR's
own run from seeing it. The scans that hold `security-events: write` do not rely on it for their
inputs: `actions-audit.yaml` and `semgrep.yaml` take `.github/requirements/*` from the base branch
before `pip install`, and refuse to fall back to the PR's copy.

**Implementation** (in jobs that consume project source — e.g. `detect-projects`, the test stages, and the security scans; *not* the `secrets-scan` job, which only fetches `.gitleaks.toml`):
```yaml
- name: Fetch trusted configuration files from main branch
  run: |
    echo "Fetching configuration files from main branch to prevent malicious overrides..."
    
    # Fetch the main branch (fails the step if it cannot)
    git fetch origin main:main-branch

    # List of configuration files that should come from trusted main branch
    config_files=(
      ".editorconfig"
      "Directory.Build.props"
      "Directory.Build.targets"
      "BannedSymbols.txt"
      "*.globalconfig"
      "*.ruleset"
      "*.editorconfig"
      "coverlet.runsettings"
      ".config/dotnet-tools.json"
      "*.DotSettings"
      ".github/workflows/*.yml"
      ".github/workflows/*.yaml"
    )
    
    # Copy each configuration file from main branch if it exists
    for config_file in "${config_files[@]}"; do
      # [Copy logic - see workflow file for full implementation]
    done
```

### 3. Credential Protection

**Mechanism**: `persist-credentials: false`

All checkout steps include `persist-credentials: false` to prevent the checkout token from being written to git config. Under `pull_request` the default ref is the PR's merge commit, so no explicit `ref:` is needed; the one job that must not see PR content (the SARIF upload) checks out `github.event.pull_request.base.sha`. Actions are pinned by commit SHA:

```yaml
- name: Checkout code
  uses: actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1 # v7.0.1
  with:
    persist-credentials: false
```

Workflows that need to authenticate a `git fetch` pass the token for that one command
(`git -c http.extraheader="AUTHORIZATION: basic ..." fetch ...`), never in the remote URL, so it
cannot end up in `.git/config` or in a logged URL.

**Note**: This prevents the token from being stored in git config, but does NOT prevent steps from accessing `GITHUB_TOKEN` if explicitly exposed.

### 4. Minimal Permissions

The workflow runs with minimal required permissions:

```yaml
permissions:
  contents: read
```

This limits the impact if the `GITHUB_TOKEN` is somehow exposed or misused.

## Attack Scenarios Prevented

### Scenario 1: Malicious Workflow Modification
**Attack**: PR modifies `.github/workflows/pr.yaml` to disable security checks
**Prevention**: `protected-files.yaml` fails any PR that changes a workflow alongside other files, so a workflow edit must arrive as a configuration-only PR reviewed on its own. The edit does take effect in its own PR's run - that is the point of `pull_request` - but it cannot ride along unnoticed with a code change.
**Status**: ✅ Protected

### Scenario 2: Configuration File Tampering
**Attack**: PR modifies `.editorconfig` to disable security analyzers
**Prevention**: Configuration files are fetched from main branch after checkout
**Status**: ✅ Protected

### Scenario 3: Credential Theft
**Attack**: PR contains malicious code that tries to access GitHub credentials
**Prevention**: `persist-credentials: false` + minimal permissions
**Status**: ✅ Protected

### Scenario 4: Code Analysis Bypass
**Attack**: PR modifies `BannedSymbols.txt` or `.ruleset` to allow dangerous APIs
**Prevention**: These files are fetched from main branch after checkout
**Status**: ✅ Protected

## Validation

The following manual validation scenarios can be used when reviewing changes to the workflow security model:

1. **Configuration Fetch Validation**: Confirm that configuration files are fetched from the `main` branch during workflow execution
2. **Malicious Modification Validation**: Simulate a PR that modifies `.editorconfig` to disable analyzers and confirm the workflow replaces it with the trusted version from `main`

## Maintenance

### Making Changes to Protected Configuration Files

To update protected configuration files (`.editorconfig`, `BannedSymbols.txt`, etc.), follow this workflow:

1. **Create a PR with your configuration changes**
   - Make changes to the configuration file(s) in your PR branch
   - The PR workflow will still fetch and use the current main branch version for testing
   - This means your PR will be tested against the **existing** configuration standards

2. **Get your PR reviewed and merged to main**
   - Once merged, your configuration changes become the new "trusted" version on main
   - Future PRs will automatically use your updated configuration

3. **Why this works:**
   - Configuration changes are intentionally one commit behind during PR validation
   - This ensures you can't weaken security standards in the same PR that adds problematic code
   - After merge, the new standards apply to all subsequent PRs

**Example Workflow:**
```
PR #1: Update .editorconfig to add new rule
  ↓ (tested with old .editorconfig from main)
  ↓ (approved and merged)
  ↓
Main: Now has updated .editorconfig

PR #2: New feature
  ↓ (tested with updated .editorconfig from main)
  ↓ (builds/tests using new rules)
```

**Important Notes:**
- If you need to relax a security rule AND add code that violates the old rule in the same change, you'll need two PRs:
  1. First PR: Update the configuration file only
  2. Second PR: Add the code that requires the relaxed rules
- This is intentional security design to prevent simultaneous weakening of standards and addition of problematic code

### Adding New Protected Configuration Files

When adding new configuration files that control code quality or security:

1. Add the file name to every trusted-fetch list in `pr.yaml` - the bash `config_files` arrays and the Stage 2 PowerShell list (search `pr.yaml` for `Fetch trusted configuration files` to find them all). The `secrets-scan` job does not consume project config files and does not need to be updated.
2. Add the pattern to **`.github/workflows/protected-files.yaml`** - the authoritative guard (a required check).
3. Add it to the "Detect protected configuration file changes" step in `pr.yaml` as well; the two classify the same files and are kept in lock-step.
4. Test that the file is correctly fetched from main branch.
5. Update this documentation (both lists above).

Changes to these workflows are themselves protected-file changes, so they go in a protected-only PR.

## References

- [GitHub Actions Security Hardening](https://docs.github.com/en/actions/security-guides/security-hardening-for-github-actions)
- [Keeping your GitHub Actions secure](https://docs.github.com/en/actions/security-guides/security-hardening-for-github-actions#using-third-party-actions)
- [Understanding pull_request_target](https://securitylab.github.com/research/github-actions-preventing-pwn-requests/)
