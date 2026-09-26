# Uinsure policy assessment

A small home-insurance policy API and React demonstration built as a technical assessment. The project will support selling, retrieving, cancelling, and renewing Household and Buy to Let policies, with the optional cancellation rules and a focused browser UI.

This repository is being delivered through small, reviewed pull requests. PR 1 establishes the agreed product, architecture, assumptions, and delivery conventions; it contains no application implementation or passing-test claims.

## Start here

- [Project brief](docs/project-brief.md)
- [Requirements and planned evidence](docs/requirements.md)
- [Accepted assumptions](docs/assumptions.md)
- [Architecture decision](docs/decisions/001-architecture-and-delivery-boundaries.md)
- [Current context](docs/project-context.md)
- [Delivery plan](docs/project-plan.md)
- [Contribution and PR workflow](CONTRIBUTING.md)
- [AI assistance log](docs/ai-use.md)

## Current status

Documentation foundation only. The API, database, tests, CI, local tooling, and frontend are planned for later reviewed PRs. Docker CLI is present in the development environment, but SQL Server container execution has not been verified because the Docker Linux engine was not running when PR 1 was prepared.

## Deliberate boundaries

This is an unauthenticated local assessment application using synthetic data. It does not implement real payments, pricing, claims administration, policy amendments, customer communications, cloud deployment, or production security controls. See the [project brief](docs/project-brief.md) and [assumptions](docs/assumptions.md) for the complete boundary.

No licence has yet been selected. Until one is added, normal copyright rules apply.
