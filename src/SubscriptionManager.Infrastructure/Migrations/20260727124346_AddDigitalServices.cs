using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubscriptionManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDigitalServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DigitalServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsPredefined = table.Column<bool>(type: "bit", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomCategoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IconKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ManagementUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalServices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UX_DigitalServices_Custom_OwnerId_Key",
                table: "DigitalServices",
                columns: new[] { "OwnerId", "Key" },
                unique: true,
                filter: "[IsPredefined] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_DigitalServices_Predefined_Key",
                table: "DigitalServices",
                column: "Key",
                unique: true,
                filter: "[IsPredefined] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DigitalServices");
        }
    }
}
