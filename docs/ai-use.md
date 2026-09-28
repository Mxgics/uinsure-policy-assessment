# AI assistance log

This public log records material assistance, decisions, corrections, and verification without raw conversations, credentials, supplied documents, personal paths/data, or private interview preparation.

Earlier PR sections preserve their original observations. Current review and delivery status is recorded in the dated follow-up below.

## PR 1 — project documentation foundation

- Task: turn the reviewed plan into a public project brief, requirement matrix, assumptions, ADR, context/plan, and contribution conventions.
- Assistance: inspected the agreed planning pack and existing documentation patterns; checked targeted official platform documentation; drafted the repository documents and traceability structure; checked Git boundaries and publication metadata.
- Human decisions already provided: .NET 10/controller API, EF Core/SQL Server, React/TypeScript, local-first delivery, complete must/should/could scope, focused PRs, and an owner review gate after every PR.
- Important correction preserved: lifecycle writes update the shared policy row; child insertion alone would not make a policy rowversion detect every cancel/renew race.
- Review decision: pending repository-owner review of PR 1. Do not record approval until it happens.
- Verification: isolated root/branch, whitespace, 24 unique requirement IDs, relative links, private path/contact patterns, public visibility, and the `main` default were checked on 2026-09-26. Pull-request checks and owner review remain pending; see `docs/changes/001-project-foundation.md`.
- Remaining limits: no application/test evidence; Docker/SQL execution unverified; dependency pins and CI deferred to PR 2.

## PR 2 — API and SQL foundation

- Task: scaffold the .NET solution, API/error/OpenAPI contract, SQL migration path, integration tests, local tooling, and pinned CI.
- Assistance: resolved official registry versions/digests/action SHAs; drafted the host, tests, scripts, workflow, and evidence; ran and interpreted checks.
- Human decision: after PR 1 merged, the owner changed delivery to stacked PRs and authorized continuous implementation of PRs 2–7.
- Corrections: selected portable test logging after Event Log access masked responses; replaced a non-pullable manifest digest with Docker's verified amd64 repo digest; retained SQL Server rather than substituting a database.
- Verification: Release build, one domain boundary test, five HTTP contract tests, and one real SQL Server migration test passed. CI is pending push.
- Remaining limit: the migration is intentionally schema-empty; policy behaviour begins in PR 3.

## PR 3 — sell and retrieve

- Task: implement the sell, policy retrieval, and term retrieval slice with domain rules and SQL persistence.
- Assistance: drafted domain entities, explicit HTTP contracts, EF mappings/migration, deterministic tests, and aligned evidence documents.
- Corrections: supplied the SQL connection through test-host configuration instead of ineffective EF registration replacement; treated `Location` as an absolute URI; removed test-order assumptions from persistence assertions.
- Verification: 14 domain and 10 API/integration tests passed in Release against SQL Server in Docker.
- Review decision: pending repository-owner review; PR 3 remains stacked on PR 2.
- Remaining limits: lifecycle operations, conflict handling, browser workflows, and CI results belong to later work.

## PR 4 — cancellation

- Task: implement quote/execution, refunds, history, atomicity, and cancellation conflicts.
- Assistance: translated the agreed examples into literal tests before the calculator, implemented the domain/persistence/API slice, and constructed real-SQL race and rollback checks.
- Corrections: fixed an informational unused-day expectation; explicitly marked the new cancellation graph as added; narrowed duplicate-key conflict handling to the named cancellation index after the deterministic race exposed SQL command ordering.
- Verification: 24 domain and 16 API/integration tests passed in Release against SQL Server in Docker, including two-connection concurrency and injected later-write rollback.
- Review decision: pending repository-owner review; PR 4 remains stacked on PR 3.

## PR 5 — renewal

- Task: implement renewal windows, successor history, conditional payments, predecessor integrity, and remaining lifecycle races.
- Assistance: wrote the domain contract before implementation, added the domain/EF/API slice, and extended deterministic SQL race/rollback verification.
- Decisions preserved: the window has 31 inclusive dates; manual renewal can be unpaid; claims reset only on the successor; a cancelled successor is retained and never replaced.
- Verification: 34 domain and 26 API/SQL integration tests passed in Release against SQL Server in Docker, exercising Card/DirectDebit/manual paths, invalid no-write cases, aggregate races, and injected later-write rollback.
- Review decision: pending repository-owner review; PR 5 remains stacked on PR 4.

## PR 6 — React UI

- Task: implement an accessible, responsive demonstration for the complete policy lifecycle and add frontend CI.
- Assistance: used official React/Vite/Playwright guidance and the UI-review workflow to design semantic forms, state/error handling, responsive styling, component tests, browser journeys, and rendered visual checks.
- Corrections: used Vitest's typed config and excluded Playwright specs; aligned a browser assertion with the actual recalculation warning; added initial focus and focus return for cancellation confirmation.
- Verification: production build, three component tests, and six desktop/mobile Playwright journeys passed; rendered captures were inspected; npm audit reported zero known vulnerabilities.
- Review decision: pending repository-owner review; PR 6 remains stacked on PR 5.

## PR 7 — final readiness

