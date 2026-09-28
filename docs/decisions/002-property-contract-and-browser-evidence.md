# ADR 002: Assessment property fields and real browser evidence

- Status: Accepted and locally verified in delivery stage 8 (GitHub PR #9)
- Date: 2026-09-27

## Decision

Follow the supplied property contract with required Address Line 1/Postcode and optional Address Lines 2/3. Retain City as an optional compatibility field; remove Bedrooms from the application, API and database. The owner selected this over retaining optional Bedrooms or removing both extras. Neither City nor Bedrooms was a source requirement. City is independent of Address Line 3 and existing values are not repurposed.

Trim text before applying storage limits. Optional blank strings become null. Existing consumers must tolerate null City and the removed Bedrooms response member. Unknown JSON members retain the API's existing handling, so a legacy `bedrooms` input is ignored rather than persisted. No new underwriting rule is introduced.

The new migration preserves policy history and financial rows but deliberately drops bedroom values. It does not rewrite previous migrations. An empty database can roll back; a populated property table causes SQL error 51002 before rollback changes begin. Restore a pre-upgrade backup when historical bedroom values are needed. Do not invent replacement data.

## Browser evidence

Keep intercepted browser tests for deterministic failures and add separate real browser/API/SQL journeys. A test-only console runner owns a disposable SQL Server container, explicit migration and domain-based synthetic seeding, real API and Vite processes, Playwright execution, and fresh-context SQL assertions. The runner is not referenced by the production application. It adds no seed/reset/clock HTTP endpoints and changes no production clock.

The runner uses dedicated loopback API/UI ports and never reuses existing servers. Synthetic references and fixture dates are saved in ignored artifacts; connection credentials go only to the API process environment. UTC rollover fails the run rather than asserting stale fixture expectations. Interactive mode exposes the same disposable seeded application for a renewal demonstration and removes it on exit.

## Alternatives and consequences

- Keeping Bedrooms optional would preserve its data, but adds a field the owner explicitly chose to remove.
- Mocked browser tests alone are faster, but do not meet the original browser-to-SQL verification goal. Both suites remain separately labelled.
- Production seed endpoints or a configurable API clock would enlarge the runtime surface solely for testing; isolated seeding avoids that.
- The full-stack suite adds Docker, process orchestration and runtime to CI. Locked versions and independent failure artifacts make those costs visible.

## References

- [EF Core migration management](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing)
- [ASP.NET Core OpenAPI transformers](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/customize-openapi?view=aspnetcore-10.0)
- [Native modal dialog technique](https://www.w3.org/WAI/WCAG22/Techniques/html/H102)
