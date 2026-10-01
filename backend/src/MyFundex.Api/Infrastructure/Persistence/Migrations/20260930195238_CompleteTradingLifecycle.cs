using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyFundex.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteTradingLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "myfund_prod_trading");

            migrationBuilder.EnsureSchema(name: "myfund_sandbox_trading");

            migrationBuilder.EnsureSchema(name: "myfund_prod_wallet");

            migrationBuilder.AddColumn<string>(
                name: "BeneficiaryReference",
                schema: "fundex_withdrawal",
                table: "Requests",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PayoutStartedAt",
                schema: "fundex_withdrawal",
                table: "Requests",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "ProviderPayoutId",
                schema: "fundex_withdrawal",
                table: "Requests",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<long>(
                name: "UserInternalId",
                schema: "fundex_withdrawal",
                table: "Requests",
                type: "bigint",
                nullable: false,
                defaultValue: 0L
            );

            migrationBuilder.AddColumn<decimal>(
                name: "FundedDailyLossPercent",
                schema: "fundex_subscription",
                table: "PlanVersions",
                type: "numeric(20,4)",
                precision: 20,
                scale: 4,
                nullable: false,
                defaultValue: 5m
            );

            migrationBuilder.AddColumn<decimal>(
                name: "FundedTotalLossPercent",
                schema: "fundex_subscription",
                table: "PlanVersions",
                type: "numeric(20,4)",
                precision: 20,
                scale: 4,
                nullable: false,
                defaultValue: 10m
            );

            migrationBuilder.AddColumn<bool>(
                name: "IsLiquidation",
                schema: "fundex_trading",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<decimal>(
                name: "ReservedCash",
                schema: "fundex_trading",
                table: "Orders",
                type: "numeric(20,4)",
                precision: 20,
                scale: 4,
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.AddColumn<bool>(
                name: "UsesLiveBook",
                schema: "fundex_trading",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<string>(
                name: "LedgerAccount",
                schema: "fundex_wallet",
                table: "Entries",
                type: "text",
                nullable: false,
                defaultValue: "TraderPayable"
            );

            migrationBuilder.AddColumn<string>(
                name: "BrokerUserId",
                schema: "fundex_accounts",
                table: "Accounts",
                type: "text",
                nullable: true
            );

            migrationBuilder.CreateTable(
                name: "Books",
                schema: "myfund_prod_trading",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Equity = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    DayOpeningEquity = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    TradingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ValuedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    CloseRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    CloseReason = table.Column<string>(type: "text", nullable: true),
                    Cash = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    ReservedCash = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    RealizedProfit = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
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
                    table.PrimaryKey("PK_Books", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Books",
                schema: "myfund_sandbox_trading",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    AccountInternalId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservedCash = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    Cash = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    Equity = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    RealizedProfit = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    DayOpeningEquity = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    TradingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TradingDays = table.Column<int>(type: "integer", nullable: false),
                    LastTradeDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ValuedAt = table.Column<DateTimeOffset>(
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
                    table.PrimaryKey("PK_Books", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "ChallengeNotices",
                schema: "fundex_subscription",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    NoticeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserInternalId = table.Column<long>(type: "bigint", nullable: false),
                    SubscriptionInternalId = table.Column<long>(type: "bigint", nullable: false),
                    Outcome = table.Column<string>(type: "text", nullable: false),
                    DeliveredAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    LeaseUntil = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ChallengeNotices", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Positions",
                schema: "myfund_prod_trading",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstrumentToken = table.Column<string>(type: "text", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    ReservedQuantity = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    AverageCost = table.Column<decimal>(
                        type: "numeric(28,10)",
                        precision: 28,
                        scale: 10,
                        nullable: false
                    ),
                    LastPrice = table.Column<decimal>(
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
                name: "Positions",
                schema: "myfund_sandbox_trading",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstrumentToken = table.Column<string>(type: "text", nullable: false),
                    Symbol = table.Column<string>(type: "text", nullable: false),
                    ReservedQuantity = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    Quantity = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    AverageCost = table.Column<decimal>(
                        type: "numeric(28,10)",
                        precision: 28,
                        scale: 10,
                        nullable: false
                    ),
                    LastPrice = table.Column<decimal>(
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
                name: "ProfitDistributions",
                schema: "myfund_prod_trading",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    DistributionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountInternalId = table.Column<long>(type: "bigint", nullable: false),
                    SettlementReference = table.Column<string>(type: "text", nullable: false),
                    GrossProfit = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    Fees = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    TraderReward = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    PlatformReward = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
                    DeliveredAt = table.Column<DateTimeOffset>(
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
                    table.PrimaryKey("PK_ProfitDistributions", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "WithdrawalHolds",
                schema: "myfund_prod_wallet",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    WithdrawalId = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletInternalId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(
                        type: "numeric(20,4)",
                        precision: 20,
                        scale: 4,
                        nullable: false
                    ),
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
                    table.PrimaryKey("PK_WithdrawalHolds", x => x.Id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ProviderPayoutId",
                schema: "fundex_withdrawal",
                table: "Requests",
                column: "ProviderPayoutId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Requests_Status_Id",
                schema: "fundex_withdrawal",
                table: "Requests",
                columns: new[] { "Status", "Id" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Executions_OrderInternalId_BrokerExecutionId",
                schema: "fundex_trading",
                table: "Executions",
                columns: new[] { "OrderInternalId", "BrokerExecutionId" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Entries_TransactionInternalId",
                schema: "fundex_wallet",
                table: "Entries",
                column: "TransactionInternalId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_BrokerProvider_BrokerUserId",
                schema: "fundex_accounts",
                table: "Accounts",
                columns: new[] { "BrokerProvider", "BrokerUserId" },
                unique: true,
                filter: "\"BrokerUserId\" IS NOT NULL"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Books_AccountId",
                schema: "myfund_prod_trading",
                table: "Books",
                column: "AccountId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Books_AccountId1",
                schema: "myfund_sandbox_trading",
                table: "Books",
                column: "AccountId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeNotices_DeliveredAt_LeaseUntil",
                schema: "fundex_subscription",
                table: "ChallengeNotices",
                columns: new[] { "DeliveredAt", "LeaseUntil" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeNotices_SubscriptionInternalId_Outcome",
                schema: "fundex_subscription",
                table: "ChallengeNotices",
                columns: new[] { "SubscriptionInternalId", "Outcome" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Positions_AccountId_InstrumentToken",
                schema: "myfund_prod_trading",
                table: "Positions",
                columns: new[] { "AccountId", "InstrumentToken" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Positions_AccountId_InstrumentToken1",
                schema: "myfund_sandbox_trading",
                table: "Positions",
                columns: new[] { "AccountId", "InstrumentToken" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_ProfitDistributions_AccountId_SettlementReference",
                schema: "myfund_prod_trading",
                table: "ProfitDistributions",
                columns: new[] { "AccountId", "SettlementReference" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_ProfitDistributions_DistributionId",
                schema: "myfund_prod_trading",
                table: "ProfitDistributions",
                column: "DistributionId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalHolds_WithdrawalId",
                schema: "myfund_prod_wallet",
                table: "WithdrawalHolds",
                column: "WithdrawalId",
                unique: true
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_Transactions_TransactionInternalId",
                schema: "fundex_wallet",
                table: "Entries",
                column: "TransactionInternalId",
                principalSchema: "fundex_wallet",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Executions_Orders_OrderInternalId",
                schema: "fundex_trading",
                table: "Executions",
                column: "OrderInternalId",
                principalSchema: "fundex_trading",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entries_Transactions_TransactionInternalId",
                schema: "fundex_wallet",
                table: "Entries"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Executions_Orders_OrderInternalId",
                schema: "fundex_trading",
                table: "Executions"
            );

            migrationBuilder.DropTable(name: "Books", schema: "myfund_prod_trading");

            migrationBuilder.DropTable(name: "Books", schema: "myfund_sandbox_trading");

            migrationBuilder.DropTable(name: "ChallengeNotices", schema: "fundex_subscription");

            migrationBuilder.DropTable(name: "Positions", schema: "myfund_prod_trading");

            migrationBuilder.DropTable(name: "Positions", schema: "myfund_sandbox_trading");

            migrationBuilder.DropTable(name: "ProfitDistributions", schema: "myfund_prod_trading");

            migrationBuilder.DropTable(name: "WithdrawalHolds", schema: "myfund_prod_wallet");

            migrationBuilder.DropIndex(
                name: "IX_Requests_ProviderPayoutId",
                schema: "fundex_withdrawal",
                table: "Requests"
            );

            migrationBuilder.DropIndex(
                name: "IX_Requests_Status_Id",
                schema: "fundex_withdrawal",
                table: "Requests"
            );

            migrationBuilder.DropIndex(
                name: "IX_Executions_OrderInternalId_BrokerExecutionId",
                schema: "fundex_trading",
                table: "Executions"
            );

            migrationBuilder.DropIndex(
                name: "IX_Entries_TransactionInternalId",
                schema: "fundex_wallet",
                table: "Entries"
            );

            migrationBuilder.DropIndex(
                name: "IX_Accounts_BrokerProvider_BrokerUserId",
                schema: "fundex_accounts",
                table: "Accounts"
            );

            migrationBuilder.DropColumn(
                name: "BeneficiaryReference",
                schema: "fundex_withdrawal",
                table: "Requests"
            );

            migrationBuilder.DropColumn(
                name: "PayoutStartedAt",
                schema: "fundex_withdrawal",
                table: "Requests"
            );

            migrationBuilder.DropColumn(
                name: "ProviderPayoutId",
                schema: "fundex_withdrawal",
                table: "Requests"
            );

            migrationBuilder.DropColumn(
                name: "UserInternalId",
                schema: "fundex_withdrawal",
                table: "Requests"
            );

            migrationBuilder.DropColumn(
                name: "FundedDailyLossPercent",
                schema: "fundex_subscription",
                table: "PlanVersions"
            );

            migrationBuilder.DropColumn(
                name: "FundedTotalLossPercent",
                schema: "fundex_subscription",
                table: "PlanVersions"
            );

            migrationBuilder.DropColumn(
                name: "IsLiquidation",
                schema: "fundex_trading",
                table: "Orders"
            );

            migrationBuilder.DropColumn(
                name: "ReservedCash",
                schema: "fundex_trading",
                table: "Orders"
            );

            migrationBuilder.DropColumn(
                name: "UsesLiveBook",
                schema: "fundex_trading",
                table: "Orders"
            );

            migrationBuilder.DropColumn(
                name: "LedgerAccount",
                schema: "fundex_wallet",
                table: "Entries"
            );

            migrationBuilder.DropColumn(
                name: "BrokerUserId",
                schema: "fundex_accounts",
                table: "Accounts"
            );
        }
    }
}
