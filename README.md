# Uinsure policy assessment

A small home-insurance policy API and React demonstration built as a technical assessment. The project will support selling, retrieving, cancelling, and renewing Household and Buy to Let policies, with the optional cancellation rules and a focused browser UI.

This repository is delivered through small stacked pull requests. PR 1 established the product and architecture; PR 2 adds the executable API/SQL/test/CI foundation. Dependent branches add behaviour and UI as separate review diffs.

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

The backend foundation builds and its HTTP/real-SQL checks pass locally. Policy lifecycle behaviour and the frontend remain on later stacked branches. See the [local runbook](docs/runbooks/local-development.md) and [test evidence](docs/test-plan.md).

## Deliberate boundaries

This is an unauthenticated local assessment application using synthetic data. It does not implement real payments, pricing, claims administration, policy amendments, customer communications, cloud deployment, or production security controls. See the [project brief](docs/project-brief.md) and [assumptions](docs/assumptions.md) for the complete boundary.

No licence has yet been selected. Until one is added, normal copyright rules apply.
