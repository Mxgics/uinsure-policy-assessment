# Accepted assumptions and deliberate simplifications

These decisions make ambiguous assessment wording executable. They are project conventions, not claims about insurance law or Uinsure production systems.

## Property contract clarification - 2026-09-27

The owner approved delivery stage 8 (GitHub PR #9) to add Address Line 3, retain optional City, and remove Bedrooms. Only Address Line 1 and Postcode are required by the property contract. Optional blank lines/City normalize to null. Neither City nor Bedrooms is required by the assessment. The [property decision](decisions/002-property-contract-and-browser-evidence.md) supersedes the earlier optional-Bedrooms proposal and explains migration/compatibility consequences.

## Dates and eligibility

- Capture UTC today once per operation through an injected `TimeProvider`. UTC is an assessment simplification.
- JSON dates use `YYYY-MM-DD`, .NET uses `DateOnly`, and SQL uses `date`.
- A holder qualifies when `DateOfBirth.AddYears(16) <= StartDate`; exact age 16 is accepted despite the source phrase “over 16”. Reject future, invalid, and overflowing dates.
- Coverage uses an exclusive boundary one calendar year after start. The exposed end date is the preceding day. A leap-day start on `2028-02-29` ends `2029-02-27`.
- Renewal is allowed from `EndDate - 30 days` through `EndDate`, inclusive. That is 31 eligible calendar dates.

## Money and data normalisation

- All money is GBP `decimal`/SQL `decimal(18,2)`. Submitted premiums are positive and have at most two meaningful fractional digits; do not silently round input.
- Submitted premium cannot exceed `9999999999999999.99`. Normalized holder names are at most 100 characters, address lines 200, City 100, and Postcode 8; validate before SQL writes.
- Round only the final refund to two places, away from zero. There are no fees, tax, instalments, or interest.
- Trim required names/addresses; preserve meaningful internal content. Trim and uppercase postcodes, allow at most eight characters, and do not invent a restrictive postcode regex or external lookup.
- Required booleans and enums cannot become valid through language defaults. JSON enums accept one declared name after trimming, ignoring case; numeric, unknown and combined names are rejected. Responses use canonical names.

## Policy and term history

- A policy has a stable generated reference and one or more immutable historical term snapshots.
- Claims are term-local. Renewal retains the earlier term’s claim history and resets the new term to no claims. This does not model a real underwriting decision.
- Temporal state (`Scheduled`, `Current`, `Expired`, `Cancelled`) is derived; cancellation wins. Payment state (`Recorded`, `NotRecorded`) is separate and does not claim settlement or cover validity.
- Card, Direct Debit, and cheque are accepted on initial sale. The cheque restriction applies only to automatic renewal.
- An unpaid renewal term is permitted when auto-renew is false. It can later be cancelled for zero refund and renewed if otherwise eligible.

## Cancellation

- A quote is hypothetical, read-only, and may use an eligible supplied date. It neither reserves a result nor authorises a backdated cancellation.
- Execution always takes effect on the captured UTC today and recalculates. It accepts no execution date or client-calculated amount.
- Calculation precedence is: no payment; claim on this term; before start; start through start+13 days; otherwise pro-rata unused days. The cancellation date itself is unused.
- A zero refund still records the cancellation and reason but creates no refund record. A positive refund uses the original payment method.
- A non-cancelled successor blocks cancellation of its parent; cancel the successor first. A cancelled successor remains history.

## Renewal, concurrency, and retries

- Renewal creates a successor on the day after the prior inclusive end, copies snapshots/premium/auto-renew, and resets claims. Repricing and preference changes are out of scope.
- Auto-renew requires Card or Direct Debit and atomically records payment. Manual renewal requires no payment method and creates no payment.
- A term that has ever had a successor cannot be renewed again, even if that successor was cancelled.
- Every lifecycle mutation updates the parent policy revision and SQL `rowversion` so competing child inserts share a concurrency boundary. Expected races/known unique conflicts return 409; unrelated SQL failures do not.
- Sale has no request-idempotency key. A retry after a lost response can create another valid policy; clients must not automatically retry mutations.

## Operational boundary

- Local development and integration tests target SQL Server 2022 Developer in a pinned Linux x64 container. Docker engine, adequate memory, and licence acceptance are prerequisites; failure is explicit and never triggers a database substitution.
- Local services bind to loopback and use synthetic data. No authentication, CORS exposure, cloud deployment, or production suitability is implied.
