# Project context

## Current phase

PR 4 adds cancellation quote/execution on the PR 3 sell/retrieve foundation. Refund calculation, cancellation/refund persistence, rollback, and competing cancellations are verified against SQL Server. Renewal, mixed lifecycle races, and the frontend remain on later stacked PRs.

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

- GitHub CI evidence is pending the PR 2 push.
- Lifecycle behaviour and the browser app remain pending later stacked PRs.
- A repository licence has not been selected.
- GitHub CI evidence for the stacked branches remains pending.
