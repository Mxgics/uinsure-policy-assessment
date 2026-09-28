# Requirements and evidence

PRs #1–#7 and delivery-stage 8 GitHub PR #9 are merged. Delivery stage 9 improves demo usability without changing the API wire shape or database schema. The [test plan](test-plan.md) is the single source for executed results; stage 9 local gates are recorded and the owner confirmed manual testing complete on 28 September 2026. Publication CI, final PR review and merge are separate gates.

| ID | Requirement | Planned evidence | Target PR | Status |
| --- | --- | --- | --- | --- |
| M1 | Sell a policy | Persisted policy, initial term, payment, 201/Location, domain/API/SQL tests | 3 | Verified in PR 8: invalid-enum/null-holder/storage-limit no-write regressions and real sale journey |
| M2 | Retrieve a policy | Ordered history, term GET, unknown/mismatched resource tests | 3 | Verified in PR 8: canonical enum round trips and real reload/retrieval |
| S1 | Cancel a policy term | State, calculation, cancellation/refund history, transaction/race tests | 4 | Verified locally |
| S2 | Renew a policy term | Window, successor, history, payment and race tests | 5 | Verified locally |
| C1 | Quote cancellation before execution | Read-only quote using the execution calculator | 4 | Verified in PR 8 with unrelated cancellation rows; term-scoped no-write assertions |
| C2 | Suppress refund when the term has a claim | Zero-refund cancellation and retained history | 4 | Verified locally |
| C3 | Reject cheque for automatic renewal | Sale and renewal validation plus no-write proof; Card/DD acceptance | 5/9 | Verified locally at initial sale and renewal boundaries with no-write proof |
| B1 | Support Household and Buy to Let | Both values round-trip through API and SQL | 3 | Verified locally |
| B2 | Generate a unique policy reference | Generator plus SQL unique constraint | 3 | Verified locally |
| B3 | Start at most 60 days ahead and never in the past | Fixed-clock boundary cases | 3 | Verified locally |
| B4 | One-year policy term | Server-derived dates and leap-day cases | 3 | Verified locally |
| B5 | One to three policyholders | 0/1/3/4-holder cases | 3/9 | Backend boundaries and stage 9 add/remove, payload and retrieved-history UI verified locally |
| B6 | Policyholder age rule | Exact accepted boundary and future/invalid DOB cases | 3/9 | Backend/leap boundaries and stage 9 start-date-relative picker maximum verified locally |
| B7 | Renewal window and no renewal after expiry | End-31/end-30/end/end+1 cases | 5 | Verified locally |
| B8 | Auto-renew controls renewal payment creation | Paid automatic and unpaid manual successor cases | 5 | Verified locally |
| B9 | Full and pro-rata cancellation refunds | Day 1/14/15/final day, leap and rounding cases | 4 | Verified locally |
| B10 | Refund through the original payment method | Card/DD/cheque persistence checks | 4 | PR 8 Card/DirectDebit/Cheque payment/refund persistence verified |
| D1 | Persist required policy, holder, property, payment and refund data | DTO, migration, constraint and retrieval assertions | 3–5 | PR 8 three-line/minimal property round trips, renewal copying and migration preservation verified |
| X1 | C# REST API | ASP.NET Core controller endpoints and HTTP examples | 2–5 | Verified locally |
| X2 | Automated tests | Executed local/CI results with named tests | 2–7 | Stage 9 local suites recorded in the current test summary; exact-head publication CI tracked separately |
| X3 | Informative responses | Consistent validation, 404, 409 and unexpected-error Problem Details | 2–5 | PR 8 no-write 400 regressions, string-enum OpenAPI and UI conflict/refresh cases verified |
| X4 | Public GitHub repository | Public repository and reviewed PR links | 1–7 | Public repository verified; final PR review/merge pending |
| U1 | Small React demonstration | Accessible sell, find/view, quote/cancel and renew browser journeys | 6/9 | Stage 9 scenario shortcuts, three-holder authoring and renewal availability passed component and desktop/mobile browser checks |
| U2 | Real-project PRs, documentation and AI context | Reviewable PRs, aligned docs, concise AI log | 1–7 | Public change explanations and AI log maintained; final PR review pending |

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
