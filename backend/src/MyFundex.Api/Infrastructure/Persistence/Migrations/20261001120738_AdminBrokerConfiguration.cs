using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyFundex.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdminBrokerConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BrokerUserId",
                schema: "fundex_broker",
                table: "Accounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                schema: "fundex_broker",
                table: "Accounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                schema: "fundex_broker",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProtectedCredentials",
                schema: "fundex_broker",
                table: "Accounts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SessionExpiresAt",
                schema: "fundex_broker",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseForMarketData",
                schema: "fundex_broker",
                table: "Accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Environment",
                schema: "fundex_broker",
                table: "Accounts",
                column: "Environment",
                unique: true,
                filter: "\"IsDeleted\" = false AND \"IsActive\" = true AND \"IsDefault\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ProviderCode_Environment_AccountReference",
                schema: "fundex_broker",
                table: "Accounts",
                columns: new[] { "ProviderCode", "Environment", "AccountReference" },
                unique: true,
                filter: "\"IsDeleted\" = false AND \"ProtectedCredentials\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UseForMarketData",
                schema: "fundex_broker",
                table: "Accounts",
                column: "UseForMarketData",
                unique: true,
                filter: "\"IsDeleted\" = false AND \"IsActive\" = true AND \"UseForMarketData\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_Environment",
                schema: "fundex_broker",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_ProviderCode_Environment_AccountReference",
                schema: "fundex_broker",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_UseForMarketData",
                schema: "fundex_broker",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "BrokerUserId",
                schema: "fundex_broker",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                schema: "fundex_broker",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                schema: "fundex_broker",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ProtectedCredentials",
                schema: "fundex_broker",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "SessionExpiresAt",
                schema: "fundex_broker",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "UseForMarketData",
                schema: "fundex_broker",
                table: "Accounts");
        }
    }
}
