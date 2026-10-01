START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001111918_ConfigurableAssessmentTerms') THEN
    ALTER TABLE fundex_subscription."Stages" ALTER COLUMN "MinimumTradingDays" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001111918_ConfigurableAssessmentTerms') THEN
    ALTER TABLE fundex_subscription."Stages" ADD "MaximumLeverage" numeric(20,4);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001111918_ConfigurableAssessmentTerms') THEN
    ALTER TABLE fundex_subscription."Stages" ADD "TradingPeriod" integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001111918_ConfigurableAssessmentTerms') THEN
    INSERT INTO fundex_integration."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261001111918_ConfigurableAssessmentTerms', '8.0.10');
    END IF;
END $EF$;
COMMIT;

