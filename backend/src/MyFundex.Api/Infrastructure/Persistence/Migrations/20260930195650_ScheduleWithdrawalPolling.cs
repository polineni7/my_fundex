using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyFundex.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ScheduleWithdrawalPolling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NextCheckAt",
                schema: "fundex_withdrawal",
                table: "Requests",
                type: "timestamp with time zone",
                nullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Requests_Status_NextCheckAt",
                schema: "fundex_withdrawal",
                table: "Requests",
                columns: new[] { "Status", "NextCheckAt" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Requests_Status_NextCheckAt",
                schema: "fundex_withdrawal",
                table: "Requests"
            );

            migrationBuilder.DropColumn(
                name: "NextCheckAt",
                schema: "fundex_withdrawal",
                table: "Requests"
            );
        }
    }
}
