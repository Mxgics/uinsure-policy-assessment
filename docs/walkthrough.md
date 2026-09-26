# Reviewer walkthrough

This path demonstrates the assessment without requiring knowledge of the implementation order. Use synthetic details only.

## Run

1. Follow the [local runbook](runbooks/local-development.md) to start SQL Server, apply migrations, and run the API.
2. From `web/`, run `npm ci` then `npm run dev`; open `http://127.0.0.1:5173`.
3. Keep `/openapi/v1.json` available for the explicit HTTP contract and `/health` for process liveness.

## Demonstrate

1. Sell a Household policy beginning today for GBP 365 with one adult holder and Card payment. Point out the server-generated reference/end date and separate temporal/payment states.
2. Reload it by reference. Explain that holder/property values are term snapshots and the stable policy reference owns ordered history.
3. Request a hypothetical cancellation quote. Explain cooling-off precedence, unused-day calculation, final rounding, and that the quote writes nothing.
4. Confirm cancellation. Explain the fresh calculation for today, optional same-method Refund row, one transaction, shared policy revision, and 409 conflict refresh.
5. For renewal, use the automated/domain examples because a new same-day sale cannot naturally enter a historical renewal window. Explain inclusive end-minus-30 through end, automatic Card/DirectDebit versus manual unpaid renewal, copied snapshots, and claims reset.

## Navigate the implementation

- `src/Uinsure.Domain/Policies`: invariants, calendar/refund calculation, cancellation, and renewal.
- `src/Uinsure.Api/Policies`: explicit request/response contracts and orchestration.
- `src/Uinsure.Api/Persistence`: SQL Server mappings, constraints, and migrations.
- `tests/Uinsure.Domain.Tests`: fast literal boundary examples.
- `tests/Uinsure.Api.IntegrationTests`: real SQL migrations, HTTP behaviour, rollback, and deterministic races.
- `web/src`: typed fetch client and accessible workflow UI; `web/e2e` contains desktop/mobile browser contracts.

## Decisions to understand

- Exact age 16 is accepted; policy dates are UTC calendar dates and the end is inclusive.
- A recorded payment/refund is ledger evidence, not provider settlement.
- The parent Policy row is updated for every lifecycle mutation so independent child inserts compete; named unique constraints remain defence in depth.
- A cancelled successor remains history and is never replaced in this exercise.
- A sale has no idempotency key; clients must not automatically retry a lost mutation response.
- The app is unauthenticated, loopback-only, local assessment software—not a production insurance platform or cloud deployment claim.
