using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubscriptionManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Subscriptions",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_OwnerId",
                table: "Subscriptions",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_OwnerId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Subscriptions");
        }
    }
}
