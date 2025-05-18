using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jmgram_mk1.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatorUserIdToChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorUserId",
                table: "Chats",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "Chats");
        }
    }
}
