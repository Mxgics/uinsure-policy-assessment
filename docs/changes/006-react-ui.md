# Change 006: React policy desk

- Date: 2026-09-26
- Scope: stacked PR 6, based on PR 5

## Change and rationale

The local React/TypeScript/Vite demonstration now covers sell, find/history, hypothetical cancellation quote, confirmed cancellation, and renewal. It uses the API as the source of amounts and state, refreshes after conflicts, prevents duplicate mutations while a request is active, and stores no policy data in local storage. A loopback Vite proxy preserves the same-origin local boundary.

The visual direction is a compact operational desk rather than a marketing replica: synthetic-data banner, high-contrast forest/cream palette, readable term timeline, explicit state/payment labels, and destructive coral only for cancellation. Desktop uses paired action panels; mobile collapses to a single readable column.

## UI-review evidence and corrections

All native controls have visible labels; fieldsets group sale data; status/errors are live; term history is ordered; the cancellation dialog receives initial focus, closes with Escape, and returns focus to its trigger. Focus indicators are explicit and reduced-motion preferences are respected.

The first build exposed a Vite/Vitest config typing mismatch and Vitest discovering Playwright specs; configuration was corrected. The first browser run passed four journeys and failed two because the assertion said “fresh calculation” while the actual, clearer warning said the API recalculates today. The assertion was aligned and initial destructive-button focus was added before the passing rerun.

## Verification

- production TypeScript/Vite build passed;
- three component tests passed;
- six Playwright journeys passed: sell, quote/cancel, and renew at desktop and Pixel 7 viewports;
- rendered desktop and mobile full-page captures were inspected for overflow, hierarchy, control sizing, contrast, and responsive stacking;
- npm audit reported zero known vulnerabilities for the locked 153-package graph.

Playwright uses deterministic intercepted API responses for browser presentation/interaction checks; real API/SQL contracts and lifecycle behaviour remain covered by the backend integration suite. This is not misrepresented as one full-stack browser test.

## Later review qualification - 2026-09-27

The original passing checks did not establish complete accessibility or failure recovery. Subsequent browser probes found focus could escape the open dialog, destructive-button text contrast was approximately 3.71:1, business-conflict detail was hidden, and a failed refresh after a successful mutation left misleading stale state. These findings remain open; see R5/R6/R7/R9 in the [review checkpoint](review-checkpoint-2026-09-27.md).
