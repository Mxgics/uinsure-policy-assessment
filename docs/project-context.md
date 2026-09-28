# Project context

## Current phase

PRs #1–#7 and delivery-stage 8 [GitHub PR #9](https://github.com/Mxgics/uinsure-policy-assessment/pull/9) are merged into `main` at `8c845fa`. Delivery stage 9 is implemented and locally verified on `codex/demo-usability`: opt-in stable synthetic scenarios, one-to-three-holder UI support, matching age/payment constraints and clearer lifecycle availability. The owner confirmed manual testing complete on 28 September 2026. The final submission review aligns documentation and makes mocked UI tests independent of the wall clock. Executed evidence belongs in the [test plan](test-plan.md); final PR review remains the gate before an explicitly authorized merge.

## Observed environment

Observed while preparing PR 1 on 26 September 2026:

- .NET SDK `10.0.400`;
- Node.js `24.19.0` and npm `11.5.1`;
- Docker CLI `29.7.2`;
- Docker Engine `29.7.2` was started and SQL Server 2022 CU27 migration execution was verified through Testcontainers;
- the project uses an isolated Git root rather than the parent user-profile repository.

These observations describe one development machine. Exact pins and evidence are in `docs/changes/002-api-foundation.md` and `docs/test-plan.md`; GitHub CI supplies the portable check after push.

## Accepted direction

The application will be a local modular monolith: ASP.NET Core 10 controller API, domain project, EF Core 10 with SQL Server 2022, and a React/TypeScript/Vite frontend. It deliberately avoids extra service boundaries and framework layers until an actual dependency problem justifies them.

The backend remains ahead of UI work. Business behaviour uses deterministic clocks/calculations and real SQL integration evidence. Lifecycle changes share a policy-level optimistic concurrency boundary and one atomic save.

## Information boundary

The repository contains only public project material and synthetic examples. Supplied assessment PDFs, credentials, personal paths/data, raw conversations, and private interview preparation stay outside it. The public AI log records assistance, decisions, corrections, and verification without reproducing private context.

## Known unresolved issues

- Delivery stage 9 has recorded local gates and owner-confirmed manual testing; publication CI and final PR review are tracked separately. Do not merge without explicit instruction.
- Bedrooms removal is intentionally data-losing for that column; the runbook documents backup and guarded rollback. City is optional and Address Line 3 is retained.
- Historical PR 3 red/green chronology remains unavailable; it has not been invented.
- A repository licence has not been selected.
