# Test plan and evidence

## PR 2 evidence — 2026-09-26

- Release solution build: succeeded with zero warnings/errors.
- Domain architecture: one passed; no ASP.NET Core or EF Core references.
- HTTP contracts: five passed for liveness, OpenAPI, automatic validation, 404 Problem Details, and sanitized unexpected errors.
- SQL migration: passed against Docker Engine `29.7.2` and the pinned SQL Server 2022 CU27 amd64 image; the initial migration applied with none pending.
- Maintained local path: Compose reported SQL healthy on `127.0.0.1:14333`, the explicit migration created `UinsureAssessment`, and the API returned `Healthy` plus an OpenAPI document containing `/health` on `127.0.0.1:5080`; shutdown retained the named data volume.
- Dependency audit: all four projects reported no known vulnerable direct or transitive packages from NuGet.org.

The first HTTP run exposed Windows Event Log access in the test host; portable console logging fixed the infrastructure. The first SQL pull exposed an incorrect manifest digest; Docker's verified amd64 repo digest replaced it before the passing test. These are recorded corrections, not hidden green-only history.

Policy behaviour, rollback injection, lifecycle races, and browser journeys remain pending their stacked PRs. CI evidence is pending branch push.

## Planned layers

- Domain: deterministic rules with independent literal expectations.
- API/database: controller pipeline and real isolated SQL Server databases/migrations.
- Atomicity/concurrency: deterministic barriers/interceptors, separate contexts/connections, and fresh final-state reads.
- UI: component tests and Playwright workflows with synthetic historical fixtures.

Tests fail rather than skip when Docker is unavailable. EF InMemory and SQLite are not substitutes.
