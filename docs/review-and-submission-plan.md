# Review, testing and submission plan

Delivery stages 1–8, including [GitHub PR #9](https://github.com/Mxgics/uinsure-policy-assessment/pull/9), are merged. Delivery stage 9 is the focused `codex/demo-usability` branch. Nothing in this plan authorizes a merge.

The owner confirmed manual testing complete on 28 September 2026 and authorized publishing a focused final PR. This does not authorize a merge or an external submission. Existing local automated results remain in the [test evidence](test-plan.md); the final PR records CI for its exact head.

## 1. Final PR review checkpoint

Read [the stage 9 change note](changes/010-demo-usability.md), then review the changes in this order:

| Area | Decisions to understand | Acceptance evidence |
| --- | --- | --- |
| Demo data | Six exact stable references, current renewal/cancellation dates, opt-in destructive scope limited to demo records | Real SQL repeatable seeding after mutations; non-demo policy retained |
| Input validation | One-to-three holders; exact 16+ boundary; automatic renewal cannot use Cheque | Domain and HTTP 400/no-write regressions; Card/Direct Debit and manual Cheque paths retained |
| UI usability/accessibility | Add/remove holders, age-aware picker, explicit renewal mode, visible holders/payment/window, compact scenario disclosure | Component tests, keyboard checks, desktop/mobile screenshots and no-overflow assertion |
| Full-stack verification | Disposable real SQL, actual API and browser, independent fixtures and fresh database assertions | Desktop/mobile sale, cancellation, automatic renewal and manual renewal journeys |
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

Manual testing is already owner-confirmed complete; the following walkthrough is optional for another reviewer. Run `pwsh -File scripts/Start-Local.ps1 -SeedDemo` and use the UI's prepared-policy disclosure. Follow [the walkthrough](walkthrough.md): sell three holders, reload by reference, quote/cancel both cancellation examples, renew an automatic policy and renew a manual policy unpaid. Check mobile layout and keyboard-only cancellation.

Before approving, check all three CI jobs on the current PR head: backend, frontend and fullstack. Do not substitute an older green run. Inspect failed-test artifacts rather than retrying unexplained failures. See [executed evidence](test-plan.md) for actual results and boundaries.

## 3. Merge stage 9 only after explicit approval

Confirm the stage 9 PR targets `main`, its diff is limited to demo usability and aligned documentation, and backend, frontend and fullstack checks passed on the exact reviewed head. Then stop for explicit owner authorization. Do not merge on the strength of an older green run.

## 4. Final submission checkpoint

After an authorized stage 9 merge, verify a fresh checkout and the final `main` CI run. Update delivery status with the actual merged commit and results. Confirm the README's run instructions and assumptions, and scan tracked files for private material or generated credentials. Keep the public change explanations aligned.

The owner then sends the repository link and availability to the recruiter. No external message or submission is authorized by this plan. Authentication, real payment settlement, production hosting and other documented non-goals remain outside this assessment.
