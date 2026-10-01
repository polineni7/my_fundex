using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyFundex.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurableAssessmentTerms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MinimumTradingDays",
                schema: "fundex_subscription",
                table: "Stages",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<decimal>(
                name: "MaximumLeverage",
                schema: "fundex_subscription",
                table: "Stages",
                type: "numeric(20,4)",
                precision: 20,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TradingPeriod",
                schema: "fundex_subscription",
                table: "Stages",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaximumLeverage",
                schema: "fundex_subscription",
                table: "Stages");

            migrationBuilder.DropColumn(
                name: "TradingPeriod",
                schema: "fundex_subscription",
                table: "Stages");

            migrationBuilder.AlterColumn<int>(
                name: "MinimumTradingDays",
                schema: "fundex_subscription",
                table: "Stages",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
