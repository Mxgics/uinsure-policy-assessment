# Requirements and planned evidence

Status: sell/retrieve evidence is recorded below; later lifecycle and UI behaviour remains pending. A planned test or document is not proof that behaviour works.

| ID | Requirement | Planned evidence | Target PR | Status |
| --- | --- | --- | --- | --- |
| M1 | Sell a policy | Persisted policy, initial term, payment, 201/Location, domain/API/SQL tests | 3 | Verified locally |
| M2 | Retrieve a policy | Ordered history, term GET, unknown/mismatched resource tests | 3 | Verified locally |
| S1 | Cancel a policy term | State, calculation, cancellation/refund history, transaction/race tests | 4 | Verified locally |
| S2 | Renew a policy term | Window, successor, history, payment and race tests | 5 | Pending |
| C1 | Quote cancellation before execution | Read-only quote using the execution calculator | 4 | Verified locally |
| C2 | Suppress refund when the term has a claim | Zero-refund cancellation and retained history | 4 | Verified locally |
| C3 | Reject cheque for automatic renewal | Validation plus no-write proof; Card/DD acceptance | 5 | Pending |
| B1 | Support Household and Buy to Let | Both values round-trip through API and SQL | 3 | Verified locally |
| B2 | Generate a unique policy reference | Generator plus SQL unique constraint | 3 | Verified locally |
| B3 | Start at most 60 days ahead and never in the past | Fixed-clock boundary cases | 3 | Verified locally |
| B4 | One-year policy term | Server-derived dates and leap-day cases | 3 | Verified locally |
| B5 | One to three policyholders | 0/1/3/4-holder cases | 3 | Verified locally |
| B6 | Policyholder age rule | Exact accepted boundary and future/invalid DOB cases | 3 | Verified locally |
| B7 | Renewal window and no renewal after expiry | End-31/end-30/end/end+1 cases | 5 | Pending |
| B8 | Auto-renew controls renewal payment creation | Paid automatic and unpaid manual successor cases | 5 | Pending |
| B9 | Full and pro-rata cancellation refunds | Day 1/14/15/final day, leap and rounding cases | 4 | Verified locally |
| B10 | Refund through the original payment method | Card/DD/cheque persistence checks | 4 | Verified locally |
| D1 | Persist required policy, holder, property, payment and refund data | DTO, migration, constraint and retrieval assertions | 3–5 | Verified locally |
| X1 | C# REST API | ASP.NET Core controller endpoints and HTTP examples | 2–5 | Partial: sell/retrieve verified |
| X2 | Automated tests | Executed local/CI results with named tests | 2–7 | In progress: local backend suites pass |
| X3 | Informative responses | Consistent validation, 404, 409 and unexpected-error Problem Details | 2–5 | Partial: validation/404/409/500 verified |
| X4 | Public GitHub repository | Public repository and reviewed PR links | 1–7 | In progress |
| U1 | Small React demonstration | Accessible sell, find/view, quote/cancel and renew browser journeys | 6 | Pending |
| U2 | Real-project PRs, documentation and AI context | Reviewable PRs, aligned docs, concise AI log | 1–7 | In progress |

## Acceptance examples

Use literal expected results independent of production calculations.

- With today `2026-10-01`, accept starts `2026-10-01` and `2026-11-30`; reject `2026-09-30` and `2026-12-01`.
- A holder born `2010-10-01` qualifies for a `2026-10-01` start under the accepted 16+ interpretation; `2010-10-02` does not.
- A `2028-02-29` term ends `2029-02-27` inclusive; its successor starts `2029-02-28`.
- For GBP 365 paid on `2026-10-01`–`2027-09-30`, refund GBP 365 before start and through `2026-10-14`, GBP 351 on `2026-10-15`, GBP 1 on `2027-09-30`, and reject `2027-10-01`.
- Claims or no recorded payment produce a zero refund while cancellation is still recorded. A calculated zero creates no refund row.
- For an end date of `2027-09-30`, reject renewal on `2027-08-30`, accept `2027-08-31` through `2027-09-30`, and reject `2027-10-01`.
- Concurrent cancel/cancel, renew/renew, and cancel/renew permit only one winning lifecycle effect and leave no partial financial records.

Detailed business interpretations are in [accepted assumptions](assumptions.md); delivery sequencing is in [the project plan](project-plan.md).
