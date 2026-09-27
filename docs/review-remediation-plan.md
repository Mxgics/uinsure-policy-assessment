# PR 8 implementation plan

- Approved: 2026-09-27.
- Base: PR 7 branch `codex/pr7-final-readiness`, reviewed application commit `806ecab`.
- Branch: `codex/pr8-review-fixes`.
- Current execution evidence: [PR 8 change note](changes/008-review-fixes.md) and [test plan](test-plan.md).

## Approved decisions

Deliver one corrective PR on top of the existing stack. This supersedes the earlier proposal for four correction PRs. Preserve the existing documentation changes, use focused commits, stop at owner review, and merge nothing without explicit instruction.

The owner chose to remove Bedrooms, retain optional City, and add optional Address Line 3. Address Line 1 and Postcode remain required. The owner also chose real browser-to-SQL verification rather than deferring it. These are accepted choices, replacing the earlier optional-Bedrooms planning default.

Keep routes, lifecycle rules, canonical enum names, framework/dependency versions, SQL Server, and the one-save transaction boundary. No authentication, settlement, deployment, sale idempotency, or architectural expansion.

## Corrections

1. R1/R3/R4: reject combined/numeric/unknown enums before persistence, add domain guards, validate null holders by index, and apply normalized storage-aligned text and decimal bounds. Preserve string-enum OpenAPI schemas.
2. R2: propagate all address lines and optional City through requests, responses, snapshots, renewal, EF and UI; remove Bedrooms. Add a new migration preserving retained data. Rollback only on an empty property table; populated databases require a pre-upgrade backup because discarded Bedrooms values cannot be recreated.
3. R5/R6: preserve conflict details and confirmed mutation results independently from GET refresh. Block stale lifecycle mutations, provide GET-only recovery, prevent duplicate POSTs and selection changes while pending, and identify uncertain transport outcomes.
4. R7/R9: native modal dialog, safe initial focus, keyboard boundary wrapping, inert background and focus restoration; destructive text contrast at least 4.5:1.
5. R8: arrange unrelated prior cancellations and scope quote/refund assertions to the relevant policy/term/payment.

## Verification

Record actual failing regressions before fixes, the smallest passing implementation, refactor/recheck, and limitations. Newly added passing coverage is not retroactive TDD.

Cover exact/overflowing field limits and premium bounds, null/missing holders, invalid dates, canonical enums, accepted three-holder policies, persisted Card/DirectDebit/Cheque refunds, Address Line 3 retrieval/renewal, minimal properties, populated migration preservation and rollback refusal. Preserve existing deterministic races and later-write rollback tests.

Keep mocked component/browser tests for precise error and accessibility checks. Add a test-only console runner owning disposable SQL, migration, domain-based historical seeding, actual API/Vite processes, Playwright and fresh SQL assertions. Use dedicated ports, no server reuse, no production test endpoints or clock changes, and fail on UTC rollover. Interactive mode provides a disposable historical renewal demo.

Final gate: maintained backend script; frontend build/component/browser checks; real full-stack journeys; desktop/mobile visual inspection; clean checkout; CI at the actual candidate head; Markdown/privacy/whitespace checks.

## Evidence and review

Update requirements, assumptions, ADR, runbook, walkthrough, readiness report, AI log, PR 8 note and the separate private interview ledger with actual results. Preserve prior historical entries and their evidence limitations. Any failed or blocked check remains explicit until it is executed successfully. Review and merge the existing stack bottom-up only after owner approval.
