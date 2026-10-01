# Assessment plan configuration

Plan creation is now split into plan details, per-stage rules, and review/publication. `GET /api/v1/admin/plans/options` supplies active policies and stable period identifiers, so the admin UI does not maintain a duplicate period map.

Period identifiers: 0 unlimited, 1 one week, 2 two weeks, 3 three weeks, 4 one month, 5 two months, 6 three months, 7 six months, 8 one year. Month/year durations use the stage start date in India and calendar arithmetic, including leap years. Existing `MaximumCalendarDays` values remain supported when `TradingPeriod` is null. Supplying both is rejected.

Optional minimum trading days and leverage preserve null in storage and responses. Zero minimum days is explicitly no minimum; null means not specified (the evaluator imposes no minimum). Leverage zero is invalid, not unlimited. Blank leverage means no plan-specific ceiling. Capital, fee, profit target, loss limits, and funded reward percentages remain required and validated; deductions accept explicit zero.

Maximum leverage is stored per assessment stage as a ceiling, from 1:1 to 1:100. Current cash-only execution remains stricter (1:1). This setting does not enable margin borrowing or change broker buying power. It does not configure a separate funded-stage leverage policy.

Migration `20261001111918_ConfigurableAssessmentTerms` adds nullable `TradingPeriod` and `MaximumLeverage` and makes `MinimumTradingDays` nullable in `fundex_subscription.Stages`. Existing stage values are preserved. Deploy the migration before the API and UI. The matching deployment SQL is `database/009_configurable_assessment_terms.sql`.

Notifications use a shared store and survive navigation. Login success is explicit; mutation messages can describe the completed action. The notification identifier no longer depends on browser secure-context crypto APIs.

## Validation (1 October 2026)

- Backend build: zero warnings/errors; no pending EF model changes.
- 51 backend tests passed against the isolated Neon test database, including migration, calendar periods, and null/zero persistence.
- Admin and trader frontend lint/build passed.
- API smoke: login, options (9 periods and active policies), plan creation, draft save/read, null/zero round trip, invalid period rejection (400), publication, and public catalogue labels passed in the isolated test database.
- Browser: network-error toast appeared with a dismiss control. Successful login notification is implemented, but was not manually exercised in the browser in this pass.
- Migration applied successfully to Neon UAT. Restart the API and refresh the UI to load the new endpoints/assets.
