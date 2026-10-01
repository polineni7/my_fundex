using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyFundex.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PlanProfitDeductions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OtherDeductionPercent",
                schema: "fundex_prod_trading",
                table: "ProfitDistributions",
                type: "numeric(20,4)",
                precision: 20,
                scale: 4,
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.AddColumn<decimal>(
                name: "OtherDeductions",
                schema: "fundex_prod_trading",
                table: "ProfitDistributions",
                type: "numeric(20,4)",
                precision: 20,
                scale: 4,
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.AddColumn<decimal>(
                name: "TaxWithheld",
                schema: "fundex_prod_trading",
                table: "ProfitDistributions",
                type: "numeric(20,4)",
                precision: 20,
                scale: 4,
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.AddColumn<decimal>(
                name: "TaxWithholdingPercent",
                schema: "fundex_prod_trading",
                table: "ProfitDistributions",
                type: "numeric(20,4)",
                precision: 20,
                scale: 4,
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.AddColumn<decimal>(
                name: "OtherDeductionPercent",
                schema: "fundex_subscription",
                table: "PlanVersions",
                type: "numeric(20,4)",
                precision: 20,
                scale: 4,
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.AddColumn<decimal>(
                name: "TaxWithholdingPercent",
                schema: "fundex_subscription",
                table: "PlanVersions",
                type: "numeric(20,4)",
                precision: 20,
                scale: 4,
                nullable: false,
                defaultValue: 0m
            );

            migrationBuilder.AddColumn<decimal>(
                name: "OtherDeductionPercent",
                schema: "fundex_trading",
                table: "Executions",
                type: "numeric(20,4)",
                precision: 20,
                scale: 4,
                nullable: true
            );

            migrationBuilder.AddColumn<decimal>(
                name: "RealizedProfit",
                schema: "fundex_trading",
                table: "Executions",
                type: "numeric(20,4)",
                precision: 20,
                scale: 4,
                nullable: true
            );

            migrationBuilder.AddColumn<decimal>(
                name: "TaxWithholdingPercent",
                schema: "fundex_trading",
                table: "Executions",
                type: "numeric(20,4)",
                precision: 20,
                scale: 4,
                nullable: true
            );

            migrationBuilder.AddColumn<decimal>(
                name: "TraderSharePercent",
                schema: "fundex_trading",
                table: "Executions",
                type: "numeric(20,4)",
                precision: 20,
                scale: 4,
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtherDeductionPercent",
                schema: "fundex_prod_trading",
                table: "ProfitDistributions"
            );

            migrationBuilder.DropColumn(
                name: "OtherDeductions",
                schema: "fundex_prod_trading",
                table: "ProfitDistributions"
            );

            migrationBuilder.DropColumn(
                name: "TaxWithheld",
                schema: "fundex_prod_trading",
                table: "ProfitDistributions"
            );

            migrationBuilder.DropColumn(
                name: "TaxWithholdingPercent",
                schema: "fundex_prod_trading",
                table: "ProfitDistributions"
            );

            migrationBuilder.DropColumn(
                name: "OtherDeductionPercent",
                schema: "fundex_subscription",
                table: "PlanVersions"
            );

            migrationBuilder.DropColumn(
                name: "TaxWithholdingPercent",
                schema: "fundex_subscription",
                table: "PlanVersions"
            );

            migrationBuilder.DropColumn(
                name: "OtherDeductionPercent",
                schema: "fundex_trading",
                table: "Executions"
            );

            migrationBuilder.DropColumn(
                name: "RealizedProfit",
                schema: "fundex_trading",
                table: "Executions"
            );

            migrationBuilder.DropColumn(
                name: "TaxWithholdingPercent",
                schema: "fundex_trading",
                table: "Executions"
            );

            migrationBuilder.DropColumn(
                name: "TraderSharePercent",
                schema: "fundex_trading",
                table: "Executions"
            );
        }
    }
}
