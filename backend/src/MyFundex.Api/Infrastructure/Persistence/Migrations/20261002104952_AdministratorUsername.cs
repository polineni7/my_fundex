using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyFundex.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdministratorUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Username",
                schema: "fundex_identity",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                schema: "fundex_identity",
                table: "Users",
                column: "Username",
                unique: true,
                filter: "\"IsDeleted\" = false AND \"Username\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                schema: "fundex_identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Username",
                schema: "fundex_identity",
                table: "Users");
        }
    }
}
