# Trading lifecycle implementation — 2026-10-01

## Implemented in this change

- Separate PaperTradingService and ProductionTradingService behind TradingGateway. A provider registry selects IBrokerAdapter by the provider retained on each account/order. Only Upstox is currently registered; an unregistered provider fails closed.
- Provider and execution environment stay on the order so later configuration changes cannot redirect cancellation to another broker or mode. Production requires an account credential reference.
- Active paid evaluation eligibility is checked through the Subscription contract. Funded trading remains blocked until verified graduation and allocation exist; a mode string alone does not grant live access.
- Upstox V3 sandbox origin corrected to api-sandbox.upstox.com. Cancellation requests use the account's original route. Cancellation acknowledgement does not release buying power or claim final settlement.
- Purchase idempotency with unique request references. Explicit repurchase accepts only an owned failed subscription for the same plan, creates PendingPayment, and preserves earlier attempts. The trader plan screen sends an idempotency key. API clients must now supply it.
- Owned pending Razorpay checkout can be resumed; trader subscriptions screen opens Standard Checkout and verifies payment on the server. Checkout remains configuration-gated.
- Paper/real labels on trader orders/account selectors and admin account list. Cancellation action, pending state, debounced search and duplicate-click prevention.
- Wallet credit rejects nonpositive amounts and uses a unique posting key to reject duplicate credits or mismatched settlement amounts. This is not a complete balanced accounting/settlement implementation. Older ledger rows have no posting key and need a separately reviewed historical reconciliation.
- Additive EF migration and database/003_trading_routes_and_purchase_references.sql. Existing broker provider values are backfilled to Upstox. Existing fundex_* schemas are preserved; myfund_sandbox_* / myfund_prod_* schema separation has not been implemented.

## Validation

- Restore succeeded after allowing access to the user's NuGet configuration.
- Backend build succeeded, zero warnings/errors. Backend suite: 26 passed, one PostgreSQL integration test skipped.
- Both frontend lint/build checks passed.
- EF reports no pending model changes; generated upgrade SQL inspected.
- Applying the migration failed because PostgreSQL at 127.0.0.1:5432 refused the connection. No database-backed lifecycle or live provider result is claimed.

## Decision required to complete evaluation execution

Upstox's current sandbox documentation lists place/modify/cancel operations, but not fills, and explicitly says sandbox postbacks are not functional. Sources:

- https://upstox.com/developer/api-documentation/sandbox/
- https://upstox.com/developer/api-documentation/v3/place-order/
- https://upstox.com/developer/api-documentation/v3/cancel-order/
- https://razorpay.com/docs/payments/payment-gateway/web-integration/standard/integration-steps/

The user has been asked whether evaluations should use an internal quote-driven paper engine while Upstox Sandbox remains an integration-test adapter. This is not implemented without that decision. Broker acknowledgement is never treated as a fill.

## Still unfinished

Execution ingestion/reconciliation, SELL inventory, capital reservation recovery, positions/P&L, daily equity snapshots, continuous drawdown enforcement, stage progression, failure finalization, verified live graduation/allocation, withdrawals, balanced bookkeeping, congratulation mail, complete admin controls and mode-separated persistence. Cancellation stays pending until reconciliation exists. Real customer trading must remain disabled. Passing tests do not establish production readiness.

API smoke check with database initialization/workers disabled: health 200, Swagger 200, protected accounts 401. Windows DPAPI key-ring initialization failed in the restricted environment; Google sign-in and encrypted profile operations are therefore not validated. The temporary API process was stopped after checks.
