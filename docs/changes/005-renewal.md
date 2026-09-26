# Change 005: renewal and aggregate lifecycle concurrency

- Date: 2026-09-26
- Scope: stacked PR 5, based on PR 4

## Change and rationale

The API now renews an eligible term from inclusive end-minus-30 through end date. A successor retains policy/type, premium, auto-renew preference, holder/property snapshots, and the policy reference; it starts after the prior inclusive end, receives a new identifier, and resets claims. Automatic renewal requires Card or DirectDebit and records a payment; manual renewal rejects a supplied method and remains explicitly unpaid.

Predecessor identity is stored with the policy identity and backed by a composite foreign key, preventing a cross-policy predecessor. A filtered unique predecessor index prevents duplicate successors. A term that ever had a successor cannot be renewed again. A non-cancelled successor blocks cancellation of its parent; cancelling the successor first permits parent cancellation without erasing history.

## Test-first evidence and corrections

Renewal domain tests were written first and failed because `Policy.Renew` and predecessor data did not exist. The implementation made those literal window/payment/history examples green without changing their expectations.

The deterministic save barrier introduced for cancellation was generalized to pause either new cancellations or successor terms at the shared policy mutation boundary. This proves renew/renew and cancel/renew races with separate request scopes and SQL connections.

## Verification

The final Release run passed 34 domain tests and 26 API/SQL integration tests. It verifies automatic Card/DirectDebit and manual unpaid renewal, invalid combinations without writes, repeated renewal, cancelled-successor history, unpaid successor cancellation, deterministic renew/renew and cancel/renew races, and rollback when the later payment insert fails.

## Limitations

Renewal is explicitly consumer-invoked; there is no background scheduler, repricing, payment collection, amendment, or replacement of a cancelled successor.
