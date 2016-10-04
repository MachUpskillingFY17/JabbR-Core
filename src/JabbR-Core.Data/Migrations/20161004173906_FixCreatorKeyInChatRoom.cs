using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JabbRCore.Migrations
{
    public partial class FixCreatorKeyInChatRoom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRooms_ChatUsers_Creator_Key",
                table: "ChatRooms");

            migrationBuilder.RenameColumn(
                name: "Creator_Key",
                table: "ChatRooms",
                newName: "CreatorKey");

            migrationBuilder.RenameIndex(
                name: "IX_ChatRooms_Creator_Key",
                table: "ChatRooms",
                newName: "IX_ChatRooms_CreatorKey");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_ChatUsers_CreatorKey",
                table: "ChatRooms",
                column: "CreatorKey",
                principalTable: "ChatUsers",
                principalColumn: "Key",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRooms_ChatUsers_CreatorKey",
                table: "ChatRooms");

            migrationBuilder.RenameColumn(
                name: "CreatorKey",
                table: "ChatRooms",
                newName: "Creator_Key");

            migrationBuilder.RenameIndex(
                name: "IX_ChatRooms_CreatorKey",
                table: "ChatRooms",
                newName: "IX_ChatRooms_Creator_Key");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRooms_ChatUsers_Creator_Key",
                table: "ChatRooms",
                column: "Creator_Key",
                principalTable: "ChatUsers",
                principalColumn: "Key",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
