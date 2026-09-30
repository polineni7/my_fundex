START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930190224_TradingRoutesAndPurchaseReferences') THEN
    ALTER TABLE fundex_wallet."Transactions" ADD "PostingKey" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930190224_TradingRoutesAndPurchaseReferences') THEN
    ALTER TABLE fundex_subscription."Subscriptions" ADD "PreviousSubscriptionId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930190224_TradingRoutesAndPurchaseReferences') THEN
    ALTER TABLE fundex_subscription."Subscriptions" ADD "PurchaseRequestId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930190224_TradingRoutesAndPurchaseReferences') THEN
    ALTER TABLE fundex_trading."Orders" ADD "BrokerProvider" text NOT NULL DEFAULT 'Upstox';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930190224_TradingRoutesAndPurchaseReferences') THEN
    ALTER TABLE fundex_accounts."Accounts" ADD "BrokerProvider" text NOT NULL DEFAULT 'Upstox';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930190224_TradingRoutesAndPurchaseReferences') THEN
    CREATE UNIQUE INDEX "IX_Transactions_PostingKey" ON fundex_wallet."Transactions" ("PostingKey");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930190224_TradingRoutesAndPurchaseReferences') THEN
    CREATE UNIQUE INDEX "IX_Subscriptions_PurchaseRequestId" ON fundex_subscription."Subscriptions" ("PurchaseRequestId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930190224_TradingRoutesAndPurchaseReferences') THEN
    INSERT INTO fundex_integration."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260930190224_TradingRoutesAndPurchaseReferences', '8.0.10');
    END IF;
END $EF$;
COMMIT;

