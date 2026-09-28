# PR #9 coherence review

- Date: 2026-09-27
- Scope: repaired `codex/pr8-review-fixes` candidate against merged `main`
- Status: fixes and final local gates verified on `2709595`; current-head CI and owner review pending

This register separates confirmed defects from concerns and accepted limitations. Earlier stage 8 evidence remains historical in [008-review-fixes.md](008-review-fixes.md); executed current-candidate results belong in the [test evidence record](../test-plan.md).

| Severity | Location | Observed behaviour | Expected behaviour | Evidence | Disposition |
| --- | --- | --- | --- | --- | --- |
| High | `web/src/App.tsx`, sale submission | A lost sale response used the generic “Please try again” path despite no sale idempotency | Preserve the draft, explain creation may have succeeded, warn that resubmission can duplicate, and never retry automatically | Failing component regression for network failure; green sale-specific recovery tests | Fixed in `e6bdde4` |
| High | `web/src/api.ts` | Invalid JSON became `{}` and successful bodies were cast to expected types, allowing unusable state to render | Parse explicitly and reject invalid, empty or structurally unusable success payloads before state replacement; malformed errors remain safe | Four red/green API-boundary cases plus uncertain-mutation component case | Fixed in `e6bdde4` |
| Medium | `SellPolicyRequest` OpenAPI | Required sale members were absent from `required` and nullable binder types advertised `null` | Required request fields are present and non-null in the public schema while nullable CLR members still detect omission | Isolated OpenAPI regression failed, then passed | Fixed in `e6bdde4` |
| Medium | `web/src/App.tsx`, money boundary | The backend decimal range exceeds JavaScript's exact-integer range in pence | The demo input stays within JavaScript-safe pence and response guards reject amounts that cannot be represented safely | Failing max-attribute regression; boundary guard unit cases | Fixed in `76abadb`; API retains its wider decimal contract |
| Low | `Quote` component | A hypothetical result did not display the calculation date returned by the API | Present the authoritative quote date beside the result | Failing component regression, then green | Fixed in `76abadb` |
| Low | date input defaults | Defaults are derived from UTC, but an already-open page is not scheduled to update exactly at UTC midnight | API remains authoritative; users can edit dates; avoid adding timer complexity to the assessment demo | Source inspection and UTC-default browser coverage; no reproduced incorrect server mutation | Accepted limitation |
| Medium | field validation UX | Server field errors are announced in one live summary but are not linked to individual inputs or focused | Current summary must remain readable; field association/focus would improve a production form | Component/browser inspection; no claim of field-level association | Accepted limitation for the focused demo |
| Low | request ordering | The UI serializes all actions with one busy guard, so concurrent lookup requests cannot currently be issued through the interface | If independent lookups are later allowed, abort or sequence them to prevent stale overwrite | Component duplicate-submit evidence and source trace | Not currently applicable; future trigger recorded |

No unresolved confirmed correctness defect remains in this register. Passing targeted checks do not replace the final maintained gates, rendered review, disposable migration exercise, repository scan or CI on the exact PR head.

## Repair evidence

The former PR #9 tip `5b3c79f` is preserved as local branch `backup/pr9-before-main-merge-20260927`. Refreshed `origin/main` was `8d4f289`; its tree `145945b5…` exactly matched the recorded PR 7 baseline. All ancestry conflicts were therefore resolved to the correction side, and the staged tree `dabe9830…` exactly matched the backed-up tip before merge commit `5ed4e6d` was created. The resulting file comparison against `main` contains corrections and subsequent coherence work rather than repeated delivery of PRs #2–#7.

## Final gate follow-up — 2026-09-28

Clean-checkout verification caught a comment-formatting failure, a required-field error-key regression and a TypeScript test-matcher compile error. Commits `e877c0e` and `2709595` correct these without changing the agreed business rules. The existing missing-boolean test supplies the observed red case; the JSON validation metadata provider restores camel-case keys while Required annotations preserve the schema. Complete backend, frontend, mocked and real-browser gates then passed. The test plan records exact counts, rendered/keyboard checks, disposable HTTP demo, migration evidence and scan limits. Current-head CI and owner review remain separate gates.
