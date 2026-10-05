window.BENCHMARK_DATA = {
  "lastUpdate": 1791219941512,
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
      }
    ]
  }
}