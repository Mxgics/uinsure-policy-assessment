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

## PR 3 — sell and retrieve

- Task: implement the sell, policy retrieval, and term retrieval slice with domain rules and SQL persistence.
- Assistance: drafted domain entities, explicit HTTP contracts, EF mappings/migration, deterministic tests, and aligned evidence documents.
- Corrections: supplied the SQL connection through test-host configuration instead of ineffective EF registration replacement; treated `Location` as an absolute URI; removed test-order assumptions from persistence assertions.
- Verification: 14 domain and 10 API/integration tests passed in Release against SQL Server in Docker.
- Review decision: pending repository-owner review; PR 3 remains stacked on PR 2.
- Remaining limits: lifecycle operations, conflict handling, browser workflows, and CI results belong to later work.

## PR 4 — cancellation

- Task: implement quote/execution, refunds, history, atomicity, and cancellation conflicts.
- Assistance: translated the agreed examples into literal tests before the calculator, implemented the domain/persistence/API slice, and constructed real-SQL race and rollback checks.
- Corrections: fixed an informational unused-day expectation; explicitly marked the new cancellation graph as added; narrowed duplicate-key conflict handling to the named cancellation index after the deterministic race exposed SQL command ordering.
- Verification: 24 domain and 16 API/integration tests passed in Release against SQL Server in Docker, including two-connection concurrency and injected later-write rollback.
- Review decision: pending repository-owner review; PR 4 remains stacked on PR 3.
