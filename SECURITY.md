# Security Policy

## Supported Versions

Security fixes are released for the **latest published version** only. If you are on an older
version, upgrade to the latest release to receive the fix. Pre-1.0 releases (0.x) follow the same
rule: the newest 0.x release is the supported one.

## Reporting a Vulnerability

If you discover a security vulnerability, please follow these steps:

1. **Do not** create a public issue on this repository.
2. Open the private report form: https://github.com/Chris-Wolfgang/DbContextBuilder/security/advisories/new
   (or, from the repository's **Security** tab, click **Report a vulnerability**).
   GitHub's guide to this process: https://docs.github.com/en/code-security/security-advisories/guidance-on-reporting-and-writing-information-about-vulnerabilities/privately-reporting-a-security-vulnerability
3. Fill out the provided form with:
   - A description of the vulnerability
   - Steps to reproduce the issue
   - Potential impact
   - Suggested fix (if you have one)

## Response Timeline

This is a single-maintainer project; the commitments below are realistic for that, not aspirational.

| Stage | Target |
|-------|--------|
| Acknowledgement of the report | Within 48 hours |
| Initial assessment (confirmed / not a vulnerability / need more information) and severity | Within 7 days |
| Fix for **Critical** severity | Within 30 days |
| Fix for **High** or **Medium** severity | Within 90 days |
| Fix for **Low** severity | Next scheduled release |

If a target is going to slip, you will hear that from the maintainer in the advisory thread before
the deadline passes, not after.

## Disclosure Process

1. **Report** — you open a private advisory (above). It is not public, but it is not a
   two-person conversation either. GitHub grants access to the repository owner, to organization
   owners, to security managers, and to anyone with the repository's admin role, plus any
   collaborator explicitly added to the advisory. Repository write access alone does not grant it.
   Nobody outside that set sees the report until it is published.
2. **Triage** — the maintainer confirms the issue and assigns a severity, discussed with you in the
   advisory thread.
3. **Fix** — the fix is developed in a temporary private fork attached to the advisory, so nothing
   about the vulnerability is visible in public pull requests until the fix is released.
4. **Release** — a new version ships with the fix. The release notes say a security issue was fixed
   without giving details that would help exploit unpatched versions.
5. **Publish** — the advisory is published, which notifies dependents through Dependabot. A CVE is
   requested from GitHub for confirmed vulnerabilities; assignment is GitHub's decision, so the
   advisory may publish without one. Publication happens at release time, or after 90 days from the
   initial report if no fix is possible, whichever comes first.

Please keep the details private until the advisory is published.

## Verifying the supply chain

Every release publishes evidence that the packages on NuGet were built from this
repository, unmodified. Consumers (and enterprise procurement) can verify each link:

- **Build provenance (SLSA).** `release.yaml` generates a signed provenance
  attestation for every `.nupkg` via `actions/attest-build-provenance`. It proves
  the package was built by this repo's release workflow at a specific commit.
  Verify a downloaded package with the GitHub CLI:

  ```bash
  gh attestation verify Wolfgang.DbContextBuilder-Core.<version>.nupkg \
    --repo Chris-Wolfgang/DbContextBuilder
  ```

- **Repository signature.** NuGet.org applies a repository signature to every
  published package. Verify it with:

  ```bash
  nuget verify -Signatures Wolfgang.DbContextBuilder-Core.<version>.nupkg
  ```

  > Author (code-signing-certificate) signing is not currently applied — packages
  > carry NuGet.org's repository signature plus the build-provenance attestation
  > above. Author signing may be added later; it requires a code-signing certificate
  > (tracked separately, see #376).

- **SBOM.** A CycloneDX SBOM (`*.bom.json`) for each package is attached to the
  GitHub Release, listing the exact dependency set the package was built against.

## Credit

Reporters are credited in the published advisory and in the release notes, unless you ask not to be.
Tell us in the report how you would like to be named.

## Thank You

Your help is greatly appreciated!
Responsible disclosure of security vulnerabilities helps protect our entire community.
