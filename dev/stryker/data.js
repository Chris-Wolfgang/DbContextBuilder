window.BENCHMARK_DATA = {
  "lastUpdate": 1791472494746,
  "repoUrl": "https://github.com/Chris-Wolfgang/DbContextBuilder",
  "entries": {
    "Mutation score": [
      {
        "commit": {
          "author": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "committer": {
            "name": "GitHub",
            "username": "web-flow",
            "email": "noreply@github.com"
          },
          "id": "bd6b789ba60fe4f2edfadba698c88517bb27392c",
          "message": "ci(docfx): retry the gh-pages push after rebasing onto a newer tip (#527)\n\nThe docs deploy pushed to gh-pages once. benchmarks.yaml's /dev/bench chart\nand the stryker.yaml publish job's /dev/stryker chart also push there, and\none landing between the deploy's fetch and its push made the release's docs\ndeploy fail as non-fast-forward. Those writers touch only dev/, which the\ndeploy preserves, so on a rejected push the deploy now fetches gh-pages,\nrebases its single commit onto the new tip and pushes again, up to 5 tries.\nA rebase conflict aborts the rebase and fails the deploy.\n\nA shared concurrency group was the alternative; GitHub keeps one pending\njob per group, so a queued release deploy could be cancelled.\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-03T02:47:15Z",
          "url": "https://github.com/Chris-Wolfgang/DbContextBuilder/commit/bd6b789ba60fe4f2edfadba698c88517bb27392c"
        },
        "date": 1791105949679,
        "tool": "customBiggerIsBetter",
        "benches": [
          {
            "name": "Mutation score",
            "value": 90.65,
            "unit": "%"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "committer": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "id": "cf291d1dd9f2b40950e0d5edfe9231172112c58d",
          "message": "ci(stryker): give the full run 240 minutes; it outgrew 120 (#300)\n\nThe full run took 54 min on 2026-09-30 (run 36770590098), 76 min on\n2026-10-04 (37188029950), and was cancelled at the 120-minute limit on\n2026-10-05 (37318653068) with 1240 mutants still in its test phase. A\nthird of detected mutants are timeouts (370 of 1074, 374 of 1173), each\nwaiting out Stryker's per-mutant timeout, so the run grows with the test\nsuite. 240 stays under GitHub's 360-minute job limit. Raising Stryker's\nconcurrency instead would load the runner and turn more mutants into\nload-sensitive timeouts, moving the score.\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-05T15:45:37Z",
          "url": "https://github.com/Chris-Wolfgang/DbContextBuilder/commit/cf291d1dd9f2b40950e0d5edfe9231172112c58d"
        },
        "date": 1791219940251,
        "tool": "customBiggerIsBetter",
        "benches": [
          {
            "name": "Mutation score",
            "value": 97.59,
            "unit": "%"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "committer": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "id": "876cf4a9e7a135c82a9236216848f75eff59d7a6",
          "message": "exp: additional-timeout 30000 for a measurement run (#544); not for merge",
          "timestamp": "2026-10-05T15:49:43Z",
          "url": "https://github.com/Chris-Wolfgang/DbContextBuilder/commit/876cf4a9e7a135c82a9236216848f75eff59d7a6"
        },
        "date": 1791220211708,
        "tool": "customBiggerIsBetter",
        "benches": [
          {
            "name": "Mutation score",
            "value": 97.59,
            "unit": "%"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "committer": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "id": "ae054afbc584418761170c175473936e07094021",
          "message": "exp: unit tests only (test-case-filter) for a measurement run (#544); not for merge",
          "timestamp": "2026-10-05T17:17:32Z",
          "url": "https://github.com/Chris-Wolfgang/DbContextBuilder/commit/ae054afbc584418761170c175473936e07094021"
        },
        "date": 1791225765040,
        "tool": "customBiggerIsBetter",
        "benches": [
          {
            "name": "Mutation score",
            "value": 97.59,
            "unit": "%"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "committer": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "id": "33d9b767e8951a221dd8e6f4a8efef9dc9e3c026",
          "message": "exp: coverage-analysis off for a measurement run (#544); not for merge",
          "timestamp": "2026-10-05T18:44:26Z",
          "url": "https://github.com/Chris-Wolfgang/DbContextBuilder/commit/33d9b767e8951a221dd8e6f4a8efef9dc9e3c026"
        },
        "date": 1791228843803,
        "tool": "customBiggerIsBetter",
        "benches": [
          {
            "name": "Mutation score",
            "value": 99.6,
            "unit": "%"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang",
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com"
          },
          "committer": {
            "name": "GitHub",
            "username": "web-flow",
            "email": "noreply@github.com"
          },
          "id": "2dde5fa486a767b1c00ab37eb5a6b6b584fd6d64",
          "message": "fix(sqlite): register EF SQLite services once and keep a single model customizer (#549)\n\nThe guard around AddEntityFrameworkSqlite() looked for a ServiceType of\nSqliteOptionsExtension, which is an options extension and never a DI\nservice, so it was always true. It now looks for the\nIDatabaseProvider -> DatabaseProvider<SqliteOptionsExtension> descriptor\nthat AddEntityFrameworkSqlite registers (verified present on EF Core 6-10).\n\nOld IModelCustomizer registrations were removed before\nAddEntityFrameworkSqlite ran, so EF's default ModelCustomizer was\nTryAdd-ed back next to the SQLite one. Removal now runs after EF's\nregistration, leaving exactly one customizer.\n\nCo-authored-by: Claude Opus 5.5 <noreply@anthropic.com>",
          "timestamp": "2026-10-08T12:52:25Z",
          "url": "https://github.com/Chris-Wolfgang/DbContextBuilder/commit/2dde5fa486a767b1c00ab37eb5a6b6b584fd6d64"
        },
        "date": 1791472493309,
        "tool": "customBiggerIsBetter",
        "benches": [
          {
            "name": "Mutation score",
            "value": 91.71,
            "unit": "%"
          }
        ]
      }
    ]
  }
}