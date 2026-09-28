# Project context

## Current phase

PRs #1–#7 are merged. Delivery stage 8 remains [GitHub PR #9](https://github.com/Mxgics/uinsure-policy-assessment/pull/9) on `codex/pr8-review-fixes`. Its existing tip was backed up, current `origin/main` was merged without force-push, and the ancestry-conflict resolution was verified to reproduce the correction tip tree before coherence work. The [stage 8 note](changes/008-review-fixes.md) preserves its earlier clean-checkout evidence; the [coherence review](changes/009-coherence-review.md) records later fixes. Clean-checkout gates, rendered/keyboard review and the disposable demo passed on `2709595`, and all three CI jobs passed on `1f7615c`. The subsequent owner-requested error-handling refactor passed complete local gates (including 18 frontend tests); its exact-head CI result belongs in PR #9 before owner review.

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

- GitHub PR #9 requires current-head CI and owner review; do not merge without explicit instruction.
- Bedrooms removal is intentionally data-losing for that column; the runbook documents backup and guarded rollback. City is optional and Address Line 3 is retained.
- Historical PR 3 red/green chronology remains unavailable; it has not been invented.
- A repository licence has not been selected.
- PR #9 history still contains the original stacked commits, but its file comparison against `main` contains only stage 8 corrections and later coherence work.
