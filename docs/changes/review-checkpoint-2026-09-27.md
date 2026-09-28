# Final review and explanation audit - 2026-09-27

Historical baseline audit: the findings and delivery statements below describe `806ecab` before correction. See the [PR 8 note](008-review-fixes.md) for subsequent implementation and verification.

## Scope and outcome

Reviewed application baseline: `806ecab` on `codex/pr7-final-readiness`. Application and test code have not changed since the preceding review. The remediation document exists locally, but the correction series has not been implemented.

All seven existing PRs have a separate explanation in this folder, introduced alongside their original work. The [folder index](README.md) connects each PR, note, and initial commit. Notes cover rationale, verification, corrections, and limits; historical TDD evidence is incomplete and is identified honestly.

The project is not ready for unconditional sign-off: nine previously reproduced findings remain open. This checkpoint improves explanation and evidence accuracy; it does not fix application behavior.

## Open application and test findings

| ID | Priority | Finding at the unchanged baseline | Planned correction |
| --- | --- | --- | --- |
| R1 | P1 | A combined payment-method name becomes undefined enum value 3, is committed, then causes sale and subsequent retrieval to return 500 | Strict named enums and domain guards |
| R2 | P2 | Address Line 3 is discarded; City/Bedrooms are required implementation additions | Reconcile the property contract and add the missing persisted field |
| R3 | P2 | A null policyholder entry causes a dereference and 500 | Indexed validation before mapping |
| R4 | P2 | Oversized names, addresses, and premiums reach SQL and return 500 | Validation matching normalized storage limits |
| R5 | P2 | UI business-conflict details are replaced; failed refresh still claims latest state | Preserve API detail and report refresh outcome accurately |
| R6 | P2 | A successful mutation followed by failed retrieval loses its success indication and leaves stale actionable state | Preserve confirmed result and require GET-only recovery |
| R7 | P2 | Keyboard focus leaves the open cancellation dialog | Native modal behavior and focus tests |
| R8 | P2 | Quote test assumes a globally empty cancellation table | Scope assertions to the test's policy/term |
| R9 | P2 | Destructive-button text contrast is approximately 3.71:1 | Accessible destructive-button colors |

These behaviors were reproduced during the preceding review using disposable SQL fixtures and browser probes. This audit checked that their relevant code is unchanged; it did not rerun those probes or claim they are fixed. See the [implementation plan](../review-remediation-plan.md) for acceptance tests and delivery order.

## Assessment scope clarification

The assessment property model specifies Address Lines 1, 2, and 3 and Postcode; Address Line 1 and Postcode are required. City and Bedrooms were introduced by the implementation, not required by the assessment. The source permits additional fields, but requiring these extras is an implementation choice. There is no recorded original business rationale specifically justifying Bedrooms.

The remediation plan currently proposes retaining City/Bedrooms as optional compatibility fields. This is a planning default, not an approved requirement or an implemented change. Removing Bedrooms instead has been discussed but not instructed. The supplied assessment documents remain outside the repository.

## Documentation corrections and remaining evidence gaps

- Corrected current-state claims that still described implemented lifecycle/UI work or CI as pending. PRs 2-7 remain open despite successful CI; PR 1 is merged.
- Added explicit links between PR explanations, current review, planned remediation, and test evidence. Earlier dated results remain historical observations.
- Narrowed the requirement matrix where checked-in tests do not cover accepted three-holder policies, the promised additional date-validation cases, or persisted DirectDebit/Cheque refunds.
- Clarified the PR 5 predecessor explanation: domain renewal supplies the matching policy identity; the composite foreign key verifies the referenced predecessor pair. It does not independently enforce equality between a child's `PolicyId` and `PredecessorPolicyId`. No direct-database-write test was executed in this audit, and no reachable API cross-policy defect is claimed.
- Added a later qualification to the PR 6 visual/accessibility evidence because focus containment and destructive-button contrast failed subsequent review.
- PR descriptions summarize the work, but several do not fill every section of the repository template. The separate notes and AI log supply additional context; the remote PR descriptions were not edited by this review.
- PR 3 does not have recorded red/green chronology. PRs 4-5 describe it, but do not preserve a complete granular refactor transcript. This cannot be retroactively represented as executed evidence.
- The original delivery plan proposed SQL-backed browser fixtures and historical local demo seeding. Current browser evidence uses intercepted contracts with separate real API/SQL integration tests; a full-stack browser demonstration and the proposed local seed tooling are not established by that evidence.
- Added concise startup commands and key assumptions to the public README. Reconciled the separately maintained private preparation overview and ledger with the current review; private files and correspondence remain outside this repository.

## Verification and provenance

Executed for this audit: Git root/status/history inspection, comparison of numbered-note introduction commits, reading the relevant code and explanatory documents, and live GitHub PR/CI inspection. Local Markdown links, changed-document whitespace, and private-material checks are recorded after completion in the test plan.

The prior review executed the maintained backend script and frontend checks at the same application commit: Release build with zero warnings/errors, 34 domain tests, 26 API/SQL tests, three component tests, and six intercepted browser journeys passed. No application tests were rerun for this documentation-only audit; the known failures remain outside the passing suite.

Live successful CI runs observed on 2026-09-27:

| PR | Head | CI run |
| --- | --- | --- |
| 2 | `48646ce` | [36255455450](https://github.com/Mxgics/uinsure-policy-assessment/actions/runs/36255455450) |
| 3 | `d3d1f62` | [36256281998](https://github.com/Mxgics/uinsure-policy-assessment/actions/runs/36256281998) |
| 4 | `6286c90` | [36257022019](https://github.com/Mxgics/uinsure-policy-assessment/actions/runs/36257022019) |
| 5 | `48b68e8` | [36257475522](https://github.com/Mxgics/uinsure-policy-assessment/actions/runs/36257475522) |
| 6 | `021fe9b` | [36260830724](https://github.com/Mxgics/uinsure-policy-assessment/actions/runs/36260830724) |
| 7 | `806ecab` | [36261277631](https://github.com/Mxgics/uinsure-policy-assessment/actions/runs/36261277631) |

The new documentation and remediation plan are local and uncommitted at this checkpoint. These CI runs do not cover the new local documentation. No PR was created, updated, or merged and no message was posted to GitHub.
