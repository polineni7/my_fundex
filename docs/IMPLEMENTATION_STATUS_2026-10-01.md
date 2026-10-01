# MyFundex lifecycle implementation — 2026-10-01

The repository now contains the purchase-to-evaluation-to-funded-trading lifecycle and its trader/admin screens. Local compilation and automated service tests pass. This is **not a production acceptance sign-off**: no reachable PostgreSQL database or configured provider accounts were available for full integration/manual acceptance testing.

## Implemented flow

1. Google sign-in and authenticated trader ownership checks remain in place. Admin financial operations require ADMIN; a TRADER receives 403. Manager access remains limited to existing management permissions.
2. INR, equity-only plan versions hold two/three evaluation stages, targets, daily/total losses, minimum days, expiry, funded loss limits and reward share. Verified Razorpay purchase provisioning creates a fresh evaluation account with assigned policy and starting capital. Payment Test/Live mode is independent of trading mode.
3. Evaluation orders use the internal PaperTradingEngine. Market/executable limit orders simulate full fills at current quotes; resting limit orders reserve cash/inventory and are matched by the worker. Cancellation releases only that order's reservation. There are no real broker orders during evaluation.
4. Paper cash, inventory, executions, equity and realized P&L update atomically through one module context with optimistic book concurrency. Missing/stale prices, missing sessions, oversells and invalid tick/lot sizes reject new orders. Policies and challenge limits are enforced.
5. A failed attempt cancels pending paper orders, closes simulated positions and preserves history. A new paid purchase linked to the failed subscription provisions a new account with fresh capital. Historical balances are not overwritten.
6. A passed stage advances to a fresh next-stage account. All required stages must pass before funded entitlement exists. Outcome and SMTP notification intent are saved together; workers retry progression and delivery. SMS remains disabled.
7. ADMIN activates a separate funded account using completed entitlement and a verified broker profile/credential reference. Live trading additionally requires Trading__LiveEnabled=true. One broker account cannot be allocated to multiple MyFundex accounts.
8. The same authorized order API routes by owned account mode. Funded orders use broker adapters and limit-only cash/inventory reservations. Broker submissions happen after committing order intent, outside database transactions. Ambiguous submissions are reconciled rather than blindly repeated.
9. Verified broker partial fills update live cash, inventory and P&L once per execution. Cancellation releases only unfilled reservations after broker confirmation. Interrupted cancellation is retried against the verified original broker order.
10. Funded drawdown monitoring and ADMIN square-off block new exposure, cancel outstanding orders, wait for reconciliation, and submit inventory liquidation limits. Closing is complete only when flat.
11. ADMIN records verified broker settlement/fees on a flat account. Eligible realized surplus is removed from trading cash, split according to the plan and delivered idempotently to the wallet. New wallet transactions use balanced entries.
12. Traders request withdrawals against withdrawable funds. ADMIN approves an externally verified beneficiary or rejects/releases the hold. RazorpayX payout requests use idempotency keys; confirmed payment and reversal update holds and paired ledger entries once. Unknown payout IDs require verified administrative recovery rather than indefinite resubmission.

## Screens and operational controls

Trader: catalogue and purchase, challenge progress/stages, retry purchase, paper/real account selection, buy/sell orders, cancellation, portfolio, wallet ledger, withdrawal requests/history.

Admin: plan/version editing, funded rules/reward share, graduation, account suspension/history, live activation, order reconciliation, square-off, settlement, withdrawal review/recovery and verified exchange-session calendar. Financial actions are ADMIN-only.

## Architecture and data

Broker placement/cancellation, reconciliation, verification, market quotes, account lifecycle, eligibility, wallet posting, payouts and messaging use injectable contracts. Upstox is the supplied adapter; a replacement broker must implement and register these contracts and map its instruments/executions. Evaluation accounting is independent of broker order execution.

Paper books/positions use myfund_sandbox_trading. Live books/positions/distributions use myfund_prod_trading; withdrawal holds use myfund_prod_wallet. Shared account metadata, order/execution envelopes, subscriptions and existing wallets remain in module-specific fundex_* schemas. No business tables are intentionally mapped to public. This is logical mode separation, not separate physical databases.

The EF model and upgrade SQL include tables, foreign keys, unique constraints, indexes, audit fields, soft-delete filters and concurrency versions. Historical single-sided ledger records or legacy broker evaluation orders require a separate reconciliation/backfill; the migration does not invent missing history.

## Installation and configuration

Use the administrator/bootstrap, Google, encryption and secrets guidance in SETUP_AND_VALIDATION.md. Never commit populated secrets.

