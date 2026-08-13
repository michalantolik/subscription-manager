using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubscriptionManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingWebhookIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastProviderEventCreatedAt",
                table: "BillingSubscriptions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProcessedBillingEvents",
                columns: table => new
                {
                    ProviderEventId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ProviderEventCreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedBillingEvents", x => x.ProviderEventId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedBillingEvents_ProcessedAt",
                table: "ProcessedBillingEvents",
                column: "ProcessedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedBillingEvents");

            migrationBuilder.DropColumn(
                name: "LastProviderEventCreatedAt",
                table: "BillingSubscriptions");
        }
    }
}
