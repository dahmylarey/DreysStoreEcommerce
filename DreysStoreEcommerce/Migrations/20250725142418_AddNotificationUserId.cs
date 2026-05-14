using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DreysStoreEcommerce.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Notifications");
        }
    }
}
