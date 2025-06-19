using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jmgram_mk1.Migrations
{
    /// <inheritdoc />
    public partial class CreateChatInvationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChatId = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    SenderUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RecipientUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatInvitations_AspNetUsers_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChatInvitations_AspNetUsers_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChatInvitations_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatInvitations_ChatId",
                table: "ChatInvitations",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatInvitations_RecipientUserId",
                table: "ChatInvitations",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatInvitations_SenderUserId_RecipientUserId",
                table: "ChatInvitations",
                columns: new[] { "SenderUserId", "RecipientUserId" },
                unique: true,
                filter: "[SenderUserId] IS NOT NULL AND [RecipientUserId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatInvitations");
        }
    }
}
