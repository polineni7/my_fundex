START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044915_AssessmentEnrollmentTerminology') THEN
    ALTER TABLE fundex_subscription."Subscriptions" DROP CONSTRAINT "PK_Subscriptions";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044915_AssessmentEnrollmentTerminology') THEN
    ALTER TABLE fundex_subscription."Subscriptions" RENAME TO "AssessmentEnrollments";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044915_AssessmentEnrollmentTerminology') THEN
    ALTER TABLE fundex_subscription."PlanVersions" RENAME COLUMN "RegistrationFee" TO "AssessmentFee";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044915_AssessmentEnrollmentTerminology') THEN
    ALTER TABLE fundex_subscription."AssessmentEnrollments" RENAME COLUMN "SubscriptionId" TO "EnrollmentId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044915_AssessmentEnrollmentTerminology') THEN
    ALTER TABLE fundex_subscription."AssessmentEnrollments" RENAME COLUMN "SubscribedAt" TO "EnrolledAt";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044915_AssessmentEnrollmentTerminology') THEN
    ALTER TABLE fundex_subscription."AssessmentEnrollments" RENAME COLUMN "PreviousSubscriptionId" TO "PreviousEnrollmentId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044915_AssessmentEnrollmentTerminology') THEN
    ALTER INDEX fundex_subscription."IX_Subscriptions_SubscriptionId" RENAME TO "IX_AssessmentEnrollments_EnrollmentId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044915_AssessmentEnrollmentTerminology') THEN
    ALTER INDEX fundex_subscription."IX_Subscriptions_PurchaseRequestId" RENAME TO "IX_AssessmentEnrollments_PurchaseRequestId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044915_AssessmentEnrollmentTerminology') THEN
    ALTER TABLE fundex_subscription."AssessmentEnrollments" ADD CONSTRAINT "PK_AssessmentEnrollments" PRIMARY KEY ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20261001044915_AssessmentEnrollmentTerminology') THEN
    INSERT INTO fundex_integration."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261001044915_AssessmentEnrollmentTerminology', '8.0.10');
    END IF;
END $EF$;
COMMIT;

