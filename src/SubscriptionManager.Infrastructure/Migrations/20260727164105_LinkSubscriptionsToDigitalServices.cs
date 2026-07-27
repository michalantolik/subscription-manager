using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubscriptionManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkSubscriptionsToDigitalServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Subscriptions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomCategoryName",
                table: "Subscriptions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DigitalServiceId",
                table: "Subscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconKey",
                table: "Subscriptions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagementUrl",
                table: "Subscriptions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_DigitalServiceId",
                table: "Subscriptions",
                column: "DigitalServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_DigitalServices_DigitalServiceId",
                table: "Subscriptions",
                column: "DigitalServiceId",
                principalTable: "DigitalServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_DigitalServices_DigitalServiceId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_DigitalServiceId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "CustomCategoryName",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DigitalServiceId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "IconKey",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "ManagementUrl",
                table: "Subscriptions");
        }
    }
}