```powershell
dotnet tool restore
dotnet restore backend/MyFundex.sln
dotnet ef database update --project backend/src/MyFundex.Api --context DevBootstrapDbContext
```

For SQL-based upgrades from TradingRoutesAndPurchaseReferences, use database/004_complete_trading_lifecycle.sql. It includes CompleteTradingLifecycle and ScheduleWithdrawalPolling and is idempotent through migration history. A fresh database must receive the initial and preceding migrations too; using `database update` applies the complete chain. database/002_demo_catalogue.sql contains illustrative draft reference plans, not production commercial terms.

Configure Broker__Upstox__MarketData__AccessToken for evaluation/live valuations. The internal evaluation engine does not use Broker__Upstox__Sandbox__AccessToken for fills. Keep market-data and account-trading credentials separate. Re-sync Upstox instruments before trading: JSON tick sizes are normalized from paise to INR. Configure verified NSE/BSE trading sessions, including holidays; missing sessions block trading.

Keep Workers__Enabled=true for progression, matching, reconciliation, notices and payouts. Configure SMTP via Messaging__Smtp__*. Configure Razorpay checkout separately from RazorpayX payouts. Keep Trading__LiveEnabled, Razorpay__CheckoutEnabled and RazorpayX__Enabled disabled until acceptance testing and account/beneficiary verification are complete.

## Validation actually executed

- dotnet restore: passed.
- dotnet build: passed, zero warnings and errors.
- Backend tests: 36 passed, 1 skipped (PostgreSQL integration test), 37 total.
- EF pending-model check: no pending changes. Upgrade SQL generated successfully.
- Database migration application: attempted, failed because 127.0.0.1:5432 refused the connection. No PostgreSQL runtime/migration success claimed.
- Both frontends: npm ci, ESLint and production Vite builds passed. These projects use JavaScript; no TypeScript compilation applies.
- API started with database initialization and workers disabled. Windows DPAPI initialization passed when run with the normal user-key access required by Windows.
- HTTP smoke: health and Swagger 200; unauthenticated challenge/wallet/withdrawal/admin routes 401; TRADER tokens on admin lifecycle routes 403. Database readiness correctly returned 503. Process stopped after checks.
- Service tests cover paid activation, two-stage completion, failure and repurchase, duplicate requests, paper matching/cancellation/inventory, live partial fills and cancellation, interrupted cancellation recovery, funded loss boundary, reward ledger, payout/reversal idempotency, quote identity/freshness, plus existing ownership, validation, schema, soft-delete and version regression tests.

Service tests use in-memory EF and fake providers; they do not establish PostgreSQL isolation or real-provider correctness. Set MYFUNDEX_TEST_POSTGRES to a disposable database whose name ends in _tests to run the PostgreSQL test.

## Manual acceptance results and remaining limitations

Login/Google consent, plan CRUD, real checkout, funded activation, market-session orders, portfolio settlement, payout, SMTP delivery, admin square-off, audit UI and concurrent database order scenarios have **not** been manually exercised end-to-end. They need a reachable test database, populated policies/calendar/instruments and provider credentials. SonarQube quality gate, penetration, load, backup/restore and deployment tests were not run.

Paper execution currently uses full simulated fills at quotes: no market depth, queue priority, slippage, corporate actions or exchange fees. It is not Upstox sandbox execution. Quotes older than two minutes fail closed; illiquid shares and market closures can defer valuation. Monitoring is periodic; it cannot guarantee tick-by-tick loss protection. Daily opening equity uses the last persisted valuation rather than a reconstructed official closing snapshot.

Real orders are long-only equity LIMIT orders. A liquidation limit can remain unfilled; broker rejection or unresolved prior-day order history needs operator review. Brokerage funding, custody, KYC, beneficiary onboarding and authoritative broker cash/holdings reconciliation are external operational responsibilities, not automatic transfers created by changing account names. Broker charges are verified manually at distribution. Provider account entitlements and applicable operating approvals must be established before launch.

SMTP delivery is at-least-once; a crash after sending but before recording delivery can duplicate a notice. Stable immutable posting references prevent financial double posting. Refresh/session revocation, full password recovery, corporate-action processing and complete configurable permission administration remain outside this implementation.

Primary API references consulted: [Upstox instruments](https://upstox.com/developer/api-documentation/instruments/), [quotes](https://upstox.com/developer/api-documentation/get-full-market-quote/), [order details](https://upstox.com/developer/api-documentation/get-order-details/), [order trades](https://upstox.com/developer/api-documentation/get-trades-by-order/), [RazorpayX payout idempotency](https://razorpay.com/docs/api/x/payout-idempotency/).
