# Change 004: cancellation quotes and execution

- Date: 2026-09-26
- Scope: stacked PR 4, based on PR 3

## Change and rationale

The API now quotes a hypothetical cancellation date without writing, and executes cancellation using the captured UTC date. One domain calculator owns precedence, day counts, rounding, claims/no-payment suppression, and original-method refunds. Execution persists cancellation history and an optional positive refund in the same EF transaction, increments the shared policy mutation revision, and returns the applied calculation.

Cancellation and refund uniqueness are enforced in SQL. A policy rowversion makes competing lifecycle requests share a mutation boundary; the one expected cancellation uniqueness collision is mapped to 409 while unrelated SQL errors remain 500.

## Test-first evidence and corrections

Literal calculator tests were written first and failed to compile because the calculator contract did not exist. The initial implementation then exposed an incorrect test expectation: a day-14 cooling-off refund is full, but its informational unused-day count is 352 rather than 365. The expectation was corrected without changing the rule.

SQL-backed tests found that a new cancellation graph with assigned GUIDs was tracked as modified rather than added; explicitly adding the graph corrected the atomic insert. A deterministic two-request barrier demonstrated the actual collision path could be the unique term-cancellation index before the rowversion update, so only that named SQL constraint is also mapped to conflict.

## Verification

The final Release run passed 24 domain tests and 16 API/SQL integration tests. It covers read-only quotes, execution recalculation/history, claims/no-payment zero refunds, repeated cancellation, mismatch/validation responses, a deterministic cancel/cancel race, and a database-triggered later refund failure whose cancellation, refund, and policy revision all roll back.

## Limitations

Successor blocking and mixed cancel/renew races arrive with renewal in PR 5. Recorded refunds are assessment ledger entries, not payment-provider transfers.
