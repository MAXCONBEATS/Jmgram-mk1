using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jmgram_mk1.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeysContactRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactRequests_UserProfiles_RecipientUserId",
                table: "ContactRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactRequests_UserProfiles_SenderUserId",
                table: "ContactRequests");

            migrationBuilder.DropIndex(
                name: "IX_ContactRequests_SenderUserId_RecipientUserId",
                table: "ContactRequests");

            migrationBuilder.AlterColumn<string>(
                name: "SenderUserId",
                table: "ContactRequests",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "RecipientUserId",
                table: "ContactRequests",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_ContactRequests_SenderUserId_RecipientUserId",
                table: "ContactRequests",
                columns: new[] { "SenderUserId", "RecipientUserId" },
                unique: true,
                filter: "[SenderUserId] IS NOT NULL AND [RecipientUserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactRequests_AspNetUsers_RecipientUserId",
                table: "ContactRequests",
                column: "RecipientUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactRequests_AspNetUsers_SenderUserId",
                table: "ContactRequests",
                column: "SenderUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactRequests_AspNetUsers_RecipientUserId",
                table: "ContactRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactRequests_AspNetUsers_SenderUserId",
                table: "ContactRequests");

            migrationBuilder.DropIndex(
                name: "IX_ContactRequests_SenderUserId_RecipientUserId",
                table: "ContactRequests");

            migrationBuilder.AlterColumn<string>(
                name: "SenderUserId",
                table: "ContactRequests",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RecipientUserId",
                table: "ContactRequests",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactRequests_SenderUserId_RecipientUserId",
                table: "ContactRequests",
                columns: new[] { "SenderUserId", "RecipientUserId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactRequests_UserProfiles_RecipientUserId",
                table: "ContactRequests",
                column: "RecipientUserId",
                principalTable: "UserProfiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactRequests_UserProfiles_SenderUserId",
                table: "ContactRequests",
                column: "SenderUserId",
                principalTable: "UserProfiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
