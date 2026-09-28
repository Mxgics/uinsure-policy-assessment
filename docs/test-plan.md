# Test plan and evidence

## Current candidate summary

Candidate: delivery stage 9 on `codex/demo-usability`, based on merged `main` at `8c845fa`. The owner confirmed manual testing complete and satisfactory on **28 September 2026**. This is an owner-reported result; no per-case manual log or new screenshots are inferred.

| Layer | Latest recorded result | Evidence boundary |
| --- | --- | --- |
| Backend | 40 domain and 75 real SQL integration tests; locked restore, formatting and Release build passed | Stage 9 local execution below; not rerun during the documentation pass |
| Frontend | Production build and 23 component/API-boundary tests passed | Rerun during final submission preparation after fixing the mocked test clock |
| Browser contracts | 12 desktop/mobile Chromium tests passed | Intercepted API responses; fixed historical clock |
| Full stack | 8 real browser/API/SQL journeys passed | Stage 9 local execution below; retains real UTC today |
| Owner manual testing | Complete and satisfactory | Owner confirmation, separate from automated results and final PR approval |
| Publication | Exact-head backend, frontend and fullstack CI tracked in [PR #10](https://github.com/Mxgics/uinsure-policy-assessment/pull/10) | Merge and final-main verification require separate authorization |

The dated entries below are historical execution records. Statements that checks were pending describe that checkpoint, not necessarily the current candidate. No current-candidate clean-clone execution is claimed from an earlier clone.

## PR 2 evidence — 2026-09-26

- Release solution build: succeeded with zero warnings/errors.
- Domain architecture: one passed; no ASP.NET Core or EF Core references.
- HTTP contracts: five passed for liveness, OpenAPI, automatic validation, 404 Problem Details, and sanitized unexpected errors.
- SQL migration: passed against Docker Engine `29.7.2` and the pinned SQL Server 2022 CU27 amd64 image; the initial migration applied with none pending.
- Maintained local path: Compose reported SQL healthy on `127.0.0.1:14333`, the explicit migration created `UinsureAssessment`, and the API returned `Healthy` plus an OpenAPI document containing `/health` on `127.0.0.1:5080`; shutdown retained the named data volume.
- Dependency audit: all four projects reported no known vulnerable direct or transitive packages from NuGet.org.

The first HTTP run exposed Windows Event Log access in the test host; portable console logging fixed the infrastructure. The first SQL pull exposed an incorrect manifest digest; Docker's verified amd64 repo digest replaced it before the passing test. These are recorded corrections, not hidden green-only history.

Policy behaviour, rollback injection, lifecycle races, and browser journeys remain pending their stacked PRs. CI evidence is pending branch push.

## PR 3 evidence — 2026-09-26

- Release solution suite: 14 domain tests and 10 API/integration tests passed.
- Domain boundaries: sale-date limits, one-year/leap calculation, holder count, age, supported types, premium precision, and normalisation passed with a fixed clock.
- HTTP/SQL behaviour: sale returned 201 and a retrievable Location; policy and term retrieval matched persisted snapshots; required booleans and numeric enums were rejected; unknown/mismatched resources returned 404; the current migration applied to SQL Server.
- Persistence: returned policy/term identifiers were found in fresh EF queries with a recorded payment.

The first SQL-backed feature run failed because the test host replaced EF registrations while the application still resolved its deferred configuration callback. Supplying the isolated connection string through test configuration fixed the host accurately. A subsequent assertion was corrected to compare the path of the valid absolute Location URI. The final full run passed against Docker; no result was skipped or substituted.

## Planned layers

- Domain: deterministic rules with independent literal expectations.
- API/database: controller pipeline and real isolated SQL Server databases/migrations.
- Atomicity/concurrency: deterministic barriers/interceptors, separate contexts/connections, and fresh final-state reads.
- UI: component tests and Playwright workflows with synthetic historical fixtures.

Tests fail rather than skip when Docker is unavailable. EF InMemory and SQLite are not substitutes.

## PR 4 evidence — 2026-09-26

- Test-first calculator cycle: the new literal tests failed before cancellation types existed, then passed after implementation; one day-count expectation was corrected transparently.
- Final Release suite: 24 domain tests and 16 API/SQL integration tests passed.
- Calculation: before-start, days 1/14/15, final day, leap term, away-from-zero rounding, claims, no payment, and after-end rejection passed.
- HTTP/persistence: quote was read-only; execution recalculated and stored cancellation plus optional same-method refund; repeated/mismatched operations returned 409/404.
- Concurrency: a deterministic two-context barrier produced one 201 and one 409 with one cancellation/refund and one policy revision.
- Atomicity: a temporary SQL trigger failed the later refund write; a fresh context found no cancellation, no added refund, and no policy revision update. The trigger was removed in `finally`.

## PR 5 evidence — 2026-09-26

- Final Release suite: 34 domain tests and 26 API/SQL integration tests passed.
- Test-first renewal cycle: new domain tests failed because renewal behaviour/types did not exist, then passed after implementation without expectation changes.
- Domain: end-minus-31/end-minus-30/end/end-plus-1, snapshot copying, claims reset, paid automatic renewal, unpaid manual renewal, invalid payment combinations, and no replacement successor passed.
- HTTP/SQL: Card and DirectDebit automatic successors were paid; manual successor was unpaid and cancelled with `NoPayment`; invalid combinations wrote no successor.
- Integrity/history: duplicate renewal conflicted, active successor blocked parent cancellation, cancelled successor remained history, parent cancellation then succeeded, and replacement renewal remained blocked.
- Concurrency: deterministic renew/renew and cancel/renew pairs each produced one 201 and one 409 with exactly one lifecycle effect.
- Atomicity: an injected SQL payment-trigger failure left no successor and no policy revision update.

## PR 6 evidence — 2026-09-26

- Locked install/audit: 153 packages audited with zero known vulnerabilities.
- Production build: TypeScript project references and Vite production build passed.
- Components: three tests passed for labelled controls/demo warning, surfaced API detail, policy history, and conflict refresh.
- Browser contracts: six Playwright tests passed—sell, quote/cancel, and renew in desktop Chromium and a Pixel 7 viewport.
- Accessibility/interaction: visible labels/grouping, keyboard focus styling, destructive dialog focus, live errors/status, disabled in-flight mutations, Escape close/focus return, and reduced motion were checked.
- Visual review: full-page desktop and mobile captures were inspected; no horizontal overflow, clipped controls, or broken responsive stacking was observed.

Browser tests intercept deterministic API contracts. They verify the rendered workflow and request/response integration at the browser boundary; backend tests separately exercise the real HTTP/SQL stack.

## PR 7 clean-clone evidence — 2026-09-26

Fresh clone of remote branch `codex/pr7-final-readiness` at `ed1fc15`:

- exact locked .NET and npm restores passed; npm high-severity audit found zero vulnerabilities in 153 packages;
- non-mutating .NET formatting check passed;
- Release build passed with zero warnings and zero errors;
- 34 domain and 26 SQL-backed integration tests passed without skips;
- production frontend build, three component tests, and six desktop/mobile Chromium journeys passed;
- 127 tracked files were scanned: all relative Markdown links resolved, privacy/credential patterns were absent, no PDF was tracked, whitespace checks passed, and Git status was clean.

GitHub check inspection showed successful backend jobs on PRs 2–5 and successful backend/frontend jobs on PR 6. PR 7 CI is checked at the review checkpoint after the final push.

## Final review and documentation audit — 2026-09-27

Application baseline remains `806ecab`; this audit changes documentation only. Earlier dated entries above describe their original checkpoints, including checks that were pending then.

- Verified the exact project Git root and checked each PR explanation's introduction commit. All PRs 1–7 have notes; PRs 2–3 share one checkpoint in the separately maintained private ledger.
- Inspected live PR and workflow metadata: PR 1 is merged; PRs 2–7 remain open with successful CI on their recorded heads. Run links are in the dated review checkpoint. Those runs do not validate the local uncommitted documentation.
- Checked 25 repository Markdown files and 54 local file links: no unresolved file targets. This check does not validate heading anchors or external URLs.
- Checked the separate private planning/preparation pack: 16 Markdown files and 31 local links resolved. Updated its stale status and added the final findings, per-PR decisions/trade-offs, evidence limits, and rehearsal prompts. Private content remains outside the repository.
- Repository scans found no tracked PDFs or matches for the selected private-path, supplied-document-name, recruiter-text, interview-ledger-link, and credential patterns. This is a targeted pattern check, not a comprehensive secret audit.
- Git diff whitespace checks and a trailing-whitespace scan of all 14 changed/new Markdown files passed. No application or test source files changed.
- The preceding application review at the same baseline passed the maintained backend script (34 domain and 26 API/SQL tests, Release build with zero warnings/errors) and frontend build, three component tests and six intercepted browser journeys. Additional diagnostic probes reproduced nine findings outside that passing coverage.

Application suites were not rerun for these documentation edits. No new business behaviour, test-first cycle, full-stack browser journey, fix, merge, or submission is claimed. The remediation plan remains unimplemented; the review checkpoint qualifies earlier readiness and accessibility claims.

## PR 8 correction evidence — 2026-09-27

The preceding audit is historical. PR 8 implements its corrections. Evidence below is from executed local commands, not inherited PR 7 CI:

- `pwsh -File scripts/Test-Local.ps1`: locked tool/package restores, formatting, Release build with zero warnings/errors, **38 domain and 72 API/SQL tests passed**, no skips.
- Real SQL coverage includes empty/populated property migration, retained data, guarded rollback error 51002, strict enum/normalized limits/no-write validation, all three refund methods, holder/date boundaries, and the existing race/rollback suite.
- `npm run build` and `npm test -- --reporter=dot` from `web/`: TypeScript/browser-config compilation, production build, **eight component tests passed**.
- `npm run test:e2e`: **ten intercepted desktop/mobile Chromium checks passed**, including focus containment/dismissal/restoration, rendered destructive-text contrast and long-reference layout/action reachability.
- `pwsh -File scripts/Test-FullStack.ps1`: **eight real desktop/mobile journeys passed**, with no API interception. Fresh SQL reads confirmed original payments, cancellation/refund linkage and amount, automatic/manual renewal payment choices, history, copied Address Line 3 and policy revisions. The runner stopped its processes and disposed SQL after the run.
- Inspected desktop/mobile full-page captures and the repaired long-reference mobile layout. No overflow or clipped controls was observed in those views. This is scoped visual evidence, not certification of every browser or all accessibility criteria.

Recorded failures/corrections: API red run 12 failed/13 passed, then the same 25 passed; property/isolation red run three failed; UI red run four failed/two passed. Later checks found an OpenAPI integer-schema regression, native backward-Tab wrapping, and long-reference mobile action obstruction; each was corrected and rerun. Full-stack initially passed six/failed two mobile renewal journeys; after wrapping long references it passed all eight. The layout assertion was also tightened to the configured viewport width, since mobile `innerWidth` can expand with overflow.

Infrastructure interruption: the usage-limit pause was followed by Docker Desktop startup failure on stale runtime sockets. The verified socket-only directories were preserved outside the repository and the engine recovered without a data reset; Docker 29.7.2 then ran the SQL checks. A nullable test annotation, misplaced guard, temporary local-variable collision and TypeScript fixture inclusion were authoring/build corrections, not product regressions.

Clean-checkout, repository privacy/link checks and final-head CI results are appended when executed. Private preparation updates remain outside the repository.

### Clean-checkout and delivery verification

On 2026-09-27, a separate local clone of `50f415146b14a1ace88e3e3374aab5f928784be7` passed the maintained backend gate (38 domain, 72 API/SQL; zero skips, build warnings or errors), `npm ci`, production build, eight component tests, ten intercepted browser tests, and eight real browser/API/SQL journeys with fresh SQL assertions and cleanup. Its working tree remained clean. Initial restricted-process attempts failed at restore and Vite child-process startup; rerunning with the required host access passed without changing code.

[CI run 36335909255](https://github.com/Mxgics/uinsure-policy-assessment/actions/runs/36335909255) passed on that commit, including backend, frontend and fullstack jobs. The subsequent documentation-only delivery commit records these results and the [review/testing/submission plan](review-and-submission-plan.md); its own current-head CI result belongs in the live [PR #9](https://github.com/Mxgics/uinsure-policy-assessment/pull/9) review checkpoint. The planned stage number remains 8.

## Repaired PR #9 coherence evidence — 2026-09-27

Historical counts above are not current-candidate results. The repaired branch added genuine red/green regressions before its final complete gates:

- Frontend red: six failures reproduced invalid/empty/unusable success bodies, malformed error bodies, uncertain-sale retry wording, and unusable sale success; the focused rerun passed 14/14 after the response-boundary fix.
- OpenAPI red: the sale schema had no `required` set; after required/non-null schema correction the isolated test passed 1/1.
- UI red: quote date was absent and the premium input had no JavaScript-safe upper bound; the focused rerun passed 15/15 across the component and API-boundary files.
- The first sandboxed Vite run failed to spawn its helper; the authorized host rerun produced the behavioural red evidence. An initial isolated .NET invocation hung before output and was terminated; the host run then exposed a test compilation correction before the genuine schema failure. Neither infrastructure attempt is counted as a passing test.

The final maintained backend, frontend, mocked-browser and fullstack commands remain to be recorded below after they run from a clean checkout of the exact candidate. Current-head CI is also pending; older green stage 8 runs do not satisfy that gate.

### Checkpoint documentation review — 2026-09-27

At local HEAD `76abadb7ac9495bf55f30fffc483d4e72b902ede`, reviewed the uncommitted explanatory comments and delivery status. Live GitHub metadata showed PR #9 already targets `main`, but its remote head remains `5b3c79f1af74192ed7d1ef99703c3dc81b1e2534` and is reported unmergeable. Publishing the local repair and updating the obsolete PR description remain pending.

Checked 94 local Markdown file targets across public README/contribution/docs and the separate private question pack: none were unresolved. Anchors and external URLs were not validated. `git diff --check` passed; Git reported line-ending normalization notices for two commented source files. All 64 private core question headings remain present. No application suite, full privacy/secret scan, rendered review or migration exercise was executed in this documentation review; final candidate gates above remain pending.

## Final candidate clean-checkout verification — 2026-09-28

A separate local clone of `27095955ee04dc1681fd04742ee79d4f68c6b284` passed:

- `pwsh -File scripts/Test-Local.ps1`: locked restores, non-mutating formatting, Release build with zero warnings/errors, **38 domain and 72 API/SQL integration tests**, zero skips.
- `npm ci`: 153 packages audited, zero reported vulnerabilities. `npm run build` and `npm test`: production build and **15 component/API-boundary tests** passed.
- Chromium installation and `npm run test:e2e`: **10 intercepted desktop/mobile browser checks** passed, including modal keyboard containment, dismissal, focus restoration, destructive-text contrast and long-reference layout.
- `pwsh -File scripts/Test-FullStack.ps1`: **8 actual desktop/mobile browser/API/SQL journeys** passed, followed by fresh SQL financial/history assertions and owned-process/container cleanup.
- `pwsh -File scripts/Test-FullStack.ps1 -Serve`: disposable historical demo started. The runbook HTTP payloads, using its API port, sold/retrieved/quoted/cancelled a synthetic policy (GBP 365 same-day refund); paid and manual historical fixtures renewed with Recorded/NotRecorded payment states respectively. Ctrl+C ran cleanup; both application ports were no longer listening. The PTY wrapper returned exit 1 on interruption, so normal wrapper exit success is not claimed.
- Additional real-demo keyboard checks at 1280×900 and 412×915: Tab/Enter lookup, reachable cancellation trigger, initial Keep term focus, backward/forward modal wrapping, Escape/focus return and no horizontal overflow. Reduced-motion preference was enabled. Desktop/mobile screenshots, including long references and the dialog, were inspected with no clipped controls observed. This is scoped Chromium evidence, not comprehensive accessibility certification.
- The backend gate exercised `Property_upgrade_preserves_history_and_refuses_lossy_rollback`: empty reversal, populated upgrade with retained property/holder/payment data, and populated rollback rejection 51002 with state preserved. A production backup restore drill was not performed.
- The clone and source working trees were clean before this evidence-only update. Scans covered **146 tracked files, 29 Markdown files and 73 local Markdown targets**: no broken file targets, forbidden generated/private file types, or selected private-path/credential-pattern hits. Diff whitespace checks passed. External links/anchors and exhaustive secret detection are not claimed.

### Gate failures and corrections

The first clean gate stopped at formatting for the newly inserted expression-body comments; moving them above the methods fixed it (`e877c0e`). The next backend run passed 38 domain and 71 integration tests but failed the existing missing-boolean field-key regression: new Required annotations used CLR property names. The smallest correction registers the framework JSON validation metadata provider, preserving the existing camel-case error contract and required OpenAPI metadata. The frontend production build also exposed an overly strict generic type on a partial error matcher; removing that generic retained the assertions and fixed compilation. Both corrections are in `2709595`; complete gates above passed afterward. These are observed failures, not retroactively invented test-first steps. No further refactor was needed.

The field-name correction follows the official [JSON validation metadata provider](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.modelbinding.metadata.systemtextjsonvalidationmetadataprovider?view=aspnetcore-10.0). CI for the final documentation head is checked separately after normal push. Owner review, authorized merge and final-main verification remain outstanding.

## Error-handling refactor verification — 2026-09-28

The owner requested a focused readability follow-up to `1f7615c`. Before production edits, the existing API/SQL suite passed 72/72 and frontend tests passed 15/15. Added three HTTP 500 characterization cases (sale, renewal, cancellation); the unchanged implementation passed all 18 frontend tests. No failing business behaviour or artificial red cycle is claimed.

After refactoring the backend exception switch/shared writer and frontend named recovery helper:

- `pwsh -File scripts/Test-Local.ps1` passed locked restores, formatting, Release build (zero warnings/errors), **38 domain and 72 real API/SQL tests**, zero skips. Existing validation/conflict/unknown-exception contracts remained green.
- `npm run build`, `npm test`, and `npm run test:e2e` passed: **18 frontend tests and 10 mocked desktop/mobile browser checks**.
- `pwsh -File scripts/Test-FullStack.ps1` passed **8 real browser/API/SQL journeys**, fresh SQL assertions and owned-process/container cleanup.
- `git diff --check` passed. These runs used the working checkout; the earlier separate clean-checkout/manual demo evidence remains its dated checkpoint. No rendered styles or migrations changed in this follow-up.

The new final head supersedes the previous review candidate. All three CI jobs must pass on that exact pushed head; the result is recorded in the live PR description. Owner review, explicit merge authorization and final-main verification remain outstanding.

## Delivery stage 9 demo-usability evidence — 2026-09-28

The new automatic-renewal/Cheque domain regression first failed because no exception was thrown. Adding the cross-field domain guard made that focused test pass; its API/SQL regression then returned HTTP 400 with `paymentMethod` detail and proved the policy count was unchanged.

The first complete backend gate found two older Cheque fixtures still set `autoRenew=true`: 40 domain tests passed and 72/74 API tests passed. The fixtures were corrected to manual renewal without weakening their Cheque round-trip/refund assertions. A failed-seed interceptor case was then added to prove transaction rollback retains the original six demo policies. The final rerun of `pwsh -File scripts/Test-Local.ps1` passed locked restores, format verification, a Release build with zero warnings/errors, **40 domain tests and 75 real SQL integration tests**, zero skips.

The SQL seeder regression uses six exact stable references. It seeds, renews and cancels through the real API, reseeds, verifies one pristine term per scenario, holder/payment/claims variants, and preservation of a non-demo policy. An initial direct-aggregate mutation in the test encountered an EF concurrency exception outside the production orchestration path; changing the test to exercise the real API corrected the test design rather than hiding a product failure.

Frontend evidence:

- `npm run build` passed the TypeScript and Vite production build.
- `npm test -- --reporter=dot` passed **23 component/API-boundary tests**, including three-holder payload/history, dynamic age limits, automatic/manual payment behaviour, stable scenario loading and lifecycle-action availability.
- `npm run test:e2e` passed **12 desktop/mobile Chromium checks**, including keyboard expansion/collapse of prepared scenarios, three-holder sale, long references, cancellation modal focus/contrast and renewal history.
- `pwsh -File scripts/Test-FullStack.ps1` passed **eight real browser/API/SQL journeys** and fresh-context financial/history assertions; owned processes and disposable SQL were cleaned up.
- Inspected final desktop/mobile sale and full-stack captures. The prepared-scenario disclosure, holder cards, renewal facts and actions were readable with no observed clipping or horizontal overflow. This remains scoped Chromium evidence rather than comprehensive accessibility certification.

No migration, response-schema change, scheduled renewal, production seed endpoint or configurable API clock was introduced. Final diff/link/privacy checks, publication CI, owner review and merge remain separate gates.

## Final submission audit — 2026-09-28

- Reviewed the complete stage 9 candidate diff, domain lifecycle rules, controller/request/service boundaries, SQL mappings and seeder, frontend recovery and availability logic, local scripts and CI workflow. No application behaviour or API contract was changed during this submission pass.
- Corrected the README startup sequence, stale contribution/status wording, broken change-index table, and manual-plan claims about client-supplied references, leap birthdays, field-error association and uncertain-sale recovery. The current README explicitly identifies the candidate branch while its PR is unmerged.
- Reproduced the mocked-fixture expiry at a diagnostic date of 1 January 2027: **7 failed / 16 passed** component/API-boundary tests. Fixed only the test clock (Date in Vitest, browser time before Playwright navigation) to 28 September 2026. `npm run build`, `npm test -- --reporter=dot` (**23 passed**) and `npm run test:e2e` (**12 passed**) then completed successfully. The initial restricted-process attempt stopped at Vite `spawn EPERM`; it supplied no behavioural evidence. Host execution produced the recorded failure and passing rerun.
- Audited **150 candidate files**, including all four previously untracked delivery files, **31 Markdown files**, **75 local file/fragment links** and **4 heading anchors**. All targets resolved after correction. No forbidden generated/private file types or matches for the selected private-path, token, embedded-password and private-contact patterns were found. This is a targeted scan, not an exhaustive secret audit; external documentation URLs were not comprehensively checked.
- `git diff --check` passed. Compared quick-start commands and pinned prerequisites with the maintained scripts, package commands and CI. Live GitHub metadata confirmed public visibility, default branch `main` and no existing open PR; the remote main tip matched `8c845fa`.
- The owner confirmed manual testing complete and satisfactory. That confirmation is distinct from a per-case execution log. Backend/full-stack local evidence above was retained without claiming a rerun during this documentation and test-harness pass. Final publication CI is verified against the exact PR head and recorded in its description; merging and final-main verification remain separate gates.

Publication: committed the reviewed candidate as `d3b20ae`, pushed `codex/demo-usability`, and opened [PR #10](https://github.com/Mxgics/uinsure-policy-assessment/pull/10) against `main`. A documentation-only follow-up links the published PR and records its first commit; final CI is checked on that follow-up head. The candidate contains 32 changed files, including all four previously untracked delivery files. No merge was performed.
