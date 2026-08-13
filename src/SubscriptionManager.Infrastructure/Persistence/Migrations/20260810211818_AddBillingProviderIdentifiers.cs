using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubscriptionManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingProviderIdentifiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProviderCustomerId",
                table: "BillingSubscriptions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderPriceId",
                table: "BillingSubscriptions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderSubscriptionId",
                table: "BillingSubscriptions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillingSubscriptions_ProviderCustomerId",
                table: "BillingSubscriptions",
                column: "ProviderCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingSubscriptions_ProviderSubscriptionId",
                table: "BillingSubscriptions",
                column: "ProviderSubscriptionId",
                unique: true,
                filter: "[ProviderSubscriptionId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BillingSubscriptions_ProviderCustomerId",
                table: "BillingSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_BillingSubscriptions_ProviderSubscriptionId",
                table: "BillingSubscriptions");

            migrationBuilder.DropColumn(
                name: "ProviderCustomerId",
                table: "BillingSubscriptions");

            migrationBuilder.DropColumn(
                name: "ProviderPriceId",
                table: "BillingSubscriptions");

            migrationBuilder.DropColumn(
                name: "ProviderSubscriptionId",
                table: "BillingSubscriptions");
        }
    }
}
