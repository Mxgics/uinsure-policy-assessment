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
