using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubscriptionManager.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserBaseCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BaseCurrency",
                table: "AspNetUsers",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PLN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseCurrency",
                table: "AspNetUsers");
        }
    }
}
