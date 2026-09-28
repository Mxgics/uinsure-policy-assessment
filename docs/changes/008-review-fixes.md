# PR 8: Review corrections and complete browser evidence

Delivery stage 8 is [GitHub PR #9](https://github.com/Mxgics/uinsure-policy-assessment/pull/9). See the [owner review and submission plan](../review-and-submission-plan.md).

Status: implemented and verified locally and in a clean checkout at `50f4151`; three-job CI passed on that commit. See [delivery evidence](../test-plan.md#clean-checkout-and-delivery-verification) and the live PR for final-head checks. Base: `codex/pr7-final-readiness` at `806ecab`. Branch: `codex/pr8-review-fixes`. No merge is authorized by this change.

## Problem and correction

The passing PR 7 suite did not exercise malformed enum combinations, storage limits, partial UI failures, or full keyboard containment. The property model also omitted Address Line 3 while requiring extra fields. The owner requested one corrective PR on top of the stack, removed Bedrooms, retained optional City, and included real browser-to-SQL evidence.

| Finding | Correction | Regression evidence |
| --- | --- | --- |
| R1 | Accept one declared enum name; guard domain values; preserve string-enum OpenAPI schemas | `Invalid_enum_names_are_validation_errors_without_writes`, `Renewal_rejects_invalid_names_without_new_rows_or_revision`, `Undefined_enums_are_rejected_without_JSON`, `OpenApi_preserves_named_enums_and_the_property_contract` |
| R2 | Persist/copy Address Line 3, make City optional, remove Bedrooms | `Minimal_property_needs_only_line_one_and_postcode`, `Three_holders_and_all_address_lines_survive_renewal`, populated migration test |
| R3 | Validate each null holder before mapping and report its index | `Null_holder_is_an_indexed_validation_error` |
| R4 | Share normalized string limits with mappings; cap decimal premium before SQL | `Storage_limits_are_checked_after_normalization`, `Invalid_premium_is_rejected_before_SQL`, `Maximum_premium_round_trips_exactly` |
| R5 | Preserve business conflict detail and distinguish refresh success/failure | Component conflict/GET-only recovery tests |
| R6 | Retain confirmed POST results; block stale mutations; never automatically replay POST | Component cancellation/renewal failed-GET, uncertain transport and duplicate-request tests |
| R7 | Native dialog, safe initial focus, explicit boundary wrapping, inert background, focus restoration | Desktop/mobile modal keyboard and cancellation tests |
| R8 | Scope quote assertions to its term/payment; arrange an unrelated cancellation first | `Quote_is_read_only_and_execution_recalculates_and_persists_history` |
| R9 | White destructive text on `#b63f2d`; decorative coral unchanged | Rendered Chromium contrast assertion of at least 4.5:1 |

## Red, green, refactor and corrections

- API regressions initially produced **12 failures and 13 passes**. Combined enum strings returned 201 or 500 instead of 400; null holders and oversized values returned 500. After strict parsing, indexed validation and normalized bounds, the same 25 cases passed.
- Property/isolation regressions initially produced **three failures**: minimal/three-line properties returned 400 and the quote assertion found an unrelated cancellation. After contract, migration and scoped assertions, the integration suite passed 55 cases at that checkpoint.
- UI regression run produced **four failures and two passes**: conflict details were replaced and confirmed mutations were lost after failed GET. Separation of mutation result from refresh, a stale-state guard and GET-only recovery corrected these. The expanded component suite subsequently passed eight cases.
- Native dialog tests exposed that Shift+Tab could leave the document for browser chrome. Added explicit first/last-button wrapping while preserving native inertness; all eight desktop/mobile browser-contract checks then passed.
- A new OpenAPI test found the custom converter was described as an integer. A schema transformer now supplies canonical string names; the failing contract test passed after that correction.
- Real-browser renewal fixtures exposed a long-reference mobile overflow that obstructed action clicks. Added a viewport/action regression, wrapped long references and retained the longer fixtures. The real suite changed from six passes/two failures to eight passes; the expanded mocked suite passed ten checks.
- Refactoring shares storage constants and refresh handling instead of duplicating them. Existing policy lifecycle and one-save transaction boundaries remain intact. Additional passing coverage is not labelled retroactive TDD.
- Test authoring corrections included a nullable parse annotation, placing enum guards in sale validation, a jsdom-only dialog shim, and including imported fixture/types in the browser TypeScript project. Actual top-layer behavior is tested in Chromium, not claimed from the shim.

## Verification and delivery

Executed results and outstanding checks are maintained in the [test evidence](../test-plan.md). The full-stack suite passed eight real browser journeys plus fresh SQL assertions. The final clean-checkout/CI gate must pass before this PR is labelled ready. Earlier PR green runs do not cover these changes.

The [property/browser ADR](../decisions/002-property-contract-and-browser-evidence.md) and [runbook](../runbooks/local-development.md) explain migration loss/rollback and the test runner. Private preparation remains separate. Prior historical TDD and review entries are preserved rather than rewritten.
