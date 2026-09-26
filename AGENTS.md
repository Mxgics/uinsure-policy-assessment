# Uinsure policy assessment

Read `docs/project-context.md`, `docs/project-plan.md`, and `docs/requirements.md` before changing scope or behaviour. Architecture and boundaries are recorded in `docs/decisions/001-architecture-and-delivery-boundaries.md`.

The repository root must resolve to this project before any add, commit, or push. Never stage from a parent directory. Use `main` plus focused `codex/` branches; stop for owner review after each PR and never merge without explicit instruction.

PR 1 is documentation only. No application restore, build, migration, test, or run command exists yet. Do not invent results. For current checks use `git diff --check`, inspect `git status --short`, and validate Markdown links.

From PR 2 onward, keep exact commands in this file and evidence in `docs/changes/` and the test plan. Use genuine red/green/refactor for business behaviour; record the failing case, smallest implementation, refactor, and rerun.

Keep supplied assessment PDFs, credentials, local machine paths, personal/interview notes, real customer data, and generated secrets out of the repository. Use synthetic data only. Do not substitute another database when Docker or SQL Server is unavailable.

Keep the requirements matrix, assumptions, plan, ADRs, change note, and AI log aligned with actual progress. Separate planned checks from executed evidence. Prefer targeted official documentation for version-sensitive claims.
