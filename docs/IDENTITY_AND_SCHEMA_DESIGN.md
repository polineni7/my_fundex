# Identity and schema design

## Consistent schema names

All current application schemas start with `fundex_`. Migration `StandardizeSchemasAndIdentity` renames the existing schemas in place:

| Previous schema | Current schema | Tables |
| --- | --- | --- |
| myfund_sandbox_trading | fundex_sandbox_trading | Books, Positions |
| myfund_prod_trading | fundex_prod_trading | Books, Positions, ProfitDistributions |
| myfund_prod_wallet | fundex_prod_wallet | WithdrawalHolds |

PostgreSQL ALTER SCHEMA preserves rows, table identities, sequences, indexes, ownership and grants. The migration is transactional. Historical migrations retain the old names so an empty database can replay the complete migration chain correctly. Do not edit or reapply historical SQL to perform this upgrade. Use EF database update or database/005_standardize_schemas_and_identity.sql after the preceding migrations. Back up existing data and stop old application instances before upgrading: old binaries still reference the previous schema names.

## One identity, multiple trading accounts

A person has one `fundex_identity.Users` row. Profile names, login identity, password hash or Google subject and active/suspended status live there. Separate sandbox-user and real-user tables would duplicate credentials and make graduation/account ownership fragile.

Application roles and trading eligibility serve different purposes:

- TRADER: trader access. Google entry mints trader-only claims even if the identity has staff memberships.
- MANAGER: existing operational permissions; no administrative identity or financial operations.
- ADMIN: administrative identity and financial controls.
- Evaluation/Funded: the owned funded-account TradingMode, with subscription/stage eligibility checked independently. One person can have historical failed evaluations, an active evaluation and a funded account.

## Tables and relationships

| Schema | Tables / responsibility |
| --- | --- |
| fundex_identity | Users (profile, login identifiers, hashed password, SecurityVersion), Roles, Permissions, UserRoles, RolePermissions, IdentityEvents |
| fundex_subscription | Plans, plan versions/stages, subscriptions, challenge attempts and outcome notice outbox |
| fundex_accounts | Funded account ownership, mode, capital allocation, broker link, account status/history |
| fundex_payments | Purchase payments, invoices and receipts |
| fundex_risk | Policies, assignments and rule evaluation records |
| fundex_trading | Shared order intent, order events and broker executions |
| fundex_sandbox_trading | Simulated evaluation cash/inventory books |
| fundex_prod_trading | Real trading cash/inventory books and reward distributions |
| fundex_wallet / fundex_prod_wallet | Wallet balances, balanced ledger entries, transactions and withdrawal holds |
| fundex_withdrawal | Withdrawal requests, review and payout progress |
| fundex_market | Instruments and verified market sessions |
| fundex_audit | General audit events |

UserRoles and RolePermissions now have explicit foreign keys, restrictive deletion and unique active membership indexes. Public role/permission IDs are unique. The migration seeds ADMIN/MANAGER/TRADER without passwords and assigns TRADER only to existing users with no active membership; it never promotes a user to administrator. Legacy orphan or duplicate associations stop the migration rather than being silently discarded.

IdentityEvents records password-login success/failure, successful Google exchange, profile changes, access changes and global session revocation. Events inherit common audit fields and are append-only through the application. Unknown login attempts store no supplied email or password. Google failures that happen before a verified identity exchange remain in authentication middleware logs. This is not a device/session inventory table.

Cross-module ownership is represented by stable IDs and checked through contracts. Module-local identity associations have database foreign keys. Avoid cross-module writes just to maintain a second user record.

## APIs and screens

- GET/PUT `/api/v1/me`: own profile, with optimistic version checks. Email/provider linking changes are intentionally excluded from this endpoint.
- POST `/api/v1/me/logout-all`: revoke all existing access tokens.
- ADMIN `/api/v1/admin/identity/users`: cursor-paged users and roles.
- ADMIN `/api/v1/admin/identity/users/{userId}/access`: audited role/status change, expected version and reason required. Self-access edits and removing the last active administrator are rejected. A serializable transaction protects concurrent administrator changes.
- ADMIN `/api/v1/admin/identity/users/{userId}/history`, `/roles`, `/permissions`: identity history and access catalogue.
- Trader UI: My profile and sign out on all devices.
- Admin UI: Users and access, role/status editing and identity history.

JWT validation checks the active user and SecurityVersion in the database. Access changes and logout-all increment SecurityVersion, so old tokens fail immediately. Tokens issued before this upgrade lack the claim and require a fresh login. The UIs clear their local session when an API returns 401. Names retain existing authenticated encryption; passwords remain BCrypt hashes, not reversible encryption.

This adds one indexed identity lookup per authenticated request in exchange for immediate suspension/revocation. No positive-result cache is used that could extend revoked access. Bound list queries avoid per-user role queries.

## Validation

- Migration applied successfully to Neon fundx_uat; read-only inspection confirmed the renamed tables and IdentityEvents.
- Backend build: zero warnings/errors.
- Full backend suite against isolated Neon database fundx_identity_tests: 38 passed, none skipped.
- Includes real PostgreSQL migrations, concurrent capital reservation, unique role memberships, audit events, security-version changes and absence of myfund_* schemas.
- Both frontend lint and production builds passed.
- HTTP smoke against the isolated test database: admin login, trader registration/login, profile read/edit, trader denied admin access, suspend/revoke, failed suspended login, identity history, restore/login and logout-all/revoke passed.
- Found and fixed a C# compiler/EF expression-tree incompatibility by pinning LangVersion to 12.0 for this .NET 8 solution. Reference: https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/10.0/csharp-overload-resolution

The isolated test database retains generated test fixtures and is not an application environment. Real Google OAuth/provider flows, device-level refresh sessions, password recovery, custom permission editing and full trading acceptance remain separate work. This schema update does not constitute real-money deployment approval.
