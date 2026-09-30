# MyFundex setup and validation — 2026-09-30

## What is implemented

- One class-library project per existing business module, split into Domain, Application, Infrastructure and Contracts folders with DependencyInjection.cs. Empty extension folders contain .gitkeep; they do not imply implemented features.
- Module-specific `fundex_*` default schemas. Initial EF migration, snapshot and idempotent SQL script are included. The monolith host owns the aggregate deployment migration; modules still own their persistence models.
- Shared BIGINT key/audit/soft-delete/version behavior, default numeric(20,4) money precision, and append-only execution/ledger/receipt/audit entities.
- Google OAuth authorization-code flow with PKCE, verified provider email, short-lived secure external cookie, fixed configured UI return origin, and token exchange. Google sign-in creates a trader identity and does not silently link existing email/password administrators.
- BCrypt passwords; encrypted first/last names using ASP.NET Data Protection. Email and other searchable identifiers remain plaintext. Full PII encryption/searchable email blind indexes are not implemented.
- Authentication rate limits, 64KB request limit, structured console logging, explicit CORS origins, 30-minute access tokens.
- Account-level routing: Evaluation -> Upstox Sandbox; Funded -> Upstox Production with an explicit per-account credential reference. The browser cannot select the environment. Orders retain the chosen route.
- Razorpay Test/Live key-prefix validation independent of the trading environment. Production evaluations can therefore use live purchase payments while routing evaluation orders to sandbox.
- Two-/three-step plan models, configurable stage targets/loss limits/trading days/expiry, draft creation, metadata editing, active-policy publication validation, and a two-query public catalogue.
- Pure challenge evaluator with loss-boundary, expiry, minimum-day, realized-profit and open-position/order checks. This evaluator is not yet wired to a verified execution-driven stage progression worker.
- Paid-subscription provisioning worker creates the initial evaluation attempt, sandbox account, wallet and assigned policy through contracts. Unique provisioning references make retries recoverable. It has not been exercised against PostgreSQL/providers here.
- Separate MyFundex.Messaging project with injected SMTP and disabled SMS transports. SMTP is not yet connected to a durable challenge-passed notification workflow.
- Trader Google entry, INR catalogue and stage tables. Admin plan/version creation and publishing UI. Smaller React components, lazy entry bundles, ESLint/Prettier scripts.
- Instrument sync uses a dictionary lookup instead of one DB query per instrument.

## Database installation

For a NEW empty database:

```powershell
dotnet tool restore
dotnet restore backend/MyFundex.sln
dotnet ef database update --project backend/src/MyFundex.Api --context DevBootstrapDbContext
```

Alternatively, apply `database/001_fundex_schema.sql`. It creates migration history in `fundex_integration` and module tables in their own schemas. `database/002_demo_catalogue.sql` inserts INR reference data, roles, permissions and demonstration draft plans. Example capital/fees are illustrative, not approved commercial terms. Stage policy IDs must be assigned before publishing.

**Existing databases:** the initial migration is NOT an upgrade/rename script for old schemas or plaintext names. Back up and rehearse a separate data migration before using this code with existing business data. Do not delete the old database or run both schemas side-by-side and assume the data moved. Encrypted name conversion requires a controlled backfill under the same protected key ring.

For actual PostgreSQL integration tests, set `MYFUNDEX_TEST_POSTGRES` to an isolated database whose name ends in `_tests`. The test applies migrations and checks concurrent buying-power reservations. It adds uniquely named test data and does not drop the database.

## Administrator setup

In a private environment, set `Bootstrap__AdminEmail`, `Bootstrap__AdminPassword` and `Bootstrap__SeedAdmin=true` for one startup after migrations. The seed hashes the password and creates the ADMIN membership transactionally. It refuses to promote an already-existing identity. Remove the password and turn SeedAdmin off immediately after successful creation.

Development retains the original development-only bootstrap account when the user table is empty. Never use that account for a production deployment.

## Google configuration

Set `Authentication__Google__ClientId`, `Authentication__Google__ClientSecret`, and `Authentication__TraderOrigin` to the HTTPS trader origin. Register `https://YOUR_API_HOST/signin-google` as the Google authorized redirect URI. Set the UI's `VITE_API_URL` to the HTTPS API `/api/v1` URL and allow that trader origin in CORS. Local OAuth also requires HTTPS; the external cookie is intentionally Secure.

The application redirects through `/api/v1/auth/google`, `/signin-google`, and `/api/v1/auth/google/complete`. The trader UI exchanges the secure external cookie through `/api/v1/auth/google/exchange`. Tokens are not placed in callback query strings. OTP is deferred. Session revocation/refresh, account linking and full password lifecycle remain pending.

## Secret and encryption configuration

`.env.example` lists configuration names without secrets. ASP.NET does not automatically read this file; supply environment variables through your process/container/secrets manager.

Production requires a persistent `DataProtection__KeyDirectory` and a private encryption certificate at `DataProtection__CertificatePath`, plus its password if applicable. All replicas need the same key ring, certificate and application name. Back these up securely: losing keys prevents profile-name decryption and invalidates external cookies. Restrict file ownership to the application identity. Windows development uses DPAPI; Linux development keys need protected host storage.

Do not write credentials into runtime settings or source control. Email still needs storage access control/encryption at the database-volume layer. This is not a penetration-test certification.

## Broker and payment modes

Razorpay defaults to Test mode and rejects mismatched key prefixes. Set `Razorpay__Mode=Live` only with live keys in the production deployment. Checkout remains disabled by default until the full financial lifecycle is validated.

Evaluation orders always use `Broker__Upstox__Sandbox__AccessToken`. Funded orders require the account's credential reference and `Broker__Upstox__Accounts__REFERENCE__AccessToken`. There is no automatic broker account creation or funding API in this implementation. Promotion is not a name change: verified completion, no unresolved orders/positions, approved allocation and a valid broker account link are required.

## Validation results

- Restore and backend builds executed; final results are reported in the task response.
- Backend unit/regression suite covers signatures, input validation, account ownership, missing policies, schema SQL, stage decisions, soft delete, immutable records and stale versions.
- PostgreSQL migration application attempted against localhost:5432 and failed to connect. No database creation, data migration or provider lifecycle is claimed tested.
- Both frontend installs/builds and lint checks executed; final results are reported in the task response. JavaScript remains in use, so there is no TypeScript check.
- API started with database initialization and workers disabled for HTTP smoke checks. Health/Swagger/providers returned 200; protected routes returned 401; invalid registration returned 400. This is not a database-backed startup test.
- SonarQube configuration is supplied, but no scanner/server quality gate was executed. Use the .NET SonarScanner begin/build/end workflow with private CI credentials.
- Dockerfile supplied, not built or deployed here because Docker is unavailable.

## Still required before production

Verified broker fill ingestion, order cancellation, sell inventory reservations, capital reconciliation, execution-driven positions/P&L, continuous drawdown monitoring, stage advancement/graduation, live activation/linking, congrats mail outbox, wallet settlement, withdrawals/payouts, admin square-off, complete RBAC management, deployment/backup/load/security tests, and provider sandbox end-to-end tests remain unfinished.

The current code is a substantial refactor and setup implementation, **not the complete production trading MVP**. Do not enable real-money customer operations on the strength of build/unit-test results alone.
