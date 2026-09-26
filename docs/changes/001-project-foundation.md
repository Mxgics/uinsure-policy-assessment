# Change 001: Project documentation foundation

- Date: 2026-09-26
- Scope: PR 1 documentation only

## Problem

The public repository needed an agreed, reviewable baseline before implementation: product scope, requirement traceability, ambiguous business interpretations, architecture boundaries, delivery stages, contribution rules, and a safe AI/documentation workflow.

## Cause and decision

The assessment source is intentionally small and leaves several date, payment, history, concurrency, and operational details open. The reviewed plan resolved those choices. This change records them in the repository so later PRs can implement and test one stable contract rather than rediscovering it.

## Change

- Added the project brief, requirements/evidence matrix, accepted assumptions, current context, and seven-PR plan.
- Accepted ADR 001 for a .NET controller API, domain project, EF Core/SQL Server persistence, and a later React/Vite demonstration.
- Added concise agent instructions, contribution guidance, PR template, ignore rules, and public AI assistance log.
- Kept supplied/private material outside the repository and labelled all application/test evidence pending.

## Verification

Executed on 2026-09-26 before commit:

- `git rev-parse --show-toplevel` and `git branch --show-current` confirmed the isolated project root and `codex/pr1-project-foundation` branch;
- `git diff --check` completed without whitespace errors (Git emitted only the expected Windows line-ending notice);
- a PowerShell Markdown-link check found every relative link resolves;
- the requirement check found 24 unique IDs;
- `rg` found none of the configured personal path/contact patterns in repository content;
- GitHub repository metadata confirmed public visibility and `main` as the default branch after the minimal base push.

No application build, test, migration, container, or runtime verification was applicable because this PR intentionally contains documentation only. Commit/PR checks will be added to this record if they expose a correction.

## Limitations

No application code, solution, dependency lock, container configuration, database, tests, CI, or runtime command exists in this change. Docker engine and SQL Server execution remain unverified. Exact package/image/action versions are deferred to PR 2. No repository licence has been selected.
