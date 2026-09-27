# AI assistance log

This public log records material assistance, decisions, corrections, and verification without raw conversations, credentials, supplied documents, personal paths/data, or private interview preparation.

## PR 1 — project documentation foundation

- Task: turn the reviewed plan into a public project brief, requirement matrix, assumptions, ADR, context/plan, and contribution conventions.
- Assistance: inspected the agreed planning pack and existing documentation patterns; checked targeted official platform documentation; drafted the repository documents and traceability structure; checked Git boundaries and publication metadata.
- Human decisions already provided: .NET 10/controller API, EF Core/SQL Server, React/TypeScript, local-first delivery, complete must/should/could scope, focused PRs, and an owner review gate after every PR.
- Important correction preserved: lifecycle writes update the shared policy row; child insertion alone would not make a policy rowversion detect every cancel/renew race.
- Review decision: pending repository-owner review of PR 1. Do not record approval until it happens.
- Verification: isolated root/branch, whitespace, 24 unique requirement IDs, relative links, private path/contact patterns, public visibility, and the `main` default were checked on 2026-09-26. Pull-request checks and owner review remain pending; see `docs/changes/001-project-foundation.md`.
- Remaining limits: no application/test evidence; Docker/SQL execution unverified; dependency pins and CI deferred to PR 2.

## PR 2 — API and SQL foundation

- Task: scaffold the .NET solution, API/error/OpenAPI contract, SQL migration path, integration tests, local tooling, and pinned CI.
- Assistance: resolved official registry versions/digests/action SHAs; drafted the host, tests, scripts, workflow, and evidence; ran and interpreted checks.
- Human decision: after PR 1 merged, the owner changed delivery to stacked PRs and authorized continuous implementation of PRs 2–7.
- Corrections: selected portable test logging after Event Log access masked responses; replaced a non-pullable manifest digest with Docker's verified amd64 repo digest; retained SQL Server rather than substituting a database.
- Verification: Release build, one domain boundary test, five HTTP contract tests, and one real SQL Server migration test passed. CI is pending push.
- Remaining limit: the migration is intentionally schema-empty; policy behaviour begins in PR 3.
