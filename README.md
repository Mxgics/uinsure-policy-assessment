# Uinsure policy assessment

A C# REST API and React demonstration for selling, retrieving, cancelling, and renewing home-insurance policies. It supports Household and Buy to Let cover, one to three policyholders, cancellation quotes and refunds, and automatic/manual renewal payment rules.

The backend uses ASP.NET Core 10, EF Core 10 and SQL Server 2022. The frontend uses React, TypeScript and Vite. Domain tests, real SQL integration tests, and browser tests cover business boundaries, history, rollback and concurrent lifecycle changes.

## Quick start

Prerequisites: Git, .NET SDK `10.0.400`, Node `24.19.0`, PowerShell 7, and a running x64 Linux-container Docker engine with at least 2 GB available for SQL Server Developer. The container requires acceptance of its development/testing licence.

Clone the repository and enter its root:

```powershell
git clone https://github.com/Mxgics/uinsure-policy-assessment.git
Set-Location uinsure-policy-assessment
```

Until the final PR is merged, run `git switch codex/demo-usability` after cloning to review this candidate. `main` contains stages 1–8.

In the first terminal, start SQL Server, explicitly apply migrations, prepare six synthetic demo scenarios, and run the API:

```powershell
pwsh -File scripts/Start-Local.ps1 -SeedDemo
```

For an ordinary start that preserves existing demo history, omit `-SeedDemo`. Seeding resets only the six documented demo references; policies you create yourself are preserved. Ordinary API startup does not apply migrations; the script performs that step explicitly.

In a second terminal, from the repository root:

```powershell
Set-Location web
npm ci
npm run dev
```

Open [the policy desk](http://127.0.0.1:5173). Sell a policy or expand **Try a prepared policy** to demonstrate renewal and cancellation immediately. The API runs at `http://127.0.0.1:5080`, with [OpenAPI JSON](http://127.0.0.1:5080/openapi/v1.json) and a [health endpoint](http://127.0.0.1:5080/health).

Stop the UI and API with Ctrl+C, then run `pwsh -File scripts/Stop-Local.ps1` from the root to stop SQL while retaining its data. The [local runbook](docs/runbooks/local-development.md) covers Linux commands, troubleshooting, database reset and HTTP examples.

## Verification

From the repository root, with Docker running:

```powershell
pwsh -File scripts/Test-Local.ps1
Push-Location web
npm ci
npm run build
npm test
npx playwright install chromium
npm run test:e2e
Pop-Location
pwsh -File scripts/Test-FullStack.ps1
```

The backend script performs locked restore, formatting verification, Release build, domain tests and real SQL Server integration tests. Frontend checks include component tests and intercepted browser scenarios. The separate full-stack suite runs the real browser, API and disposable SQL database, then verifies persisted results. GitHub CI runs all three layers.

See [current test evidence](docs/test-plan.md#current-candidate-summary) for recorded results and [the manual testing plan](docs/manual-testing-plan.md) for repeatable review scenarios.

## Review guide

- [Reviewer walkthrough](docs/walkthrough.md): demonstrate the journeys and navigate the implementation.
- [Requirements and evidence](docs/requirements.md): assessment coverage and verification.
- [Architecture](docs/decisions/001-architecture-and-delivery-boundaries.md): project boundaries, persistence and concurrency decisions.
- [Accepted assumptions](docs/assumptions.md): dates, age, money, cancellation and renewal rules.
- [Readiness and limitations](docs/final-readiness.md): current delivery state and deliberate limits.
- [Change explanations](docs/changes/README.md) and [AI assistance log](docs/ai-use.md): implementation decisions, corrections and verification history.

The [project brief](docs/project-brief.md), [delivery plan](docs/project-plan.md), [current context](docs/project-context.md), and [contribution guide](CONTRIBUTING.md) provide supporting detail.

## Delivery status and boundaries

Stages 1–8 are merged into `main`. Stage 9 on `codex/demo-usability` contains the final demo-usability and submission-documentation changes. The owner confirmed manual testing complete on 28 September 2026. Final PR review, explicit merge authorization and verification of the resulting `main` remain separate delivery gates; see the [submission checklist](docs/review-and-submission-plan.md).

Dates use UTC today. The project explicitly interprets “over 16” as 16+, annual terms end the day before their anniversary, and the inclusive renewal window contains 31 dates. Automatic renewal remains a user-triggered demonstration action. Payments and refunds are recorded locally without provider settlement.

This is an unauthenticated local assessment using synthetic data. Real payments, pricing, claims administration, policy amendments, customer communications, cloud deployment and production security controls are outside scope. No repository licence has been selected; normal copyright rules apply.
