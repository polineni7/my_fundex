using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyFundex.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialFundexSchemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "fundex_accounts");

            migrationBuilder.EnsureSchema(name: "fundex_broker");

            migrationBuilder.EnsureSchema(name: "fundex_risk");

            migrationBuilder.EnsureSchema(name: "fundex_withdrawal");

            migrationBuilder.EnsureSchema(name: "fundex_subscription");

            migrationBuilder.EnsureSchema(name: "fundex_master");

            migrationBuilder.EnsureSchema(name: "fundex_wallet");

            migrationBuilder.EnsureSchema(name: "fundex_audit");

            migrationBuilder.EnsureSchema(name: "fundex_trading");

            migrationBuilder.EnsureSchema(name: "fundex_market");

            migrationBuilder.EnsureSchema(name: "fundex_payments");

            migrationBuilder.EnsureSchema(name: "fundex_portfolio");

            migrationBuilder.EnsureSchema(name: "fundex_notification");

            migrationBuilder.EnsureSchema(name: "fundex_identity");

            migrationBuilder.EnsureSchema(name: "fundex_configuration");

            migrationBuilder.EnsureSchema(name: "fundex_intelligence");

            migrationBuilder.CreateTable(
                name: "Accounts",
                schema: "fundex_accounts",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    ProvisioningId = table.Column<Guid>(type: "uuid", nullable: true),
                    TradingMode = table.Column<string>(type: "text", nullable: false),
                    BrokerCredentialKey = table.Column<string>(type: "text", nullable: true),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserInternalId = table.Column<long>(type: "bigint", nullable: false),
                    SubscriptionInternalId = table.Column<long>(type: "bigint", nullable: false),
                    AccountNumber = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    FundedCapital = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    WalletContribution = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    CurrentBuyingPower = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    CurrencyCode = table.Column<string>(type: "text", nullable: false),
                    ActivePolicyAssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActivatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    SuspendedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    ClosedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Accounts",
                schema: "fundex_broker",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    BrokerAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderCode = table.Column<string>(type: "text", nullable: false),
                    Environment = table.Column<string>(type: "text", nullable: false),
                    AccountReference = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Assignments",
                schema: "fundex_risk",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    FundedAccountInternalId = table.Column<long>(type: "bigint", nullable: false),
                    PolicyVersionInternalId = table.Column<long>(type: "bigint", nullable: false),
                    EffectiveFrom = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    EffectiveTo = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Calculations",
                schema: "fundex_withdrawal",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    CalculationId = table.Column<Guid>(type: "uuid", nullable: false),
                    WithdrawalInternalId = table.Column<long>(type: "bigint", nullable: false),
                    GrossProfit = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    TaxAmount = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    PlatformFee = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    PlatformFeeTax = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    ProfitAfterDeductions = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    TraderSharePercentage = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    TraderEntitlement = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    CompanyShare = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    PolicyVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calculations", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "CapitalAllocations",
                schema: "fundex_accounts",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    AllocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FundedAccountInternalId = table.Column<long>(type: "bigint", nullable: false),
                    AllocationType = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    EffectiveAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    ReferenceType = table.Column<string>(type: "text", nullable: true),
                    ReferenceId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapitalAllocations", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "ChallengeAttempts",
                schema: "fundex_subscription",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionInternalId = table.Column<long>(type: "bigint", nullable: false),
                    StageInternalId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    CompletedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeAttempts", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Countries",
                schema: "fundex_master",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Currencies",
                schema: "fundex_master",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    CurrencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DecimalPlaces = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Entries",
                schema: "fundex_wallet",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    EntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletInternalId = table.Column<long>(type: "bigint", nullable: false),
                    TransactionInternalId = table.Column<long>(type: "bigint", nullable: false),
                    Direction = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entries", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Events",
                schema: "fundex_audit",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    Module = table.Column<string>(type: "text", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    OldValue = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Events",
                schema: "fundex_broker",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    BrokerEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    Environment = table.Column<string>(type: "text", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    ProviderOrderId = table.Column<string>(type: "text", nullable: true),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Exchanges",
                schema: "fundex_master",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    ExchangeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exchanges", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Executions",
                schema: "fundex_trading",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    ExecutionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderInternalId = table.Column<long>(type: "bigint", nullable: false),
                    BrokerExecutionId = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    Price = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    GrossValue = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    ExecutedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Executions", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Instruments",
                schema: "fundex_market",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    InstrumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstrumentToken = table.Column<string>(type: "text", nullable: false),
                    ExchangeCode = table.Column<string>(type: "text", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    TradingSymbol = table.Column<string>(type: "text", nullable: false),
                    Isin = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SecurityType = table.Column<string>(type: "text", nullable: false),
                    TickSize = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    LotSize = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instruments", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Invoices",
                schema: "fundex_payments",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentInternalId = table.Column<long>(type: "bigint", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "text", nullable: false),
                    TaxableAmount = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    TaxAmount = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    GrossAmount = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    StorageKey = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Lots",
                schema: "fundex_portfolio",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    LotId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionInternalId = table.Column<long>(type: "bigint", nullable: false),
                    ExecutionInternalId = table.Column<long>(type: "bigint", nullable: false),
                    OriginalQuantity = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    RemainingQuantity = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    PurchasePrice = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    AcquiredAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    MustExitBy = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lots", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "fundex_notification",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserInternalId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "OrderEvents",
                schema: "fundex_trading",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    OrderEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderInternalId = table.Column<long>(type: "bigint", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    OldStatus = table.Column<string>(type: "text", nullable: true),
                    NewStatus = table.Column<string>(type: "text", nullable: false),
                    ProviderPayload = table.Column<string>(type: "text", nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderEvents", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Orders",
                schema: "fundex_trading",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    BrokerEnvironment = table.Column<string>(type: "text", nullable: false),
                    BrokerCredentialKey = table.Column<string>(type: "text", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    FundedAccountInternalId = table.Column<long>(type: "bigint", nullable: false),
                    InstrumentToken = table.Column<string>(type: "text", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Side = table.Column<string>(type: "text", nullable: false),
                    OrderType = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<decimal>(
                        type: "numeric(20,6)",
                        precision: 20,
                        scale: 6,
                        nullable: false
                    ),
                    RequestedPrice = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: true
                    ),
                    EstimatedPrice = table.Column<decimal>(
                        type: "numeric(20,6)",
                        precision: 20,
                        scale: 6,
                        nullable: false
                    ),
                    FilledQuantity = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    AverageFillPrice = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: true
                    ),
                    Status = table.Column<string>(type: "text", nullable: false),
                    BrokerOrderId = table.Column<string>(type: "text", nullable: true),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlacedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    CompletedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Payments",
                schema: "fundex_payments",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    FulfilledAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserInternalId = table.Column<long>(type: "bigint", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    ProviderOrderId = table.Column<string>(type: "text", nullable: true),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderPaymentId = table.Column<string>(type: "text", nullable: true),
                    Amount = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    CurrencyCode = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Purpose = table.Column<string>(type: "text", nullable: false),
                    PaidAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Permissions",
                schema: "fundex_identity",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Plans",
                schema: "fundex_subscription",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "PlanVersions",
                schema: "fundex_subscription",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    Path = table.Column<string>(type: "text", nullable: false),
                    RewardSharePercent = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    PlanVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanInternalId = table.Column<long>(type: "bigint", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    ChallengeCapital = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    RegistrationFee = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    Status = table.Column<string>(type: "text", nullable: false),
                    EffectiveFrom = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    EffectiveTo = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanVersions", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "PolicySets",
                schema: "fundex_risk",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PolicyType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicySets", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "PolicyVersions",
                schema: "fundex_risk",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    PolicyVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicySetInternalId = table.Column<long>(type: "bigint", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    EffectiveFrom = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    EffectiveTo = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    ApplicationMode = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyVersions", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Positions",
                schema: "fundex_portfolio",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FundedAccountInternalId = table.Column<long>(type: "bigint", nullable: false),
                    InstrumentToken = table.Column<string>(type: "text", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    AverageCost = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    RealizedPnl = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    UnrealizedPnl = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    Status = table.Column<string>(type: "text", nullable: false),
                    OpenedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    ClosedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Receipts",
                schema: "fundex_payments",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    ReceiptId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentInternalId = table.Column<long>(type: "bigint", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "text", nullable: false),
                    StorageKey = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipts", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Requests",
                schema: "fundex_withdrawal",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    WithdrawalId = table.Column<Guid>(type: "uuid", nullable: false),
                    FundedAccountInternalId = table.Column<long>(type: "bigint", nullable: false),
                    RequestedAmount = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    EligibleAmount = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    NetPayoutAmount = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                schema: "fundex_identity",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    RoleInternalId = table.Column<long>(type: "bigint", nullable: false),
                    PermissionInternalId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "fundex_identity",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Rules",
                schema: "fundex_risk",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    RuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyVersionInternalId = table.Column<long>(type: "bigint", nullable: false),
                    RuleCode = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Operator = table.Column<string>(type: "text", nullable: false),
                    ViolationAction = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DecimalValue = table.Column<decimal>(
                        type: "numeric(20,6)",
                        precision: 20,
                        scale: 6,
                        nullable: true
                    ),
                    IntegerValue = table.Column<int>(type: "integer", nullable: true),
                    StringValue = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rules", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "SecurityTypes",
                schema: "fundex_master",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    SecurityTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityTypes", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Settings",
                schema: "fundex_configuration",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    SettingId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettingKey = table.Column<string>(type: "text", nullable: false),
                    SettingValue = table.Column<string>(type: "text", nullable: false),
                    ValueType = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Environment = table.Column<string>(type: "text", nullable: false),
                    IsSensitive = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Signals",
                schema: "fundex_intelligence",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    SignalId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstrumentToken = table.Column<string>(type: "text", nullable: false),
                    SignalTime = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    Direction = table.Column<string>(type: "text", nullable: false),
                    Strength = table.Column<string>(type: "text", nullable: false),
                    Confidence = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    ExpectedMoveMin = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: true
                    ),
                    ExpectedMoveMax = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: true
                    ),
                    RiskScore = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    MarketRegime = table.Column<string>(type: "text", nullable: true),
                    ModelVersion = table.Column<string>(type: "text", nullable: false),
                    ReasonsJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signals", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Stages",
                schema: "fundex_subscription",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    ProfitTargetPercent = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    MaxDailyLossPercent = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    MaxTotalLossPercent = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    MinimumTradingDays = table.Column<int>(type: "integer", nullable: false),
                    MaximumCalendarDays = table.Column<int>(type: "integer", nullable: true),
                    StageId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanVersionInternalId = table.Column<long>(type: "bigint", nullable: false),
                    StageNumber = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StartingCapital = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    PolicySetId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequiredForCompletion = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stages", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "StatusHistory",
                schema: "fundex_accounts",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    StatusHistoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    FundedAccountInternalId = table.Column<long>(type: "bigint", nullable: false),
                    FromStatus = table.Column<string>(type: "text", nullable: true),
                    ToStatus = table.Column<string>(type: "text", nullable: false),
                    ReasonCode = table.Column<string>(type: "text", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusHistory", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                schema: "fundex_subscription",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserInternalId = table.Column<long>(type: "bigint", nullable: false),
                    PlanInternalId = table.Column<long>(type: "bigint", nullable: false),
                    PlanVersionInternalId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SubscribedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    ActivatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    CompletedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    ExpiredAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "TradingCalendar",
                schema: "fundex_market",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    CalendarDayId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExchangeCode = table.Column<string>(type: "text", nullable: false),
                    IsTradingDay = table.Column<bool>(type: "boolean", nullable: false),
                    SessionOpen = table.Column<TimeOnly>(
                        type: "time without time zone",
                        nullable: true
                    ),
                    SessionClose = table.Column<TimeOnly>(
                        type: "time without time zone",
                        nullable: true
                    ),
                    HolidayName = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradingCalendar", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Transactions",
                schema: "fundex_wallet",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<string>(type: "text", nullable: false),
                    ReferenceType = table.Column<string>(type: "text", nullable: false),
                    ReferenceId = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "fundex_identity",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    UserInternalId = table.Column<long>(type: "bigint", nullable: false),
                    RoleInternalId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "fundex_identity",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    GoogleSubject = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Violations",
                schema: "fundex_risk",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    ViolationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FundedAccountInternalId = table.Column<long>(type: "bigint", nullable: false),
                    PolicyRuleInternalId = table.Column<long>(type: "bigint", nullable: false),
                    ObservedValue = table.Column<string>(type: "text", nullable: true),
                    LimitValue = table.Column<string>(type: "text", nullable: true),
                    ActionTaken = table.Column<string>(type: "text", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Violations", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Wallets",
                schema: "fundex_wallet",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    FundedAccountInternalId = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyCode = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CachedAvailableBalance = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    CachedWithdrawableBalance = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccountId",
                schema: "fundex_accounts",
                table: "Accounts",
                column: "AccountId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccountNumber",
                schema: "fundex_accounts",
                table: "Accounts",
                column: "AccountNumber",
                unique: true,
                filter: "\"IsDeleted\" = false"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ProvisioningId",
                schema: "fundex_accounts",
                table: "Accounts",
                column: "ProvisioningId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_BrokerAccountId",
                schema: "fundex_broker",
                table: "Accounts",
                column: "BrokerAccountId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_FundedAccountInternalId_PolicyVersionInternalId",
                schema: "fundex_risk",
                table: "Assignments",
                columns: new[] { "FundedAccountInternalId", "PolicyVersionInternalId" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeAttempts_AttemptId",
                schema: "fundex_subscription",
                table: "ChallengeAttempts",
                column: "AttemptId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeAttempts_SubscriptionInternalId_StageInternalId",
                schema: "fundex_subscription",
                table: "ChallengeAttempts",
                columns: new[] { "SubscriptionInternalId", "StageInternalId" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Code",
                schema: "fundex_master",
                table: "Countries",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                schema: "fundex_master",
                table: "Currencies",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Events_AuditId",
                schema: "fundex_audit",
                table: "Events",
                column: "AuditId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Events_Module_OccurredAt",
                schema: "fundex_audit",
                table: "Events",
                columns: new[] { "Module", "OccurredAt" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Events_BrokerEventId",
                schema: "fundex_broker",
                table: "Events",
                column: "BrokerEventId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Exchanges_Code",
                schema: "fundex_master",
                table: "Exchanges",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_InstrumentId",
                schema: "fundex_market",
                table: "Instruments",
                column: "InstrumentId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_InstrumentToken",
                schema: "fundex_market",
                table: "Instruments",
                column: "InstrumentToken",
                unique: true,
                filter: "\"IsDeleted\" = false"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_TradingSymbol",
                schema: "fundex_market",
                table: "Instruments",
                column: "TradingSymbol"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_NotificationId",
                schema: "fundex_notification",
                table: "Notifications",
                column: "NotificationId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserInternalId_IsRead",
                schema: "fundex_notification",
                table: "Notifications",
                columns: new[] { "UserInternalId", "IsRead" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Orders_FundedAccountInternalId_CreatedAt",
                schema: "fundex_trading",
                table: "Orders",
                columns: new[] { "FundedAccountInternalId", "CreatedAt" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Orders_IdempotencyKey",
                schema: "fundex_trading",
                table: "Orders",
                column: "IdempotencyKey",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderId",
                schema: "fundex_trading",
                table: "Orders",
                column: "OrderId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Payments_IdempotencyKey",
                schema: "fundex_payments",
                table: "Payments",
                column: "IdempotencyKey",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentId",
                schema: "fundex_payments",
                table: "Payments",
                column: "PaymentId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ProviderOrderId",
                schema: "fundex_payments",
                table: "Payments",
                column: "ProviderOrderId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ProviderPaymentId",
                schema: "fundex_payments",
                table: "Payments",
                column: "ProviderPaymentId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SubscriptionId",
                schema: "fundex_payments",
                table: "Payments",
                column: "SubscriptionId",
                unique: true,
                filter: "\"Status\" <> 'Failed' AND \"IsDeleted\" = false"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                schema: "fundex_identity",
                table: "Permissions",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Plans_Code",
                schema: "fundex_subscription",
                table: "Plans",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Plans_PlanId",
                schema: "fundex_subscription",
                table: "Plans",
                column: "PlanId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_PlanVersions_PlanInternalId_VersionNumber",
                schema: "fundex_subscription",
                table: "PlanVersions",
                columns: new[] { "PlanInternalId", "VersionNumber" },
                unique: true,
                filter: "\"IsDeleted\" = false"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PolicySets_Code",
                schema: "fundex_risk",
                table: "PolicySets",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false"
            );

            migrationBuilder.CreateIndex(
                name: "IX_PolicySets_PolicyId",
                schema: "fundex_risk",
                table: "PolicySets",
                column: "PolicyId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Positions_FundedAccountInternalId_InstrumentToken",
                schema: "fundex_portfolio",
                table: "Positions",
                columns: new[] { "FundedAccountInternalId", "InstrumentToken" },
                filter: "\"IsDeleted\" = false AND \"Status\"='Open'"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Positions_PositionId",
                schema: "fundex_portfolio",
                table: "Positions",
                column: "PositionId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_PaymentInternalId",
                schema: "fundex_payments",
                table: "Receipts",
                column: "PaymentInternalId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Requests_WithdrawalId",
                schema: "fundex_withdrawal",
                table: "Requests",
                column: "WithdrawalId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Code",
                schema: "fundex_identity",
                table: "Roles",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false"
            );

            migrationBuilder.CreateIndex(
                name: "IX_SecurityTypes_Code",
                schema: "fundex_master",
                table: "SecurityTypes",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Settings_SettingKey_Environment",
                schema: "fundex_configuration",
                table: "Settings",
                columns: new[] { "SettingKey", "Environment" },
                unique: true,
                filter: "\"IsDeleted\" = false"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Signals_InstrumentToken_SignalTime",
                schema: "fundex_intelligence",
                table: "Signals",
                columns: new[] { "InstrumentToken", "SignalTime" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Signals_SignalId",
                schema: "fundex_intelligence",
                table: "Signals",
                column: "SignalId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_SubscriptionId",
                schema: "fundex_subscription",
                table: "Subscriptions",
                column: "SubscriptionId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_TradingCalendar_ExchangeCode_TradeDate",
                schema: "fundex_market",
                table: "TradingCalendar",
                columns: new[] { "ExchangeCode", "TradeDate" },
                unique: true,
                filter: "\"IsDeleted\" = false"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "fundex_identity",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "\"IsDeleted\" = false"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Users_GoogleSubject",
                schema: "fundex_identity",
                table: "Users",
                column: "GoogleSubject",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserId",
                schema: "fundex_identity",
                table: "Users",
                column: "UserId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_FundedAccountInternalId",
                schema: "fundex_wallet",
                table: "Wallets",
                column: "FundedAccountInternalId",
                unique: true,
                filter: "\"IsDeleted\" = false"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_WalletId",
                schema: "fundex_wallet",
                table: "Wallets",
                column: "WalletId",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Accounts", schema: "fundex_accounts");

            migrationBuilder.DropTable(name: "Accounts", schema: "fundex_broker");

            migrationBuilder.DropTable(name: "Assignments", schema: "fundex_risk");

            migrationBuilder.DropTable(name: "Calculations", schema: "fundex_withdrawal");

            migrationBuilder.DropTable(name: "CapitalAllocations", schema: "fundex_accounts");

            migrationBuilder.DropTable(name: "ChallengeAttempts", schema: "fundex_subscription");

            migrationBuilder.DropTable(name: "Countries", schema: "fundex_master");

            migrationBuilder.DropTable(name: "Currencies", schema: "fundex_master");

            migrationBuilder.DropTable(name: "Entries", schema: "fundex_wallet");

            migrationBuilder.DropTable(name: "Events", schema: "fundex_audit");

            migrationBuilder.DropTable(name: "Events", schema: "fundex_broker");

            migrationBuilder.DropTable(name: "Exchanges", schema: "fundex_master");

            migrationBuilder.DropTable(name: "Executions", schema: "fundex_trading");

            migrationBuilder.DropTable(name: "Instruments", schema: "fundex_market");

            migrationBuilder.DropTable(name: "Invoices", schema: "fundex_payments");

            migrationBuilder.DropTable(name: "Lots", schema: "fundex_portfolio");

            migrationBuilder.DropTable(name: "Notifications", schema: "fundex_notification");

            migrationBuilder.DropTable(name: "OrderEvents", schema: "fundex_trading");

            migrationBuilder.DropTable(name: "Orders", schema: "fundex_trading");

            migrationBuilder.DropTable(name: "Payments", schema: "fundex_payments");

            migrationBuilder.DropTable(name: "Permissions", schema: "fundex_identity");

            migrationBuilder.DropTable(name: "Plans", schema: "fundex_subscription");

            migrationBuilder.DropTable(name: "PlanVersions", schema: "fundex_subscription");

            migrationBuilder.DropTable(name: "PolicySets", schema: "fundex_risk");

            migrationBuilder.DropTable(name: "PolicyVersions", schema: "fundex_risk");

            migrationBuilder.DropTable(name: "Positions", schema: "fundex_portfolio");

            migrationBuilder.DropTable(name: "Receipts", schema: "fundex_payments");

            migrationBuilder.DropTable(name: "Requests", schema: "fundex_withdrawal");

            migrationBuilder.DropTable(name: "RolePermissions", schema: "fundex_identity");

            migrationBuilder.DropTable(name: "Roles", schema: "fundex_identity");

            migrationBuilder.DropTable(name: "Rules", schema: "fundex_risk");

            migrationBuilder.DropTable(name: "SecurityTypes", schema: "fundex_master");

            migrationBuilder.DropTable(name: "Settings", schema: "fundex_configuration");

            migrationBuilder.DropTable(name: "Signals", schema: "fundex_intelligence");

            migrationBuilder.DropTable(name: "Stages", schema: "fundex_subscription");

            migrationBuilder.DropTable(name: "StatusHistory", schema: "fundex_accounts");

            migrationBuilder.DropTable(name: "Subscriptions", schema: "fundex_subscription");

            migrationBuilder.DropTable(name: "TradingCalendar", schema: "fundex_market");

            migrationBuilder.DropTable(name: "Transactions", schema: "fundex_wallet");

            migrationBuilder.DropTable(name: "UserRoles", schema: "fundex_identity");

            migrationBuilder.DropTable(name: "Users", schema: "fundex_identity");

            migrationBuilder.DropTable(name: "Violations", schema: "fundex_risk");

            migrationBuilder.DropTable(name: "Wallets", schema: "fundex_wallet");
        }
    }
}
