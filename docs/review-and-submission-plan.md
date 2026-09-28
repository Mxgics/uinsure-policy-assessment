# Review, testing and submission plan

Delivery stage 8 is [GitHub PR #9](https://github.com/Mxgics/uinsure-policy-assessment/pull/9), targeting `main` after PRs #1–#7 merged, with final local verification recorded in the test plan. The branch and change-note numbering retain the agreed stage number. Nothing in this plan authorizes a merge.

## 1. Owner review checkpoint

Read [the correction note](changes/008-review-fixes.md), then review the commits in this order:

| Area | Decisions to understand | Acceptance evidence |
| --- | --- | --- |
| Input validation and property contract | One named enum value; storage limits checked before writes; Bedrooms removed; Address Line 3 supported; City optional | Invalid requests return validation errors without writes; snapshots survive renewal; OpenAPI matches JSON |
| Migration | Upgrade drops bedroom data; retained address/payment/history data survives; populated rollback is blocked because removed values cannot be reconstructed | Empty-database reversal and populated-upgrade/rollback tests |
| UI recovery and accessibility | A successful mutation remains successful when refresh fails; stale actions stay disabled until GET recovery; uncertain outcomes are never automatically replayed | Component failure tests, browser keyboard/focus checks and mobile long-reference regression |
| Full-stack verification | Disposable real SQL, actual API and browser, independent fixtures and fresh database assertions | Desktop/mobile sale, cancellation, paid renewal and manual renewal journeys |
| Explanations | Separate planned checks from executed results; keep private interview preparation outside this repository | Change notes, requirements matrix, AI log and test plan |

Record review findings with the affected file, observed behaviour, expected behaviour and severity. Blocking correctness/data-loss/contract issues must be fixed with a failing regression first. Rerun the affected suite and the maintained final gates after code changes. Record accepted limitations explicitly. Owner approval is a separate decision from green CI.

## 2. Testing gate

From a clean checkout with the documented prerequisites:

```powershell
pwsh -File scripts/Test-Local.ps1
Push-Location web
npm ci
npm run build
npm test
npx playwright install chromium
npm run test:e2e
Pop-Location
pwsh -File scripts/Test-FullStack.ps1
```

Stop on any failed command. The backend gate covers locked restore, formatting, Release build, domain behaviour and real SQL integration/migrations. Component tests cover partial failures; mocked browser tests cover UI interactions; the separate full-stack suite proves browser/API/SQL wiring and persisted lifecycle results. Mocked journeys alone are insufficient.

For an owner walkthrough, use `pwsh -File scripts/Test-FullStack.ps1 -Serve` and the synthetic references it prints. Follow [the walkthrough](walkthrough.md): sell with optional address fields, reload by reference, quote/cancel, renew a paid policy and renew an unpaid policy manually. Check mobile layout and keyboard-only cancellation. This owner walkthrough is planned, not recorded as completed automated evidence. Stop the disposable session with Ctrl+C.

Before approving, check all three CI jobs on the current PR head: backend, frontend and fullstack. Do not substitute an older green run. Inspect failed-test artifacts rather than retrying unexplained failures. See [executed evidence](test-plan.md) for actual results and boundaries.

## 3. Merge PR #9 only after explicit approval

PRs #1–#7 are already merged. Confirm PR #9 still targets `main`, its file diff contains only corrections/coherence work, and backend, frontend and fullstack checks passed on the exact reviewed head. Then stop for explicit owner authorization. Do not merge on the strength of an older green correction-branch run.

## 4. Final submission checkpoint

After an authorized PR #9 merge, verify a fresh checkout and the final `main` CI run. Update delivery status with the actual merged commit and results. Confirm the README's run instructions and assumptions, and scan tracked files for private material or generated credentials. Keep the public change explanations and private evidence ledger aligned.

The owner then sends the repository link and availability to the recruiter. No external message or submission is authorized by this plan. Authentication, real payment settlement, production hosting and other documented non-goals remain outside this assessment.
