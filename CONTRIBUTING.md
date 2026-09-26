# Contributing

## Branch and review workflow

`main` is the default and only long-lived branch. Start each coherent change from an up-to-date `main` and use a descriptive `codex/` branch. Open a pull request into `main`, explain the decision and verification, and wait for repository-owner review before merging or beginning the next planned PR.

Keep commits reviewable. A commit should represent one understandable step and must not claim tests, Docker execution, or runtime behaviour that was not observed. Do not rewrite shared history or combine unrelated cleanup with a feature.

## Pull requests

Use the repository pull request template. Every PR must state:

- the problem and scope;
- important design or business decisions;
- exact verification performed and its result;
- requirement IDs affected;
- documentation updated;
- AI assistance and human review/corrections;
- limitations, unresolved issues, and deferred work.

PR descriptions and public documentation must not include supplied PDFs, credentials, personal filesystem paths, private interview preparation, raw AI conversations, or personal data.

## Engineering expectations

- Implement meaningful business behaviour with red/green/refactor. Preserve the first failing test and reason, the minimum green change, any refactor, and the final rerun in the change evidence.
- Use deterministic tests and independent literal expectations. Do not make production code depend on test-only clocks, seed endpoints, or sleeps.
- Use SQL Server for persistence and real database integration tests. Never silently replace it with EF InMemory, SQLite, or another database when Docker is blocked.
- Update the requirement matrix, plan/context, assumptions, ADRs, change notes, test evidence, runbooks, and AI log when the related facts change.
- Keep secrets in ignored local configuration. Use synthetic demonstration data and loopback-only local services.

## Before requesting review

Run the commands documented for the current phase in `AGENTS.md`. Review the full diff, confirm the exact Git root, run `git diff --check`, scan tracked files for secrets/private material, and ensure every evidence claim names an executed command or observable result.
