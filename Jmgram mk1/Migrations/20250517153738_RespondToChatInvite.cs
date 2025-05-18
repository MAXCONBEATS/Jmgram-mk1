using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jmgram_mk1.Migrations
{
    /// <inheritdoc />
    public partial class RespondToChatInvite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "userId",
                table: "Notifications",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "timestamp",
                table: "Notifications",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "source",
                table: "Notifications",
                newName: "Source");

            migrationBuilder.RenameColumn(
                name: "senderUserId",
                table: "Notifications",
                newName: "SenderUserId");

            migrationBuilder.RenameColumn(
                name: "message",
                table: "Notifications",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "isRead",
                table: "Notifications",
                newName: "IsRead");

            migrationBuilder.AddColumn<string>(
                name: "ChatId",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Notifications",
                newName: "userId");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Notifications",
                newName: "timestamp");

            migrationBuilder.RenameColumn(
                name: "Source",
                table: "Notifications",
                newName: "source");

            migrationBuilder.RenameColumn(
                name: "SenderUserId",
                table: "Notifications",
                newName: "senderUserId");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "Notifications",
                newName: "message");

            migrationBuilder.RenameColumn(
                name: "IsRead",
                table: "Notifications",
                newName: "isRead");
        }
    }
}
