# Uinsure policy assessment

A small home-insurance policy API and React demonstration built as a technical assessment. It supports selling, retrieving, cancelling, and renewing Household and Buy to Let policies, with the optional cancellation rules and a focused browser UI.

PRs #1–#7 are merged into `main`. Delivery stage 8 is the open [GitHub PR #9](https://github.com/Mxgics/uinsure-policy-assessment/pull/9), repaired to target `main` and containing the review corrections, coherence fixes and real browser-to-SQL verification. It remains at the owner-review gate; nothing has been merged or submitted for the owner.

## Run locally

Prerequisites: .NET SDK `10.0.400`, Node `24.19.0`, PowerShell 7, and an x64 Linux-container Docker engine with at least 2 GB available for SQL Server Developer.

From the repository root:

```powershell
pwsh -File scripts/Start-Local.ps1
```

This starts SQL, explicitly applies migrations, and runs the API at `http://127.0.0.1:5080`. Ordinary API startup does not apply migrations. In a second terminal:

```powershell
Set-Location web
npm ci
npm run dev
```

Open `http://127.0.0.1:5173`. Run backend verification from the repository root with `pwsh -File scripts/Test-Local.ps1`. See the [runbook](docs/runbooks/local-development.md) for frontend checks, Linux commands, prerequisites, and shutdown.

Key assumptions: dates use UTC today; the brief says “over 16” but this project explicitly interprets that as 16+; annual terms end the day before their anniversary; and the inclusive renewal window contains 31 dates. Payments and refunds are recorded locally without provider settlement. The [assumptions](docs/assumptions.md) explain the conventions and property contract.

## Start here

- [Project brief](docs/project-brief.md)
- [Requirements and evidence](docs/requirements.md)
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

The complete backend lifecycle and React demonstration are implemented. Historical checkpoints and the current candidate are distinguished in the single [test evidence record](docs/test-plan.md). The [baseline review](docs/changes/review-checkpoint-2026-09-27.md), [delivery-stage 8 corrections](docs/changes/008-review-fixes.md), and [coherence findings](docs/changes/009-coherence-review.md) record what changed and what remains. Final clean-checkout gates and current-head CI must pass before owner review concludes.

## Deliberate boundaries

This is an unauthenticated local assessment application using synthetic data. It does not implement real payments, pricing, claims administration, policy amendments, customer communications, cloud deployment, or production security controls. See the [project brief](docs/project-brief.md) and [assumptions](docs/assumptions.md) for the complete boundary.

No licence has yet been selected. Until one is added, normal copyright rules apply.
