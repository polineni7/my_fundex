START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001120738_AdminBrokerConfiguration') THEN
    ALTER TABLE fundex_broker."Accounts" ADD "BrokerUserId" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001120738_AdminBrokerConfiguration') THEN
    ALTER TABLE fundex_broker."Accounts" ADD "DisplayName" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001120738_AdminBrokerConfiguration') THEN
    ALTER TABLE fundex_broker."Accounts" ADD "IsDefault" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001120738_AdminBrokerConfiguration') THEN
    ALTER TABLE fundex_broker."Accounts" ADD "ProtectedCredentials" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001120738_AdminBrokerConfiguration') THEN
    ALTER TABLE fundex_broker."Accounts" ADD "SessionExpiresAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001120738_AdminBrokerConfiguration') THEN
    ALTER TABLE fundex_broker."Accounts" ADD "UseForMarketData" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001120738_AdminBrokerConfiguration') THEN
    CREATE UNIQUE INDEX "IX_Accounts_Environment" ON fundex_broker."Accounts" ("Environment") WHERE "IsDeleted" = false AND "IsActive" = true AND "IsDefault" = true;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001120738_AdminBrokerConfiguration') THEN
    CREATE UNIQUE INDEX "IX_Accounts_ProviderCode_Environment_AccountReference" ON fundex_broker."Accounts" ("ProviderCode", "Environment", "AccountReference") WHERE "IsDeleted" = false AND "ProtectedCredentials" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001120738_AdminBrokerConfiguration') THEN
    CREATE UNIQUE INDEX "IX_Accounts_UseForMarketData" ON fundex_broker."Accounts" ("UseForMarketData") WHERE "IsDeleted" = false AND "IsActive" = true AND "UseForMarketData" = true;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001120738_AdminBrokerConfiguration') THEN
    INSERT INTO fundex_integration."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261001120738_AdminBrokerConfiguration', '8.0.10');
    END IF;
END $EF$;
COMMIT;

