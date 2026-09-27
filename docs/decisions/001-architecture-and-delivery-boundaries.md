# ADR 001: Local modular monolith with SQL Server persistence

- Status: Accepted and locally verified
- Date: 2026-09-26
- Decision owners: repository owner, informed by the reviewed assessment plan

## Context

The assessment needs a C# REST API for a small but rule-heavy policy lifecycle, automated evidence, informative errors, and a public repository. The selected extension adds all should/could behaviours and a small React demonstration. It must remain understandable in an interview and achievable through focused PRs.

The principal technical risks are boundary correctness, financial/history integrity, concurrent lifecycle mutations, and reproducible SQL-backed verification—not service scale. Authentication, external payment/pricing/claims systems, cloud infrastructure, and production operation are outside scope.

## Decision

Build a local modular monolith on .NET 10:

- `Uinsure.Domain` holds policy concepts and pure rules and has no ASP.NET Core or EF dependency.
- `Uinsure.Api` is an ASP.NET Core controller API with explicit DTOs, feature services, Problem Details, OpenAPI, EF Core 10, and the Microsoft SQL Server provider.
- SQL Server 2022 Developer in a pinned Linux x64 container is the only local/test database. Use real migrations and constraints.
- A React/TypeScript/Vite single-page demonstration calls the same-origin `/api` proxy after backend behaviour is complete.
- One process and database are sufficient. Do not add microservices, a message broker, Redis, or speculative application/infrastructure projects.
- Normal mutations use one `SaveChangesAsync`; the parent policy revision/rowversion is updated for cancellation and renewal so aggregate races share an optimistic-concurrency boundary.
- GitHub Actions provides CI only. It does not imply deployment, Azure, or production readiness.

Use .NET 10 because it is the agreed stack and is an active LTS release. The SDK is pinned to `10.0.400`; packages, the SQL image, and CI actions are locked exactly. Node `24.19.0` is pinned for frontend tooling.

## Alternatives considered

### Minimal APIs

Viable for the endpoint count, but controllers were selected to make request contracts, automatic validation behaviour, response metadata, and interview navigation explicit. ASP.NET Core officially supports both approaches.

### Additional application and infrastructure projects

They can enforce boundaries in a larger system, but add ceremony without a demonstrated dependency problem here. Cohesive domain and API projects keep the assessment navigable; this can be revised if implementation produces a concrete coupling issue.

### EF InMemory, SQLite, or a non-SQL Server fallback

Rejected. They do not prove SQL Server migrations, rowversion/constraints, transaction rollback, or real concurrency semantics. A blocked Docker engine is an explicit prerequisite failure, not permission to substitute persistence.

### Microservices or asynchronous messaging

Rejected. The lifecycle is one small transactional aggregate with no external network calls. Distribution would make atomicity and local operation harder without meeting a requirement.

### Frontend framework/SSR/global state framework

A small client-rendered React app with local form state and typed fetch is sufficient. SSR and global state add operational/conceptual cost without helping the demonstration journeys.

## Consequences

- The repository remains compact and explainable, and domain rules can be tested independently.
- SQL-backed tests require a working Docker Linux engine, x64 support, sufficient memory, and SQL Server licence acceptance. They must fail clearly rather than skip or change providers.
- The API owns more composition because there is no separate application layer; feature cohesion and dependency direction must be reviewed as behaviour grows.
- Optimistic concurrency and SQL constraints need deterministic integration tests; rowversion alone on an unchanged parent would be insufficient.
- The local unauthenticated app is not production-ready. Adding identity, external systems, hosting, or stronger operational controls requires separate decisions.

## Official references checked

- [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy) — .NET 10 is an active LTS release.
- [ASP.NET Core controller APIs](https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-10.0) — controller, validation, routing, and Problem Details behaviour.
- [EF Core SQL Server provider](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/) — maintained SQL Server integration.
- [SQL Server Linux containers](https://learn.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-ver17) and [Linux requirements](https://learn.microsoft.com/en-us/sql/linux/install-upgrade/setup) — SQL Server 2022 container path, Developer default, memory, and x64 constraints.
- [Node.js releases](https://nodejs.org/en/about/previous-releases) — Node 24 is LTS.
- [React with TypeScript](https://react.dev/learn/typescript) and [Vite guide](https://vite.dev/guide/) — selected lightweight frontend toolchain and supported Node baseline.
- [GitHub Actions settings](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/enabling-features-for-your-repository/managing-github-actions-settings-for-a-repository) — full-length action SHA enforcement.
