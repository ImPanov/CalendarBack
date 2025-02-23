using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CalendarBack.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationSentFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotificationSent",
                table: "CalendarEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationSent",
                table: "CalendarEntries");
        }
    }
}
