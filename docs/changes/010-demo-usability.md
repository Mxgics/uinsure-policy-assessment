# Delivery stage 9: demo data and workflow usability

## Problem and cause

The backend supported one to three holders and both renewal modes, but the browser form captured only one holder, did not constrain birth dates to the accepted age rule, and hid `AutoRenew` after creation. New policies could not naturally demonstrate renewal because their end dates were nearly a year away. The accepted assumption also allowed an automatic-renewal policy to start with Cheque even though automatic renewal can record only Card or Direct Debit.

## Change and rationale

- Added opt-in `Start-Local.ps1 -SeedDemo` seeding for six stable, date-relative automatic/manual renewal and refund/claims cancellation scenarios. Reseeding replaces only those exact synthetic references in one transaction and preserves user-created policies.
- Added one-to-three-holder authoring and retrieved holder history, dynamic 16+ birth-date limits, explicit automatic/manual choices, initial automatic/Cheque rejection, payment-method visibility and lifecycle availability explanations.
- Kept renewal user-triggered and used the existing GET-by-reference API for scenario shortcuts. No seed endpoint, production clock override, schema migration or policy response change was added.

## Red/green/refactor record

- Red: the new domain test accepted automatic renewal with Cheque instead of throwing validation.
- Green: one cross-field domain guard made the focused test pass; the API regression then proved HTTP 400 and no SQL write.
- The first seeder mutation test used direct aggregate mutation and encountered EF concurrency behaviour outside the application orchestration path. The test was corrected to exercise renewal/cancellation through the real API, then proved repeatable SQL reseeding and preservation of a non-demo policy.
- UI changes were covered by component and desktop/mobile browser regressions; the first layout review found the always-expanded mobile scenario grid too long, so it was refactored into a disclosure that collapses after selection.

## Verification

The maintained backend gate passed formatting, a zero-warning Release build, 40 domain tests and 75 real SQL integration tests, including failed-seed rollback. The frontend production build, 23 component/API-boundary tests, 12 intercepted desktop/mobile Chromium checks and eight real browser/API/SQL journeys passed. Rendered desktop/mobile captures were inspected; the compact disclosure and policy actions had no observed clipping or horizontal overflow. Exact commands and corrections are in the [test plan](../test-plan.md), and the [manual plan](../manual-testing-plan.md) covers the owner walkthrough.

## Limits

Demo records are local synthetic data and are created only by the explicit flag. Auto-renew remains an explicit demonstration action, not scheduled processing. A prepared-policy shortcut returns 404 when demo data has not been seeded.

## Final submission preparation — 2026-09-28

Reworked the README around a reproducible reviewer startup, verification commands and a short review guide. Aligned current delivery status, manual completion, requirements, assumptions, readiness and contribution guidance; fixed the PR-index table and corrected impossible or overstated manual checks. Earlier execution records remain historical.

Source review found mocked UI fixtures expiring on 12 October 2026 while their tests used the host clock. A diagnostic run at 1 January 2027 reproduced seven failures (16 tests still passed). Pinning only Date in the component setup and fixing browser time before navigation keeps the historical contracts deterministic without changing application time or real full-stack fixtures. The production build, all 23 frontend tests and 12 browser-contract checks passed afterward. This is a test-harness correction, not new business behaviour.

The owner confirmed manual testing complete and satisfactory. Final repository audit results are in the test plan; exact-head publication CI belongs in the final PR. No merge or external submission is authorized.
