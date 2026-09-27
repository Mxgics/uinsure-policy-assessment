# Uinsure policy assessment

A small home-insurance policy API and React demonstration built as a technical assessment. It supports selling, retrieving, cancelling, and renewing Household and Buy to Let policies, with the optional cancellation rules and a focused browser UI.

This repository is delivered through small stacked pull requests. PR 1 is merged; PRs 2–7 contain the API, SQL persistence, lifecycle behaviour, UI, and verification documentation and remain open for review.

## Run locally

Prerequisites: .NET SDK `10.0.400`, Node `24.19.0`, PowerShell 7, and an x64 Linux-container Docker engine with at least 2 GB available for SQL Server Developer.

From the repository root:

```powershell
pwsh -File scripts/Start-Local.ps1
```

This starts SQL, explicitly applies migrations, and runs the API at `http://127.0.0.1:5080`. In a second terminal:

```powershell
Set-Location web
npm ci
npm run dev
```

Open `http://127.0.0.1:5173`. Run backend verification from the repository root with `pwsh -File scripts/Test-Local.ps1`. See the [runbook](docs/runbooks/local-development.md) for frontend checks, Linux commands, prerequisites, and shutdown.

Key assumptions: dates use UTC today, exactly age 16 qualifies, annual terms end the day before their anniversary, and the inclusive renewal window contains 31 dates. Payments are recorded locally without settlement. The [assumptions](docs/assumptions.md) explain refund conventions and the reviewed property contract.

## Start here

- [Project brief](docs/project-brief.md)
- [Requirements and planned evidence](docs/requirements.md)
- [Accepted assumptions](docs/assumptions.md)
- [Architecture decision](docs/decisions/001-architecture-and-delivery-boundaries.md)
- [Current context](docs/project-context.md)
- [Delivery plan](docs/project-plan.md)
- [Contribution and PR workflow](CONTRIBUTING.md)
- [AI assistance log](docs/ai-use.md)
- [Explanations for every PR](docs/changes/README.md)
- [Reviewer walkthrough](docs/walkthrough.md)
- [Final readiness and limits](docs/final-readiness.md)

## Current status

The complete backend lifecycle and React demonstration build and pass their local automated checks. See the [local runbook](docs/runbooks/local-development.md) and [test evidence](docs/test-plan.md).

The [2026-09-27 review](docs/changes/review-checkpoint-2026-09-27.md) identified nine findings. [PR 8 corrections](docs/changes/008-review-fixes.md) implement their fixes, remove Bedrooms, add Address Line 3, retain optional City, and add real browser/API/SQL verification. Local checks pass; owner review and merge remain pending.

## Deliberate boundaries

This is an unauthenticated local assessment application using synthetic data. It does not implement real payments, pricing, claims administration, policy amendments, customer communications, cloud deployment, or production security controls. See the [project brief](docs/project-brief.md) and [assumptions](docs/assumptions.md) for the complete boundary.

No licence has yet been selected. Until one is added, normal copyright rules apply.
