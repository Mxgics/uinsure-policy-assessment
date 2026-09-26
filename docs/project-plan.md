# Project plan

## Delivery principles

Use small coherent PRs, truthful evidence, and owner review gates. Meaningful business behaviour follows red/green/refactor; setup and documentation do not need artificial failing tests. Update context, requirements, assumptions, ADRs, change/test evidence, runbooks, and AI notes with the code they describe.

## Pull request sequence

| PR | Deliverable | Exit condition | Status |
| --- | --- | --- | --- |
| 1 | Brief, requirements, assumptions, architecture ADR, context/plan, agent/contribution/PR conventions | Public documentation PR is verified and ready for owner review; no implementation claims | Ready for owner review |
| 2 | ASP.NET controllers/SQL/migrations, reproducible tests/CI, OpenAPI/errors, local tooling | Clean local foundation runs; real SQL migration/test path and CI are verified; Docker execution is evidenced | Not started |
| 3 | Sell, policy retrieval, term retrieval | Validated domain/API/SQL behaviour and history for M1/M2 and related boundaries | Not started |
| 4 | Cancellation quote/execution | Refund rules, claims/no-payment, atomicity, rollback and cancel races verified; assessment minimum complete | Not started |
| 5 | Renewal | Window, successor/history, payments/cheque, and mixed lifecycle races verified | Not started |
| 6 | React UI | Accessible responsive workflows and real browser journeys verified | Not started |
| 7 | Final readiness | Clean-clone proof, complete requirement evidence, walkthrough, limits and public-source review | Not started |

Do not merge a PR or begin the next one until the repository owner reviews the checkpoint.

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

Exact executed commands and results will be added only when the corresponding foundations exist.

## Documentation set

Maintain this plan and context, the requirement matrix, assumptions, consequential ADRs, per-PR change notes, test plan, local runbook, and AI-use log. Link rather than duplicate details. A source link or planned check is not execution evidence.
