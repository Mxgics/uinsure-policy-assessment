# Project plan

## Delivery principles

Use small coherent commits, truthful evidence, and owner review. PRs #2–#7 were originally delivered as a stack and are now merged. Meaningful business behaviour follows red/green/refactor; setup and documentation do not need artificial failing tests.

## Pull request sequence

| PR | Deliverable | Exit condition | Status |
| --- | --- | --- | --- |
| 1 | Brief, requirements, assumptions, architecture ADR, context/plan, agent/contribution/PR conventions | Public documentation PR is verified and ready for owner review; no implementation claims | Merged |
| 2 | ASP.NET controllers/SQL/migrations, reproducible tests/CI, OpenAPI/errors, local tooling | Clean local foundation runs; real SQL migration/test path and CI are verified; Docker execution is evidenced | Merged |
| 3 | Sell, policy retrieval, term retrieval | Validated domain/API/SQL behaviour and history for M1/M2 and related boundaries | Merged |
| 4 | Cancellation quote/execution | Refund rules, claims/no-payment, atomicity, rollback and cancel races verified; assessment minimum complete | Merged |
| 5 | Renewal | Window, successor/history, payments/cheque, and mixed lifecycle races verified | Merged |
| 6 | React UI | Accessible responsive workflows and real browser journeys verified | Merged |
| 7 | Final readiness | Clean-clone proof, complete requirement evidence, walkthrough, limits and public-source review | Merged; historical checkpoint later qualified by review |
| 8 | Review corrections and coherence | R1-R9, missing boundaries, property migration, UI recovery/accessibility, real browser/API/SQL proof and final coherence fixes | GitHub PR #9 open; final local gates passed, current-head CI and owner review pending |

Review only GitHub PR #9 against `main`. After explicit merge authorization, verify the resulting `main` commit and CI before the owner submits the repository.

## Review follow-up - 2026-09-27

Nine review findings were open at `806ecab`. The owner approved one delivery-stage 8 correction plan, removing Bedrooms, retaining optional City, and including real browser-to-SQL tests. After PRs #2–#7 merged, PR #9 was repaired by merging `main` into its existing branch without rewriting history. The [stage 8 note](changes/008-review-fixes.md), [historical review](changes/review-checkpoint-2026-09-27.md), and [coherence findings](changes/009-coherence-review.md) separate earlier evidence from the final candidate.

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
