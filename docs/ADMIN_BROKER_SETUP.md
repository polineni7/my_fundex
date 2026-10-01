# Admin broker setup and validation

## Configuration

Open **Settings > Broker setup** as an administrator. Create separate SANDBOX and PRODUCTION connections. Credentials are encrypted with ASP.NET Core Data Protection in `fundex_broker.Accounts.ProtectedCredentials`; API responses and audit entries omit credentials. Back up the configured Data Protection key ring alongside database backups. Losing those keys makes saved credentials unreadable.

Blank credential fields on edit preserve existing values. Disable a connection to revoke its use. Session expiry is enforced when credentials are resolved. Only one enabled default connection per trading environment and one enabled market-data connection are allowed. Updates require the current version to prevent overwriting another administrator's changes.

Upstox execution and account verification use these database credentials. Angel One and Groww configurations can be saved but cannot be enabled: their order, instrument mapping, cancellation and reconciliation adapters are not implemented. No simulated successful broker verification is returned.

Assessment execution still uses the internal paper engine. A production market-data connection provides prices without submitting assessment orders to the live broker. Saving a SANDBOX token does not replace this engine with Upstox sandbox execution.

Use **Runtime settings** to set `Trading.LiveEnabled` with scope `GLOBAL` and value `true` or `false`. Missing settings disable live trading. Both account activation and order placement read this setting. Enabling it does not bypass assessment, broker verification or account eligibility checks. Runtime setting cache TTL is 30 seconds; other running instances may observe updates after that interval.

Payment and SMTP secrets have not been migrated into database settings; their existing secure deployment configuration remains required. Do not add a runtime setting expecting it to replace an unsupported deployment option.

## Admin navigation and profile

Assessment plans link to risk policies in the same tab. The plan terms draft is preserved in session storage while navigating, and removed on sign-out. Admin authentication uses portal-specific storage keys. Breadcrumbs and a return action appear on each admin page. Clicking the profile button opens name editing and password change. Passwords use bcrypt; changing a password revokes existing sessions.

## Database

Migration: `20261001120738_AdminBrokerConfiguration`.
SQL: `database/010_admin_broker_configuration.sql`.
Applied to Neon UAT on 2026-10-01 after isolated database validation. Existing broker rows are preserved; new selection flags default to false. Existing deployment broker tokens must be entered through Settings before relying on the new database resolver.

## Checks performed 2026-10-01

- Backend builds: zero errors and warnings.
- 58 backend tests passed, including PostgreSQL migrations/concurrency and broker encryption/environment/expiry tests; zero skipped.
- Admin frontend lint and production build passed.
- Updated API returns the reported policy UUID successfully.
- Broker create/edit works; blank secrets are retained; responses exclude secrets; stale update returns 409.
- Disabled Angel One and Groww configuration saves work.
- Lifecycle subscription/order/withdrawal reads work.
- Isolated user profile update persists; trader access to admin broker settings returns 403.
- Password change succeeds, previous JWT returns 401, new password signs in.

No real orders, payouts, provider credential verification or full funded-account financial lifecycle were exercised. Shared corporate broker allocation remains restricted by the existing account model. These checks do not establish production readiness or regulatory approval.

The stale local API was rebuilt and restarted on ports 50900/50901. UAT admin login, the exact reported policy GET, broker settings GET and profile GET all succeeded. Isolated policy creation and retrieval persisted all three rules. Browser interaction/visual review was not performed in this verification pass.
