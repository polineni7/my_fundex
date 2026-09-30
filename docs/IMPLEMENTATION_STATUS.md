> Historical report from 2026-09-26. Superseded by [current setup and validation](SETUP_AND_VALIDATION.md).

# MyFundex — implementation status

Reviewed against `MY_FUNDEX_MASTER_DOCUMENTATION.md` on 2026-09-26.

## Current result

This repository is a partial V1 implementation, not a completed trading platform. The solution builds, both UIs build, and regression tests cover selected access, validation, payment, policy and database-model safeguards. No live provider or PostgreSQL integration tests have been performed in this environment.

## Implemented in this pass

- Corrected broken references across backend modules and missing relational/HTTP dependencies.
- Reused module model configuration in development database creation, retaining unique indexes and precision settings. Fixed index filters and identity model configuration.
- Restricted development database creation and default admin seeding to Development. Production requires a non-development JWT key and separately provisioned schema.
- Added registration input validation and rejected inactive users at login.
- Added manager permission checks on existing admin routes. Removed internal user IDs from the admin account response.
- Added account-ownership checks, strict order validation and idempotency payload matching before order submission.
- Disabled automatic HTTP retries for order placement. Uncertain submissions and provider rejections retain capital and require reconciliation; they are not assumed safe to repeat.
- Blocked SELL until inventory reservation exists. Missing, expired and unsupported risk policies fail closed.
- Added Razorpay order creation, checkout signature verification, server-side captured-payment verification, signed webhook handling and receipts. Credentials come from configuration/environment, not the settings table.
- Added plan drafting/version/stage creation endpoints, subscription creation/history, payment history and receipt lookup.
- Added trader registration, plan browsing, subscriptions and payment history; improved table loading/error/empty states.
- Corrected frontend API port, preserved unrelated local storage on logout, and added dependency lockfiles.
- Moved broker secret lookups to environment configuration and rejected secret writes to runtime settings.

## Remaining functional work

| Area | Still required |
| --- | --- |
| Identity | Refresh/session revocation, password reset, email verification, role/permission administration, rate limiting, MFA decision |
| Plans/challenges | Publish validation, policy existence checks, stage attempts, automatic progression/failure, retry rules, payment-to-challenge provisioning |
| Payments | Durable fulfilment/outbox, provider-creation reconciliation, failed-payment retry workflow, refunds, tax invoices, Razorpay Checkout UI and sandbox integration tests |
| Funded accounts | Idempotent allocation after challenge completion, capital history, suspension/reactivation workflows |
| Risk | Daily/total loss, position and exposure limits, policy builder, assignment management, effective-mode handling, business-day holding limits, same-day repurchase, evaluation history |
| Trading | Durable broker work queue/outbox, capital reservation reconciliation, sell inventory reservation, cancellation/modification, broker execution verification, partial fills, idempotent position updates |
| Portfolio | Cost basis/lots, holdings, realized/unrealized P&L refresh, execution consumption, holding deadline enforcement |
| Wallet | Idempotent double-entry settlement, profit allocation and deductions, balance locks, ledger reconciliation; existing profit-credit service remains incomplete |
| Withdrawals | Eligibility/calculation snapshots, balance locking, approval workflow, RazorpayX payout integration, idempotency and payout reconciliation |
| Operations | Forced exits, account/user administration, error viewer, notification delivery, read acknowledgements, audited recovery tooling |
| Intelligence | Signal generation/ranking, research/RAG, orchestration and presentation |
| Infrastructure | Reviewed production migrations, snake_case schema convergence, durable outbox dispatcher, readiness/metrics, backup/restore, load tests, deployment and recovery runbooks |

## Explicit gates

- `Razorpay:CheckoutEnabled` defaults to false. Leave it false until paid challenge provisioning and integration tests are complete. A captured payment currently creates a receipt but does not activate a subscription or create a funded account.
- Draft plans have no publish endpoint yet. Do not publish by editing the database for a live service.
- SELL is blocked. `ReconciliationRequired`, `PendingReservation` and `PendingSubmission` orders need operator investigation; there is no automatic reconciler yet.
- The Upstox webhook is restricted to administrators as an interim measure. Provider callbacks are not a supported execution-processing path yet. Do not make it anonymous without a validated ingestion/reconciliation design.
- The generic V1 database currently retains its original PascalCase table/column convention. The master document's snake_case standard still needs a reviewed migration.
- Financial services outside the new safeguards have not been certified safe for real money.

## Validation performed

- `dotnet test backend/MyFundex.sln`: regression suite, including model SQL generation without a database connection.
- `npm run build`: both frontends.
- Dependency installation: both frontend audits reported zero vulnerabilities at install time.
- Docker is unavailable and no PostgreSQL Windows service was found. Database connectivity, concurrency and startup have not been verified.
- Razorpay/Upstox credentials were not supplied. No live payment, payout or broker order was placed.

## Next implementation sequence

1. Provision a development PostgreSQL instance; establish reviewed migrations and database integration tests.
2. Complete policy authoring and immutable plan publication with validation.
3. Implement paid-subscription fulfilment, challenge progression and idempotent funded-account allocation using a durable outbox.
4. Complete broker work/reconciliation, inventory/capital reservation, execution processing and portfolio updates.
5. Implement profit settlement, wallet ledger and withdrawal/payout workflows.
6. Complete admin/trader workflows and operational tooling; run provider sandbox and full lifecycle tests before enabling checkout/trading.

## Provider references

- [Razorpay Orders](https://razorpay.com/docs/api/orders/create/)
- [Razorpay Standard Checkout verification](https://razorpay.com/docs/payments/payment-gateway/web-integration/standard/integration-steps/)
- [Upstox V3 order placement](https://upstox.com/developer/api-documentation/v3/place-order/)

