# Local development runbook

## Prerequisites

- .NET SDK `10.0.400` selected by `global.json`.
- Docker Desktop or a Linux-container Docker Engine on x86-64 with at least 2 GB available to SQL Server.
- PowerShell 7 for maintained Windows scripts. Node `24.19.0` is pinned for the frontend.
- Acceptance of the SQL Server Developer container licence for non-production development/testing.

SQL Server is pinned to `2022-CU27-ubuntu-22.04` and its immutable amd64 repo digest. Services bind only to loopback. A password is generated into ignored `.local/sql.env`; it is neither printed nor committed.

## PowerShell 7

```powershell
pwsh -File scripts/Initialize-Local.ps1
pwsh -File scripts/Start-Sql.ps1
pwsh -File scripts/Migrate.ps1
pwsh -File scripts/Start-Local.ps1
```

`Start-Local.ps1` also starts SQL and applies migrations. The API listens at `http://127.0.0.1:5080`; liveness is `/health` and OpenAPI is `/openapi/v1.json`.

In a second terminal, start the React demonstration:

```powershell
Set-Location web
npm ci
npm run dev
```

Open `http://127.0.0.1:5173`. Vite proxies `/api` to the loopback API; there is no broad CORS policy and no policy data is placed in browser storage.

```powershell
pwsh -File scripts/Test-Local.ps1
pwsh -File scripts/Stop-Local.ps1
pwsh -File scripts/Reset-Local.ps1 # destructive; prompts before removing the project volume
```

## Linux direct commands

```bash
mkdir -p .local
test -f .local/sql.env || printf 'MSSQL_SA_PASSWORD=U!%sa1' "$(openssl rand -hex 24)" > .local/sql.env
docker compose --env-file .local/sql.env up -d --wait
set -a; . .local/sql.env; set +a
export ConnectionStrings__Uinsure="Server=127.0.0.1,14333;Database=UinsureAssessment;User Id=sa;Password=${MSSQL_SA_PASSWORD};Encrypt=True;TrustServerCertificate=True"
dotnet tool restore
dotnet ef database update --project src/Uinsure.Api/Uinsure.Api.csproj --startup-project src/Uinsure.Api/Uinsure.Api.csproj
dotnet run --project src/Uinsure.Api/Uinsure.Api.csproj --no-launch-profile --urls http://127.0.0.1:5080
```

Use the restore/format/build/test commands in `.github/workflows/ci.yml` for Linux verification. Frontend verification is `npm ci`, `npm run build`, `npm test`, `npx playwright install chromium`, then `npm run test:e2e` from `web/`.

## Diagnosis and recovery

- `docker info` fails: start a Linux-container engine. Do not switch database providers.
- SQL health times out: verify x86-64 and memory, then inspect `docker compose --env-file .local/sql.env logs sqlserver` without copying credentials into issues.
- A migration lacks its connection string: run the scripts or export `ConnectionStrings__Uinsure` as above.
- Port `14333` is busy: stop the conflict or deliberately update Compose and the documented connection string; never expose SQL on all interfaces.
- Locked restore fails: dependency declarations and committed lockfiles differ; regenerate intentionally and review the full graph/advisories.

Ordinary API startup never migrates automatically. Stop the API, correct configuration/engine health, rerun the explicit migration, and restart. The PR 2 migration is schema-empty by design; later schema migrations document their own compatibility and rollback.

## PR 8 property migration

`AlignPropertyContract` adds optional Address Line 3, makes City nullable, and removes Bedrooms. Stop the application and back up any retained database before using the explicit migration script. Bedroom values are deliberately discarded; all other property/history/financial values are preserved. Existing clients must tolerate nullable City and the removed Bedrooms response member.

Reverse migration is permitted only when `Properties` is empty. SQL error 51002 refuses a populated rollback before altering the schema. Recover the prior application/schema from a pre-upgrade backup when needed; do not fabricate bedroom values. Ordinary startup still does not migrate.

## Real browser-to-SQL verification and historical demo

Install frontend dependencies and Chromium once from `web/` with `npm ci` and `npx playwright install chromium` (Linux CI uses `--with-deps`). From the repository root run:

```powershell
pwsh -File scripts/Test-FullStack.ps1
```

This builds the solution and owns a disposable SQL Server container, explicit migrations, synthetic fixtures, the real API on `127.0.0.1:5081`, Vite on `127.0.0.1:5174`, and Playwright. Existing listeners cause failure; no running developer server or persistent demo database is reused. After browser journeys, fresh EF contexts verify payments, refunds, successor history and policy revisions. The API uses its ordinary system clock. A UTC date change fails the run and requires fresh fixtures.

For a manual historical-policy demonstration:

```powershell
pwsh -File scripts/Test-FullStack.ps1 -Serve
```

Open `http://127.0.0.1:5174` and use the printed synthetic references. Each viewport's fixture set contains paid/manual renewal candidates, day-15 cancellation, claims, and a leap-year example. Ctrl+C stops the owned processes and disposes the database. Synthetic manifests, receipts and redacted process logs remain in ignored `TestResults/fullstack-*`; browser traces/screenshots use ignored `web/test-results`. Connection credentials are not written into the fixture manifest or frontend configuration.

The real suite is separate from `npm run test:e2e`, whose intercepted responses remain useful for deterministic UI failures. Both are required by CI. If Docker startup fails, restore a working Linux engine and rerun; a different database or a skipped suite is not a substitute.
