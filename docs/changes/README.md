# PR explanations and review evidence

This folder records what each change did, why it was made, corrections discovered during development, verification, and limitations. Notes describe their original PR stage; later reviews can identify gaps without invalidating an earlier test result.

## Existing PRs

Checked against Git history and live PR metadata on 2026-09-27. Each numbered note was introduced in the corresponding implementation/documentation commit, rather than added only at final review.

| PR | Explanation | First commit | Current delivery status |
| --- | --- | --- | --- |
| [1: documentation foundation](https://github.com/Mxgics/uinsure-policy-assessment/pull/1) | [Scope, assumptions, architecture, and documentation boundaries](001-project-foundation.md) | `a01d780` | Merged |
| [2: API and SQL foundation](https://github.com/Mxgics/uinsure-policy-assessment/pull/2) | [Host, persistence tooling, errors, tests, and CI decisions](002-api-foundation.md) | `48646ce` | Merged |
| [3: sell and retrieve](https://github.com/Mxgics/uinsure-policy-assessment/pull/3) | [Policy snapshots, validation, persistence, and test corrections](003-sell-and-retrieve.md) | `d3d1f62` | Merged |
| [4: cancellation](https://github.com/Mxgics/uinsure-policy-assessment/pull/4) | [Refund calculation, atomic writes, and concurrency](004-cancellation.md) | `6286c90` | Merged |
| [5: renewal](https://github.com/Mxgics/uinsure-policy-assessment/pull/5) | [Successors, payment choices, history, and lifecycle races](005-renewal.md) | `48b68e8` | Merged |
| [6: React policy desk](https://github.com/Mxgics/uinsure-policy-assessment/pull/6) | [Browser workflows, UI decisions, and test boundaries](006-react-ui.md) | `021fe9b` | Merged |
| [7: final readiness checkpoint](https://github.com/Mxgics/uinsure-policy-assessment/pull/7) | [Walkthrough, clean-clone checks, and delivery limits](007-final-readiness.md) | `ed1fc15` | Merged |
| [9: delivery stage 8 corrections](https://github.com/Mxgics/uinsure-policy-assessment/pull/9) | [Review fixes and browser-to-SQL evidence](008-review-fixes.md); [coherence findings](009-coherence-review.md) | `1d9eb57` | Merged into `main` at `8c845fa` |
| Delivery stage 9 | [Demo data and workflow usability](010-demo-usability.md) | Pending | Local gates recorded; owner manual testing complete; publication CI and final PR review tracked separately |

## Current review

The [2026-09-27 review checkpoint](review-checkpoint-2026-09-27.md) preserves the findings at PR 7. The [stage 8 explanation](008-review-fixes.md) maps each original correction to regression evidence; the [coherence register](009-coherence-review.md) records later confirmed defects, concerns and dispositions. Those corrections are merged. Stage 9 is a separate focused usability change and remains at its owner-review gate.

Supporting records:

- [Test plan and executed evidence](../test-plan.md)
- [AI assistance, decisions, and corrections](../ai-use.md)
- [Accepted assumptions](../assumptions.md)
- [Architecture decision](../decisions/001-architecture-and-delivery-boundaries.md)

PR 3 explicitly lacks recorded red/green chronology. PRs 4 and 5 describe test-first failures, implementation, and corrections; a complete command-by-command red/green/refactor transcript is not preserved. Do not manufacture missing historical evidence. Future business changes must record their failing regression, smallest passing change, refactor or reason none was needed, and final rerun.
