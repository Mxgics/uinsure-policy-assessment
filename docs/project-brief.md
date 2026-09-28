# Project brief

## Outcome

Build a small, complete home-insurance application that is easy to run, review, and explain in an interview. The backend is the primary deliverable: a C# REST API that sells, retrieves, cancels, and renews policies. A small React UI will demonstrate the principal journeys after the backend is complete.

## Users and journeys

The assessment user is a local reviewer using synthetic data. The supported journeys are:

1. sell a Household or Buy to Let policy;
2. retrieve a policy and its ordered term, payment, cancellation, and refund history;
3. calculate a hypothetical cancellation quote and cancel an eligible term;
4. renew an eligible term, with payment creation controlled by auto-renew;
5. demonstrate these journeys in a small accessible browser UI.

## Success criteria

- Every requirement in [the matrix](requirements.md) has a traceable implementation and real evidence by the final PR.
- Business boundaries are deterministic and covered at their important dates, money values, error cases, transaction boundaries, and races.
- Local setup uses the documented .NET, Node, Docker, and SQL Server toolchain and works from a clean clone.
- API responses are informative and consistent, history remains auditable, and lifecycle mutations are atomic under concurrency.
- Documentation distinguishes decisions, assumptions, executed verification, and remaining limitations.
- Each focused change remains independently reviewable; PRs #1–#7 are merged and PR #9 is the remaining owner-review gate.

## In scope

All assessment must/should/could behaviours, both policy types, term history, recorded payments/refunds, real SQL Server migrations, automated domain/API/database/concurrency/UI tests, local tooling, GitHub CI, OpenAPI, Problem Details, and a small React/TypeScript demonstration.

## Out of scope

Authentication, authorization, real customers, real money movement, pricing engines, claim intake/administration, amendments, instalments, brokers, scheduled auto-renewal, notifications, microservices, cloud deployment, Bicep implementation, production observability, and reconstructed Uinsure branding.

## Delivery contract

Use small pull requests into `main`. Backend foundations and behaviour preceded the UI. The historical dependent stack is merged; review the repaired PR #9 against `main` and stop for explicit owner approval. If time or tooling requires a scope reduction, make it explicit rather than quietly dropping requirements or evidence.
