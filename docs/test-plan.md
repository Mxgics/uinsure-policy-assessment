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

## PR 3 evidence — 2026-09-26

- Release solution suite: 14 domain tests and 10 API/integration tests passed.
- Domain boundaries: sale-date limits, one-year/leap calculation, holder count, age, supported types, premium precision, and normalisation passed with a fixed clock.
- HTTP/SQL behaviour: sale returned 201 and a retrievable Location; policy and term retrieval matched persisted snapshots; required booleans and numeric enums were rejected; unknown/mismatched resources returned 404; the current migration applied to SQL Server.
- Persistence: returned policy/term identifiers were found in fresh EF queries with a recorded payment.

The first SQL-backed feature run failed because the test host replaced EF registrations while the application still resolved its deferred configuration callback. Supplying the isolated connection string through test configuration fixed the host accurately. A subsequent assertion was corrected to compare the path of the valid absolute Location URI. The final full run passed against Docker; no result was skipped or substituted.

## Planned layers

- Domain: deterministic rules with independent literal expectations.
- API/database: controller pipeline and real isolated SQL Server databases/migrations.
- Atomicity/concurrency: deterministic barriers/interceptors, separate contexts/connections, and fresh final-state reads.
- UI: component tests and Playwright workflows with synthetic historical fixtures.

Tests fail rather than skip when Docker is unavailable. EF InMemory and SQLite are not substitutes.
