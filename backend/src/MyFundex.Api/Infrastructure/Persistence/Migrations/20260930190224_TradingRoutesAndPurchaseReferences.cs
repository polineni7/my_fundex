using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyFundex.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TradingRoutesAndPurchaseReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PostingKey",
                schema: "fundex_wallet",
                table: "Transactions",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<Guid>(
                name: "PreviousSubscriptionId",
                schema: "fundex_subscription",
                table: "Subscriptions",
                type: "uuid",
                nullable: true
            );

            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseRequestId",
                schema: "fundex_subscription",
                table: "Subscriptions",
                type: "uuid",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "BrokerProvider",
                schema: "fundex_trading",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "Upstox"
            );

            migrationBuilder.AddColumn<string>(
                name: "BrokerProvider",
                schema: "fundex_accounts",
                table: "Accounts",
                type: "text",
                nullable: false,
                defaultValue: "Upstox"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PostingKey",
                schema: "fundex_wallet",
                table: "Transactions",
                column: "PostingKey",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PurchaseRequestId",
                schema: "fundex_subscription",
                table: "Subscriptions",
                column: "PurchaseRequestId",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_PostingKey",
                schema: "fundex_wallet",
                table: "Transactions"
            );

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_PurchaseRequestId",
                schema: "fundex_subscription",
                table: "Subscriptions"
            );

            migrationBuilder.DropColumn(
                name: "PostingKey",
                schema: "fundex_wallet",
                table: "Transactions"
            );

            migrationBuilder.DropColumn(
                name: "PreviousSubscriptionId",
                schema: "fundex_subscription",
                table: "Subscriptions"
            );

            migrationBuilder.DropColumn(
                name: "PurchaseRequestId",
                schema: "fundex_subscription",
                table: "Subscriptions"
            );

            migrationBuilder.DropColumn(
                name: "BrokerProvider",
                schema: "fundex_trading",
                table: "Orders"
            );

            migrationBuilder.DropColumn(
                name: "BrokerProvider",
                schema: "fundex_accounts",
                table: "Accounts"
            );
        }
    }
}
