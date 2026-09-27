# Project plan

## Delivery principles

Use small coherent stacked PRs, truthful evidence, and bottom-up review. The owner approved continuous implementation of PRs 2–7 on 2026-09-26; each PR targets its predecessor until that dependency merges. Meaningful business behaviour follows red/green/refactor; setup and documentation do not need artificial failing tests.

## Pull request sequence

| PR | Deliverable | Exit condition | Status |
| --- | --- | --- | --- |
| 1 | Brief, requirements, assumptions, architecture ADR, context/plan, agent/contribution/PR conventions | Public documentation PR is verified and ready for owner review; no implementation claims | Merged |
| 2 | ASP.NET controllers/SQL/migrations, reproducible tests/CI, OpenAPI/errors, local tooling | Clean local foundation runs; real SQL migration/test path and CI are verified; Docker execution is evidenced | Open for owner review; CI passed |
| 3 | Sell, policy retrieval, term retrieval | Validated domain/API/SQL behaviour and history for M1/M2 and related boundaries | Ready for review; local verification passed |
| 4 | Cancellation quote/execution | Refund rules, claims/no-payment, atomicity, rollback and cancel races verified; assessment minimum complete | Ready for review; local verification passed |
| 5 | Renewal | Window, successor/history, payments/cheque, and mixed lifecycle races verified | Ready for review; local verification passed |
| 6 | React UI | Accessible responsive workflows and real browser journeys verified | Ready for review; local verification passed |
| 7 | Final readiness | Clean-clone proof, complete requirement evidence, walkthrough, limits and public-source review | Original checkpoint; subsequently qualified by review |
| 8 | Review corrections | R1-R9, missing boundaries, property migration, UI recovery/accessibility, real browser/API/SQL proof | Implemented; local verification passed; owner review pending |

Review and merge bottom-up. After a base PR merges, retarget the next PR to `main`; never merge a dependent PR before its base.

## Review follow-up - 2026-09-27

The statuses above describe the original delivery checkpoints, not unconditional approval. Live inspection found PR 1 merged and PRs 2-7 open with successful CI. Nine review findings were open at `806ecab`. The owner approved one [PR 8 correction plan](review-remediation-plan.md) on top of PR 7, removing Bedrooms, retaining optional City, and including real browser-to-SQL tests. The [PR 8 note](changes/008-review-fixes.md) records implemented corrections and verification. The [explanation index](changes/README.md) and [review audit](changes/review-checkpoint-2026-09-27.md) record the evidence and documentation limits. New correction PRs stop for owner review individually under `AGENTS.md`.

## Planned structure

```text
Uinsure.slnx
src/
  Uinsure.Domain/
  Uinsure.Api/
tests/
  Uinsure.Domain.Tests/
  Uinsure.Api.IntegrationTests/
web/
docs/
  decisions/
  changes/
  runbooks/
scripts/
```

The domain project references no ASP.NET Core or EF packages. The API contains controller contracts, feature services, and persistence and references the domain. Add no extra application/infrastructure projects without a concrete dependency problem and an ADR update.

## Verification strategy

- Domain: fast xUnit tests with literal boundary expectations and pure calculation inputs.
- API/database: `WebApplicationFactory`, real SQL Server via Testcontainers, real migrations, isolated databases, and no InMemory/SQLite substitute.
- Atomicity/races: separate contexts/connections, deterministic barriers/interceptors, later-command failure injection, and final-state assertions from a fresh context.
- UI: Vitest/React Testing Library for component behaviour and Playwright for sell/retrieve, quote/cancel, and renew.
- CI: Ubuntu x64, locked restores, Release build, non-mutating format checks, tests/builds as introduced, read-only permissions, full-SHA actions, no deployment.

Executed commands and results are recorded in the test plan and per-PR change notes; planned checks are never presented as passes.

## Documentation set

Maintain this plan and context, the requirement matrix, assumptions, consequential ADRs, per-PR change notes, test plan, local runbook, and AI-use log. Link rather than duplicate details. A source link or planned check is not execution evidence.
