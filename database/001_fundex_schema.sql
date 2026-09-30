DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'fundex_integration') THEN
        CREATE SCHEMA fundex_integration;
    END IF;
END $EF$;
CREATE TABLE IF NOT EXISTS fundex_integration."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'fundex_accounts') THEN
            CREATE SCHEMA fundex_accounts;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'fundex_broker') THEN
            CREATE SCHEMA fundex_broker;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'fundex_risk') THEN
            CREATE SCHEMA fundex_risk;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'fundex_withdrawal') THEN
            CREATE SCHEMA fundex_withdrawal;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'fundex_subscription') THEN
            CREATE SCHEMA fundex_subscription;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'fundex_master') THEN
            CREATE SCHEMA fundex_master;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'fundex_wallet') THEN
            CREATE SCHEMA fundex_wallet;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'fundex_audit') THEN
            CREATE SCHEMA fundex_audit;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'fundex_trading') THEN
            CREATE SCHEMA fundex_trading;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'fundex_market') THEN
            CREATE SCHEMA fundex_market;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'fundex_payments') THEN
            CREATE SCHEMA fundex_payments;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'fundex_portfolio') THEN
            CREATE SCHEMA fundex_portfolio;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'fundex_notification') THEN
            CREATE SCHEMA fundex_notification;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'fundex_identity') THEN
            CREATE SCHEMA fundex_identity;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'fundex_configuration') THEN
            CREATE SCHEMA fundex_configuration;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
        IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'fundex_intelligence') THEN
            CREATE SCHEMA fundex_intelligence;
        END IF;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_accounts."Accounts" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "ProvisioningId" uuid,
        "TradingMode" text NOT NULL,
        "BrokerCredentialKey" text,
        "AccountId" uuid NOT NULL,
        "UserInternalId" bigint NOT NULL,
        "SubscriptionInternalId" bigint NOT NULL,
        "AccountNumber" text NOT NULL,
        "Status" text NOT NULL,
        "FundedCapital" numeric(20,4) NOT NULL,
        "WalletContribution" numeric(20,4) NOT NULL,
        "CurrentBuyingPower" numeric(20,4) NOT NULL,
        "CurrencyCode" text NOT NULL,
        "ActivePolicyAssignmentId" uuid,
        "ActivatedAt" timestamp with time zone,
        "SuspendedAt" timestamp with time zone,
        "ClosedAt" timestamp with time zone,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Accounts" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_broker."Accounts" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "BrokerAccountId" uuid NOT NULL,
        "ProviderCode" text NOT NULL,
        "Environment" text NOT NULL,
        "AccountReference" text NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Accounts" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_risk."Assignments" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "AssignmentId" uuid NOT NULL,
        "FundedAccountInternalId" bigint NOT NULL,
        "PolicyVersionInternalId" bigint NOT NULL,
        "EffectiveFrom" timestamp with time zone NOT NULL,
        "EffectiveTo" timestamp with time zone,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Assignments" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_withdrawal."Calculations" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "CalculationId" uuid NOT NULL,
        "WithdrawalInternalId" bigint NOT NULL,
        "GrossProfit" numeric(20,4) NOT NULL,
        "TaxAmount" numeric(20,4) NOT NULL,
        "PlatformFee" numeric(20,4) NOT NULL,
        "PlatformFeeTax" numeric(20,4) NOT NULL,
        "ProfitAfterDeductions" numeric(20,4) NOT NULL,
        "TraderSharePercentage" numeric(20,4) NOT NULL,
        "TraderEntitlement" numeric(20,4) NOT NULL,
        "CompanyShare" numeric(20,4) NOT NULL,
        "PolicyVersionId" uuid NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Calculations" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_accounts."CapitalAllocations" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "AllocationId" uuid NOT NULL,
        "FundedAccountInternalId" bigint NOT NULL,
        "AllocationType" text NOT NULL,
        "Amount" numeric(20,4) NOT NULL,
        "EffectiveAt" timestamp with time zone NOT NULL,
        "ReferenceType" text,
        "ReferenceId" text,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_CapitalAllocations" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_subscription."ChallengeAttempts" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "AttemptId" uuid NOT NULL,
        "SubscriptionInternalId" bigint NOT NULL,
        "StageInternalId" bigint NOT NULL,
        "AccountId" uuid,
        "Status" text NOT NULL,
        "StartedAt" timestamp with time zone NOT NULL,
        "CompletedAt" timestamp with time zone,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_ChallengeAttempts" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_master."Countries" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "CountryId" uuid NOT NULL,
        "Code" text NOT NULL,
        "Name" text NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Countries" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_master."Currencies" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "CurrencyId" uuid NOT NULL,
        "Code" text NOT NULL,
        "Name" text NOT NULL,
        "DecimalPlaces" integer NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Currencies" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_wallet."Entries" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "EntryId" uuid NOT NULL,
        "WalletInternalId" bigint NOT NULL,
        "TransactionInternalId" bigint NOT NULL,
        "Direction" text NOT NULL,
        "Amount" numeric(20,4) NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Entries" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_audit."Events" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "AuditId" uuid NOT NULL,
        "Module" text NOT NULL,
        "EntityType" text NOT NULL,
        "EntityId" text NOT NULL,
        "Action" text NOT NULL,
        "OldValue" text,
        "NewValue" text,
        "IpAddress" text,
        "CorrelationId" uuid NOT NULL,
        "OccurredAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Events" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_broker."Events" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "BrokerEventId" uuid NOT NULL,
        "Provider" text NOT NULL,
        "Environment" text NOT NULL,
        "EventType" text NOT NULL,
        "ProviderOrderId" text,
        "Payload" text NOT NULL,
        "ReceivedAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Events" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_master."Exchanges" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "ExchangeId" uuid NOT NULL,
        "Code" text NOT NULL,
        "Name" text NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Exchanges" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_trading."Executions" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "ExecutionId" uuid NOT NULL,
        "OrderInternalId" bigint NOT NULL,
        "BrokerExecutionId" text,
        "Quantity" numeric(20,4) NOT NULL,
        "Price" numeric(20,4) NOT NULL,
        "GrossValue" numeric(20,4) NOT NULL,
        "ExecutedAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Executions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_market."Instruments" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "InstrumentId" uuid NOT NULL,
        "InstrumentToken" text NOT NULL,
        "ExchangeCode" text NOT NULL,
        "Symbol" text NOT NULL,
        "TradingSymbol" text NOT NULL,
        "Isin" text,
        "Name" text NOT NULL,
        "SecurityType" text NOT NULL,
        "TickSize" numeric(20,4) NOT NULL,
        "LotSize" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Instruments" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_payments."Invoices" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "InvoiceId" uuid NOT NULL,
        "PaymentInternalId" bigint NOT NULL,
        "InvoiceNumber" text NOT NULL,
        "TaxableAmount" numeric(20,4) NOT NULL,
        "TaxAmount" numeric(20,4) NOT NULL,
        "GrossAmount" numeric(20,4) NOT NULL,
        "StorageKey" text,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Invoices" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_portfolio."Lots" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "LotId" uuid NOT NULL,
        "PositionInternalId" bigint NOT NULL,
        "ExecutionInternalId" bigint NOT NULL,
        "OriginalQuantity" numeric(20,4) NOT NULL,
        "RemainingQuantity" numeric(20,4) NOT NULL,
        "PurchasePrice" numeric(20,4) NOT NULL,
        "AcquiredAt" timestamp with time zone NOT NULL,
        "MustExitBy" timestamp with time zone,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Lots" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_notification."Notifications" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "NotificationId" uuid NOT NULL,
        "UserInternalId" bigint NOT NULL,
        "Type" text NOT NULL,
        "Title" text NOT NULL,
        "Message" text NOT NULL,
        "IsRead" boolean NOT NULL,
        "ReadAt" timestamp with time zone,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Notifications" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_trading."OrderEvents" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "OrderEventId" uuid NOT NULL,
        "OrderInternalId" bigint NOT NULL,
        "EventType" text NOT NULL,
        "OldStatus" text,
        "NewStatus" text NOT NULL,
        "ProviderPayload" text,
        "OccurredAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_OrderEvents" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_trading."Orders" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "BrokerEnvironment" text NOT NULL,
        "BrokerCredentialKey" text,
        "OrderId" uuid NOT NULL,
        "AccountId" uuid NOT NULL,
        "FundedAccountInternalId" bigint NOT NULL,
        "InstrumentToken" text NOT NULL,
        "Symbol" text NOT NULL,
        "Side" text NOT NULL,
        "OrderType" text NOT NULL,
        "Quantity" numeric(20,6) NOT NULL,
        "RequestedPrice" numeric(20,4),
        "EstimatedPrice" numeric(20,6) NOT NULL,
        "FilledQuantity" numeric(20,4) NOT NULL,
        "AverageFillPrice" numeric(20,4),
        "Status" text NOT NULL,
        "BrokerOrderId" text,
        "IdempotencyKey" uuid NOT NULL,
        "CorrelationId" uuid NOT NULL,
        "PlacedAt" timestamp with time zone,
        "CompletedAt" timestamp with time zone,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Orders" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_payments."Payments" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "FulfilledAt" timestamp with time zone,
        "PaymentId" uuid NOT NULL,
        "UserInternalId" bigint NOT NULL,
        "SubscriptionId" uuid,
        "Provider" text NOT NULL,
        "ProviderOrderId" text,
        "IdempotencyKey" uuid NOT NULL,
        "ProviderPaymentId" text,
        "Amount" numeric(20,4) NOT NULL,
        "CurrencyCode" text NOT NULL,
        "Status" text NOT NULL,
        "Purpose" text NOT NULL,
        "PaidAt" timestamp with time zone,
        "FailureReason" text,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Payments" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_identity."Permissions" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "PermissionId" uuid NOT NULL,
        "Code" text NOT NULL,
        "Name" text NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Permissions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_subscription."Plans" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "PlanId" uuid NOT NULL,
        "Code" text NOT NULL,
        "Name" text NOT NULL,
        "Description" text,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Plans" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_subscription."PlanVersions" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "Path" text NOT NULL,
        "RewardSharePercent" numeric(20,4) NOT NULL,
        "PlanVersionId" uuid NOT NULL,
        "PlanInternalId" bigint NOT NULL,
        "VersionNumber" integer NOT NULL,
        "ChallengeCapital" numeric(20,4) NOT NULL,
        "RegistrationFee" numeric(20,4) NOT NULL,
        "Status" text NOT NULL,
        "EffectiveFrom" timestamp with time zone NOT NULL,
        "EffectiveTo" timestamp with time zone,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_PlanVersions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_risk."PolicySets" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "PolicyId" uuid NOT NULL,
        "Code" text NOT NULL,
        "Name" text NOT NULL,
        "PolicyType" text NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_PolicySets" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_risk."PolicyVersions" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "PolicyVersionId" uuid NOT NULL,
        "PolicySetInternalId" bigint NOT NULL,
        "VersionNumber" integer NOT NULL,
        "Status" text NOT NULL,
        "EffectiveFrom" timestamp with time zone NOT NULL,
        "EffectiveTo" timestamp with time zone,
        "ApplicationMode" text NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_PolicyVersions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_portfolio."Positions" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "PositionId" uuid NOT NULL,
        "FundedAccountInternalId" bigint NOT NULL,
        "InstrumentToken" text NOT NULL,
        "Symbol" text NOT NULL,
        "Quantity" numeric(20,4) NOT NULL,
        "AverageCost" numeric(20,4) NOT NULL,
        "RealizedPnl" numeric(20,4) NOT NULL,
        "UnrealizedPnl" numeric(20,4) NOT NULL,
        "Status" text NOT NULL,
        "OpenedAt" timestamp with time zone NOT NULL,
        "ClosedAt" timestamp with time zone,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Positions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_payments."Receipts" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "ReceiptId" uuid NOT NULL,
        "PaymentInternalId" bigint NOT NULL,
        "ReceiptNumber" text NOT NULL,
        "StorageKey" text,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Receipts" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_withdrawal."Requests" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "WithdrawalId" uuid NOT NULL,
        "FundedAccountInternalId" bigint NOT NULL,
        "RequestedAmount" numeric(20,4) NOT NULL,
        "EligibleAmount" numeric(20,4) NOT NULL,
        "NetPayoutAmount" numeric(20,4) NOT NULL,
        "Status" text NOT NULL,
        "RequestedAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Requests" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_identity."RolePermissions" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "RoleInternalId" bigint NOT NULL,
        "PermissionInternalId" bigint NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_RolePermissions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_identity."Roles" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "RoleId" uuid NOT NULL,
        "Code" text NOT NULL,
        "Name" text NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Roles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_risk."Rules" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "RuleId" uuid NOT NULL,
        "PolicyVersionInternalId" bigint NOT NULL,
        "RuleCode" text NOT NULL,
        "Name" text NOT NULL,
        "Category" text NOT NULL,
        "Operator" text NOT NULL,
        "ViolationAction" text NOT NULL,
        "Priority" integer NOT NULL,
        "IsEnabled" boolean NOT NULL,
        "DecimalValue" numeric(20,6),
        "IntegerValue" integer,
        "StringValue" text,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Rules" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_master."SecurityTypes" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "SecurityTypeId" uuid NOT NULL,
        "Code" text NOT NULL,
        "Name" text NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_SecurityTypes" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_configuration."Settings" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "SettingId" uuid NOT NULL,
        "SettingKey" text NOT NULL,
        "SettingValue" text NOT NULL,
        "ValueType" text NOT NULL,
        "Category" text NOT NULL,
        "Environment" text NOT NULL,
        "IsSensitive" boolean NOT NULL,
        "Description" text,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Settings" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_intelligence."Signals" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "SignalId" uuid NOT NULL,
        "InstrumentToken" text NOT NULL,
        "SignalTime" timestamp with time zone NOT NULL,
        "Direction" text NOT NULL,
        "Strength" text NOT NULL,
        "Confidence" numeric(20,4) NOT NULL,
        "ExpectedMoveMin" numeric(20,4),
        "ExpectedMoveMax" numeric(20,4),
        "RiskScore" numeric(20,4) NOT NULL,
        "MarketRegime" text,
        "ModelVersion" text NOT NULL,
        "ReasonsJson" text NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Signals" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_subscription."Stages" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "ProfitTargetPercent" numeric(20,4) NOT NULL,
        "MaxDailyLossPercent" numeric(20,4) NOT NULL,
        "MaxTotalLossPercent" numeric(20,4) NOT NULL,
        "MinimumTradingDays" integer NOT NULL,
        "MaximumCalendarDays" integer,
        "StageId" uuid NOT NULL,
        "PlanVersionInternalId" bigint NOT NULL,
        "StageNumber" integer NOT NULL,
        "Name" text NOT NULL,
        "StartingCapital" numeric(20,4) NOT NULL,
        "PolicySetId" uuid NOT NULL,
        "RequiredForCompletion" boolean NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Stages" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_accounts."StatusHistory" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "StatusHistoryId" uuid NOT NULL,
        "FundedAccountInternalId" bigint NOT NULL,
        "FromStatus" text,
        "ToStatus" text NOT NULL,
        "ReasonCode" text,
        "Reason" text,
        "ChangedAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_StatusHistory" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_subscription."Subscriptions" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "SubscriptionId" uuid NOT NULL,
        "UserInternalId" bigint NOT NULL,
        "PlanInternalId" bigint NOT NULL,
        "PlanVersionInternalId" bigint NOT NULL,
        "Status" text NOT NULL,
        "SubscribedAt" timestamp with time zone NOT NULL,
        "ActivatedAt" timestamp with time zone,
        "CompletedAt" timestamp with time zone,
        "ExpiredAt" timestamp with time zone,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Subscriptions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_market."TradingCalendar" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "CalendarDayId" uuid NOT NULL,
        "TradeDate" date NOT NULL,
        "ExchangeCode" text NOT NULL,
        "IsTradingDay" boolean NOT NULL,
        "SessionOpen" time without time zone,
        "SessionClose" time without time zone,
        "HolidayName" text,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_TradingCalendar" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_wallet."Transactions" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "TransactionId" uuid NOT NULL,
        "TransactionType" text NOT NULL,
        "ReferenceType" text NOT NULL,
        "ReferenceId" text NOT NULL,
        "Description" text NOT NULL,
        "OccurredAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Transactions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_identity."UserRoles" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "UserInternalId" bigint NOT NULL,
        "RoleInternalId" bigint NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_UserRoles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_identity."Users" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "GoogleSubject" text,
        "UserId" uuid NOT NULL,
        "Email" text NOT NULL,
        "PasswordHash" text NOT NULL,
        "FirstName" text NOT NULL,
        "LastName" text NOT NULL,
        "Status" text NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_risk."Violations" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "ViolationId" uuid NOT NULL,
        "FundedAccountInternalId" bigint NOT NULL,
        "PolicyRuleInternalId" bigint NOT NULL,
        "ObservedValue" text,
        "LimitValue" text,
        "ActionTaken" text NOT NULL,
        "OccurredAt" timestamp with time zone NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Violations" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE TABLE fundex_wallet."Wallets" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "WalletId" uuid NOT NULL,
        "FundedAccountInternalId" bigint NOT NULL,
        "CurrencyCode" text NOT NULL,
        "Status" text NOT NULL,
        "CachedAvailableBalance" numeric(20,4) NOT NULL,
        "CachedWithdrawableBalance" numeric(20,4) NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "CreatedBy" bigint NOT NULL,
        "UpdatedAt" timestamptz,
        "UpdatedBy" bigint,
        "DeletedAt" timestamptz,
        "DeletedBy" bigint,
        "IsDeleted" boolean NOT NULL,
        "Version" bigint NOT NULL,
        CONSTRAINT "PK_Wallets" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Accounts_AccountId" ON fundex_accounts."Accounts" ("AccountId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Accounts_AccountNumber" ON fundex_accounts."Accounts" ("AccountNumber") WHERE "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Accounts_ProvisioningId" ON fundex_accounts."Accounts" ("ProvisioningId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Accounts_BrokerAccountId" ON fundex_broker."Accounts" ("BrokerAccountId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Assignments_FundedAccountInternalId_PolicyVersionInternalId" ON fundex_risk."Assignments" ("FundedAccountInternalId", "PolicyVersionInternalId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_ChallengeAttempts_AttemptId" ON fundex_subscription."ChallengeAttempts" ("AttemptId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_ChallengeAttempts_SubscriptionInternalId_StageInternalId" ON fundex_subscription."ChallengeAttempts" ("SubscriptionInternalId", "StageInternalId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Countries_Code" ON fundex_master."Countries" ("Code") WHERE "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Currencies_Code" ON fundex_master."Currencies" ("Code") WHERE "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Events_AuditId" ON fundex_audit."Events" ("AuditId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE INDEX "IX_Events_Module_OccurredAt" ON fundex_audit."Events" ("Module", "OccurredAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Events_BrokerEventId" ON fundex_broker."Events" ("BrokerEventId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Exchanges_Code" ON fundex_master."Exchanges" ("Code") WHERE "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Instruments_InstrumentId" ON fundex_market."Instruments" ("InstrumentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Instruments_InstrumentToken" ON fundex_market."Instruments" ("InstrumentToken") WHERE "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE INDEX "IX_Instruments_TradingSymbol" ON fundex_market."Instruments" ("TradingSymbol");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Notifications_NotificationId" ON fundex_notification."Notifications" ("NotificationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE INDEX "IX_Notifications_UserInternalId_IsRead" ON fundex_notification."Notifications" ("UserInternalId", "IsRead");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE INDEX "IX_Orders_FundedAccountInternalId_CreatedAt" ON fundex_trading."Orders" ("FundedAccountInternalId", "CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Orders_IdempotencyKey" ON fundex_trading."Orders" ("IdempotencyKey");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Orders_OrderId" ON fundex_trading."Orders" ("OrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Payments_IdempotencyKey" ON fundex_payments."Payments" ("IdempotencyKey");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Payments_PaymentId" ON fundex_payments."Payments" ("PaymentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Payments_ProviderOrderId" ON fundex_payments."Payments" ("ProviderOrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Payments_ProviderPaymentId" ON fundex_payments."Payments" ("ProviderPaymentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Payments_SubscriptionId" ON fundex_payments."Payments" ("SubscriptionId") WHERE "Status" <> 'Failed' AND "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Permissions_Code" ON fundex_identity."Permissions" ("Code") WHERE "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Plans_Code" ON fundex_subscription."Plans" ("Code") WHERE "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Plans_PlanId" ON fundex_subscription."Plans" ("PlanId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_PlanVersions_PlanInternalId_VersionNumber" ON fundex_subscription."PlanVersions" ("PlanInternalId", "VersionNumber") WHERE "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_PolicySets_Code" ON fundex_risk."PolicySets" ("Code") WHERE "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_PolicySets_PolicyId" ON fundex_risk."PolicySets" ("PolicyId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE INDEX "IX_Positions_FundedAccountInternalId_InstrumentToken" ON fundex_portfolio."Positions" ("FundedAccountInternalId", "InstrumentToken") WHERE "IsDeleted" = false AND "Status"='Open';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Positions_PositionId" ON fundex_portfolio."Positions" ("PositionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Receipts_PaymentInternalId" ON fundex_payments."Receipts" ("PaymentInternalId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Requests_WithdrawalId" ON fundex_withdrawal."Requests" ("WithdrawalId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Roles_Code" ON fundex_identity."Roles" ("Code") WHERE "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_SecurityTypes_Code" ON fundex_master."SecurityTypes" ("Code") WHERE "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Settings_SettingKey_Environment" ON fundex_configuration."Settings" ("SettingKey", "Environment") WHERE "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE INDEX "IX_Signals_InstrumentToken_SignalTime" ON fundex_intelligence."Signals" ("InstrumentToken", "SignalTime");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Signals_SignalId" ON fundex_intelligence."Signals" ("SignalId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Subscriptions_SubscriptionId" ON fundex_subscription."Subscriptions" ("SubscriptionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_TradingCalendar_ExchangeCode_TradeDate" ON fundex_market."TradingCalendar" ("ExchangeCode", "TradeDate") WHERE "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Users_Email" ON fundex_identity."Users" ("Email") WHERE "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Users_GoogleSubject" ON fundex_identity."Users" ("GoogleSubject");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Users_UserId" ON fundex_identity."Users" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Wallets_FundedAccountInternalId" ON fundex_wallet."Wallets" ("FundedAccountInternalId") WHERE "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    CREATE UNIQUE INDEX "IX_Wallets_WalletId" ON fundex_wallet."Wallets" ("WalletId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM fundex_integration."__EFMigrationsHistory" WHERE "MigrationId" = '20260930143856_InitialFundexSchemas') THEN
    INSERT INTO fundex_integration."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260930143856_InitialFundexSchemas', '8.0.10');
    END IF;
END $EF$;
COMMIT;

