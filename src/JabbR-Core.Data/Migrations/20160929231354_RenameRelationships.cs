using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JabbRCore.Migrations
{
    public partial class RenameRelationships : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatRoomChatUser1");

            migrationBuilder.DropTable(
                name: "ChatRoomChatUsers");

            migrationBuilder.DropTable(
                name: "ChatUserChatRooms");

            migrationBuilder.CreateTable(
                name: "UserRoom",
                columns: table => new
                {
                    ChatUser_Key = table.Column<int>(nullable: false),
                    ChatRoom_Key = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoom", x => new { x.ChatUser_Key, x.ChatRoom_Key });
                    table.ForeignKey(
                        name: "FK_UserRoom_ChatRooms_ChatRoom_Key",
                        column: x => x.ChatRoom_Key,
                        principalTable: "ChatRooms",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoom_ChatUsers_ChatUser_Key",
                        column: x => x.ChatUser_Key,
                        principalTable: "ChatUsers",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoomAllowed",
                columns: table => new
                {
                    ChatRoom_Key = table.Column<int>(nullable: false),
                    ChatUser_Key = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoomAllowed", x => new { x.ChatRoom_Key, x.ChatUser_Key });
                    table.ForeignKey(
                        name: "FK_UserRoomAllowed_ChatRooms_ChatRoom_Key",
                        column: x => x.ChatRoom_Key,
                        principalTable: "ChatRooms",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoomAllowed_ChatUsers_ChatUser_Key",
                        column: x => x.ChatUser_Key,
                        principalTable: "ChatUsers",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoomOwner",
                columns: table => new
                {
                    ChatRoom_Key = table.Column<int>(nullable: false),
                    ChatUser_Key = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoomOwner", x => new { x.ChatRoom_Key, x.ChatUser_Key });
                    table.ForeignKey(
                        name: "FK_UserRoomOwner_ChatRooms_ChatRoom_Key",
                        column: x => x.ChatRoom_Key,
                        principalTable: "ChatRooms",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoomOwner_ChatUsers_ChatUser_Key",
                        column: x => x.ChatUser_Key,
                        principalTable: "ChatUsers",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoom_ChatRoom_Key",
                table: "UserRoom",
                column: "ChatRoom_Key");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoomAllowed_ChatUser_Key",
                table: "UserRoomAllowed",
                column: "ChatUser_Key");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoomOwner_ChatUser_Key",
                table: "UserRoomOwner",
                column: "ChatUser_Key");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRoom");

            migrationBuilder.DropTable(
                name: "UserRoomAllowed");

            migrationBuilder.DropTable(
                name: "UserRoomOwner");

            migrationBuilder.CreateTable(
                name: "ChatRoomChatUser1",
                columns: table => new
                {
                    ChatRoom_Key = table.Column<int>(nullable: false),
                    ChatUser_Key = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRoomChatUser1", x => new { x.ChatRoom_Key, x.ChatUser_Key });
                    table.ForeignKey(
                        name: "FK_ChatRoomChatUser1_ChatRooms_ChatRoom_Key",
                        column: x => x.ChatRoom_Key,
                        principalTable: "ChatRooms",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatRoomChatUser1_ChatUsers_ChatUser_Key",
                        column: x => x.ChatUser_Key,
                        principalTable: "ChatUsers",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatRoomChatUsers",
                columns: table => new
                {
                    ChatRoom_Key = table.Column<int>(nullable: false),
                    ChatUser_Key = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRoomChatUsers", x => new { x.ChatRoom_Key, x.ChatUser_Key });
                    table.ForeignKey(
                        name: "FK_ChatRoomChatUsers_ChatRooms_ChatRoom_Key",
                        column: x => x.ChatRoom_Key,
                        principalTable: "ChatRooms",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatRoomChatUsers_ChatUsers_ChatUser_Key",
                        column: x => x.ChatUser_Key,
                        principalTable: "ChatUsers",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatUserChatRooms",
                columns: table => new
                {
                    ChatUser_Key = table.Column<int>(nullable: false),
                    ChatRoom_Key = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatUserChatRooms", x => new { x.ChatUser_Key, x.ChatRoom_Key });
                    table.ForeignKey(
                        name: "FK_ChatUserChatRooms_ChatRooms_ChatRoom_Key",
                        column: x => x.ChatRoom_Key,
                        principalTable: "ChatRooms",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatUserChatRooms_ChatUsers_ChatUser_Key",
                        column: x => x.ChatUser_Key,
                        principalTable: "ChatUsers",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomChatUser1_ChatUser_Key",
                table: "ChatRoomChatUser1",
                column: "ChatUser_Key");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoomChatUsers_ChatUser_Key",
                table: "ChatRoomChatUsers",
                column: "ChatUser_Key");

            migrationBuilder.CreateIndex(
                name: "IX_ChatUserChatRooms_ChatRoom_Key",
                table: "ChatUserChatRooms",
                column: "ChatRoom_Key");
        }
    }
}
