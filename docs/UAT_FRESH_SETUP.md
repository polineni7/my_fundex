# Fresh UAT baseline — 2026-10-02

The user explicitly requested removal of all old application data and encryption keys and recreation of defaults in `fundx_uat`.

- Cleared 51 `fundex_` application tables, restarting identities. Preserved schema migration history and unrelated databases.
- Recreated India/INR/NSE/equity master data, role/permission catalogue, two draft assessment plans and default risk policy.
- Created one administrator with username `Admin` and email `admin@myfundex.local`. Password stored as bcrypt; no password is committed here.
- Added nullable, unique username support and admin username/email login. Username values are normalized to lowercase.
- Created a new certificate-wrapped encryption ring in the settings table and a new JWT signing key, invalidating old tokens.
- Removed obsolete local key files, the pre-migration config backup and old application key-ring certificates. No old profile ciphertext remains in UAT.
- Broker configurations are empty; real trading/payment gates remain disabled. Provider setup must be entered before use.

Verified new Admin login (HTTP 200), readable profile without recovery warnings, one user, two plans, one policy and zero broker configurations. Incorrect password returns 401. Frontend lint and build passed. Temporary verification API stopped to leave Visual Studio build output unlocked.

The wrapping certificate remains outside the database in the Windows current user's Personal certificate store. Additional application instances need that certificate/private key provisioned securely.
