# Change 003: sell and retrieve policies

- Date: 2026-09-26
- Scope: stacked PR 3, based on PR 2

## Change and rationale

The API now sells Household and Buy to Let policies, persists an initial one-year term and recorded payment atomically, and retrieves a policy or an individual term. Explicit request/response contracts prevent callers from setting references, end dates, or payment amounts. The domain owns calendar, holder-count, age, premium, and normalisation rules; SQL constraints and unique indexes reinforce persistence invariants.

Terms are immutable snapshots of policyholders and the insured property. Responses expose temporal state separately from recorded-payment state so a later unpaid renewal is not misrepresented as paid.

## Verification and corrections

The Release suite passed 14 domain tests and 10 API/SQL integration tests against SQL Server in Docker. The first SQL-backed HTTP run showed that replacing EF registrations did not override the application's deferred configuration callback; the test host now supplies the connection string through configuration. A later assertion incorrectly assumed a relative `Location` header and was corrected to verify the absolute URI path. Persistence assertions use returned identifiers rather than test execution order.

Business tests were added with this feature, but their chronology was not recorded as red/green evidence. Genuine test-first lifecycle work begins with PR 4; this note deliberately does not overclaim TDD.

## Limitations

Cancellation, renewal, lifecycle conflicts, and the frontend remain in later stacked PRs. Payment rows record the assessment transaction; they do not collect or settle money.
