# Manual testing plan

This plan validates the assessment through the browser against the real local API and SQL Server. Use synthetic people and addresses only. Run it from a freshly reset database so policy history and refund results are unambiguous.

The owner confirmed manual testing complete on 28 September 2026. This is an owner-reported completion, not a reconstructed case-by-case execution log. The procedures below remain available for reviewers; automated results are recorded separately in the [test evidence](test-plan.md).

## Fresh environment

From the repository root, stop any running API/UI terminals, remove only this project's containers and SQL volume, then start and seed a new database:

```powershell
pwsh -File scripts/Reset-Local.ps1
pwsh -File scripts/Start-Local.ps1 -SeedDemo
```

`Reset-Local.ps1` asks before deleting `uinsure-policy-assessment` containers and `uinsure-policy-assessment-sql-data`. `Start-Local.ps1` creates SQL, applies migrations, transactionally inserts the six synthetic scenarios, and holds the API at `http://127.0.0.1:5080`.

In a second terminal:

```powershell
Set-Location web
npm ci
npm run dev
```

Open `http://127.0.0.1:5173`. Confirm `http://127.0.0.1:5080/health` responds before testing. Keep a simple evidence log containing case ID, result, created policy reference, observed amounts/statuses, browser/viewport, and a screenshot for any failure.

## Prepared scenarios

Expand **Try a prepared policy** in the UI and load these through the ordinary retrieval endpoint:

| Reference | Purpose | Expected starting facts |
| --- | --- | --- |
| `POL-DEMO-AUTO-HH` | Automatic Household renewal | Renewal window open; Card payment; one holder |
| `POL-DEMO-AUTO-BTL` | Automatic Buy to Let renewal | Renewal window open; Direct Debit payment; three holders |
| `POL-DEMO-MANUAL-HH` | Manual Household renewal | Renewal window open; Cheque payment; two holders |
| `POL-DEMO-MANUAL-BTL` | Manual Buy to Let renewal | Renewal window open; Card payment; one holder |
| `POL-DEMO-CANCEL-REFUND` | Pro-rata cancellation | Active paid term; no claims; two holders |
| `POL-DEMO-CANCEL-CLAIMS` | Claims cancellation | Active paid term; claims present; three holders; no refund expected |

Each destructive scenario is single-use. Rerun the seeded startup to reset only these six references; policies sold manually are preserved. Use a full reset when a completely empty database is required.

## Core browser journeys

### MT-01 — Sell and retrieve both policy types

Run once for Household/Card and once for Buy to Let/Direct Debit.

1. Select a start date from today through today plus 60 days and enter a valid premium/property.
2. Add one holder in the first run and three in the second. Confirm a fourth cannot be added and the last holder cannot be removed.
3. Choose Manual renewal for one sale and Automatic for the other.
4. Submit, record the generated reference, then retrieve it again by reference.

Expected: sale succeeds, the reference is non-empty and unique, the term end is the day before the first anniversary, all holders/property values round-trip, payment shows the selected method/state, and renewal mode is prominent.

### MT-02 — Date and holder boundaries

1. Confirm the start picker rejects yesterday and a date 61 days ahead, while today and day 60 are selectable.
2. For the chosen start date, use the latest allowed birth date: exact age 16 must be accepted. One day younger must be blocked by the picker and rejected by the API if manually forced.
3. Exercise a 29 February birth date with a non-leap 16th-anniversary year; the holder becomes eligible on 28 February; a 29 February birth date remains selectable for that start date.
4. Confirm empty required holder fields and invalid dates show validation without creating a policy.

Expected: the browser limits agree with server validation, failed submissions retain useful input, and no policy reference is returned for rejected sales.

### MT-03 — Renewal-mode/payment consistency

1. Select Automatic renewal. Confirm Card and Direct Debit remain available, Cheque is disabled, and explanatory copy states that renewal still requires an explicit action.
2. Switch to Manual, select Cheque, then switch back to Automatic. Confirm the method returns to a valid automatic option.
3. Submit Automatic plus Cheque directly through the API/OpenAPI client to bypass the UI.

Expected: the bypass attempt returns HTTP 400 validation and creates no policy. Initial Cheque payment remains valid for a Manual policy.

### MT-04 — Automatic renewals

1. Load `POL-DEMO-AUTO-HH`; confirm the renewal window is open and press **Record automatic renewal**.
2. Repeat with `POL-DEMO-AUTO-BTL`.
3. Retrieve each policy again.

Expected: each explicit action creates exactly one successor term and a Card/Direct Debit payment respectively; copied property and all holders are visible; the original cannot be renewed again or cancelled; the reason is displayed. Automatic mode never renews merely by loading or waiting.