- Task: reconcile final evidence, walkthrough, limitations, public-information boundary, and merge order.
- Assistance: generated the reviewer path, found and corrected stale review-workflow/runtime-status wording, executed a genuine fresh-clone verification, and queried live GitHub checks without exposing credentials.
- Verification: locked restores, formatting, Release build, 34 domain tests, 26 SQL integration tests, frontend build, three component tests, six browser journeys, links, privacy patterns, PDF absence, whitespace, and Git cleanliness passed from the clone.
- CI observation: PRs 2–5 backend and PR 6 backend/frontend jobs were successful; PR 7 CI is not predicted before its final push.
- Review decision: pending repository-owner review; no PR is merged by this work.

## Final review and explanation audit - 2026-09-27

- Task: review current readiness and check that every PR has a separate explanation.
- Assistance: compared local note-introduction commits with live metadata for PRs 1-7, checked CI results, traced remaining findings against unchanged code, and added a linked explanation index and review checkpoint.
- Corrections: removed stale current-status claims, narrowed coverage claims to checked-in tests, clarified that City/Bedrooms are implementation additions, qualified prior UI accessibility claims, and corrected the explanation of the predecessor foreign-key guarantee.
- Evidence limits: PR 3 has no recorded red/green chronology; historical evidence was not invented. PRs 2-7 have successful CI but remain open. The prior application's 34 domain/26 API/3 component/6 browser test results are historical at the unchanged baseline; this documentation audit did not rerun them.
- Remaining work: all nine reproduced findings and the missing coverage remain open. The remediation plan is local proposed work, not implemented corrections. City/Bedrooms optionality is a planning default, not an owner decision.
- Delivery: documentation changes only; no commits, pushes, PR edits, messages, or merges were performed during this audit.

## PR 8 — review corrections and real browser evidence

- Owner decisions: one correction PR above PR 7; remove Bedrooms, retain optional City, add Address Line 3; include real browser-to-SQL verification. No merge authorized.
- Assistance: implemented validation, migration, UI recovery/modal/contrast, regression tests, test-only full-stack runner and CI job; updated public explanations and the separately maintained private preparation ledger.
- Review corrections: preserved the observed failing cases; fixed custom-converter OpenAPI metadata, native backward-Tab wrapping, long-reference mobile overflow, and test/build authoring errors. Prior PR 3 TDD evidence was not manufactured.
- Verification: maintained backend gate passed 38 domain and 72 API/SQL tests with a clean Release build; frontend build/eight component tests, ten intercepted browser tests and eight actual browser/API/SQL journeys passed. Desktop/mobile captures were inspected. Clean-checkout and final-head CI evidence are recorded separately.
- Environment: Docker startup temporarily failed on stale runtime sockets after an interruption; preserved the verified socket-only directories and restored the engine. No database substitution or Docker data reset was used.
- Limits: unauthenticated local assessment, recorded rather than settled payments, no sale idempotency or deployment. Bedrooms migration discards that column and refuses populated rollback. Historical review/verification entries remain dated observations.

## PR #9 repair and coherence review

- Task: preserve the existing correction PR, repair its ancestry against merged `main`, review the combined implementation and reconcile public/private explanations.
- Assistance: verified Git/GitHub state, preserved the old tip, compared baseline trees, resolved ancestry-only conflicts, traced API/domain/SQL/UI paths, wrote failing boundary regressions, added concise rationale comments, and reconciled documentation.
- Corrections: uncertain sale outcomes no longer invite retries; malformed or unusable responses cannot replace rendered state; required/non-null OpenAPI sale fields match the accepted contract; quote results show their date; UI premiums stay within JavaScript-safe pence.
- Judgment: retained the settled architecture and scope, used focused runtime guards instead of a validation framework, and recorded UTC-midnight defaults and field-level error association as explicit demo limitations rather than expanding product scope.
- Evidence: targeted red/green results are in `docs/test-plan.md`; the final complete gates and current-head CI remain separate pending evidence. No merge or recruiter submission is authorized.

### Checkpoint documentation review

- Compared the local repair checkpoint with source and live PR metadata; corrected the distinction between the already-updated PR base and the unpublished local head.
- Reviewed private rehearsal claims against actual sale handling: a warning against manual resubmission is not an enforced submit lock or server idempotency. Updated the separate private ledger while preserving 64 questions.
- Verification: 94 local Markdown targets resolved and diff whitespace checks passed. This review did not rerun application suites or complete final delivery gates; see the test plan.

### Final clean-checkout gates — 2026-09-28

- Executed the maintained gates in a separate local clone. Corrected comment formatting, a Required-validation camel-case key regression and a TypeScript partial-matcher compile error exposed by those gates; retained the failing regression rather than weakening its assertion.
- Final local results on `2709595`: 38 domain, 72 API/SQL, 15 frontend, 10 mocked-browser and 8 full-stack tests passed. Exercised the disposable HTTP demo, reviewed rendered desktop/mobile output and keyboard focus, and verified cleanup.
- Recorded migration/scan scope and remaining limits in the test plan. Current-head CI and owner review remain separate; no merge or recruiter message is authorized.

### Error-handling readability follow-up — 2026-09-28

- Owner requested simpler branching. Refactored backend exception mapping to a pattern switch/shared response write and extracted frontend recovery into a helper with conflict-first guards and an operation switch. Preserved messages, response contracts, recovery precedence and finally cleanup.
- Added HTTP 500 characterization for sale/renewal/cancellation and verified it passed before production changes. This is refactoring under passing tests, not a fabricated red/green cycle.
- Executed final local gates: 38 domain, 72 API/SQL, 18 frontend, 10 mocked-browser and 8 real full-stack tests passed; Release build had zero warnings/errors. Current-head CI and owner review remain separate gates.
