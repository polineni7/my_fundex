# Default data

`database/006_default_data.sql` is the complete current default-data script. Apply it after the schema migrations. It is embedded in the API and can also be run directly in a PostgreSQL SQL client.

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
./scripts/Invoke-Uat.ps1 -Action Seed
```

The script uses a transaction and a seed lock. It adds missing defaults; it does not overwrite edited plans/policies, revive deleted records, reset passwords or create customer subscriptions/trades. Repeated execution is covered by a real PostgreSQL integration test. Stage policy assignment only fills the empty policy reference on the supplied draft plans. Do not use the older 002 demonstration script as the full setup.

## Seeded in Neon fundx_uat

- India, INR (2 decimal places), NSE and EQUITY reference records.
- ADMIN, MANAGER and TRADER roles.
- 22 permission definitions and baseline role mappings. Managers receive plan/policy management and selected read permissions; identity and financial administrator actions remain ADMIN-only.
- Default equity policy with EQUITY_ONLY and MAX_ORDER_VALUE (INR 100,000).
- Two demonstration draft plans: INR 100,000 two-step and three-step evaluations, INR 999 entry fee, 80% reward share.
- Five stages: 8% first-stage target, 5% later targets, 5% daily loss, 10% total loss, minimum five trading days, no calendar-day expiry. Funded daily/total limits are 5%/10%.

Plans stay Draft until reviewed and published in the admin UI. These values are starter data, not automatic approval of commercial terms. Live trading, checkout and payouts remain disabled by the existing UAT environment configuration. No fabricated instruments, prices, market holidays, payments or funded-account balances are inserted.

## UAT administrator

An ADMIN account was created at `admin@myfundex.local`. Its generated password is now Windows-DPAPI-protected in the git-ignored `.local/uat-admin.json` file (ProtectedPassword). Only the same Windows user can decrypt this local copy. Database passwords are salted BCrypt hashes; they cannot be decrypted. Credentials are not embedded in SQL or tracked configuration. Names are encrypted by the application key ring and the password is BCrypt-hashed. Preserve the key ring when moving the API to a different host.

The old automatic development account with a shared fixed password has been removed. For another environment, supply Bootstrap__AdminEmail, Bootstrap__AdminPassword and Bootstrap__SeedAdmin=true for a single explicit bootstrap run; then disable the flag and remove the bootstrap password. Default-data re-runs do not recreate this account.

## Validation

39 backend tests passed, including PostgreSQL seed repeatability, migrations, identity constraints and concurrency. The backend builds without warnings or errors. UAT seeding completed successfully.

UAT API checks passed: administrator login, 2 plans, 1 policy, 3 roles, 39 role-permission mappings and 1 administrator user. The local smoke-test server was stopped afterward.
