# License audit policy files

`license-audit.yaml` runs `nuget-license` over every `src/` project and fails when a package's
licence is not allowed. These files configure it. They are protected files: change them only in a
PR that touches nothing else.

| File | What it holds |
|---|---|
| `allowed-licenses.json` | The SPDX licence identifiers a dependency may have. |
| `url-license-mappings.json` | Maps packages that expose a licence **URL** instead of an SPDX expression to an identifier (the dotnet/standard `LICENSE.TXT` is MIT; the `go.microsoft.com` LinkId=329770 URL is MS-.NET-Library). |
| `package-overrides.json` | Packages whose licence is a file inside the package (`nuget-license` type "File", which it cannot map): package id, pinned version and the verified licence. |
| `ignored-packages.json` | Packages the audit skips entirely. This file is a plain JSON string array, so it cannot carry a comment; the reasons are below. |

## Why each ignored package is ignored

- **`SonarAnalyzer.CSharp`** — a build-time analyzer, referenced with `PrivateAssets=all`, so it is
  never shipped in or depended on by our packages. Its licence (SONAR Source-Available) is not an
  OSI licence, but it imposes no obligation on consumers of this library.

Add a line here whenever you add a package to `ignored-packages.json`.
