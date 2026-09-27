# Change 002: API, SQL, test, CI, and local foundation

- Date: 2026-09-26
- Scope: stacked PR 2; no policy behaviour

## Change and rationale

The project now has the agreed .NET controller host, domain dependency boundary, EF Core SQL Server provider, explicit migrations, consistent Problem Details, OpenAPI, liveness, real SQL integration testing, pinned dependencies/image/actions, and local/CI commands.

Database configuration is required only when a context is resolved; process liveness does not claim database readiness. Migrations are explicit and never run during ordinary startup. The schema-empty initial migration proves the migration/history path before policy schema arrives.

## Verification and corrections

Executed evidence is in `../test-plan.md`, including the maintained Compose/migrate/run smoke check and dependency audit. The portable test logger replaced a Windows Event Log provider that masked responses. Docker's verified amd64 repo digest replaced a non-pullable manifest digest. A restricted-session build-server issue is avoided by serialized local builds; Ubuntu CI retains a normal build for independent evidence.

## Limitations

No policy entities/endpoints or UI exist in this PR. The initial migration proves tooling, SQL connectivity, migration history, and isolation—not application persistence. CI remains pending until push.
