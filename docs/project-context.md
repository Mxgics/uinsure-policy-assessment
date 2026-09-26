# Project context

## Current phase

PR 1 establishes the public documentation baseline. There is no application solution, source code, database schema, migration, test suite, CI workflow, container configuration, or frontend yet. Accordingly, no runtime or test result is claimed.

The public repository uses `main` as its default branch and a focused `codex/pr1-project-foundation` branch for this work. PR review is the gate before any PR 2 foundation implementation begins.

## Observed environment

Observed while preparing PR 1 on 26 September 2026:

- .NET SDK `10.0.400`;
- Node.js `24.19.0` and npm `11.5.1`;
- Docker CLI `29.7.2`;
- Docker’s Linux engine pipe was absent, so server health and SQL Server container execution remain unverified;
- the project uses an isolated Git root rather than the parent user-profile repository.

These observations describe one development machine, not portable prerequisites or successful project execution. PR 2 must resolve and pin compatible dependency/container versions, verify Docker-backed execution, and record exact commands and results.

## Accepted direction

The application will be a local modular monolith: ASP.NET Core 10 controller API, domain project, EF Core 10 with SQL Server 2022, and a React/TypeScript/Vite frontend. It deliberately avoids extra service boundaries and framework layers until an actual dependency problem justifies them.

The backend remains ahead of UI work. Business behaviour uses deterministic clocks/calculations and real SQL integration evidence. Lifecycle changes share a policy-level optimistic concurrency boundary and one atomic save.

## Information boundary

The repository contains only public project material and synthetic examples. Supplied assessment PDFs, credentials, personal paths/data, raw conversations, and private interview preparation stay outside it. The public AI log records assistance, decisions, corrections, and verification without reproducing private context.

## Known unresolved issues

- Docker engine/SQL Server execution is not yet verified.
- Exact stable NuGet, npm, container tag/digest, and GitHub Action SHAs will be resolved in PR 2.
- A repository licence has not been selected.
- All implementation and test evidence remains pending.
