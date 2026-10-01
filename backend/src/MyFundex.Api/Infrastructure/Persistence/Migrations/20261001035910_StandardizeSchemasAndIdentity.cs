using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyFundex.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StandardizeSchemasAndIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename the schemas themselves: table data, sequences, indexes and grants remain attached.
            migrationBuilder.Sql("ALTER SCHEMA myfund_prod_trading RENAME TO fundex_prod_trading;");
            migrationBuilder.Sql(
                "ALTER SCHEMA myfund_sandbox_trading RENAME TO fundex_sandbox_trading;"
            );
            migrationBuilder.Sql("ALTER SCHEMA myfund_prod_wallet RENAME TO fundex_prod_wallet;");

            migrationBuilder.AddColumn<long>(
                name: "SecurityVersion",
                schema: "fundex_identity",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 1L
            );

            migrationBuilder.CreateTable(
                name: "IdentityEvents",
                schema: "fundex_identity",
                columns: table => new
                {
                    Id = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserInternalId = table.Column<long>(type: "bigint", nullable: true),
                    EventType = table.Column<string>(
                        type: "character varying(60)",
                        maxLength: 60,
                        nullable: false
                    ),
                    Method = table.Column<string>(
                        type: "character varying(30)",
                        maxLength: 30,
                        nullable: false
                    ),
                    Succeeded = table.Column<bool>(type: "boolean", nullable: false),
                    Detail = table.Column<string>(
                        type: "character varying(500)",
                        maxLength: 500,
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
                    table.PrimaryKey("PK_IdentityEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityEvents_Users_UserInternalId",
                        column: x => x.UserInternalId,
                        principalSchema: "fundex_identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleInternalId",
                schema: "fundex_identity",
                table: "UserRoles",
                column: "RoleInternalId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserInternalId_RoleInternalId",
                schema: "fundex_identity",
                table: "UserRoles",
                columns: new[] { "UserInternalId", "RoleInternalId" },
                unique: true,
                filter: "\"IsDeleted\" = false"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Roles_RoleId",
                schema: "fundex_identity",
                table: "Roles",
                column: "RoleId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionInternalId",
                schema: "fundex_identity",
                table: "RolePermissions",
                column: "PermissionInternalId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleInternalId_PermissionInternalId",
                schema: "fundex_identity",
                table: "RolePermissions",
                columns: new[] { "RoleInternalId", "PermissionInternalId" },
                unique: true,
                filter: "\"IsDeleted\" = false"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_PermissionId",
                schema: "fundex_identity",
                table: "Permissions",
                column: "PermissionId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_IdentityEvents_EventId",
                schema: "fundex_identity",
                table: "IdentityEvents",
                column: "EventId",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_IdentityEvents_UserInternalId_CreatedAt",
                schema: "fundex_identity",
                table: "IdentityEvents",
                columns: new[] { "UserInternalId", "CreatedAt" }
            );

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionInternalId",
                schema: "fundex_identity",
                table: "RolePermissions",
                column: "PermissionInternalId",
                principalSchema: "fundex_identity",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Roles_RoleInternalId",
                schema: "fundex_identity",
                table: "RolePermissions",
                column: "RoleInternalId",
                principalSchema: "fundex_identity",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Roles_RoleInternalId",
                schema: "fundex_identity",
                table: "UserRoles",
                column: "RoleInternalId",
                principalSchema: "fundex_identity",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Users_UserInternalId",
                schema: "fundex_identity",
                table: "UserRoles",
                column: "UserInternalId",
                principalSchema: "fundex_identity",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
            migrationBuilder.Sql(
                """
                INSERT INTO fundex_identity."Roles" ("RoleId", "Code", "Name", "CreatedAt", "CreatedBy", "IsDeleted", "Version")
                SELECT gen_random_uuid(), role.code, role.name, now(), 0, false, 1
                FROM (VALUES ('TRADER', 'Trader'), ('MANAGER', 'Manager'), ('ADMIN', 'Administrator')) AS role(code,name)
                WHERE NOT EXISTS (SELECT 1 FROM fundex_identity."Roles" existing WHERE existing."Code" = role.code AND NOT existing."IsDeleted");

                INSERT INTO fundex_identity."UserRoles" ("UserInternalId", "RoleInternalId", "CreatedAt", "CreatedBy", "IsDeleted", "Version")
                SELECT member."Id", role."Id", now(), 0, false, 1
                FROM fundex_identity."Users" member
                JOIN fundex_identity."Roles" role ON role."Code" = 'TRADER' AND NOT role."IsDeleted"
                WHERE NOT member."IsDeleted" AND NOT EXISTS
                  (SELECT 1 FROM fundex_identity."UserRoles" link WHERE link."UserInternalId" = member."Id" AND NOT link."IsDeleted");
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Permissions_PermissionInternalId",
                schema: "fundex_identity",
                table: "RolePermissions"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Roles_RoleInternalId",
                schema: "fundex_identity",
                table: "RolePermissions"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Roles_RoleInternalId",
                schema: "fundex_identity",
                table: "UserRoles"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Users_UserInternalId",
                schema: "fundex_identity",
                table: "UserRoles"
            );

            migrationBuilder.DropTable(name: "IdentityEvents", schema: "fundex_identity");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_RoleInternalId",
                schema: "fundex_identity",
                table: "UserRoles"
            );

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserInternalId_RoleInternalId",
                schema: "fundex_identity",
                table: "UserRoles"
            );

            migrationBuilder.DropIndex(
                name: "IX_Roles_RoleId",
                schema: "fundex_identity",
                table: "Roles"
            );

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_PermissionInternalId",
                schema: "fundex_identity",
                table: "RolePermissions"
            );

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_RoleInternalId_PermissionInternalId",
                schema: "fundex_identity",
                table: "RolePermissions"
            );

            migrationBuilder.DropIndex(
                name: "IX_Permissions_PermissionId",
                schema: "fundex_identity",
                table: "Permissions"
            );

            migrationBuilder.DropColumn(
                name: "SecurityVersion",
                schema: "fundex_identity",
                table: "Users"
            );

            migrationBuilder.Sql("ALTER SCHEMA fundex_prod_trading RENAME TO myfund_prod_trading;");
            migrationBuilder.Sql(
                "ALTER SCHEMA fundex_sandbox_trading RENAME TO myfund_sandbox_trading;"
            );
            migrationBuilder.Sql("ALTER SCHEMA fundex_prod_wallet RENAME TO myfund_prod_wallet;");
        }
    }
}
