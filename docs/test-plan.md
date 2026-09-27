# Test plan and evidence

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
