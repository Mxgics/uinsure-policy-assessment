# Project context

## Current phase

PR 8 on `codex/pr8-review-fixes`, based on PR 7 at `806ecab`, implements the nine review corrections and real browser-to-SQL verification. Local verification passed: 38 domain, 72 API/SQL, eight component, ten browser-contract and eight real full-stack checks. The [PR 8 note](changes/008-review-fixes.md) records corrections and evidence; final clean-checkout/CI results are recorded when executed. Owner review remains pending.

After PR 1 merged, the owner approved a stacked workflow: PRs 2–7 are implemented continuously as dependent branches and reviewed/merged bottom-up.

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

- PR 8 requires owner review and bottom-up merge after the original stack.
- Bedrooms removal is intentionally data-losing for that column; the runbook documents backup and guarded rollback. City is optional and Address Line 3 is retained.
- Historical PR 3 red/green chronology remains unavailable; it has not been invented.
- A repository licence has not been selected.
- Live GitHub inspection on 2026-09-27 found successful CI for PRs 2-7. PR 1 is merged; PRs 2-7 remain open for owner review and bottom-up merge.
