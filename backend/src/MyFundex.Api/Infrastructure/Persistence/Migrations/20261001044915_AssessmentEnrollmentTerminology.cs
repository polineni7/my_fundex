using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyFundex.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AssessmentEnrollmentTerminology : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Subscriptions",
                schema: "fundex_subscription",
                table: "Subscriptions");

            migrationBuilder.RenameTable(
                name: "Subscriptions",
                schema: "fundex_subscription",
                newName: "AssessmentEnrollments",
                newSchema: "fundex_subscription");

            migrationBuilder.RenameColumn(
                name: "RegistrationFee",
                schema: "fundex_subscription",
                table: "PlanVersions",
                newName: "AssessmentFee");

            migrationBuilder.RenameColumn(
                name: "SubscriptionId",
                schema: "fundex_subscription",
                table: "AssessmentEnrollments",
                newName: "EnrollmentId");

            migrationBuilder.RenameColumn(
                name: "SubscribedAt",
                schema: "fundex_subscription",
                table: "AssessmentEnrollments",
                newName: "EnrolledAt");

            migrationBuilder.RenameColumn(
                name: "PreviousSubscriptionId",
                schema: "fundex_subscription",
                table: "AssessmentEnrollments",
                newName: "PreviousEnrollmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_SubscriptionId",
                schema: "fundex_subscription",
                table: "AssessmentEnrollments",
                newName: "IX_AssessmentEnrollments_EnrollmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_PurchaseRequestId",
                schema: "fundex_subscription",
                table: "AssessmentEnrollments",
                newName: "IX_AssessmentEnrollments_PurchaseRequestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AssessmentEnrollments",
                schema: "fundex_subscription",
                table: "AssessmentEnrollments",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AssessmentEnrollments",
                schema: "fundex_subscription",
                table: "AssessmentEnrollments");

            migrationBuilder.RenameTable(
                name: "AssessmentEnrollments",
                schema: "fundex_subscription",
                newName: "Subscriptions",
                newSchema: "fundex_subscription");

            migrationBuilder.RenameColumn(
                name: "AssessmentFee",
                schema: "fundex_subscription",
                table: "PlanVersions",
                newName: "RegistrationFee");

            migrationBuilder.RenameColumn(
                name: "PreviousEnrollmentId",
                schema: "fundex_subscription",
                table: "Subscriptions",
                newName: "PreviousSubscriptionId");

            migrationBuilder.RenameColumn(
                name: "EnrollmentId",
                schema: "fundex_subscription",
                table: "Subscriptions",
                newName: "SubscriptionId");

            migrationBuilder.RenameColumn(
                name: "EnrolledAt",
                schema: "fundex_subscription",
                table: "Subscriptions",
                newName: "SubscribedAt");

            migrationBuilder.RenameIndex(
                name: "IX_AssessmentEnrollments_PurchaseRequestId",
                schema: "fundex_subscription",
                table: "Subscriptions",
                newName: "IX_Subscriptions_PurchaseRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_AssessmentEnrollments_EnrollmentId",
                schema: "fundex_subscription",
                table: "Subscriptions",
                newName: "IX_Subscriptions_SubscriptionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subscriptions",
                schema: "fundex_subscription",
                table: "Subscriptions",
                column: "Id");
        }
    }
}
