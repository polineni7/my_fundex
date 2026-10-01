START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044224_PlanProfitDeductions') THEN
    ALTER TABLE fundex_prod_trading."ProfitDistributions" ADD "OtherDeductionPercent" numeric(20,4) NOT NULL DEFAULT 0.0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044224_PlanProfitDeductions') THEN
    ALTER TABLE fundex_prod_trading."ProfitDistributions" ADD "OtherDeductions" numeric(20,4) NOT NULL DEFAULT 0.0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044224_PlanProfitDeductions') THEN
    ALTER TABLE fundex_prod_trading."ProfitDistributions" ADD "TaxWithheld" numeric(20,4) NOT NULL DEFAULT 0.0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044224_PlanProfitDeductions') THEN
    ALTER TABLE fundex_prod_trading."ProfitDistributions" ADD "TaxWithholdingPercent" numeric(20,4) NOT NULL DEFAULT 0.0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044224_PlanProfitDeductions') THEN
    ALTER TABLE fundex_subscription."PlanVersions" ADD "OtherDeductionPercent" numeric(20,4) NOT NULL DEFAULT 0.0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044224_PlanProfitDeductions') THEN
    ALTER TABLE fundex_subscription."PlanVersions" ADD "TaxWithholdingPercent" numeric(20,4) NOT NULL DEFAULT 0.0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044224_PlanProfitDeductions') THEN
    ALTER TABLE fundex_trading."Executions" ADD "OtherDeductionPercent" numeric(20,4);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044224_PlanProfitDeductions') THEN
    ALTER TABLE fundex_trading."Executions" ADD "RealizedProfit" numeric(20,4);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044224_PlanProfitDeductions') THEN
    ALTER TABLE fundex_trading."Executions" ADD "TaxWithholdingPercent" numeric(20,4);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044224_PlanProfitDeductions') THEN
    ALTER TABLE fundex_trading."Executions" ADD "TraderSharePercent" numeric(20,4);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044224_PlanProfitDeductions') THEN
    INSERT INTO fundex_integration."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261001044224_PlanProfitDeductions', '8.0.10');
    END IF;
END $EF$;
COMMIT;