### MT-05 — Manual renewals

1. Load `POL-DEMO-MANUAL-HH` and press **Create unpaid renewal**.
2. Repeat with `POL-DEMO-MANUAL-BTL`, then retrieve each reference again.

Expected: one successor is created with no payment/unpaid state, copied property and holders, and no repeated-renewal action. Cheque on the original manual policy does not cause a renewal payment.

### MT-06 — Pro-rata cancellation and quote

1. Load `POL-DEMO-CANCEL-REFUND` and record the paid amount/method.
2. Request the cancellation quote twice without confirming.
3. Confirm cancellation, then retrieve the policy again.

Expected: repeated quotes do not change history; the day-15-or-later result is ProRata and between zero and the original payment; the refund method equals the original payment method; cancellation date, used/unused days and retained amount are consistent; repeat cancellation/renewal is unavailable with a reason.

### MT-07 — Claims cancellation

1. Load `POL-DEMO-CANCEL-CLAIMS`, quote, and confirm cancellation.
2. Retrieve it again.

Expected: cancellation succeeds, reason is HasClaims, refund is zero, and no Refund row/value is presented as paid despite the original Cheque payment.

## Additional cancellation boundaries

These dates cannot all be reached from ordinary same-day UI actions, so verify the visible same-day cases manually and use the automated domain/API evidence for the historical boundaries.

| Case | Setup/action | Expected |
| --- | --- | --- |
| MT-08 | Sell a future-start paid policy, then quote/cancel today | BeforeStart; full refund to original method |
| MT-09 | Sell a today-start paid policy, then quote/cancel today | CoolingOff; full refund to original method |
| MT-10 | Existing term at start plus 13 days | CoolingOff; full refund |
| MT-11 | Existing term at start plus 14 days | ProRata; rounded once to two decimals |
| MT-12 | Cancel unpaid manual-renewal successor | NoPayment; zero refund and no refund method |

## Negative, lifecycle, and recovery checks

| Case | Action | Expected |
| --- | --- | --- |
| MT-13 | Retrieve an unknown reference | Clear not-found state; no mutation |
| MT-14 | Attempt renewal before end minus 30 or after end | Disabled/explained in UI; API conflict if bypassed |
| MT-15 | Attempt renewal twice | One successor only; repeat blocked |
| MT-16 | Attempt cancellation on cancelled/expired term | Disabled/explained; API conflict if bypassed |
| MT-17 | Attempt cancellation when an active successor exists | Blocked with visible reason |
| MT-18 | Refresh/retrieve after a successful mutation | Complete ordered term/payment/cancellation/refund history remains visible |
| MT-19 | Lose a mutation response or simulate HTTP 500 in intercepted browser tests | No automatic POST retry; lifecycle uncertainty requires GET recovery. An uncertain sale warns against resubmission because server idempotency is not implemented |

Policy references are generated by the server; there is no client-supplied reference field to exercise in the browser. The database mapping and migration define a unique reference index. A forced duplicate-reference test is not claimed here.

## Accessibility and responsive pass

Run the sell/retrieve flow with keyboard only at desktop width and approximately 390 px mobile width. Verify visible focus, logical tab order, native disclosure/radio/date controls, accessible add/remove names, associated input labels and announced error summary (server field errors are not individually linked to inputs), no horizontal scrolling, readable renewal/payment badges, and disabled-action explanations that do not rely on colour alone. Repeat a three-holder sale and one prepared renewal on mobile.

## Restart and persistence check

After several manual actions:

1. Stop the UI with Ctrl+C and the API with Ctrl+C.
2. Run `pwsh -File scripts/Stop-Local.ps1`; this stops containers but retains SQL data.
3. Run `pwsh -File scripts/Start-Local.ps1` without `-SeedDemo`, restart `npm run dev`, and retrieve the references created/changed above.

Expected: all user-created and mutated policy history survives the full process/container restart. No demo reset occurs during ordinary startup. Finally, run `Start-Local.ps1 -SeedDemo` and verify only the six prepared references return to their original one-term states while manually sold references remain intact.

## Completion record

The manual pass is complete when MT-01–MT-19 and the accessibility/responsive pass are recorded, both insurance types and all three payment methods have been observed, the six prepared scenarios behave as documented, restart persistence passes, and every failure has a reproducible reference/date/request. Automated boundary and SQL evidence should be run separately with:

```powershell
pwsh -File scripts/Test-Local.ps1
Set-Location web
npm run build
npm test
npm run test:e2e
Set-Location ..
pwsh -File scripts/Test-FullStack.ps1
```
