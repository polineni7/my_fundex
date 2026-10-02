# Login recovery and shared encryption keys

The administrator login failure was reproduced as a missing ASP.NET Core Data Protection key while decrypting profile names. Password verification now selects credential fields independently. Missing profile keys do not grant authentication: a correct bcrypt password is still required. A successful login and profile response report `profileRecoveryRequired` when names cannot be decrypted. Existing encrypted values remain unchanged until the user explicitly saves new names. Restore the original key to recover the original names.

Passwords remain bcrypt hashes. Their format includes the algorithm, work factor and per-password random salt. No reversible password encryption or separate salt column is required.

## Settings-table key ring

`PostgresKeyRepository` uses the existing `fundex_configuration.Settings` table:

- SettingKey: `DataProtection.KeyRing.key-<key UUID>` (revocation records also use this prefix).
- SettingValue: certificate-encrypted Data Protection XML, including key identity and lifecycle metadata.
- Category: `DataProtection`.
- Environment: `GLOBAL`.
- IsSensitive: true; values are masked by the admin API.

Server-managed key records cannot be edited through runtime settings. No additional table or domain migration is required. The repository refuses to persist unencrypted key records.

Enable with infrastructure settings `DataProtection:KeyStore=Database`, `DataProtection:CertificatePath`, and `DataProtection:CertificatePassword`. All trusted API instances must use the same wrapping certificate/private key and application name `MyFundex`. Keep the private key and its password outside the database, in the deployment secret store. Backup the certificate and key records; do not delete expired keys still needed to read old data. Certificate rotation must retain access to certificates protecting existing keys.

Do not switch an existing deployment before importing and rewrapping its recoverable historical keys. Database storage cannot recreate a lost key. The initial migration was blocked by automatic approval review. After explicit user authorization, the recoverable-key migration was completed as recorded below.

## Verification

The patched UAT login and /me returned HTTP 200; incorrect password returned 401. Original profile ciphertext was preserved. Regression tests cover missing profile keys and certificate-encrypted settings shared by two independent Data Protection providers. The development API used for verification is stopped afterwards to avoid locking Visual Studio build output.

## UAT migration completed (2026-10-02)

Migrated the one recoverable application key (`262cdee0-b540-479c-ad30-fed33aaf09bb`) into the existing settings table, wrapping it with a certificate. Verified that the database provider decrypts ciphertext created by the original local provider before switching configuration. The original local key ring and configuration backup are preserved under the API's ignored `.local` directory.

UAT now uses `DataProtection:KeyStore=Database` and `DataProtection:CertificateThumbprint`. The private certificate is in the current Windows user's Personal certificate store; no plaintext certificate-password file was created. Another application or service identity must be provisioned with the same certificate/private key (or a protected PFX through the existing CertificatePath settings) and use application name `MyFundex`. The database alone is intentionally insufficient to decrypt key material.

The requested key `011f6f12-cfdf-491e-a4ff-7b9ef86411d6` remains unavailable and was not invented or substituted. Its encrypted profile data remains preserved; use a backup of that key to recover the original names, or explicitly re-enter names in the profile dialog.
